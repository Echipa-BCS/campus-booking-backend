using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Service implementation for querying sports complexes from the database.
/// </summary>
public class ComplexeSportiveService : IComplexeSportiveService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Creates a new instance of <see cref="ComplexeSportiveService"/>.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public ComplexeSportiveService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all sports complexes ordered by name.
    /// </summary>
    /// <param name="ct">Cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>A collection of <see cref="ComplexDto"/> ordered by complex name.</returns>
    public async Task<IEnumerable<ComplexDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.ComplexeSportive
            .AsNoTracking()
            .OrderBy(c => c.Nume)
            .Select(c => new ComplexDto
            {
                Id = c.Id,
                Nume = c.Nume,
                Adresa = c.Adresa,
                Descriere = c.Descriere,
                OraDeschidere = c.OraDeschidere,
                OraInchidere = c.OraInchidere
            })
            .ToListAsync(ct);
    }
}
