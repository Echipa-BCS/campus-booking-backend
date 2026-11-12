using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusBooking.Infrastructure.Services;

/// <inheritdoc cref="IDbSeeder"/>
public class DbSeeder : IDbSeeder
{
    private readonly AppDbContext _db;

    public DbSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Idempotent: do nothing if the complex table already has data.
        if (await _db.ComplexeSportive.AnyAsync(ct))
        {
            return;
        }

        // -----------------------------------------------------------------
        // 1. Complex: "Complexul Sportiv USV Suceava"
        // -----------------------------------------------------------------
        var complex = new ComplexeSportive
        {
            Nume = "Complexul Sportiv USV Suceava",
            Adresa = "Str. Universității 13, Suceava",
            OraDeschidere = TimeSpan.FromHours(8),
            OraInchidere = TimeSpan.FromHours(22),
            Descriere = "University sports complex in Suceava."
        };
        _db.ComplexeSportive.Add(complex);
        await _db.SaveChangesAsync(ct);

        // -----------------------------------------------------------------
        // 2. Courts (terenuri) — different sports, rates, indoor/outdoor
        // -----------------------------------------------------------------
        var courts = new List<Teren>
        {
            new()
            {
                ComplexId = complex.Id,
                Nume = "Teren Fotbal",
                TipSport = "Fotbal",
                EsteAcoperit = true,
                TarifOrarStandard = 60.00m, // RON/hour
                DurataMinimaOre = 1
            },
            new()
            {
                ComplexId = complex.Id,
                Nume = "Teren Tenis",
                TipSport = "Tenis",
                EsteAcoperit = true,
                TarifOrarStandard = 50.00m, // RON/hour
                DurataMinimaOre = 1
            },
            new()
            {
                ComplexId = complex.Id,
                Nume = "Teren Baschet",
                TipSport = "Baschet",
                EsteAcoperit = false,
                TarifOrarStandard = 40.00m, // RON/hour
                DurataMinimaOre = 1
            },
            new()
            {
                ComplexId = complex.Id,
                Nume = "Teren Volei",
                TipSport = "Volei",
                EsteAcoperit = false,
                TarifOrarStandard = 35.00m, // RON/hour
                DurataMinimaOre = 1
            }
        };
        _db.Terenuri.AddRange(courts);

        // -----------------------------------------------------------------
        // 3. Equipment (echipamente) — stock counts and hourly rental rates
        // -----------------------------------------------------------------
        var equipment = new List<Echipament>
        {
            new()
            {
                ComplexId = complex.Id,
                Denumire = "Racheta de teni",
                TipSport = "Tenis",
                StocTotal = 10,
                TarifInchirierePerOra = 5.00m
            },
            new()
            {
                ComplexId = complex.Id,
                Denumire = "Minge de fotbal",
                TipSport = "Fotbal",
                StocTotal = 20,
                TarifInchirierePerOra = 3.00m
            },
            new()
            {
                ComplexId = complex.Id,
                Denumire = "Minge de baschet",
                TipSport = "Baschet",
                StocTotal = 15,
                TarifInchirierePerOra = 3.00m
            },
            new()
            {
                ComplexId = complex.Id,
                Denumire = "Vest de echipa",
                TipSport = "Fotbal",
                StocTotal = 30,
                TarifInchirierePerOra = 2.00m
            }
        };
        _db.Echipamente.AddRange(equipment);

        // -----------------------------------------------------------------
        // 4. Sample users — one per role (discount rules in §3 of backendengine.md)
        // -----------------------------------------------------------------
        var users = new List<Utilizator>
        {
            new()
            {
                Nume = "Popescu",
                Prenume = "Alex",
                Email = "alex.popescu@student.usv.ro",
                ParolaHash = "hash_student",
                Telefon = "0712345678",
                Rol = RolUtilizator.STUDENT,
                DataInregistrare = DateTime.UtcNow
            },
            new()
            {
                Nume = "Ionescu",
                Prenume = "Ion",
                Email = "ion.ionescu@usm.ro",
                ParolaHash = "hash_profesor",
                Telefon = "0723456789",
                Rol = RolUtilizator.PROFESOR,
                DataInregistrare = DateTime.UtcNow
            },
            new()
            {
                Nume = "User",
                Prenume = "Standard",
                Email = "user@gmail.com",
                ParolaHash = "hash_user",
                Telefon = "0734567890",
                Rol = RolUtilizator.USER,
                DataInregistrare = DateTime.UtcNow
            }
        };
        _db.Utilizatori.AddRange(users);
        await _db.SaveChangesAsync(ct);

        // -----------------------------------------------------------------
        // 5. Subscriptions (abonamente) — sample active subscriptions
        // -----------------------------------------------------------------
        var abonamente = new List<Abonament>
        {
            new()
            {
                UtilizatorId = users[0].Id, // student
                Tip = TipAbonament.GENERAL_ACCESS,
                DataStart = DateTime.UtcNow.AddDays(-30),
                DataExpirare = DateTime.UtcNow.AddDays(30),
                SedinteRamase = 5,
                Activ = true,
            },
            new()
            {
                UtilizatorId = users[1].Id, // profesor
                Tip = TipAbonament.INTERVAL_FIX,
                DataStart = DateTime.UtcNow.AddDays(-30),
                DataExpirare = DateTime.UtcNow.AddDays(30),
                SedinteRamase = 3,
                Activ = true,
            },
        };
        _db.Abonamente.AddRange(abonamente);

        await _db.SaveChangesAsync(ct);
    }
}
