# Shared Backend — ASP.NET Core Web API

The shared business-logic and data-access layer for both modules. Exposes RESTful
endpoints consumed by the resident app (Module B) and admin portal (Module A),
backed by a single SQL Server database.

**Stack:** ASP.NET Core Web API, Entity Framework Core, SQL Server (hosted online)
**Auth:** JWT issuance; role-based access control; bcrypt password hashing

## Layers
Controllers → Services → Repositories → Domain Models (EF Core)

## Setup
_Setup steps TBD. Connection string points to the shared online SQL Server._
See `../docs/API-CONTRACT.md` and `../docs/SCHEMA.md` for the agreed contract.
