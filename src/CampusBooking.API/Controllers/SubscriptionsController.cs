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
}
