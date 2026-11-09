using CampusBooking.Infrastructure.Entities;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Idempotent database seeder for the Campus Sports Booking system.
/// Only inserts data when the corresponding tables are empty, so it is safe to run on every startup.
/// </summary>
public interface IDbSeeder
{
    /// <summary>
    /// Seeds the database with a university sports complex in Suceava, courts, equipment,
    /// and sample users for each role (STUDENT, PROFESOR, USER).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SeedAsync(CancellationToken ct = default);
}
