using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Implements the booking and concurrency engine using EF Core transactions with
/// <see cref="IsolationLevel.Serializable"/> to prevent double-booking race conditions.
/// </summary>
public class RezervareService : IRezervareService
{
    private const string UnavailableMessage = "The time slot is no longer available";

    private readonly AppDbContext _db;

    public RezervareService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Attempts to temporarily reserve a court for the requested time interval.
    ///
    /// Workflow (all inside a <see cref="IsolationLevel.Serializable"/> transaction):
    /// <list type="number">
    /// <item><description>Capture <see cref="DateTime.UtcNow"/> as the reference timestamp.</description></item>
    /// <item><description>Check availability: a conflict exists when another reservation for the same
    /// <see cref="Entities.Teren"/> overlaps the requested interval and is not already free.
    /// Overlap: existing.DataStart &lt; req.DataEnd && existing.DataEnd &gt; req.DataStart.
    /// Conflicting statuses: <see cref="StareRezervare.TEMPORAR_BLOCATA"/> (not yet expired) and
    /// <see cref="StareRezervare.CONFIRMATA"/>. Expired temporary locks
    /// (<see cref="Rezervare.DataExpirareBlocaj"/> &lt; now) are treated as free.</description></item>
    /// <item><description>If a conflict exists, return unavailable without committing (transaction rolls back).</description></item>
    /// <item><description>Otherwise, insert a new <see cref="Rezervare"/> with status
    /// <see cref="StareRezervare.TEMPORAR_BLOCATA"/> and <see cref="Rezervare.DataExpirareBlocaj"/> = now + 10 minutes.</description></item>
    /// <item><description>Save changes and commit the transaction.</description></item>
    /// </list>
    /// </summary>
    /// <param name="req">The reservation request containing user, court, and time interval.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> with the mapped response when the slot was available;
    /// otherwise <see langword="false"/> with <see cref="UnavailableMessage"/>.
    /// </returns>
    public async Task<(bool IsAvailable, TemporaryReservationResponse? Reservation, string? ErrorMessage)> ReserveTemporarilyAsync(
        TemporaryReservationRequest req,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            bool hasConflict = await _db.Rezervari
                .AsNoTracking()
                .AnyAsync(r => r.TerenId == req.TerenId
                    && r.DataStart < req.DataEnd
                    && r.DataEnd > req.DataStart
                    && (r.Stare == StareRezervare.TEMPORAR_BLOCATA || r.Stare == StareRezervare.CONFIRMATA)
                    && !(r.DataExpirareBlocaj.HasValue && r.DataExpirareBlocaj.Value < now), ct);

            if (hasConflict)
            {
                // Slot is not free: return without committing so the transaction rolls back.
                return (false, null, UnavailableMessage);
            }

            var utilizator = await _db.Utilizatori
                .FirstOrDefaultAsync(u => u.Id == req.UtilizatorId, ct);

            var teren = await _db.Terenuri
                .FirstOrDefaultAsync(t => t.Id == req.TerenId, ct);

            if (utilizator is null || teren is null)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "User or court not found.");
            }

            // -----------------------------------------------------------------
            // Pricing & Role Discounts (§3 of Skills/backendengine.md)
            // Base price = court hourly rate × booking duration (hours).
            // Discount: STUDENT 40%, PROFESOR 20%, USER 0%.
            // -----------------------------------------------------------------
            var durationHours = (req.DataEnd - req.DataStart).TotalHours;
            var basePrice = (decimal)durationHours * teren.TarifOrarStandard;

            var discountRate = utilizator.Rol switch
            {
                RolUtilizator.STUDENT => 0.40m,
                RolUtilizator.PROFESOR => 0.20m,
                _ => 0m // RolUtilizator.USER and any other role
            };

            var discountAplicat = Math.Round(basePrice * discountRate, 2);
            var pretTotal = Math.Round(basePrice - discountAplicat, 2);

            var rezervare = new Rezervare
            {
                UtilizatorId = req.UtilizatorId,
                TerenId = req.TerenId,
                CodRezervare = Guid.NewGuid().ToString("N"),
                DataStart = req.DataStart,
                DataEnd = req.DataEnd,
                DataExpirareBlocaj = now.AddMinutes(10),
                PretTotal = pretTotal,
                DiscountAplicat = discountAplicat,
                Stare = StareRezervare.TEMPORAR_BLOCATA
            };

            _db.Rezervari.Add(rezervare);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync();

            return (true, MapToResponse(rezervare), null);
        }
        catch (Exception ex) when (IsConcurrencyConflict(ex))
        {
            // Concurrency conflict / deadlock: roll back (via disposal) and report the slot as unavailable.
            return (false, null, UnavailableMessage);
        }
        catch
        {
            // Transaction rolls back via disposal; propagate unexpected errors.
            throw;
        }
    }

    private static TemporaryReservationResponse MapToResponse(Rezervare r) => new()
    {
        Id = r.Id,
        CodRezervare = r.CodRezervare,
        TerenId = r.TerenId,
        DataStart = r.DataStart,
        DataEnd = r.DataEnd,
        Stare = r.Stare.ToString(),
        DataExpirareBlocaj = r.DataExpirareBlocaj ?? DateTime.UtcNow,
        PretTotal = r.PretTotal,
        DiscountAplicat = r.DiscountAplicat
    };

    /// <summary>
    /// Determines whether an exception represents a concurrency conflict
    /// (EF Core concurrency exception or a MySQL deadlock / lock wait timeout).
    /// </summary>
    private static bool IsConcurrencyConflict(Exception ex)
    {
        var current = ex;
        while (current != null)
        {
            if (current is DbUpdateConcurrencyException)
            {
                return true;
            }

            var typeName = current.GetType().FullName ?? string.Empty;
            if (typeName.Contains("MySqlException", StringComparison.OrdinalIgnoreCase))
            {
                var message = current.Message ?? string.Empty;
                if (message.Contains("deadlock", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("lock wait timeout", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("1213")
                    || message.Contains("1205"))
                {
                    return true;
                }
            }

            current = current.InnerException;
        }

        return false;
    }
}
