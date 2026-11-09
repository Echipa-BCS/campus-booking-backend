using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ComplexeSportive> ComplexeSportive => Set<ComplexeSportive>();
    public DbSet<Teren> Terenuri => Set<Teren>();
    public DbSet<Utilizator> Utilizatori => Set<Utilizator>();
    public DbSet<Echipament> Echipamente => Set<Echipament>();
    public DbSet<Rezervare> Rezervari => Set<Rezervare>();
    public DbSet<RezervareEchipament> RezervareEchipamente => Set<RezervareEchipament>();
    public DbSet<Abonament> Abonamente => Set<Abonament>();
    public DbSet<Plata> Plati => Set<Plata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
