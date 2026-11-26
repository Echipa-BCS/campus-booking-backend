using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Allow optional git-ignored local overrides (e.g. developer-specific MySQL port/password).
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
var mysqlVersionRaw = builder.Configuration["Database:MySqlVersion"];
var serverVersion = Version.TryParse(mysqlVersionRaw, out var parsedMySqlVersion)
    ? new MySqlServerVersion(parsedMySqlVersion)
    : new MySqlServerVersion(AppDbContextFactory.DefaultMySqlVersion);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Catalog & search services
builder.Services.AddScoped<IComplexeSportiveService, ComplexeSportiveService>();
builder.Services.AddScoped<ITerenuriService, TerenuriService>();

// Booking & concurrency service
builder.Services.AddScoped<IRezervareService, RezervareService>();

// Mock payment & subscription service
builder.Services.AddScoped<IPlataService, PlataService>();

// Auth service
builder.Services.AddScoped<IAuthService, AuthService>();

// Database seeder
builder.Services.AddScoped<IDbSeeder, DbSeeder>();

builder.Services.AddControllers();

// CORS for frontend dev server (localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CampusSportsBookingSuperSecretKey12345!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CampusBooking",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CampusBookingClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// Swagger / OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Campus Sports Booking API",
        Version = "v1",
        Description = "Academic/lab project: sports complexes catalog, court search, temporary booking, mock payments & subscriptions."
    });

    // Add JWT authorization to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize database on startup when enabled via configuration:
// - Database:AutoMigrate runs EF Core MigrateAsync() so missing databases are created
//   and all pending incremental migrations are applied cleanly.
// - Database:SeedOnStartup runs the transactional, idempotent DbSeeder.
// Both flags can be disabled in Testing / CI environments so test fixtures own DB lifecycle.
var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
var seedOnStartup = app.Configuration.GetValue("Database:SeedOnStartup", true);
var recreateOnSchemaMismatch = app.Configuration.GetValue("Database:RecreateOnSchemaMismatch", false);

if (autoMigrate || seedOnStartup)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (autoMigrate)
    {
        try
        {
            await DatabaseInitializer.ApplyMigrationsAsync(db, logger, recreateOnSchemaMismatch);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize/migrate the CampusBooking database.");
            throw;
        }
    }

    if (seedOnStartup)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
        await seeder.SeedAsync();
    }
}

// Swagger middleware (enabled in all environments for easy local testing)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Campus Sports Booking API v1");
    c.RoutePrefix = string.Empty; // serve Swagger UI at the app root (/)
});

app.MapGet("/", () => "Campus Sports Booking API");
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}", statusCode: 500);
    }
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose entry point for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
