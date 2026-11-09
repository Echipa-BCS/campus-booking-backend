namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Request payload for processing a subscription-based (mock) payment that confirms a temporary reservation.
/// </summary>
public class SubscriptionPaymentRequest
{
    public int UtilizatorId { get; set; }
    public int RezervareId { get; set; }
}
