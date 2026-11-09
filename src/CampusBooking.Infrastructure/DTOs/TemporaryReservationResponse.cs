namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Response payload returned when a temporary reservation is successfully created.
/// </summary>
public class TemporaryReservationResponse
{
    public int Id { get; set; }
    public string CodRezervare { get; set; } = string.Empty;
    public int TerenId { get; set; }
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }
    public string Stare { get; set; } = string.Empty;
    public DateTime DataExpirareBlocaj { get; set; }
    public decimal PretTotal { get; set; }
    public decimal DiscountAplicat { get; set; }
}
