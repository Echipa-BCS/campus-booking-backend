using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for sports court search operations.
/// </summary>
[ApiController]
[Route("api/terenuri")]
public class TerenuriController : ControllerBase
{
    private readonly ITerenuriService _terenuriService;

    /// <summary>
    /// Creates a new instance of <see cref="TerenuriController"/>.
    /// </summary>
    /// <param name="terenuriService">The courts service.</param>
    public TerenuriController(ITerenuriService terenuriService)
    {
        _terenuriService = terenuriService;
    }

    /// <summary>
    /// Searches courts with optional location and sport type filters.
    /// </summary>
    /// <param name="location">
    /// Optional location filter matched against the complex address or name.
    /// </param>
    /// <param name="tipSport">
    /// Optional sport type filter matched against the court sport type.
    /// </param>
    /// <param name="ct">Cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>A 200 OK response containing the list of matching courts.</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<CourtDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(string? location, string? tipSport, CancellationToken ct = default)
    {
        var courts = await _terenuriService.SearchAsync(location, tipSport, ct);
        return Ok(courts);
    }
}
