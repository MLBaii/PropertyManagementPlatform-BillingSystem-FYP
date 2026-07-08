# Database Schema (Shared)

> Designed jointly. Single PostgreSQL (hosted on Supabase) database.
> Resident-owned entities and admin-owned entities share this schema.
> Backend connects via EF Core using the Npgsql.EntityFrameworkCore.PostgreSQL provider.

## ⚠️ Corrections to apply vs. the original Ch.4 ERD
The documented Table 4.3 has errors to fix when building the real schema:
- **Account** links to **Unit**, not Resident. Its PK is `AccountId`; it should hold `UnitId (FK)`, not an FK to itself.
- Remove the **duplicate Account row** that appears twice in Table 4.3.
- **AdminUser** should NOT have an `AccountId (FK)` — that was a copy-paste artifact.

## Entities (from §4.4)
Resident-owned: Resident, PaymentProof, Dispute, Notification, NotificationToken
Shared with admin: Property, Unit, Account, Bill, BillLineItem, Payment
Admin-owned: BillingItem, UnitBillingRate, AdminUser, AuditLog

## Key relationships
- Property 1—* Unit
- Unit 1—* Resident (co-owners/tenants possible)
- Unit 1—1 Account
- Account 1—* Bill, 1—* Payment
- Bill 1—* BillLineItem
- Bill 1—* PaymentProof
- Bill 1—* Dispute
- Resident 1—* NotificationToken, 1—* Notification

## Table definitions
_To be detailed per entity (columns, types, keys, constraints)._
