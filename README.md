# PropertyBill — Property Management Platform (Billing System)

A property management billing platform for Malaysian strata residential properties,
built as a Final Year Project at Tunku Abdul Rahman University of Management and Technology.

The system has two front-end modules sharing one backend:

| Module | Description | Stack | Folder |
|--------|-------------|-------|--------|
| **Module A** | Admin Billing Management Portal | Angular | `admin/` |
| **Module B** | Resident Billing & Account Portal | React Native + Expo | `resident/` |
| **Shared** | Billing API + database | ASP.NET Core Web API + SQL Server | `backend/` |

## Repository structure

```
propertybill/
├── backend/     # Shared ASP.NET Core Web API + EF Core + SQL Server migrations
├── resident/    # Module B — React Native (Expo) resident mobile app
├── admin/       # Module A — Angular admin web portal
└── docs/        # Shared API contract, database schema, progress logs
```

## Tech stack

- **Frontend (Resident):** React Native, Expo, TypeScript
- **Frontend (Admin):** Angular, TypeScript
- **Backend:** ASP.NET Core Web API, Entity Framework Core
- **Database:** SQL Server (hosted online)
- **Auth:** JWT (resident tokens stored via Expo SecureStore)

## Getting started

> Each module has its own setup steps in its folder's README.

1. Clone the repo.
2. Configure the backend connection string to the shared online SQL Server (`backend/`).
3. Run the backend API.
4. Run your module's front-end against the API.

## Workflow

- Each member works on their own branch and merges via pull request.
- `main` stays stable; no direct pushes.
- API contract and schema live in `docs/` as the single source of truth.

## Team

- **Module B (Resident app):** Marion Lim Tze Xin
- **Module A (Admin portal):** Ong Zhang Sheng
- **Supervisor:** Mr. Heng Jooi Huang
