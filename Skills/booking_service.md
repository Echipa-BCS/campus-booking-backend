# SKILL: Booking Service & Concurrency Engine (.NET 10)

## Purpose
Defines the implementation details for the court catalog, search filtering, and safe concurrent slot reservation system.

## 1. Catalog & Search Module
- Implement endpoints to fetch sports complexes (`complexe_sportive`) and courts (`terenuri`).
- Support dual filtering (by location/complex and by sport type / `tip_sport`) based on the activity diagram.

## 2. Booking & Concurrency Engine (Critical Section)
- **Step 1 (Availability Check)**: Verify if the requested time interval (`data_ora_inceput` to `data_ora_sfarsit`) for a specific court is free in the database.
- **Step 2 (Temporary Lock)**: If available, create a booking record with status `TEMPORAR_BLOCATA` and set `data_expirare_blocaj` (e.g., current time + 10 minutes).
- **Step 3 (Race Condition Protection)**: Use database transaction isolation levels or optimistic concurrency to prevent double-booking when multiple users try to book the same slot simultaneously.
- **Step 4 (Expiration Handling)**: Unpaid temporary locks must automatically expire or be ignored by the availability checker.
