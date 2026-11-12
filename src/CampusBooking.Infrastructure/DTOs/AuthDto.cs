namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Request payload for user registration.
/// </summary>
public class RegisterRequest
{
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Parola { get; set; } = string.Empty;
    public string? Telefon { get; set; }
}

/// <summary>
/// Request payload for user login.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Parola { get; set; } = string.Empty;
}

/// <summary>
/// Response payload returned after successful authentication.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

/// <summary>
/// User data transfer object exposed through auth endpoints.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string Rol { get; set; } = string.Empty;
    public DateTime DataInregistrare { get; set; }
}
