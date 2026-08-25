# Support Ticket Management System

Technical assessment project for ElectroPi. ASP.NET Core backend, Angular frontend.

## Stack

- Backend: ASP.NET Core Web API (.NET 8), EF Core, SQL Server, JWT Auth, Swagger
- Frontend: Angular 22 (standalone components, signals), Angular Material, Chart.js
- Testing: xUnit + Moq (unit tests), WebApplicationFactory + EF InMemory (integration tests)

## Run it

**Backend:**
- `cd SupportTicketSystem.API`
- `dotnet user-secrets set "Jwt:Key" "any-random-string-32-chars-or-more"`
- `dotnet run` (applies migrations + seeds test data automatically)
- API/Swagger: `https://localhost:7166/swagger`

**Frontend:**
- `cd support-ticket-frontend`
- `npm install`
- `ng serve`
- App: `http://localhost:4200` (backend must be running first — CORS is set for this origin)

## Test accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@electropi.test | Admin@123 |
| Support Agent | agent@electropi.test | Agent@123 |
| Customer | customer@electropi.test | Customer@123 |

## Tests

```
cd SupportTicketSystem.Tests
dotnet test
```

- 9 unit tests — `TicketService` isolation rules + status transition validation
- 5 integration tests — real API + in-memory DB, covering auth, isolation, validation

## Architecture

**Backend — Clean Architecture:**
- `Domain` — entities, enums, no dependencies
- `Application` — business logic, interfaces, DTOs
- `Infrastructure` — EF Core, JWT, repository implementations
- `API` — controllers only

- Data isolation is enforced once, in `TicketService` — not scattered across controllers
- Repository interfaces in `Application`, implementations in `Infrastructure` — keeps business logic testable without a real DB

**Frontend — flat structure:**
- `pages/` (one per route), `components/`, `services/`, `guards/`, `interceptors/`, `models/`
- Lazy-loaded routes
- State via Angular signals
- Ticket list, user list switch table → cards below 700px (no data hidden)
- Navbar collapses to a hamburger menu at the same breakpoint

## Notes

- Only one Admin account — seeded, can't be deleted, can't be created/promoted to via the app
- Comments/timeline require a refresh (not real-time)
- Not implemented (bonus items in the spec): refresh token rotation, Docker Compose, CI pipeline, SignalR
- JWT key set via `dotnet user-secrets`, not committed

## Postman / OpenAPI

`openapi.json` is included at the repo root — import it directly into Postman, or generate it live at `/swagger/v1/swagger.json` while the API is running.
