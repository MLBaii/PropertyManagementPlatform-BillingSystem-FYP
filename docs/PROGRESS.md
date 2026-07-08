# PROGRESS LOG — PropertyBill

## How to use this file
Update this every working session. This is the **source of truth between sessions**
and the guard against code drifting from the Chapter 4 design spec. Before adding a
new feature, check what's already built vs. planned. If the code must diverge from
the Ch.4 spec, record it here so it can be disclosed in Chapter 5/7 rather than
discovered in the viva.

## Build order (from §3.1.2)
Auth → Profile → Bills → Dashboard → PDF → Payment Proof → Notifications → Disputes + Receipts

## Log

| Date | Module | What changed | Notes |
|------|--------|--------------|-------|
| YYYY-MM-DD | — | Repo scaffolded | Initial structure + docs |
| 2026-07-08 | Backend | Switched database from SQL Server to Supabase (PostgreSQL); kept ASP.NET Core API. | Reason: free online SQL Server hosting unavailable (Azure student region-restricted, Somee impractical). |
| 2026-07-08 | Backend | Scaffolded ASP.NET Core (.NET 9) Web API in `backend/` with EF Core + Npgsql provider. Layered structure: Controllers / Services / Repositories / Models / Data. Created all 15 entities from `docs/SCHEMA.md` (incl. `UnitBillingRate`, which the schema lists as admin-owned but wasn't in the original ask — added for consistency with the doc) with the Account→Unit and AdminUser corrections applied. Added `AppDbContext` with full relationship config. Configured JWT bearer auth issuing `ResidentId`/`UnitId`/`Role` claims, 60-min expiry. Sample endpoints: `POST /api/auth/resident/login` (stub JWT, no DB lookup yet) and `GET /api/residents/bills` (`[Authorize]`, scoped by the JWT's `UnitId` claim). Created `InitialCreate` EF Core migration — **not applied**. | Connection string placeholders only (`[YOUR-PASSWORD]` in `appsettings.json`, a local-only placeholder in the gitignored `appsettings.Development.json`) — fill in the real Supabase connection string locally, then run `dotnet ef database update` from `backend/`. |
| 2026-07-08 | Backend | Applied `InitialCreate` migration to the live Supabase Postgres database (all 15 tables + FKs + indexes created). | Direct DB host (`db.<ref>.supabase.co`) is IPv6-only and this network has no IPv6 route — switched `DefaultConnection` to the Session pooler host (`aws-0-ap-northeast-1.pooler.supabase.com:5432`, username `postgres.<project-ref>`) instead. Worth remembering for any future teammate on a similar network. |
| 2026-07-08 | Backend | Implemented real resident authentication (UC-101), replacing the login stub. Added `BCrypt.Net-Next` for password hashing; `Resident.IsActive` column added via `AddResidentIsActive` migration (applied). `POST /api/auth/resident/login` now looks up the resident by email (`ResidentRepository`), verifies the bcrypt hash, returns 401 on bad credentials, 403 if `IsActive` is false, and otherwise issues a real JWT (`ResidentId`/`UnitId`/`Role`, 60-min expiry) plus basic resident info. `LoginResponse` DTO now carries `residentId`/`fullName`/`email`/`unitId` alongside the token. Added a dev-only `DbSeeder` (idempotent, guarded behind `dotnet run -- --seed`) that inserts Skyview Residence, 2 units, 2 accounts, 2 residents (bcrypt password `Test1234`), and 2 bills + line items for one resident. Seeder run against Supabase; login + the `/api/residents/bills` flow verified end-to-end via curl (valid login → 200 + token, wrong password → 401, unknown email → 401, bills endpoint scoped correctly by the token's `UnitId`, rejects unauthenticated requests with 401). | `Test1234` is a seeded dev-only password (Alice Tan `alice.tan@example.com`, Benjamin Lee `benjamin.lee@example.com`) — never use it outside local/dev testing. |
