using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for user bookings: history and cancellation.
/// </summary>
/// <remarks>
/// All endpoints require authentication. The user's ID is extracted from the JWT.
/// </remarks>
[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all reservations for the currently authenticated user,
    /// ordered by most recent first.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyBookings(CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        var now = DateTime.UtcNow;

        // Auto-expire any TEMPORAR_BLOCATA reservations whose lock has expired and restore their equipment stock
        var expiredLocks = await _db.Rezervari
            .Include(r => r.RezervareEchipamente)
            .ThenInclude(re => re.Echipament)
            .Where(r => r.UtilizatorId == userId
                && r.Stare == StareRezervare.TEMPORAR_BLOCATA
                && r.DataExpirareBlocaj.HasValue
                && r.DataExpirareBlocaj.Value < now)
            .ToListAsync(ct);

        if (expiredLocks.Count > 0)
        {
            foreach (var exp in expiredLocks)
            {
                exp.Stare = StareRezervare.EXPIRATA;
                foreach (var re in exp.RezervareEchipamente)
                {
                    if (re.Echipament is not null)
                    {
                        re.Echipament.StocTotal += re.Cantitate;
                    }
                }
            }
            await _db.SaveChangesAsync(ct);
        }

        var bookings = await _db.Rezervari
            .Include(r => r.Teren)
            .ThenInclude(t => t!.Complex)
            .Include(r => r.RezervareEchipamente)
            .ThenInclude(re => re.Echipament)
            .Where(r => r.UtilizatorId == userId)
            .OrderByDescending(r => r.DataStart)
            .Select(r => new BookingSummaryDto
            {
                Id = r.Id,
                CodRezervare = r.CodRezervare,
                TerenId = r.TerenId,
                TerenNume = r.Teren!.Nume,
                TipSport = r.Teren!.TipSport,
                ComplexNume = r.Teren!.Complex!.Nume,
                DataStart = r.DataStart,
                DataEnd = r.DataEnd,
                Stare = r.Stare.ToString(),
                PretTotal = r.PretTotal,
                DiscountAplicat = r.DiscountAplicat,
                DataExpirareBlocaj = r.DataExpirareBlocaj.HasValue
                    ? DateTime.SpecifyKind(r.DataExpirareBlocaj.Value, DateTimeKind.Utc).ToString("o")
                    : null,
                Echipamente = r.RezervareEchipamente.Select(re => new RezervareEchipamentResponse
                {
                    EchipamentId = re.EchipamentId,
                    EchipamentDenumire = re.Echipament != null ? re.Echipament.Denumire : string.Empty,
                    Cantitate = re.Cantitate,
                    SubtotalPret = re.SubtotalPret,
                    PretTotalRezervare = r.PretTotal,
                }).ToList(),
            })
            .ToListAsync(ct);

        return Ok(bookings);
    }

    /// <summary>
    /// Cancels a booking, transitioning its status to ANULATA.
    /// Only the booking owner can cancel, and only TEMPORAR_BLOCATA or CONFIRMATA bookings can be cancelled.
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(
        int id,
        CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        var rezervare = await _db.Rezervari
            .Include(r => r.RezervareEchipamente)
            .ThenInclude(re => re.Echipament)
            .Include(r => r.Plati)
            .ThenInclude(p => p.Abonament)
            .FirstOrDefaultAsync(r => r.Id == id && r.UtilizatorId == userId, ct);

        if (rezervare is null)
        {
            return NotFound(new { error = "Rezervarea nu a fost găsit sau nu vă aparține." });
        }

        if (rezervare.Stare == StareRezervare.ANULATA ||
            rezervare.Stare == StareRezervare.FINALIZATA ||
            rezervare.Stare == StareRezervare.EXPIRATA)
        {
            return BadRequest(new { error = "Această rezervare nu poate fi anulată." });
        }

        // Restore any rented equipment stock
        foreach (var re in rezervare.RezervareEchipamente)
        {
            if (re.Echipament is not null)
            {
                re.Echipament.StocTotal += re.Cantitate;
            }
        }

        // If confirmed via subscription, refund 1 session to the subscription
        foreach (var plata in rezervare.Plati)
        {
            if (plata.Abonament is not null)
            {
                plata.Abonament.SedinteRamase += 1;
            }
        }

        rezervare.Stare = StareRezervare.ANULATA;
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Rezervarea a fost anulată.", id = rezervare.Id, stare = rezervare.Stare.ToString() });
    }
}
