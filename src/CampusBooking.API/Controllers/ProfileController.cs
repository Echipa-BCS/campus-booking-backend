using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.API.Controllers;

/// <summary>
/// Controller for user profile management.
/// </summary>
/// <remarks>
/// Requires authentication. The user's ID is extracted from the JWT.
/// </remarks>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        var utilizator = await _db.Utilizatori
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (utilizator is null)
        {
            return NotFound(new { error = "Utilizatorul nu a fost găsit." });
        }

        return Ok(new UserDto
        {
            Id = utilizator.Id,
            Nume = utilizator.Nume,
            Prenume = utilizator.Prenume,
            Email = utilizator.Email,
            Telefon = utilizator.Telefon,
            Rol = utilizator.Rol.ToString(),
            DataInregistrare = utilizator.DataInregistrare,
        });
    }

    /// <summary>
    /// Updates the profile of the currently authenticated user.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] ProfileUpdateRequest req,
        CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token invalid sau expirat." });
        }

        var utilizator = await _db.Utilizatori
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (utilizator is null)
        {
            return NotFound(new { error = "Utilizatorul nu a fost găsit." });
        }

        // Check email uniqueness if changing email
        if (utilizator.Email != req.Email &&
            await _db.Utilizatori.AnyAsync(u => u.Email == req.Email, ct))
        {
            return BadRequest(new { error = "Email-ul este deja înregistrat." });
        }

        utilizator.Nume = req.Nume;
        utilizator.Prenume = req.Prenume;
        utilizator.Email = req.Email;
        utilizator.Telefon = req.Telefon;

        await _db.SaveChangesAsync(ct);

        return Ok(new UserDto
        {
            Id = utilizator.Id,
            Nume = utilizator.Nume,
            Prenume = utilizator.Prenume,
            Email = utilizator.Email,
            Telefon = utilizator.Telefon,
            Rol = utilizator.Rol.ToString(),
            DataInregistrare = utilizator.DataInregistrare,
        });
    }
}
