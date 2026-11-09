namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Response payload returned when a mock payment is successfully processed.
/// </summary>
public class PaymentResponse
{
    public int PlataId { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
    public int RezervareId { get; set; }
    public string StareRezervare { get; set; } = string.Empty;
    public decimal Suma { get; set; }
    public string Stare { get; set; } = string.Empty;
    public int? SedinteRamase { get; set; }
}
