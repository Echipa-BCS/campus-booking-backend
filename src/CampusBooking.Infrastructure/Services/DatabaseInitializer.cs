using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Safely initializes and migrates the CampusBooking MySQL database on startup.
/// <list type="bullet">
/// <item><description>Fresh / new database: runs <see cref="DatabaseFacade.MigrateAsync"/> from scratch.</description></item>
/// <item><description>Pre-existing valid <c>snake_case</c> schema without <c>__EFMigrationsHistory</c>:
/// baselines <see cref="InitialMigrationId"/> and preserves all existing data, then runs pending migrations.</description></item>
/// <item><description>Broken / incompatible local schema (e.g. legacy PascalCase tables or DDL conflict):
/// when <c>recreateOnSchemaMismatch</c> is enabled (default in Development), logs a loud ALL-CAPS warning,
/// drops all tables in the target schema, and re-applies all EF Core migrations cleanly.</description></item>
/// </list>
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

    // MySQL DDL / schema conflict error numbers (explicitly excludes auth/network errors).
    private static readonly HashSet<int> SchemaConflictMySqlNumbers =
    [
        1050, // ER_TABLE_EXISTS_ERROR
        1054, // ER_BAD_FIELD_ERROR (unknown column)
        1060, // ER_DUP_FIELDNAME
        1061, // ER_DUP_KEYNAME
        1068, // ER_MULTIPLE_PRI_KEY
        1091, // ER_CANT_DROP_FIELD_OR_KEY
        1146, // ER_NO_SUCH_TABLE
        1176, // ER_KEY_DOES_NOT_EXITS
        1215  // ER_CANNOT_ADD_FOREIGN
    ];

    public static async Task ApplyMigrationsAsync(
        AppDbContext db,
        ILogger? logger = null,
        bool recreateOnSchemaMismatch = false,
        CancellationToken ct = default)
    {
        try
        {
            await ReconcileBaselineHistoryIfNeededAsync(db, logger, ct);
            await db.Database.MigrateAsync(ct);
        }
        catch (Exception ex) when (recreateOnSchemaMismatch && IsSchemaMismatchException(ex))
        {
            logger?.LogWarning(
                ex,
                """

                ================================================================================
                WARNING: DATABASE MIGRATION / SCHEMA MISMATCH DETECTED!
                FALLBACK TRIGGERED (Database:RecreateOnSchemaMismatch = true):
                DROPPING ALL TABLES IN THE CURRENT DATABASE AND RECREATING SCHEMA FROM SCRATCH!
                ALL PREVIOUS LOCAL DATA IN THIS DATABASE WILL BE WIPED AND RE-SEEDED.
                REASON: {Reason}
                ================================================================================
                """,
                ex.Message);

            await DropAllTablesInCurrentDatabaseAsync(db, ct);
            await db.Database.MigrateAsync(ct);

            logger?.LogWarning(
                """

                ================================================================================
                DATABASE FALLBACK COMPLETED: ALL TABLES RECREATED AND EF CORE MIGRATIONS APPLIED!
                ================================================================================
                """);
        }
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
                throw new SchemaMismatchException(
                    $"Database contains a partial initial schema (missing tables: {string.Join(", ", missingTables)}) " +
                    $"without migration '{InitialMigrationId}' recorded in __EFMigrationsHistory.");
            }

            var hasValidSnakeCaseSchema = await HasExpectedSnakeCaseColumnsAsync(connection, ct);
            if (!hasValidSnakeCaseSchema)
            {
                throw new SchemaMismatchException(
                    $"Database tables already exist, but expected snake_case columns are missing " +
                    $"(likely created by legacy PascalCase migration '{LegacyBrokenInitialMigrationId}').");
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

    private static async Task DropAllTablesInCurrentDatabaseAsync(AppDbContext db, CancellationToken ct)
    {
        var openedHere = false;
        var connection = db.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await db.Database.OpenConnectionAsync(ct);
            openedHere = true;
        }

        try
        {
            var tables = await GetExistingTablesAsync(connection, ct);
            if (tables.Count == 0)
            {
                return;
            }

            await using var cmd = connection.CreateCommand();
            var dropStatements = tables.Select(t => $"DROP TABLE IF EXISTS `{t.Replace("`", "``")}`;");
            cmd.CommandText =
                "SET FOREIGN_KEY_CHECKS = 0;\n" +
                string.Join("\n", dropStatements) +
                "\nSET FOREIGN_KEY_CHECKS = 1;";

            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            if (openedHere && connection.State == ConnectionState.Open)
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }

    private static bool IsSchemaMismatchException(Exception ex)
    {
        var current = ex;
        while (current is not null)
        {
            if (current is SchemaMismatchException)
            {
                return true;
            }

            if (current is MySqlException mysqlEx && SchemaConflictMySqlNumbers.Contains(mysqlEx.Number))
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }

    private static async Task<HashSet<string>> GetExistingTablesAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken ct)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT table_name FROM information_schema.tables WHERE table_schema = DATABASE() AND table_type = 'BASE TABLE';";

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

    private sealed class SchemaMismatchException(string message) : InvalidOperationException(message);
}
