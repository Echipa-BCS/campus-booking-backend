using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for mock payment processing and subscription-based booking confirmation.
/// </summary>
/// <remarks>
/// Academic/lab implementation: simulates successful payments locally, no real Stripe calls.
/// </remarks>
[ApiController]
[Route("api/plati")]
public class PlatiController : ControllerBase
{
    private readonly IPlataService _plataService;

    public PlatiController(IPlataService plataService)
    {
        _plataService = plataService;
    }

    /// <summary>
    /// Processes a mock single-session payment that transitions a temporary reservation to CONFIRMATA.
    /// </summary>
    [HttpPost("single-session")]
    public async Task<IActionResult> ProcessSingleSession(
        [FromBody] PaymentRequest req,
        CancellationToken ct = default)
    {
        var (success, response, errorMessage) =
            await _plataService.ProcessSingleSessionPaymentAsync(req, ct);

        if (!success)
        {
            return BadRequest(new { error = errorMessage });
        }

        return Ok(response);
    }

    /// <summary>
    /// Processes a mock subscription-based payment: verifies an active subscription,
    /// decrements remaining sessions, and confirms the reservation.
    /// </summary>
    [HttpPost("subscription")]
    public async Task<IActionResult> ProcessSubscription(
        [FromBody] SubscriptionPaymentRequest req,
        CancellationToken ct = default)
    {
        var (success, response, errorMessage) =
            await _plataService.ProcessSubscriptionPaymentAsync(req, ct);

        if (!success)
        {
            return BadRequest(new { error = errorMessage });
        }

        return Ok(response);
    }
}
