namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Summary of a booking for the user's booking history page.
/// </summary>
public class BookingSummaryDto
{
    public int Id { get; set; }
    public string CodRezervare { get; set; } = string.Empty;
    public int TerenId { get; set; }
    public string TerenNume { get; set; } = string.Empty;
    public string TipSport { get; set; } = string.Empty;
    public string ComplexNume { get; set; } = string.Empty;
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }
    public string Stare { get; set; } = string.Empty;
    public decimal PretTotal { get; set; }
    public decimal DiscountAplicat { get; set; }
    public string? DataExpirareBlocaj { get; set; }
}
