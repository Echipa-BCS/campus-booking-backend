using CampusBooking.Infrastructure.DTOs;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Service contract for querying sports complexes.
/// </summary>
public interface IComplexeSportiveService
{
    /// <summary>
    /// Retrieves all sports complexes ordered by name.
    /// </summary>
    /// <param name="ct">Cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>A collection of <see cref="ComplexDto"/> ordered by complex name.</returns>
    Task<IEnumerable<ComplexDto>> GetAllAsync(CancellationToken ct = default);
}
