using System.Net;
using System.Net.Http.Json;
using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Services;
using CampusBooking.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampusBooking.Tests;

[Collection(IntegrationTestCollection.Name)]
public sealed class MigrationAndStartupTests
{
    private readonly CampusBookingApiFactory _factory;

    public MigrationAndStartupTests(CampusBookingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Migrations_AreFullyApplied_WithNoPendingMigrations()
    {
        using var scope = _factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

        Assert.Contains("20261123092828_InitialCreate", applied);
        Assert.Empty(pending);
    }

    [Fact]
    public async Task HealthEndpoint_AndSeededCatalog_WorkOnFreshMigratedDatabase()
    {
        await _factory.ResetDatabaseAsync(seed: true);
        using var client = _factory.CreateClient();

        var healthResponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);

        var complexes = await client.GetFromJsonAsync<List<ComplexDto>>("/api/complexe-sportive");
        Assert.NotNull(complexes);
        Assert.Single(complexes);
        Assert.Equal("Complexul Sportiv USV Suceava", complexes[0].Nume);

        var courts = await client.GetFromJsonAsync<List<CourtDto>>("/api/terenuri");
        Assert.NotNull(courts);
        Assert.Equal(4, courts.Count);
    }

    [Fact]
    public async Task DbSeeder_IsIdempotent_WhenExecutedMultipleTimes()
    {
        await _factory.ResetDatabaseAsync(seed: false);

        using (var scope = _factory.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
            await seeder.SeedAsync();
            await seeder.SeedAsync();
            await seeder.SeedAsync();
        }

        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(1, await db.ComplexeSportive.CountAsync());
            Assert.Equal(4, await db.Terenuri.CountAsync());
            Assert.Equal(4, await db.Echipamente.CountAsync());
            Assert.Equal(3, await db.Utilizatori.CountAsync());
            Assert.Equal(2, await db.Abonamente.CountAsync());
        }
    }

    [Fact]
    public async Task DatabaseInitializer_BaselinesPreExistingSnakeCaseSchema_WithoutDroppingData()
    {
        await _factory.ResetDatabaseAsync(seed: true);

        // Simulate a developer database created outside EF Core migrations (e.g. via SQL script):
        // all 8 snake_case tables + data exist, while __EFMigrationsHistory is empty.
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlRawAsync("DELETE FROM `__EFMigrationsHistory`;");
        }

        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DatabaseInitializer.ApplyMigrationsAsync(db);

            var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

            Assert.Contains(DatabaseInitializer.InitialMigrationId, applied);
            Assert.Empty(pending);
            // Existing seeded data must remain intact.
            Assert.Equal(1, await db.ComplexeSportive.CountAsync());
            Assert.Equal(4, await db.Terenuri.CountAsync());
        }
    }

    [Fact]
    public async Task DatabaseInitializer_BrokenLegacySchema_ThrowsWhenDisabled_AndRecreatesWhenFallbackEnabled()
    {
        // Simulate a corrupted / partial legacy schema (missing tables & PascalCase columns, no migration history).
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlRawAsync("""
                SET FOREIGN_KEY_CHECKS = 0;
                DROP TABLE IF EXISTS `plati`, `rezervare_echipamente`, `abonamente`, `echipamente`, `rezervari`, `terenuri`, `utilizatori`, `complexe_sportive`, `__EFMigrationsHistory`;
                SET FOREIGN_KEY_CHECKS = 1;
                CREATE TABLE `complexe_sportive` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Nume` varchar(200) NOT NULL,
                    `OraDeschidere` time NOT NULL,
                    PRIMARY KEY (`Id`)
                );
                """);
        }

        // 1. With fallback disabled (Production default), it must fail loudly without wiping the DB.
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
                DatabaseInitializer.ApplyMigrationsAsync(db, recreateOnSchemaMismatch: false));
        }

        // 2. With fallback enabled (Development default), it drops broken tables and recreates the full schema cleanly.
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DatabaseInitializer.ApplyMigrationsAsync(db, recreateOnSchemaMismatch: true);

            var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

            Assert.Contains(DatabaseInitializer.InitialMigrationId, applied);
            Assert.Empty(pending);

            var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
            await seeder.SeedAsync();
            Assert.Equal(1, await db.ComplexeSportive.CountAsync());
            Assert.Equal(4, await db.Terenuri.CountAsync());
        }
    }
}
