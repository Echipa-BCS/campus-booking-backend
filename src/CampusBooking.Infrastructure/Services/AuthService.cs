using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Service for user authentication and registration.
/// </summary>
/// <remarks>
/// Academic/lab implementation: passwords are hashed with SHA-256 and tokens are JWTs
/// signed with a symmetric key from configuration. No external identity provider is used.
/// </remarks>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user, automatically assigning a role based on the email suffix.
    /// </summary>
    Task<(bool Success, AuthResponse? Response, string? ErrorMessage)> RegisterAsync(
        RegisterRequest req, CancellationToken ct = default);

    /// <summary>
    /// Validates credentials and returns an auth token on success.
    /// </summary>
    Task<(bool Success, AuthResponse? Response, string? ErrorMessage)> LoginAsync(
        LoginRequest req, CancellationToken ct = default);

    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    Task<(bool Success, UserDto? User, string? ErrorMessage)> GetCurrentUserAsync(
        int userId, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<(bool Success, AuthResponse? Response, string? ErrorMessage)> RegisterAsync(
        RegisterRequest req, CancellationToken ct = default)
    {
        // Email uniqueness check
        if (await _db.Utilizatori.AnyAsync(u => u.Email == req.Email, ct))
        {
            return (false, null, "Email-ul este deja înregistrat.");
        }

        // Auto-determine role by email suffix (§1 of Skills/backendengine.md)
        var rol = DetermineRole(req.Email);

        var utilizator = new Utilizator
        {
            Nume = req.Nume,
            Prenume = req.Prenume,
            Email = req.Email,
            ParolaHash = HashPassword(req.Parola),
            Telefon = req.Telefon,
            Rol = rol,
            DataInregistrare = DateTime.UtcNow,
        };

        _db.Utilizatori.Add(utilizator);
        await _db.SaveChangesAsync(ct);

        var token = GenerateToken(utilizator);
        var response = new AuthResponse
        {
            Token = token,
            User = MapToDto(utilizator),
        };

        return (true, response, null);
    }

    /// <inheritdoc />
    public async Task<(bool Success, AuthResponse? Response, string? ErrorMessage)> LoginAsync(
        LoginRequest req, CancellationToken ct = default)
    {
        var utilizator = await _db.Utilizatori
            .FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (utilizator is null || !VerifyPassword(req.Parola, utilizator.ParolaHash))
        {
            return (false, null, "Email sau parolă incorecte.");
        }

        var token = GenerateToken(utilizator);
        var response = new AuthResponse
        {
            Token = token,
            User = MapToDto(utilizator),
        };

        return (true, response, null);
    }

    /// <inheritdoc />
    public async Task<(bool Success, UserDto? User, string? ErrorMessage)> GetCurrentUserAsync(
        int userId, CancellationToken ct = default)
    {
        var utilizator = await _db.Utilizatori
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (utilizator is null)
        {
            return (false, null, "Utilizatorul nu a fost găsit.");
        }

        return (true, MapToDto(utilizator), null);
    }

    private static RolUtilizator DetermineRole(string email)
    {
        var domain = email.Contains('@') ? email.Split('@')[1].ToLowerInvariant() : string.Empty;
        return domain switch
        {
            "student.usv.ro" => RolUtilizator.STUDENT,
            "usm.ro" => RolUtilizator.PROFESOR,
            _ => RolUtilizator.USER,
        };
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        return HashPassword(password) == storedHash;
    }

    private string GenerateToken(Utilizator user)
    {
        var keyString = _configuration["Jwt:Key"] ?? "CampusSportsBookingSuperSecretKey12345!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "CampusBooking",
            audience: _configuration["Jwt:Audience"] ?? "CampusBookingClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(Utilizator u) => new()
    {
        Id = u.Id,
        Nume = u.Nume,
        Prenume = u.Prenume,
        Email = u.Email,
        Telefon = u.Telefon,
        Rol = u.Rol.ToString(),
        DataInregistrare = u.DataInregistrare,
    };
}
