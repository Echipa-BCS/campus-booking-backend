using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Endpoints for managing reservations.
/// </summary>
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
}
