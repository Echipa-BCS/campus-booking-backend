using CampusBooking.Infrastructure.DTOs;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Defines operations for reserving sports court slots with concurrency-safe temporary locking.
/// </summary>
public interface IRezervareService
{
    /// <summary>
    /// Attempts to temporarily reserve a court for the requested time interval.
    /// </summary>
    /// <param name="req">The reservation request containing user, court, and time interval.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple where <see cref="IsAvailable"/> indicates whether the slot was free,
    /// <see cref="Reservation"/> holds the created temporary reservation when available,
    /// and <see cref="ErrorMessage"/> describes why the slot is unavailable.
    /// </returns>
    Task<(bool IsAvailable, TemporaryReservationResponse? Reservation, string? ErrorMessage)> ReserveTemporarilyAsync(
        TemporaryReservationRequest req,
        CancellationToken ct = default);
}
