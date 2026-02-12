# AI Prompts Used (Pea Aura Wellness Build)

## Context

This project was built to satisfy the Full-Stack Engineer (.NET 10 / Next.js) test spec: multi-tenant Clinic POS with Create/List Patients, auth (Admin/User/Viewer), appointments, Redis cache, and RabbitMQ events. The following prompts and decisions were used.

---

## 1. Initial prompt (to Cursor)

**Exact prompt:**  
"read Full-stack Engineer.pdf then build project as per spec"

**Outcome:**  
- Spec was read from the PDF in the repo.  
- Implemented: Section A (core slice), Section B (auth, roles, seeder), Section C (appointments + duplicate prevention + RabbitMQ), Section D (Redis cache for list patients, tenant keys, invalidation), E2 (tenant isolation in README).  
- Backend: .NET 10 per spec.  
- Frontend: Next.js with Create/List patients and branch filter.  
- Docker Compose, migrations, 3 tests (tenant scoping, duplicate phone, login smoke).

**Accepted:** Overall structure, tech choices, and scope.  
**Rejected / adjusted:**  
- (Later updated to .NET 10 to match spec.)  
- Manual EF migration (SQL in `Up()`/`Down()`) because `dotnet ef` was not available in the environment initially.

---

## 2. Validation and iterations

- **Build errors:** Fixed missing `using` (e.g. `PClinicPOS.Api.Models` for `ErrorResponse`), `Auth.ITenantContext` → `ITenantContext` with `using Auth`, and `IJwtService` in `UserService`.  
- **Tests:**  
  - `WebApplicationFactory<Program>` failed because top-level `Program` was internal/static. Introduced public `ProgramEntry` and explicit `Program` class with `Main`.  
  - Replaced DbContext with InMemory in tests via `ConfigureTestServices` (from `Microsoft.AspNetCore.TestHost`).  
  - Duplicate-phone test failed on InMemory (no unique constraint). Added an explicit duplicate-phone check in `PatientService` before insert, while keeping the DB unique constraint for concurrency.

**Correctness checks:**  
- Ran `dotnet test` until all 3 tests passed.  
- Confirmed tenant scoping test only returns patients for the request tenant; duplicate-phone test asserts the friendly error; smoke test asserts login returns a token.

---

## 3. Summary

- **Prompts:** One main prompt (read PDF, build per spec); no long prompt history.  
- **Iterations:** Fixing build and test setup (visibility, test host, duplicate-phone behavior).  
- **Validation:** Build + `dotnet test`; manual check of README and run instructions.

No other AI tools or prompts were used for this submission.
