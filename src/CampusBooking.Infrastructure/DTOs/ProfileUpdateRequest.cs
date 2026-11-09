namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Request payload for updating a user profile.
/// </summary>
public class ProfileUpdateRequest
{
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
}
