<div align="center">

# HelpCenter

**Enterprise-grade help desk platform with hierarchical RBAC and feature packages.**

[English](README.md) · [Türkçe](README.tr.md)

![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Next.js](https://img.shields.io/badge/Next.js-16-000?logo=nextdotjs&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)
![Tailwind](https://img.shields.io/badge/Tailwind-4-06B6D4?logo=tailwindcss&logoColor=white)
![License](https://img.shields.io/badge/license-GPL--3.0--only-blue)

[**Architecture**](docs/architecture.md) · [**Database Schema**](docs/database-schema.md) · [**Tech Stack**](docs/tech-stack.md) · [**ADRs**](docs/adr/) · [**Security**](SECURITY.md)

</div>

---

## ✨ Highlights

- **Hierarchical action-list RBAC** — resource × action matrix, no boolean-column bloat. 5 preset **feature packages** for one-click role setup (Project Manager, Module Manager, Support Agent, Content Editor, Read-Only Observer).
- **Clean Architecture + CQRS + MediatR** — Domain / Application / Persistence / Infrastructure / WebApi. Boundaries enforced by **NetArchTest** in CI, not by convention.
- **Security-first** — JWT access tokens are memory-only, refresh tokens are `HttpOnly` rotating cookies, and passwords use Argon2id (BCrypt is verify-only for legacy hashes). FluentValidation, endpoint-scoped native rate limiting, sanitized `IExceptionHandler` responses and `SecurityHeadersMiddleware` complete the baseline. See [SECURITY.md](SECURITY.md) for precise scope and known limitations.
- **Observability from day one** — Serilog structured logging, `X-Correlation-Id` propagation, `/health/live` + `/health/ready` (with PostgreSQL dependency check), OpenTelemetry tracing + an authenticated, rate-limited Prometheus `/metrics` endpoint. `ApiResponse<T>` wraps every **error** response; most success responses return the raw DTO, unwrapped.
- **Production delivery** — Multi-stage Dockerfiles, Caddy-managed TLS, private API/database containers, GitHub Actions CI, dependency/secret scanning and Dependabot.

## 🏗️ Architecture at a glance

```
┌────────────────────────────────────────────────────────────────┐
│  help-center-ui  (Next.js 16 · React 19 · Tailwind 4 · Radix)  │
└──────────────────────────────┬─────────────────────────────────┘
                               │  HTTPS · JWT · X-Correlation-Id
┌──────────────────────────────▼─────────────────────────────────┐
│  HelpCenter.WebApi  (ASP.NET Core 8)                           │
│  ├─ Middleware: CorrelationId · GlobalExceptionHandler · RateLimiter │
│  ├─ Controllers → IMediator (thin, no data access)             │
│  └─ Swagger · Health · Serilog                                 │
└──────────────────────────────┬─────────────────────────────────┘
                               │  MediatR pipeline
                               │  (Validation · Logging)
┌──────────────────────────────▼─────────────────────────────────┐
│  HelpCenter.Application  (CQRS handlers · DTOs · IServices)    │
│  └─ Uses IGenericRepository<T> · IUnitOfWork · IUserContext    │
└─────┬────────────────────────────────────┬────────────────────┘
      │                                    │
      ▼                                    ▼
┌────────────┐                 ┌──────────────────────────────┐
│  Domain    │                 │  Persistence / Infrastructure│
│  Entities  │                 │  EF Core 9 · PostgreSQL 17   │
│  VOs · Events                │  JWT · Hashing · SignalR     │
└────────────┘                 └──────────────────────────────┘
```

Full write-up + diagrams: [`docs/architecture.md`](docs/architecture.md).

## 🚀 Quick start

```bash
git clone https://github.com/<you>/HelpCenter.git
cd HelpCenter

# Docker Compose and wrapped terminal commands use the ignored root .env.
cp .env.example .env

# Dev PostgreSQL only in Docker — backend/frontend run locally below
docker compose --env-file .env -f dockerfiles/docker-compose.dev.yml up -d db

# Backend
./scripts/with-env.sh dotnet ef database update --project help-center-backend/HelpCenter.Persistence --startup-project help-center-backend/HelpCenter.WebApi
./scripts/with-env.sh dotnet run --project help-center-backend/HelpCenter.WebApi
# → http://localhost:5005  ·  Swagger at /swagger (non-Production only)

# Frontend (new terminal)
./scripts/with-env.sh npm --prefix help-center-ui ci
./scripts/with-env.sh npm --prefix help-center-ui run dev
# → http://localhost:3000
```

#### Local configuration and migrations

The root `.env` is read by Docker Compose when passed with `--env-file .env`. Terminal commands
read it only through `scripts/with-env.sh`, which exports its values to that command. Visual
Studio's WebApi debug profile does **not** load the repository-root `.env`; use .NET User Secrets or
explicit process environment variables. For User Secrets, run from the repository root and replace
the placeholder locally (never commit or share the value):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=HelpCenterDb;Username=helpcenter;Password=<your-local-password>" --project .\help-center-backend\HelpCenter.WebApi
# Remove the local secret when no longer needed:
dotnet user-secrets remove "ConnectionStrings:DefaultConnection" --project .\help-center-backend\HelpCenter.WebApi
```

Use this PostgreSQL format locally: `Host=localhost;Port=5432;Database=HelpCenterDb;Username=helpcenter;Password=<local-password>`.
The password must match `POSTGRES_PASSWORD` in `.env` for the Compose database. Inside Compose,
the hostname is `db` instead of `localhost`.

Apply migrations from a terminal at the repository root (the wrapper supplies the connection from
`.env`):

```bash
./scripts/with-env.sh dotnet ef database update --project help-center-backend/HelpCenter.Persistence --startup-project help-center-backend/HelpCenter.WebApi
```

For Visual Studio's Package Manager Console, set the connection in that console process first,
then run the same EF CLI command. User Secrets are loaded by WebApi at runtime, but the EF
design-time factory resolves its connection from process environment or an explicit argument:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=HelpCenterDb;Username=helpcenter;Password=<your-local-password>"
dotnet ef database update --project .\help-center-backend\HelpCenter.Persistence --startup-project .\help-center-backend\HelpCenter.WebApi
Remove-Item Env:ConnectionStrings__DefaultConnection
```

`migrations add` and `migrations script` do not open a database connection. Next.js loads public
`NEXT_PUBLIC_*` values from the root `.env`; it does not configure the backend database connection.

Default seeded admin account: `admin@helpcenter.com` / `123456`. Its password is public; rotate it (or remove the seed) before any real deployment. See [SECURITY.md](SECURITY.md).

Password hashing is configured in the API's `PasswordHashing` section: `MemorySizeKb=65536`,
`Iterations=3`, `Parallelism=4`. Environment overrides use `PasswordHashing__MemorySizeKb`,
`PasswordHashing__Iterations` and `PasswordHashing__Parallelism`. Invalid options fail startup.
Successful staff/customer logins upgrade BCrypt or outdated Argon2id hashes to this policy;
the forced password-change flag is preserved. Existing Argon2id hashes are verified using their encoded parameters.

### Windows (PowerShell)

After filling the root `.env`, run from the repository root:

```powershell
.\scripts\start-dev.ps1
```

This starts PostgreSQL and the API in Docker, applies pending migrations using the .NET SDK
container, installs missing frontend dependencies, and starts local Next.js at localhost:3000.
The API is available at localhost:5005. Docker Desktop must be running. Stop an existing
frontend on port 3000 before starting another instance.

## 🧪 Tests

| Suite | Command | Tech |
|---|---|---|
| Architecture (layer boundaries) | `cd help-center-backend && dotnet test tests/HelpCenter.ArchitectureTests` | NetArchTest, FluentAssertions |
| Application unit (handlers)     | `cd help-center-backend && dotnet test tests/HelpCenter.Application.Tests` | xUnit, NSubstitute, EF InMemory |
| WebApi integration (real DB)    | `cd help-center-backend && dotnet test tests/HelpCenter.WebApi.IntegrationTests -c Release --logger "trx;LogFileName=integration-results.trx" --results-directory TestResults` | Testcontainers.PostgreSql, WebApplicationFactory |
| Frontend unit/build             | `cd help-center-ui && npm run lint && npm run type-check && npm run api:types:check && npm test && npm run build` | ESLint, TypeScript, OpenAPI type drift check, Vitest, Next.js |
| Frontend browser smoke          | `cd help-center-ui && npx playwright install chromium && npm run e2e` | Playwright, axe WCAG A/AA (color contrast excluded) |

## 🚢 Production deployment

The production compose stack serves the UI and API from one HTTPS hostname through Caddy; the API
and PostgreSQL are not published on host ports. Point the domain's A/AAAA record at the server,
open ports 80 and 443, then configure the root `.env` from the template:

```bash
cp .env.example .env
# Set database, JWT, SMTP, public origin, APP_DOMAIN and ACME_EMAIL values.
docker compose --env-file .env -f dockerfiles/docker-compose.prod.yml up -d --build
```

Run this from the repository root. Before deploying a new version, run the same release checks as CI:

```bash
cd help-center-backend && dotnet test HelpCenter.slnx -c Release
cd ../help-center-ui && npm audit --omit=dev --audit-level=high && npm run lint && npm run type-check && npm test && npm run build
```

Database migrations are intentionally not applied on application startup. Apply them as a separate,
auditable release step from a trusted runner that can reach the private database network before
starting a new API version; do not publish the PostgreSQL port merely to run migrations.

## 📚 Documentation

- [Architecture & diagrams](docs/architecture.md)
- [Database schema reference](docs/database-schema.md)
- [Tech stack — every package, why it's here](docs/tech-stack.md)
- [Architecture Decision Records](docs/adr/)
- [Security policy](SECURITY.md)

## 📊 Project stats

- **Backend:** .NET 8, 5 production projects (Clean Architecture), 90+ endpoints across 24 controllers, MediatR CQRS
- **Frontend:** Next.js 16 App Router, React 19, TypeScript strict, Tailwind 4, Radix UI primitives
- **Data:** PostgreSQL 17, EF Core 9, clean initial migration
- **CI:** GitHub Actions: verified-secret scan, runtime npm/NuGet vulnerability gates, release build, architecture/unit/integration smoke tests, lint and frontend build on every PR
- **Ops:** Multi-stage Docker, Caddy TLS reverse proxy, and private API/database containers.

## 📝 License

GPL-3.0-only — see [LICENSE](LICENSE).
