using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampusBooking.Infrastructure;
using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;
using CampusBooking.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampusBooking.Tests;

/// <summary>
/// Integration tests verifying <see cref="System.Data.IsolationLevel.Serializable"/>
/// transactions under real MySQL InnoDB concurrency (impossible with EF Core InMemoryDatabase).
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public sealed class SerializableConcurrencyTests
{
    private readonly CampusBookingApiFactory _factory;

    public SerializableConcurrencyTests(CampusBookingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConcurrentTemporaryBookings_SameCourtAndOverlappingSlot_OnlyOneSucceeds()
    {
        await _factory.ResetDatabaseAsync(seed: true);

        int userId;
        int courtId;
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = (await db.Utilizatori.FirstAsync()).Id;
            courtId = (await db.Terenuri.FirstAsync()).Id;
        }

        var slotStart = DateTime.UtcNow.Date.AddDays(2).AddHours(10);
        var slotEnd = slotStart.AddHours(2);

        const int concurrentRequests = 5;
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async _ =>
        {
            using var client = _factory.CreateClient();
            return await client.PostAsJsonAsync("/api/rezervari/temporary", new TemporaryReservationRequest
            {
                UtilizatorId = userId,
                TerenId = courtId,
                DataStart = slotStart,
                DataEnd = slotEnd
            });
        });

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(concurrentRequests - 1, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var count = await db.Rezervari.CountAsync(r => r.TerenId == courtId);
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task ConcurrentSubscriptionPayments_LastRemainingSession_NeverGoesNegative()
    {
        await _factory.ResetDatabaseAsync(seed: true);

        int studentId;
        int court1Id;
        int court2Id;
        int subscriptionId;
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var student = await db.Utilizatori.FirstAsync(u => u.Rol == RolUtilizator.STUDENT);
            studentId = student.Id;

            var courts = await db.Terenuri.OrderBy(t => t.Id).Take(2).ToListAsync();
            court1Id = courts[0].Id;
            court2Id = courts[1].Id;

            var sub = await db.Abonamente.FirstAsync(a => a.UtilizatorId == studentId && a.Activ);
            sub.SedinteRamase = 1;
            subscriptionId = sub.Id;
            await db.SaveChangesAsync();
        }

        var start = DateTime.UtcNow.Date.AddDays(3).AddHours(12);
        var end = start.AddHours(1);

        using var setupClient = _factory.CreateClient();
        var res1Resp = await setupClient.PostAsJsonAsync("/api/rezervari/temporary", new TemporaryReservationRequest
        {
            UtilizatorId = studentId,
            TerenId = court1Id,
            DataStart = start,
            DataEnd = end
        });
        var res2Resp = await setupClient.PostAsJsonAsync("/api/rezervari/temporary", new TemporaryReservationRequest
        {
            UtilizatorId = studentId,
            TerenId = court2Id,
            DataStart = start,
            DataEnd = end
        });

        Assert.Equal(HttpStatusCode.Created, res1Resp.StatusCode);
        Assert.Equal(HttpStatusCode.Created, res2Resp.StatusCode);

        var res1 = (await res1Resp.Content.ReadFromJsonAsync<TemporaryReservationResponse>())!;
        var res2 = (await res2Resp.Content.ReadFromJsonAsync<TemporaryReservationResponse>())!;

        var paymentTasks = new[] { res1.Id, res2.Id }.Select(async rezervareId =>
        {
            using var client = _factory.CreateClient();
            return await client.PostAsJsonAsync("/api/plati/subscription", new SubscriptionPaymentRequest
            {
                UtilizatorId = studentId,
                RezervareId = rezervareId
            });
        });

        var paymentResponses = await Task.WhenAll(paymentTasks);

        Assert.Equal(1, paymentResponses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(1, paymentResponses.Count(r => r.StatusCode == HttpStatusCode.BadRequest));

        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sub = await db.Abonamente.FirstAsync(a => a.Id == subscriptionId);
            Assert.Equal(0, sub.SedinteRamase);

            var confirmedCount = await db.Rezervari.CountAsync(r => r.Stare == StareRezervare.CONFIRMATA);
            Assert.Equal(1, confirmedCount);

            var paymentsCount = await db.Plati.CountAsync();
            Assert.Equal(1, paymentsCount);
        }
    }

    [Fact]
    public async Task ConcurrentSingleSessionPayments_SameReservation_OnlyOneConfirmed()
    {
        await _factory.ResetDatabaseAsync(seed: true);

        int userId;
        int courtId;
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = (await db.Utilizatori.FirstAsync()).Id;
            courtId = (await db.Terenuri.FirstAsync()).Id;
        }

        using var setupClient = _factory.CreateClient();
        var start = DateTime.UtcNow.Date.AddDays(4).AddHours(14);
        var bookingResp = await setupClient.PostAsJsonAsync("/api/rezervari/temporary", new TemporaryReservationRequest
        {
            UtilizatorId = userId,
            TerenId = courtId,
            DataStart = start,
            DataEnd = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, bookingResp.StatusCode);
        var booking = (await bookingResp.Content.ReadFromJsonAsync<TemporaryReservationResponse>())!;

        const int concurrentAttempts = 3;
        var tasks = Enumerable.Range(0, concurrentAttempts).Select(async _ =>
        {
            using var client = _factory.CreateClient();
            return await client.PostAsJsonAsync("/api/plati/single-session", new PaymentRequest
            {
                UtilizatorId = userId,
                RezervareId = booking.Id,
                Suma = booking.PretTotal,
                MetodaPlata = "Card"
            });
        });

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(concurrentAttempts - 1, responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest));

        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(1, await db.Plati.CountAsync(p => p.RezervareId == booking.Id));
            var rez = await db.Rezervari.FirstAsync(r => r.Id == booking.Id);
            Assert.Equal(StareRezervare.CONFIRMATA, rez.Stare);
        }
    }

    [Fact]
    public async Task ConcurrentEquipmentRentals_LimitedStock_NeverOversells()
    {
        await _factory.ResetDatabaseAsync(seed: true);

        int court1Id;
        int court2Id;
        int equipmentId;
        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var courts = await db.Terenuri.OrderBy(t => t.Id).Take(2).ToListAsync();
            court1Id = courts[0].Id;
            court2Id = courts[1].Id;

            var eq = await db.Echipamente.FirstAsync();
            eq.StocTotal = 3;
            equipmentId = eq.Id;
            await db.SaveChangesAsync();
        }

        using var authClient = _factory.CreateClient();
        var user1 = await RegisterUserAsync(authClient, "race1@student.usv.ro");
        var user2 = await RegisterUserAsync(authClient, "race2@student.usv.ro");

        var start = DateTime.UtcNow.Date.AddDays(5).AddHours(15);
        var booking1 = await CreateTempBookingAsync(authClient, user1.User.Id, court1Id, start, start.AddHours(1));
        var booking2 = await CreateTempBookingAsync(authClient, user2.User.Id, court2Id, start, start.AddHours(1));

        var rentalPairs = new[]
        {
            (BookingId: booking1.Id, Token: user1.Token),
            (BookingId: booking2.Id, Token: user2.Token)
        };

        var tasks = rentalPairs.Select(async item =>
        {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", item.Token);
            return await client.PostAsJsonAsync(
                $"/api/rezervari/{item.BookingId}/echipamente",
                new RezervareEchipamentRequest
                {
                    EchipamentId = equipmentId,
                    Cantitate = 2
                });
        });

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest));

        using (var scope = _factory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eq = await db.Echipamente.FirstAsync(e => e.Id == equipmentId);
            Assert.Equal(1, eq.StocTotal);
            Assert.Equal(1, await db.RezervareEchipamente.CountAsync(re => re.EchipamentId == equipmentId));
        }
    }

    private static async Task<AuthResponse> RegisterUserAsync(HttpClient client, string email)
    {
        var resp = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Nume = "Test",
            Prenume = "User",
            Email = email,
            Parola = "Password123!",
            Telefon = "0700000000"
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        return (await resp.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private static async Task<TemporaryReservationResponse> CreateTempBookingAsync(
        HttpClient client,
        int userId,
        int courtId,
        DateTime start,
        DateTime end)
    {
        var resp = await client.PostAsJsonAsync("/api/rezervari/temporary", new TemporaryReservationRequest
        {
            UtilizatorId = userId,
            TerenId = courtId,
            DataStart = start,
            DataEnd = end
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        return (await resp.Content.ReadFromJsonAsync<TemporaryReservationResponse>())!;
    }
}
