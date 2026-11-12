using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Endpoints for managing reservations.
/// </summary>
/// <remarks>
/// The temporary reservation endpoint is public; equipment addition requires authentication.
/// </remarks>
[ApiController]
[Route("api/rezervari")]
public class RezervariController : ControllerBase
{
    private readonly IRezervareService _rezervareService;

    public RezervariController(IRezervareService rezervareService)
    {
        _rezervareService = rezervareService;
    }

    /// <summary>
    /// Temporarily reserves a court slot for the requested interval.
    /// </summary>
    /// <param name="req">The reservation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 Created with the temporary reservation when the slot is available;
    /// 409 with a JSON error body when the slot is no longer available.
    /// </returns>
    [HttpPost("temporary")]
    public async Task<IResult> ReserveTemporarily([FromBody] TemporaryReservationRequest req, CancellationToken ct = default)
    {
        var (isAvailable, reservation, _) = await _rezervareService.ReserveTemporarilyAsync(req, ct);

        if (isAvailable && reservation is not null)
        {
            return Results.Json(reservation, statusCode: 201);
        }

        return Results.Json(new { error = "The time slot is no longer available" }, statusCode: 409);
    }

    /// <summary>
    /// Adds equipment to an existing temporary reservation.
    /// Requires authentication. Decrements equipment stock and creates a booking-to-equipment link.
    /// </summary>
    [HttpPost("{rezervareId}/echipamente")]
    [Authorize]
    public async Task<IResult> AddEchipament(
        int rezervareId,
        [FromBody] RezervareEchipamentRequest req,
        CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(new { error = "Token invalid sau expirat." }, statusCode: 401);
        }

        var (success, equipment, errorMessage) =
            await _rezervareService.AddEchipamentToRezervareAsync(rezervareId, userId, req, ct);

        if (!success)
        {
            return Results.Json(new { error = errorMessage }, statusCode: 400);
        }

        return Results.Json(equipment, statusCode: 201);
    }
}
