# SKILL: Mock Payment & Subscriptions Integration (.NET 10)

## Purpose
Implements simulated payment processing and subscription management for academic/lab purposes without requiring real external API calls (Stripe webhooks are mocked locally).

## Core Rules:
1. **Mock Payment Branching**:
   - **Single Session**: Endpoint accepts payment confirmation, simulates a successful charge, links the payment record (`plati`) with `rezervare_id`, and updates status.
   - **Subscription**: Verify active subscription (`abonamente` where `activ = 1` and `sedinte_ramase > 0`). Decrement `sedinte_ramase` by 1 and confirm the booking instantly.
2. **Status Transitions**:
   - Automatically transition booking status from `TEMPORAR_BLOCATA` to `CONFIRMATA` upon successful mock payment or subscription deduction.
   - Create a corresponding record in the `plati` table with status `SUCCES`.
