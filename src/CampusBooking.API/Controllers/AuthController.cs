using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for user authentication and registration.
/// </summary>
/// <remarks>
/// Academic/lab implementation: JWT-based auth with SHA-256 password hashing.
/// Role assignment follows the rules in Skills/backendengine.md §1.
/// </remarks>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user. Role is assigned automatically based on email suffix:
    /// @student.usv.ro → STUDENT, @usm.ro → PROFESOR, other → USER.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest req,
        CancellationToken ct = default)
    {
        var (success, response, errorMessage) =
            await _authService.RegisterAsync(req, ct);

        if (!success)
        {
            return BadRequest(new { error = errorMessage });
        }

        return CreatedAtAction(nameof(Me), new { }, response);
    }

    /// <summary>
    /// Logs in a user and returns a JWT access token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req,
        CancellationToken ct = default)
    {
        var (success, response, errorMessage) =
            await _authService.LoginAsync(req, ct);

        if (!success)
        {
            return Unauthorized(new { error = errorMessage });
        }

        return Ok(response);
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        (bool success, UserDto? user, string? errorMessage) =
            await _authService.GetCurrentUserAsync(userId, ct);

        if (!success)
        {
            return NotFound(new { error = errorMessage });
        }

        return Ok(user);
    }
}
