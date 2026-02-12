# P-Clinic-POS (v1)

Multi-tenant, multi-branch B2B clinic platform. One tenant has many branches; patients belong to one tenant and may visit multiple branches. All data access is tenant-scoped.

## Architecture overview

- **Backend:** ASP.NET Core 10 (.NET 10) REST API, PostgreSQL, Redis, RabbitMQ
- **Frontend:** Next.js 16 (App Router), TypeScript, Tailwind
- **Auth:** JWT; `tenant_id` and `branch_ids` in claims. Server-side policies enforce **Patients:Create** (Admin, User), **Patients:View** (Admin, User, Viewer), **Appointments:Create** (Admin, User). Viewer cannot create patients.
- **Tenant safety:** `TenantId` is taken from the JWT (`tenant_id` claim). All reads/writes filter by this tenant. Services throw if a request tries to access another tenant. DB unique constraints (e.g. `(TenantId, PhoneNumber)` for patients) enforce uniqueness and concurrency safety within a tenant.
- **Patient–branch:** Modeled via `Patient.PrimaryBranchId`. List patients supports optional `BranchId` filter (same tenant). A separate appointment table links patient + branch + time.

## How to run (one command)

From the repo root (per spec: one command starts PostgreSQL, Redis, RabbitMQ, backend, frontend):

```bash
docker compose up --build
```

- **API:** http://localhost:5000 (Swagger: http://localhost:5000/swagger)
- **Frontend:** http://localhost:3000
- **PostgreSQL:** localhost:5432, Redis: 6379, RabbitMQ: 5672 (management: 15672)

**Migrations** apply automatically on backend startup. **Seeder** runs once (1 tenant, 2 branches, 3 users).

## Environment variables

See `.env.example`. For Docker, the compose file sets connection strings; for local runs copy `.env.example` to `.env` and adjust.

## Seeded users (login)

| Email                 | Password   | Role   |
|-----------------------|------------|--------|
| admin@peaaura.local   | Password1! | Admin  |
| user@peaaura.local    | Password1! | User   |
| viewer@peaaura.local  | Password1! | Viewer |

Viewer can only view; Admin/User can create patients and manage users (Admin only for user management).

## API examples (curl)

**Login:**

```bash
curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@peaaura.local","password":"Password1!"}'
```

Use the returned `token` in `Authorization: Bearer <token>`.

**List patients (required: TenantId; optional: branchId):**

```bash
curl -s "http://localhost:5000/api/patients?tenantId=11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Create patient:**

```bash
curl -s -X POST http://localhost:5000/api/patients \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"firstName":"John","lastName":"Doe","phoneNumber":"+66812345678","tenantId":"11111111-1111-1111-1111-111111111111"}'
```

**Create appointment:**

```bash
curl -s -X POST http://localhost:5000/api/appointments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"tenantId":"11111111-1111-1111-1111-111111111111","branchId":"22222222-2222-2222-2222-222222222221","patientId":"<PATIENT_ID>","startAt":"2025-03-01T10:00:00Z"}'
```

## How to run tests

From repo root:

```bash
cd src/backend
dotnet test PClinicPOS.Tests/PClinicPOS.Tests.csproj
```

- **Tenant scoping:** List patients only returns data for the authenticated tenant.
- **Duplicate phone:** Creating a patient with the same phone in the same tenant returns a friendly error (and DB unique constraint backs this under concurrency).
- **Smoke:** Login with seeded user returns a token.

## Assumptions and trade-offs

- Backend targets **.NET 10** per spec.
- **Patient–branch:** `PrimaryBranchId` on Patient plus optional branch filter on list; appointments link patient + branch + time. No separate “visits” table in v1.
- **Cache:** List Patients is cached in Redis with keys `tenant:{tenantId}:patients:list:{branchId|all}`. Cache is invalidated on Create Patient (and Create Appointment). If Redis is unavailable, cache is skipped (no-op).
- **RabbitMQ:** On Create Appointment, event `AppointmentCreated` is published with TenantId (and related ids). Consumer not implemented.
- **Auth:** JWT only; no refresh token. Seeder creates users with BCrypt-hashed password.

## Tenant isolation (E2)

- **Derivation:** `TenantId` comes from the JWT claim `tenant_id` set at login from the user’s `TenantId`. Optional header override is not used so the token is the single source of truth.
- **Enforcement:** `ITenantContext` reads claims; services (e.g. `PatientService`, `AppointmentService`) compare request `TenantId` to `_tenantContext.TenantId` and filter all queries by tenant. Controllers pass tenant from context or body (body must match context when context is set).
- **Preventing missing filters:** All tenant-scoped queries go through services that require `tenantId` and enforce it; no raw “get all” without tenant. DB unique indexes are per-tenant where needed (e.g. `(TenantId, PhoneNumber)`), so cross-tenant duplicates are impossible at the DB level.
