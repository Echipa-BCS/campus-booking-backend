# SKILL: Database Model & EF Core Setup (.NET 10 / MySQL)

## Purpose
This skill defines the relational database schema, Entity Framework Core requirements, and MySQL migration settings for the Campus Sports Booking system.

## 1. Entities & Relationships (ERD Mapping)
All identifiers (`id`) are auto-incrementing `INT`:
- **`complexe_sportive`**: Sports complexes (`nume`, `adresa`, `ora_deschidere`, `ora_inchidere`, `descriere`).
- **`terenuri`**: Courts/Fields (`complex_id` [FK], `nume`, `tip_sport`, `este_acoperit`, `tarif_orar_standard`, `durata_minima_ore`).
- **`utilizatori`**: System users (`nume`, `prenume`, `email` [UNIQUE], `parola_hash`, `telefon`, `rol` [ENUM: USER, STUDENT, PROFESOR, ADMIN], `data_inregistrare`).
- **`echipamente`**: Sports equipment/gear (`complex_id` [FK], `denumire`, `tip_sport`, `stoc_total`, `tarif_inchiriere_per_ora`).
- **`rezervari`**: Facility bookings (`utilizator_id` [FK], `teren_id` [FK], `cod_rezervare` [UNIQUE], timestamps, `pret_total`, `discount_aplicat`, `stare` [ENUM: TEMPORAR_BLOCATA, CONFIRMATA, EXPIRATA, ANULATA, FINALIZATA]).
- **`rezervare_echipamente`**: Booking-to-equipment mapping (`rezervare_id` [FK], `echipament_id` [FK], `cantitate`, `subtotal_pret`).
- **`abonamente`**: User subscriptions (`utilizator_id` [FK], `tip` [ENUM: GENERAL_ACCESS, INTERVAL_FIX], dates, `sedinte_ramase`, `activ`).
- **`plati`**: Payment transactions (`utilizator_id` [FK], `rezervare_id` [FK, Nullable], `abonament_id` [FK, Nullable], `stripe_session_id` [UNIQUE], `suma`, `data_plata`, `stare`, `metoda_plata`).

## 2. Technical EF Core Requirements
- Use `Pomelo.EntityFrameworkCore.MySql` provider.
- Configure entity relationships and constraints via Fluent API (`IEntityTypeConfiguration<T>`).
- Map MySQL `DATETIME` and `TIME` fields correctly to C# `DateTime` and `TimeSpan
