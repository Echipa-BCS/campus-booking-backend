using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))));

// Catalog & search services
builder.Services.AddScoped<IComplexeSportiveService, ComplexeSportiveService>();
builder.Services.AddScoped<ITerenuriService, TerenuriService>();

// Booking & concurrency service
builder.Services.AddScoped<IRezervareService, RezervareService>();

// Mock payment & subscription service
builder.Services.AddScoped<IPlataService, PlataService>();

// Database seeder
builder.Services.AddScoped<IDbSeeder, DbSeeder>();

builder.Services.AddControllers();

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
});

var app = builder.Build();

// Seed database on startup (idempotent: no-op if data already exists).
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
    await seeder.SeedAsync();
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

app.MapControllers();

app.Run();
