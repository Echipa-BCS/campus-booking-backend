using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;
using Respawn;
using Respawn.Graph;
using Testcontainers.MySql;

namespace CampusBooking.Tests.Infrastructure;

/// <summary>
/// Shared integration test host backed by a real MySQL 8.0 database so EF Core migrations
/// and <see cref="System.Data.IsolationLevel.Serializable"/> transactions behave identically to production.
///
/// By default, starts an ephemeral Testcontainers MySQL 8.0 container on a random port
/// (never touches the developer's local <c>campus_db</c>).
///
/// Optional fallback for environments without Docker: set <c>TEST_DB_CONNECTION_STRING</c>
/// pointing to a dedicated schema whose name ends with <c>_test</c> (e.g. <c>campus_db_test</c>).
/// </summary>
public sealed class CampusBookingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ExternalTestDbEnvVar = "TEST_DB_CONNECTION_STRING";

    private MySqlContainer? _mysqlContainer;
    private Respawner? _respawner;
    private string _databaseName = "campus_db_test";

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var externalCs = Environment.GetEnvironmentVariable(ExternalTestDbEnvVar);
        if (!string.IsNullOrWhiteSpace(externalCs))
        {
            var csBuilder = new MySqlConnectionStringBuilder(externalCs);
            if (string.IsNullOrWhiteSpace(csBuilder.Database)
                || !csBuilder.Database.EndsWith("_test", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Refusing to run integration tests against database '{csBuilder.Database}'. " +
                    $"{ExternalTestDbEnvVar} must target a dedicated test database whose name ends with '_test' (e.g. campus_db_test).");
            }

            _databaseName = csBuilder.Database;
            ConnectionString = csBuilder.ConnectionString;
        }
        else
        {
            _mysqlContainer = new MySqlBuilder("mysql:8.0")
                .WithDatabase(_databaseName)
                .Build();

            await _mysqlContainer.StartAsync();
            ConnectionString = _mysqlContainer.GetConnectionString();
        }

        // Apply real EF Core migrations once against the isolated test MySQL instance.
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await using var conn = new MySqlConnection(ConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.MySql,
            SchemasToInclude = [_databaseName],
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    public async Task ResetDatabaseAsync(bool seed = true)
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("Respawner is not initialized.");
        }

        await using (var conn = new MySqlConnection(ConnectionString))
        {
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }

        if (seed)
        {
            using var scope = Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
            await seeder.SeedAsync();
        }
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.UseSetting("Database:MySqlVersion", AppDbContextFactory.DefaultMySqlVersion.ToString());
        builder.UseSetting("Database:AutoMigrate", "false");
        builder.UseSetting("Database:SeedOnStartup", "false");

        builder.ConfigureTestServices(services =>
        {
            // Explicitly replace DbContext registration so local appsettings.Local.json
            // or shell ConnectionStrings__Default can never leak into tests.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    ConnectionString,
                    new MySqlServerVersion(AppDbContextFactory.DefaultMySqlVersion)));
        });
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        if (_mysqlContainer is not null)
        {
            await _mysqlContainer.DisposeAsync();
        }
    }
}
