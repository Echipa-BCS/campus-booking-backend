using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Service for mock payment processing and subscription-based booking confirmation.
/// </summary>
/// <remarks>
/// This is an academic/lab implementation: no real Stripe API calls or webhooks are used.
/// All transactions are simulated locally against the database.
/// </remarks>
public class PlataService : IPlataService
{
    private readonly AppDbContext _db;

    public PlataService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<(bool Success, PaymentResponse? Response, string? ErrorMessage)> ProcessSingleSessionPaymentAsync(
        PaymentRequest req, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);
        try
        {
            // Step 1: load the reservation belonging to the requesting user.
            var rezervare = await _db.Rezervari
                .FirstOrDefaultAsync(r => r.Id == req.RezervareId && r.UtilizatorId == req.UtilizatorId, ct);

            if (rezervare is null)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "Reservation not found or does not belong to the user.");
            }

            // Step 2: only temporary reservations can be confirmed.
            if (rezervare.Stare != StareRezervare.TEMPORAR_BLOCATA)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "Only temporary reservations can be confirmed.");
            }

            // Step 3: reject expired temporary locks.
            if (rezervare.DataExpirareBlocaj.HasValue && rezervare.DataExpirareBlocaj.Value < now)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "The temporary reservation has expired.");
            }

            // Step 4: transition to confirmed.
            rezervare.Stare = StareRezervare.CONFIRMATA;

            // Step 5: create the payment record (SUCCES).
            var plata = new Plata
            {
                UtilizatorId = req.UtilizatorId,
                RezervareId = req.RezervareId,
                AbonamentId = null,
                StripeSessionId = $"mock_{Guid.NewGuid():N}",
                Suma = req.Suma,
                DataPlata = now,
                Stare = "SUCCES",
                MetodaPlata = req.MetodaPlata,
            };

            _db.Plati.Add(plata);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return (
                true,
                new PaymentResponse
                {
                    PlataId = plata.Id,
                    StripeSessionId = plata.StripeSessionId,
                    RezervareId = plata.RezervareId ?? 0,
                    StareRezervare = rezervare.Stare.ToString(),
                    Suma = plata.Suma,
                    Stare = plata.Stare,
                    SedinteRamase = null,
                },
                null);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, PaymentResponse? Response, string? ErrorMessage)> ProcessSubscriptionPaymentAsync(
        SubscriptionPaymentRequest req, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);
        try
        {
            // Step 1: load the reservation belonging to the requesting user.
            var rezervare = await _db.Rezervari
                .FirstOrDefaultAsync(r => r.Id == req.RezervareId && r.UtilizatorId == req.UtilizatorId, ct);

            if (rezervare is null)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "Reservation not found or does not belong to the user.");
            }

            // Step 2: only temporary reservations can be confirmed.
            if (rezervare.Stare != StareRezervare.TEMPORAR_BLOCATA)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "Only temporary reservations can be confirmed.");
            }

            // Step 3: reject expired temporary locks.
            if (rezervare.DataExpirareBlocaj.HasValue && rezervare.DataExpirareBlocaj.Value < now)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "The temporary reservation has expired.");
            }

            // Step 4: verify an active subscription with remaining sessions.
            var abonament = await _db.Abonamente
                .FirstOrDefaultAsync(
                    a => a.UtilizatorId == req.UtilizatorId
                         && a.Activ
                         && a.SedinteRamase > 0
                         && a.DataExpirare > now,
                    ct);

            if (abonament is null)
            {
                await tx.RollbackAsync(ct);
                return (false, null, "No active subscription with remaining sessions available.");
            }

            // Step 5: decrement remaining sessions.
            abonament.SedinteRamase -= 1;

            // Step 6: transition to confirmed.
            rezervare.Stare = StareRezervare.CONFIRMATA;

            // Step 7: create the payment record (SUCCES), linked to the subscription.
            var plata = new Plata
            {
                UtilizatorId = req.UtilizatorId,
                RezervareId = req.RezervareId,
                AbonamentId = abonament.Id,
                StripeSessionId = $"mock_sub_{Guid.NewGuid():N}",
                Suma = 0,
                DataPlata = now,
                Stare = "SUCCES",
                MetodaPlata = "Abonament",
            };

            _db.Plati.Add(plata);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return (
                true,
                new PaymentResponse
                {
                    PlataId = plata.Id,
                    StripeSessionId = plata.StripeSessionId,
                    RezervareId = plata.RezervareId ?? 0,
                    StareRezervare = rezervare.Stare.ToString(),
                    Suma = plata.Suma,
                    Stare = plata.Stare,
                    SedinteRamase = abonament.SedinteRamase,
                },
                null);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
