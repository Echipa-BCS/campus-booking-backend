namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Request payload for adding equipment to a reservation.
/// </summary>
public class RezervareEchipamentRequest
{
    public int EchipamentId { get; set; }
    public int Cantitate { get; set; } = 1;
}

/// <summary>
/// Response payload for equipment added to a reservation.
/// </summary>
public class RezervareEchipamentResponse
{
    public int EchipamentId { get; set; }
    public string EchipamentDenumire { get; set; } = string.Empty;
    public int Cantitate { get; set; }
    public decimal SubtotalPret { get; set; }
    public decimal PretTotalRezervare { get; set; }
}
