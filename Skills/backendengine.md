# SKILL: Backend Engine & Concurrency (.NET 10)

## Purpose
Defines backend architecture, security rules, and concurrency handling for the .NET 10 Web API.

## Core Business Requirements:
1. **Authentication & Role Assignment**:
   - Automatically determine user roles during registration based on email suffix:
     - `@student.usv.ro` $\rightarrow$ `STUDENT`
     - `@usm.ro` $\rightarrow$ `PROFESOR`
     - Other domains $\rightarrow$ `USER`
2. **Concurrency & Booking Engine**:
   - Implement temporary slot locking (`TEMPORAR_BLOCATA`) with a reservation expiration timestamp (`data_expirare_blocaj`, e.g., 10 minutes).
   - Prevent double-booking race conditions during simultaneous requests for the same time slot.
3. **Pricing & Rule Engine**:
   - Dynamic calculation of total price factoring in court hourly rates, equipment sub-totals, and active subscription benefits (`abonamente`).## Pricing & Role Discounts (§3)
- Base pricing is calculated using court hourly rates (`tarif_orar_standard`) and equipment rates (`tarif_inchiriere_per_ora`).
- Automatic role-based discount logic during booking creation:
  - **STUDENT** (`@student.usv.ro`): **40% discount** on total booking price.
  - **PROFESOR** (`@usm.ro`): **20% discount** on total booking price.
  - **USER** (external): **0% discount** (standard price).
- Store the applied discount amount in `rezervari.discount_aplicat` and the final calculated price in `rezervari.pret_total`.
