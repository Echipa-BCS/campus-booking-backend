using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Service implementation for searching sports courts in the database.
/// </summary>
public class TerenuriService : ITerenuriService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Creates a new instance of <see cref="TerenuriService"/>.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public TerenuriService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Searches courts with optional filtering by location and sport type.
    /// </summary>
    /// <param name="location">
    /// Optional location filter matched against the complex address or name
    /// (case-insensitive, contains).
    /// </param>
    /// <param name="tipSport">
    /// Optional sport type filter matched against the court sport type
    /// (case-insensitive, contains).
    /// </param>
    /// <param name="ct">Cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A collection of <see cref="CourtDto"/> ordered by complex name then court name.
    /// When neither filter is provided, all courts are returned.
    /// </returns>
    public async Task<IEnumerable<CourtDto>> SearchAsync(string? location, string? tipSport, CancellationToken ct = default)
    {
        // Normalize filters once so the same casing is used for both the column and the parameter.
        var locationFilter = string.IsNullOrWhiteSpace(location) ? null : location.Trim().ToLower();
        var tipSportFilter = string.IsNullOrWhiteSpace(tipSport) ? null : tipSport.Trim().ToLower();

        var query = _db.Terenuri
            .Include(t => t.Complex)
            .AsNoTracking();

        // Dual filtering: apply location filter when provided.
        if (locationFilter is not null)
        {
            query = query.Where(t =>
                t.Complex.Adresa.ToLower().Contains(locationFilter) ||
                t.Complex.Nume.ToLower().Contains(locationFilter));
        }

        // Dual filtering: apply sport type filter when provided.
        if (tipSportFilter is not null)
        {
            query = query.Where(t => t.TipSport.ToLower().Contains(tipSportFilter));
        }

        return await query
            .OrderBy(t => t.Complex.Nume)
            .ThenBy(t => t.Nume)
            .Select(t => new CourtDto
            {
                Id = t.Id,
                ComplexId = t.ComplexId,
                ComplexNume = t.Complex.Nume,
                Nume = t.Nume,
                TipSport = t.TipSport,
                EsteAcoperit = t.EsteAcoperit,
                TarifOrarStandard = t.TarifOrarStandard,
                DurataMinimaOre = t.DurataMinimaOre
            })
            .ToListAsync(ct);
    }
}
