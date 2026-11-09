namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Request payload for processing a single-session (mock) payment that confirms a temporary reservation.
/// </summary>
public class PaymentRequest
{
    public int UtilizatorId { get; set; }
    public int RezervareId { get; set; }
    public decimal Suma { get; set; }
    public string MetodaPlata { get; set; } = string.Empty;
}
