using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Safely initializes and migrates the CampusBooking MySQL database on startup.
/// Handles both fresh databases and pre-existing databases that already have the
/// valid <c>snake_case</c> initial schema created outside EF Core migrations
/// (by baselining <see cref="InitialMigrationId"/> in <c>__EFMigrationsHistory</c>
/// before running <see cref="DatabaseFacade.MigrateAsync"/>).
/// </summary>
public static class DatabaseInitializer
{
    public const string InitialMigrationId = "20261123092828_InitialCreate";
    private const string LegacyBrokenInitialMigrationId = "20261109083824_InitialCreate";
    private const string EfProductVersion = "9.0.0";

    private static readonly string[] InitialTables =
    [
        "abonamente",
        "complexe_sportive",
        "echipamente",
        "plati",
        "rezervare_echipamente",
        "rezervari",
        "terenuri",
        "utilizatori"
    ];

    private static readonly (string Table, string Column)[] RequiredSnakeCaseColumns =
    [
        ("complexe_sportive", "ora_deschidere"),
        ("terenuri", "tarif_orar_standard"),
        ("utilizatori", "parola_hash"),
        ("rezervari", "cod_rezervare")
    ];

    public static async Task ApplyMigrationsAsync(
        AppDbContext db,
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        await ReconcileBaselineHistoryIfNeededAsync(db, logger, ct);
        await db.Database.MigrateAsync(ct);
    }

    private static async Task ReconcileBaselineHistoryIfNeededAsync(
        AppDbContext db,
        ILogger? logger,
        CancellationToken ct)
    {
        var openedHere = false;
        var connection = db.Database.GetDbConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await db.Database.OpenConnectionAsync(ct);
                openedHere = true;
            }
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.UnknownDatabase)
        {
            // Database does not exist yet — MigrateAsync() will create it and apply all migrations.
            return;
        }

        try
        {
            var existingTables = await GetExistingTablesAsync(connection, ct);
            var existingInitialTables = InitialTables
                .Where(t => existingTables.Contains(t))
                .ToList();

            if (existingInitialTables.Count == 0)
            {
                // Fresh empty database — let MigrateAsync() create the schema from scratch.
                return;
            }

            var hasHistoryTable = existingTables.Contains("__EFMigrationsHistory");
            if (hasHistoryTable && await HasMigrationRecordAsync(connection, InitialMigrationId, ct))
            {
                // Baseline migration is already recorded — MigrateAsync() will only apply newer migrations.
                return;
            }

            var missingTables = InitialTables.Except(existingInitialTables, StringComparer.OrdinalIgnoreCase).ToList();
            if (missingTables.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Database contains a partial initial schema (missing tables: {string.Join(", ", missingTables)}) " +
                    $"without migration '{InitialMigrationId}' recorded in __EFMigrationsHistory. " +
                    "Cannot safely baseline a partial schema; drop and recreate the local database once.");
            }

            var hasValidSnakeCaseSchema = await HasExpectedSnakeCaseColumnsAsync(connection, ct);
            if (!hasValidSnakeCaseSchema)
            {
                throw new InvalidOperationException(
                    $"Database tables already exist, but expected snake_case columns are missing " +
                    $"(likely created by legacy PascalCase migration '{LegacyBrokenInitialMigrationId}'). " +
                    "Reset the local database once so EF Core can recreate it with the snake_case schema.");
            }

            logger?.LogWarning(
                "Detected pre-existing snake_case CampusBooking schema without '{MigrationId}' in __EFMigrationsHistory. " +
                "Baselining __EFMigrationsHistory so existing data is preserved and future migrations run automatically.",
                InitialMigrationId);

            await EnsureHistoryTableAndBaselineAsync(connection, ct);
        }
        finally
        {
            if (openedHere && connection.State == ConnectionState.Open)
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }

    private static async Task<HashSet<string>> GetExistingTablesAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken ct)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT table_name FROM information_schema.tables WHERE table_schema = DATABASE();";

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private static async Task<bool> HasMigrationRecordAsync(
        System.Data.Common.DbConnection connection,
        string migrationId,
        CancellationToken ct)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT COUNT(*) FROM `__EFMigrationsHistory` WHERE `MigrationId` = @id;";

        var param = cmd.CreateParameter();
        param.ParameterName = "@id";
        param.Value = migrationId;
        cmd.Parameters.Add(param);

        return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct)) > 0;
    }

    private static async Task<bool> HasExpectedSnakeCaseColumnsAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken ct)
    {
        var columns = new HashSet<(string Table, string Column)>();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT LOWER(table_name), column_name FROM information_schema.columns WHERE table_schema = DATABASE();";

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            // Keep exact column casing so PascalCase columns (e.g. 'OraDeschidere') do not match 'ora_deschidere'.
            columns.Add((reader.GetString(0), reader.GetString(1)));
        }

        return RequiredSnakeCaseColumns.All(req => columns.Contains(req));
    }

    private static async Task EnsureHistoryTableAndBaselineAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken ct)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"""
            CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
                `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
            ) CHARACTER SET=utf8mb4;
            DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '{LegacyBrokenInitialMigrationId}';
            INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
            VALUES ('{InitialMigrationId}', '{EfProductVersion}');
            """;

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
