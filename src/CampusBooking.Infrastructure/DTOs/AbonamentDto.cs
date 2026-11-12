namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Subscription data transfer object exposed through the subscriptions endpoint.
/// </summary>
public class AbonamentDto
{
    public int Id { get; set; }
    public string Tip { get; set; } = string.Empty;
    public DateTime DataStart { get; set; }
    public DateTime DataExpirare { get; set; }
    public int SedinteRamase { get; set; }
    public bool Activ { get; set; }
    public string? UtilizatorEmail { get; set; }
}
