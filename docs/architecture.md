# Architecture

**Style:** Clean Architecture (pragmatic interpretation) + CQRS via MediatR
**Runtime:** ASP.NET Core 8 on Kestrel, containerized (Debian-based `aspnet:8.0` runtime image — see §4)
**Data:** SQL Server 2022 via EF Core 9
**Front-end:** Next.js 16 (App Router, React 19, Tailwind 4)

---

## 1. Layer map

```mermaid
graph TB
    subgraph WebApi["HelpCenter.WebApi — ASP.NET Core 8"]
        MW[Middleware pipeline<br/>CorrelationId · Serilog request log · GlobalExceptionHandler · RateLimiter]
        CTRL[Controllers<br/>thin, dispatch to IMediator only]
        SWAG[Swagger + XML docs]
        HC[Health checks<br/>/health/live · /health/ready · /metrics]
    end

    subgraph Application["HelpCenter.Application — CQRS"]
        HND[Command &amp; Query Handlers]
        BEH[Pipeline Behaviors<br/>Validation · Logging]
        DTO[DTOs · ApiResponse&lt;T&gt;]
        IFC[Service interfaces]
    end

    subgraph Domain["HelpCenter.Domain — pure business"]
        ENT[Entities · Aggregates]
        VO[Value Objects<br/>Email · ...]
        EVT[Domain Events<br/>UserPasswordChanged · ...]
        CST[Constants<br/>AppResourceDefinitions · FeaturePackages]
    end

    subgraph Persistence["HelpCenter.Persistence — SQL Server 2022"]
        EF[EfContext · Configurations]
        REPO[IGenericRepository&lt;T&gt; · IUnitOfWork]
        MIG[Migrations]
        QF[Global Query Filters<br/>SoftDelete]
    end

    subgraph Infra["HelpCenter.Infrastructure"]
        JWT[JWT · Hashing · Email]
        FILE[File · SignalR notifications]
    end

    CTRL --> HND
    HND --> BEH
    HND --> REPO
    HND --> IFC
    IFC --> JWT & FILE
    REPO --> EF
    EF --> ENT
    HND --> DTO
    ENT --> VO & EVT
    EF --> QF
```

**Dependency rule (enforced by `NetArchTest` in `HelpCenter.ArchitectureTests`):**

- `Domain` references **nothing** external (no EF, no ASP.NET, no third-party).
- `Application` references `Domain` only. **Never** `Persistence` or `WebApi`.
- `Persistence` references `Domain` + `Application` (implements the abstractions).
- `WebApi` composes everything, but controllers may only use `IMediator` (no `DbContext`, no repositories directly).

Violations break the build.

**Feature slices** (`HelpCenter.Application/Features/*`, one folder per bounded capability,
each with its own `Commands/`, `Queries/`, DTOs, validators and `*Rules`): `Auth`, `Companies`,
`Customers`, `Faqs`, `Guides`, `Menu`, `Modules`, `Organization`,
`Projects`, `Requests`, `RequestSubjects`, `Roles`, `Statistics`, `Status`/`Statuses`, `Users`.
A few of these have no prior coverage in this document:

- **Faqs (SSS)** — knowledge-base entries scoped to a `Project` and/or `Module` (both
  `ProjectId` and `ModuleId`), not a flat/independent list.
- **Guides** — help articles/video guides (optional `YoutubeUrl`), chainable via
  `PreviousGuideId`/`NextGuideId`, scoped to a `Project`.
- **Menu** — the admin sidebar's structure lives in the DB (`MenuItem`: label, icon, route,
  `ResourceKey`, parent/children, order), not hard-coded in the frontend; a menu item is only
  shown to a user who holds the permission for its `ResourceKey`.
- **Organization** — a singleton `OrganizationInfo` record (name, logo, contact info, tax info,
  footer text) used to white-label the portal.
- **Statistics** — admin dashboard reports/aggregate queries (`GetAdminStatistics`,
  `GetAdminReports`).

---

## 2. Request lifecycle (write path)

Traced end-to-end against a real slice — `CreateRoleCommand`
(`HelpCenter.Application/Features/Roles/Commands/CreateRole/`) — rather than an idealized example.
Routes are **not** versioned (no `/api/v1` prefix anywhere in the codebase); this endpoint is
`POST /api/admin/role/create`.

```mermaid
sequenceDiagram
    autonumber
    participant U as Browser (Next.js)
    participant W as WebApi middleware
    participant C as Controller
    participant M as MediatR pipeline
    participant L as LoggingBehavior
    participant V as ValidationBehavior (FluentValidation)
    participant H as Command handler
    participant Ru as *Rules (business rule)
    participant R as IUnitOfWork.Repository(T)
    participant DB as SQL Server 2022

    U->>W: POST /api/admin/role/create (JWT bearer)
    W->>W: CorrelationIdMiddleware sets/reads X-Correlation-Id
    W->>W: Serilog request logging
    W->>W: JwtBearer auth → ClaimsPrincipal
    W->>W: UseRateLimiter (native ASP.NET Core RateLimiter, "write" policy)
    W->>W: Authorization: [HasPermission] policy check
    W->>C: routing
    C->>M: mediator.Send(CreateRoleCommand)
    Note over M,L: Behaviors execute in registration order:<br/>LoggingBehavior is registered first, so it wraps everything below it,<br/>including ValidationBehavior and the handler.
    M->>L: LoggingBehavior starts timer
    L->>V: next() → ValidationBehavior
    V-->>L: ok, or throws ValidationException (400, mapped by GlobalExceptionHandler)
    V->>H: next() → CreateRoleCommandHandler.Handle
    H->>Ru: CreateRoleRules.RoleNameShouldBeUniqueAsync(name)
    Ru->>R: Repository(Role).FindAsync(r => r.Name == name)
    R->>DB: SELECT ... (IsDeleted query filter applied)
    Ru-->>H: ok, or throws RoleNameAlreadyExistsException
    H->>H: Role.Create(name, description) — domain factory,<br/>seeds baseline Dashboard:Read permission
    H->>R: Repository(Role).AddAsync(role)
    H->>R: unitOfWork.SaveAsync()
    R->>DB: INSERT ... (EF Core change tracking / SaveChangesAsync)
    H->>H: IAuditLogWriter.WriteAsync("RoleCreated", ...)
    H-->>V: RoleDto (via AutoMapper)
    V-->>L: propagate result
    L->>L: stop timer, write structured event to Serilog
    L-->>M: RoleDto
    M-->>C: RoleDto
    C-->>U: 200 OK + raw RoleDto body
```

**What the diagram intentionally does *not* claim, because the code doesn't do it:**

- **No `ApiResponse<T>` envelope on success.** Controllers return `Ok(dto)` directly — the DTO is
  the response body. `ApiResponse<T>` is only used (a) by `GlobalExceptionHandler` for **every
  error** response, and (b) by the two `AuthTokenController` endpoints. Most success responses
  are unwrapped.
- **No `201 Created`.** Create endpoints return `200 OK`, not `201` — there is no
  `CreatedAtAction` anywhere in the controllers.
- **No typed repository accessors** (`uow.Roles`, `uow.Users`, ...). Everything goes through the
  single generic `IUnitOfWork.Repository<T>()`.
- **No custom `RateLimitingMiddleware`/`ExceptionMiddleware` classes.** Rate limiting is the
  native ASP.NET Core `RateLimiter` (`AddRateLimiter`/`UseRateLimiter`, policies `auth`/`public`/
  `password`/`write`/`upload`/`heavy` + a global fallback); error handling is the native
  `IExceptionHandler` (`GlobalExceptionHandler`), not a hand-rolled middleware.
- **Access token storage is intentionally memory-only.** The API returns a short-lived access token
  in its response body; the frontend keeps it in `authSession`, not browser storage. A rotating
  `refresh_token` is instead an `HttpOnly`, `Secure`, `SameSite=Lax` cookie. The edge proxy can
  only use that cookie as a coarse route gate, while protected layouts verify the refreshed access
  token and permissions through the API.

---

## 3. RBAC model

```mermaid
erDiagram
    User ||--o{ UserRole : has
    Role ||--o{ UserRole : "granted to"
    Role ||--o{ RolePermission : "has"
    RolePermission ||--o{ RolePermissionAction : "grants"
    RolePermission }o--|| AppResource : "targets"

    User {
        int Id PK
        string Email UK
        string PasswordHash
        bool IsPasswordChangeRequired
        DateTime LastLoginAt
    }
    Role {
        int Id PK
        string Name UK
        string Description
        byte[] RowVersion
    }
    RolePermission {
        int Id PK
        int RoleId FK
        string ResourceKey "e.g. Users, Roles, Modules"
        string AllowedFieldsJson "widget-level filter (Dashboard)"
    }
    RolePermissionAction {
        int Id PK
        int RolePermissionId FK
        string Action "Read | Create | Update | Delete | ManageMembers | ManageExperts | ManageModules | ManageProjects | LookupSelect"
    }
```

**Why action-list instead of boolean columns:**

- Boolean columns (`CanRead`, `CanCreate`, ...) require a schema change every time a new contextual action appears (e.g. `ManageMembers`, `LookupSelect`).
- Action-list makes the permission set **open for extension, closed for modification** — new actions just insert rows.
- Feature packages become trivial: a preset is a set of `(ResourceKey, Action[])` tuples applied atomically.

## 4. Deployment topology

```mermaid
graph LR
    U["Browser<br/>Authorization: Bearer token"] -- HTTPS --> RP[Caddy<br/>TLS termination]
    RP --> UI[Next.js container<br/>standalone build<br/>node:22-alpine]
    RP --> API[WebApi container<br/>mcr.microsoft.com/dotnet/aspnet:8.0<br/>non-root $APP_UID]
    API -- 1433 --> DB[(SQL Server 2022)]
    API -- SMTP --> MAIL[Email provider]
    API -- OTLP (optional) --> OTEL[OpenTelemetry collector]
    API -- "/metrics" --> PROM[Prometheus scrape]
```

`docker-compose.prod.yml` exposes only Caddy on ports 80/443. Caddy obtains TLS certificates for
`APP_DOMAIN`, then routes `/api/*`, `/requestHub*`, and `/health/*` to the API and all other traffic to Next.js.
The API and SQL Server remain private to the Docker network. Production startup reads
`SA_PASSWORD`, `JwtSettings__SecretKey`, the complete `SmtpSettings__*` set,
`NEXT_PUBLIC_API_URL`, `NEXT_PUBLIC_APP_URL`, `AppSettings__FrontendUrl`, `APP_DOMAIN`, and
`ACME_EMAIL` from the ignored root `.env`; connection, JWT-secret, SMTP, and origin settings have
no committed fallback.
Development uses the same file and keeps direct localhost ports for local debugging.

- **Backend image is not Alpine.** The `HelpCenter.WebApi/Dockerfile` builds `FROM
  mcr.microsoft.com/dotnet/aspnet:8.0` (the Debian-based runtime image), not an `-alpine` variant.
  It does run as the non-root `$APP_UID` user. The **frontend** image genuinely is
  `node:22-alpine`.
- **Images are multi-stage** and their build contexts exclude local environment files, build output,
  logs, and uploads through service-specific `.dockerignore` files.
- OpenTelemetry OTLP trace export is optional (enabled only when `Otel:OtlpEndpoint` is
  configured); the Prometheus metrics endpoint (`/metrics`, via `MapPrometheusScrapingEndpoint()`)
  is always exposed regardless — see §5.

## 5. Cross-cutting concerns

| Concern | Where it lives | Notes |
|---|---|---|
| Logging | Serilog + `LoggingBehavior<TReq,TRes>` | Structured request outcome logging to the configured rolling file sink. The behavior is registered *first*, so it wraps `ValidationBehavior` and the handler. It does not serialize request bodies or write operational logs to the database. |
| Validation | FluentValidation + `ValidationBehavior<TReq,TRes>` | Fails fast, mapped to 400 by `GlobalExceptionHandler` |
| Errors | `GlobalExceptionHandler` (`IExceptionHandler`, `.NET 8` native — registered via `AddGlobalExceptionHandling()` / `app.UseExceptionHandler()`) | Sanitizes exceptions → `ApiResponse<T>` envelope, HTTP code by exception type. There is no separate `ExceptionMiddleware` class. |
| Auth | JWT bearer + custom `PermissionHandler`/`PermissionPolicyProvider` | Policy name is `"{Module}.{Action}"` (e.g. `"Roles.Delete"`), built dynamically by `PermissionPolicyProvider.GetPolicyAsync` from any policy string containing a `.` — not the `Perm:{Resource}:{Action}` format previously documented here. |
| Correlation | `CorrelationIdMiddleware` | Reads/generates `X-Correlation-Id`, pushed into Serilog log context |
| Rate limit | ASP.NET Core's native `RateLimiter` (`AddRateLimiter`/`UseRateLimiter`, no custom middleware class) | Partitioned by user id (or client IP as fallback). Named policies: `auth` 5/30s, `password` 3/5min, `public` 60/1min, `write` 30/1min, `upload` 15/1min, `heavy` 30/1min, plus a 200/1min global fallback. |
| Security headers | `SecurityHeadersMiddleware` (`HelpCenter.WebApi/Middleware/SecurityHeadersMiddleware.cs`) | Registered in the HTTP pipeline; supplies CSP/HSTS/X-Frame-Options and related headers. |
| Config | Strongly-typed `*Options` classes | `ValidateDataAnnotations().ValidateOnStart()` — bad config = fail to boot |
| Health / metrics | ASP.NET Core Health Checks + `AspNetCore.HealthChecks.SqlServer`; OpenTelemetry SDK (tracing + metrics) with a Prometheus scrape endpoint | `/health/live` (self), `/health/ready` (DB); `/metrics` (Prometheus, via `MapPrometheusScrapingEndpoint()`) is always on; OTLP trace export is optional, enabled only when `Otel:OtlpEndpoint` is configured. |

## 6. Testing pyramid

```
       ▲  E2E  (Playwright, planned)
       │
     Integration  (Testcontainers.MsSql + WebApplicationFactory)
       │
    Application unit  (xUnit + NSubstitute + EF InMemory)
       │
   Architecture  (NetArchTest — layers, naming)
```

Architecture tests are the base of the pyramid — they run in seconds and enforce structural rules that would otherwise decay over months.

## 7. Related documents

- [Tech stack — package-by-package rationale](tech-stack.md)
- [ADR index](adr/)
