using CampusBooking.Infrastructure.DTOs;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Service contract for searching sports courts.
/// </summary>
public interface ITerenuriService
{
    /// <summary>
    /// Searches courts with optional filtering by location and sport type.
    /// </summary>
    /// <param name="location">
    /// Optional location filter matched against the complex address or name
    /// (case-insensitive, contains). When null or whitespace, no location filter is applied.
    /// </param>
    /// <param name="tipSport">
    /// Optional sport type filter matched against the court sport type
    /// (case-insensitive, contains). When null or whitespace, no sport type filter is applied.
    /// </param>
    /// <param name="ct">Cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A collection of <see cref="CourtDto"/> ordered by complex name then court name.
    /// When neither filter is provided, all courts are returned.
    /// </returns>
    Task<IEnumerable<CourtDto>> SearchAsync(string? location, string? tipSport, CancellationToken ct = default);
}
