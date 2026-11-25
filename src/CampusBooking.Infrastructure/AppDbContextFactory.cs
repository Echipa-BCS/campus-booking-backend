using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CampusBooking.Infrastructure;

/// <summary>
/// Design-time factory for <see cref="AppDbContext"/> so EF Core CLI commands
/// (<c>dotnet ef migrations add/list/script/has-pending-model-changes</c>) work offline
/// without requiring a live MySQL connection.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public static readonly Version DefaultMySqlVersion = new(8, 0, 36);

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost;Port=3306;Database=campus_db;Uid=root;Pwd=root;";

        var versionRaw = Environment.GetEnvironmentVariable("Database__MySqlVersion");
        var serverVersion = Version.TryParse(versionRaw, out var parsed)
            ? new MySqlServerVersion(parsed)
            : new MySqlServerVersion(DefaultMySqlVersion);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AppDbContext(optionsBuilder.Options);
    }
}
