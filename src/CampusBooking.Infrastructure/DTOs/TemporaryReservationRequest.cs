namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Request payload for creating a temporary (locked) reservation of a sports court.
/// </summary>
public class TemporaryReservationRequest
{
    public int UtilizatorId { get; set; }
    public int TerenId { get; set; }
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }
}
