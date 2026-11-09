using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for sports complex catalog operations.
/// </summary>
[ApiController]
[Route("api/complexe-sportive")]
public class ComplexeSportiveController : ControllerBase
{
    private readonly IComplexeSportiveService _complexeSportiveService;

    /// <summary>
    /// Creates a new instance of <see cref="ComplexeSportiveController"/>.
    /// </summary>
    /// <param name="complexeSportiveService">The sports complexes service.</param>
    public ComplexeSportiveController(IComplexeSportiveService complexeSportiveService)
    {
        _complexeSportiveService = complexeSportiveService;
    }

    /// <summary>
    /// Gets all sports complexes ordered by name.
    /// </summary>
    /// <param name="ct">Cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>A 200 OK response containing the list of complexes.</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<ComplexDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var complexes = await _complexeSportiveService.GetAllAsync(ct);
        return Ok(complexes);
    }
}
