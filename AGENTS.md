# AGENTS.md

## Stack (initialized & verified)

- **.NET 10 SDK** (`dotnet --version` → 10.0.402)
- Solution `CampusBooking.sln` with 2 projects:
  - `src/CampusBooking.API` — Web API (entrypoint: `Program.cs`, runs on `http://localhost:5000`)
  - `src/CampusBooking.Infrastructure` — EF Core `AppDbContext` + entity configs + `Migrations/` + `DbSeeder`
- **ORM**: `Microsoft.EntityFrameworkCore` 9.0.0 + `Pomelo.EntityFrameworkCore.MySql` 9.0.0 (EF Core 9 runs on .NET 10 runtime; Pomelo 10.x not yet released)
- **Database**: MySQL 8 in Docker container `campus-mysql`, `localhost:3306`, db `campus_db`, user `root`/`root`. Schema matches ERD in `Skills/dbmodel.md` (8 tables).
- **Data seeding**: `IDbSeeder`/`DbSeeder` service registered scoped, runs on startup (idempotent — no-op if `complexe_sportive` already has data). Seeds:
  - Complex: "Complexul Sportiv USV Suceava", Str. Universității 13, Suceava
  - Courts: Fotbal (60 RON/hr, indoor), Tenis (50 RON/hr, indoor), Baschet (40 RON/hr, outdoor), Volei (35 RON/hr, outdoor)
  - Equipment: Racheta de teni, Minge de fotbal, Minge de baschet, Vest de echipa (stock + hourly rates)
  - Users: alex.popescu@student.usv.ro (STUDENT), ion.ionescu@usm.ro (PROFESOR), user@gmail.com (USER)

## Exact commands (run from repo root)

```
dotnet build CampusBooking.sln                 # build solution (0 errors)
dotnet run --project src/CampusBooking.API     # start API (http://localhost:5000)
curl http://localhost:5000/health              # → {"status":"healthy","database":"connected"}
dotnet ef migrations add <Name> --project src/CampusBooking.Infrastructure --startup-project src/CampusBooking.API --output-dir Migrations   # new migration
docker exec -i campus-mysql mysql -u root -proot campus_db   # mysql shell
```

## Live endpoints (verified)

- `GET /api/complexe-sportive` — list all sports complexes
- `GET /api/terenuri?location=<addr>&tipSport=<sport>` — courts with dual filter (location matches complex `Adresa`/`Nume`; `tipSport` matches court `tip_sport`); omit params for all courts
- `POST /api/rezervari/temporary` — body `{ utilizatorId, terenId, dataStart, dataEnd }` → `201` with `TEMPORAR_BLOCATA` + `dataExpirareBlocaj` (now + 10 min) + calculated `pretTotal` and `discountAplicat`; overlapping/occupied slot → `409 {"error":"The time slot is no longer available"}`
- Role-based pricing (§3 of `Skills/backendengine.md`, calculated inside `Serializable` transaction):
  - Base price = court `TarifOrarStandard` (RON/hr) × duration (hours)
  - STUDENT (`@student.usv.ro`): 40% discount; PROFESOR (`@usm.ro`): 20% discount; USER: 0% discount
  - Stored in `rezervari.pret_total` and `rezervari.discount_aplicat`
- `POST /api/plati/single-session` — body `{ utilizatorId, rezervareId, suma, metodaPlata }` → `200` mock payment; transitions `TEMPORAR_BLOCATA` → `CONFIRMATA`, logs `plati` with `stare=SUCCES`; errors → `400`
- `POST /api/plati/subscription` — body `{ utilizatorId, rezervareId }` → `200` mock subscription payment; verifies active `abonamente` (`activ=1`, `sedinte_ramase>0`, `data_expirare>now`), decrements `sedinte_ramase`, transitions to `CONFIRMATA`, logs `plati` with `stare=SUCCES`; errors → `400`
- `GET /swagger/v1/swagger.json` — OpenAPI 3.0 spec; Swagger UI served at app root (`/`) via `RoutePrefix = string.Empty`

## Conventions (from Skills, authoritative)

- Entities → `src/CampusBooking.Infrastructure/Entities/`
- DTOs → `src/CampusBooking.Infrastructure/DTOs/` (use auto-properties `{ get; set; }` — System.Text.Json ignores fields)
- Services → `src/CampusBooking.Infrastructure/Services/`, registered scoped in `Program.cs`
- Controllers → `src/CampusBooking.API/Controllers/`, `[Route("api/...")]`, constructor injection
- Fluent API configs → `src/CampusBooking.Infrastructure/Configurations/`, applied via `ApplyConfigurationsFromAssembly` in `AppDbContext`; every C# property mapped with `HasColumnName("<snake_case>")` to match the MySQL schema
- MySQL `DATETIME`/`TIME` mapped to C# `DateTime`/`TimeSpan`; relationships via `IEntityTypeConfiguration<T>`
- Booking concurrency: `IsolationLevel.Serializable` EF Core transaction; overlap = `existing.DataStart < req.DataEnd && existing.DataEnd > req.DataStart`; expired `TEMPORAR_BLOCATA` locks (`DataExpirareBlocaj < now`) treated as free
- Payment concurrency: `IsolationLevel.Serializable` EF Core transaction per payment; only `TEMPORAR_BLOCATA` reservations can be confirmed; expired locks rejected
- `Pomelo.EntityFrameworkCore.MySql` provider only

## Skills folder

Read the relevant skill before implementation; skills are the current source of truth for domain rules and are updated with the project.

- `Skills/dbmodel.md` — ERD, entities/relationships, EF Core + Pomelo setup, Fluent API.
- `Skills/backendengine.md` — roles by email suffix, temporary slot locking, double-booking prevention, dynamic pricing.
- `Skills/booking_service.md` — catalog/search endpoints, dual filtering, temporary lock + `dataExpirareBlocaj`, `Serializable` concurrency protection, expired-lock handling.
- `Skills/payment_stripe.md` — mock payment processing, subscription verification + `sedinte_ramase` decrement, `SUCCES` payment records, `TEMPORAR_BLOCATA` → `CONFIRMATA` transition.

If a skill and existing code disagree, the skill wins until updated. Add new skills to `Skills/` as the project evolves and keep this file pointing at them.
