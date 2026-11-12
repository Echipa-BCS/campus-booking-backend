using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for user subscriptions.
/// </summary>
/// <remarks>
/// Requires authentication. The user's ID is extracted from the JWT.
/// </remarks>
[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SubscriptionsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all subscriptions for the currently authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMySubscriptions(CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        var subscriptions = await _db.Abonamente
            .Where(a => a.UtilizatorId == userId)
            .OrderByDescending(a => a.DataStart)
            .Select(a => new AbonamentDto
            {
                Id = a.Id,
                Tip = a.Tip.ToString(),
                DataStart = a.DataStart,
                DataExpirare = a.DataExpirare,
                SedinteRamase = a.SedinteRamase,
                Activ = a.Activ,
                UtilizatorEmail = a.Utilizator!.Email,
            })
            .ToListAsync(ct);

        return Ok(subscriptions);
    }

    /// <summary>
    /// Purchases / activates a new mock subscription for the authenticated user.
    /// </summary>
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseSubscription(
        [FromBody] PurchaseSubscriptionRequest req,
        CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        var utilizator = await _db.Utilizatori.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (utilizator is null)
        {
            return NotFound(new { error = "Utilizatorul nu a fost găsit." });
        }

        var tip = string.Equals(req.Tip, "INTERVAL_FIX", StringComparison.OrdinalIgnoreCase)
            ? TipAbonament.INTERVAL_FIX
            : TipAbonament.GENERAL_ACCESS;

        var sedinte = tip == TipAbonament.INTERVAL_FIX ? 5 : 10;
        var basePrice = tip == TipAbonament.INTERVAL_FIX ? 120m : 200m;
        var discountRate = utilizator.Rol switch
        {
            RolUtilizator.STUDENT => 0.40m,
            RolUtilizator.PROFESOR => 0.20m,
            _ => 0m,
        };
        var finalPrice = Math.Round(basePrice * (1m - discountRate), 2);

        var now = DateTime.UtcNow;
        var abonament = new Abonament
        {
            UtilizatorId = userId,
            Tip = tip,
            DataStart = now,
            DataExpirare = now.AddDays(30),
            SedinteRamase = sedinte,
            Activ = true,
        };

        _db.Abonamente.Add(abonament);
        await _db.SaveChangesAsync(ct);

        var plata = new Plata
        {
            UtilizatorId = userId,
            RezervareId = null,
            AbonamentId = abonament.Id,
            StripeSessionId = $"mock_sub_buy_{Guid.NewGuid():N}",
            Suma = finalPrice,
            DataPlata = now,
            Stare = "SUCCES",
            MetodaPlata = "Card",
        };
        _db.Plati.Add(plata);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetMySubscriptions), new { }, new AbonamentDto
        {
            Id = abonament.Id,
            Tip = abonament.Tip.ToString(),
            DataStart = abonament.DataStart,
            DataExpirare = abonament.DataExpirare,
            SedinteRamase = abonament.SedinteRamase,
            Activ = abonament.Activ,
            UtilizatorEmail = utilizator.Email,
        });
    }
}
