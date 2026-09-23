# Tech Stack

Every runtime dependency in this repository, why it was chosen, and what was rejected.

## Backend (.NET 8)

### Framework

| Package | Purpose | Why this over alternatives |
|---|---|---|
| **.NET 8** | Runtime | LTS release, `IExceptionHandler`, native `RateLimiter`, AOT-ready. Chose over .NET 6 for perf + new abstractions. |
| **ASP.NET Core 8** | Web host, DI, middleware | Same reason. Kestrel + Minimal APIs available if needed, but we use MVC controllers for OpenAPI ergonomics. |

### Application layer

| Package | Purpose | Why |
|---|---|---|
| **MediatR** | In-process CQRS dispatch | Simple, battle-tested, small surface. Chose over `Wolverine` (heavier, sagas we don't need) and hand-rolled dispatcher (loses pipeline behaviors). |
| **AutoMapper** | Entity ↔ DTO mapping | Convention-based, saves ~30% of DTO glue code. v16 explicit `CreateMap` avoids surprise mappings. Alternative `Mapster` was considered — faster but weaker LINQ projection support. |
| **FluentValidation** | Command / query validation | Composable, testable, clean separation from DTO. `DataAnnotations` was rejected — validators can't easily depend on services. |

### Persistence

| Package | Purpose | Why |
|---|---|---|
| **Microsoft.EntityFrameworkCore 9** | ORM | Native to .NET stack, LINQ, migrations, global query filters. Runs on a `net8.0`-targeted app (EF Core 9 supports .NET 8); chose over Dapper because we need change tracking + relational graphs; would revisit for read-heavy hot paths. |
| **Microsoft.EntityFrameworkCore.SqlServer** | SQL Server provider | See ADR-004 for DB choice. Repository stays DB-agnostic — swapping providers is a connection-string + migration regen job. |
| `BaseEntity.PublicId` + EF Core value generator | Public entity identifiers | Each eligible entity receives a database-generated random `Guid` through `PublicIdValueGenerator`; external routes use the public identifier rather than masking integer primary keys. This replaced the removed reversible `HashService`. |

### Security

| Package | Purpose | Why |
|---|---|---|
| **Microsoft.AspNetCore.Authentication.JwtBearer** | JWT validation | Standard, in-box. `Duende IdentityServer` was overkill for a single audience. |
| **Konscious.Security.Cryptography.Argon2** | Password hashing (current default) | `PasswordService` hashes all new/changed passwords with Argon2id (OWASP 2024 params: 64 MiB memory, 3 iterations, parallelism 4) — see ADR-005. |
| **BCrypt.Net-Next** | Password hashing (legacy verify-only) | Kept solely to `Verify()` passwords still stored in the old BCrypt format. `IPasswordService.NeedsRehash()` exists to flag those for opportunistic upgrade, but nothing currently calls it from a login handler — old BCrypt hashes are **not actually being rehashed to Argon2id yet**, despite the plumbing being in place. |
| **FluentValidation** | Input validation surface (see above) | Doubles as the injection-prevention perimeter. |

### Observability

| Package | Purpose | Why |
|---|---|---|
| **Serilog.AspNetCore** | Structured logging | Structured-by-default JSON, richer than `Microsoft.Extensions.Logging` alone. |
| **Serilog.Enrichers.Environment** + **.Thread** | `MachineName`, `ThreadId` enrichment | One line per log = one row in Seq / Loki with full context. |
| **Serilog.Sinks.File** | Rolling file sink | Fallback when the observability backend is unreachable. |
| **AspNetCore.HealthChecks.SqlServer** | DB health probe | Powers `/health/ready`. |

### API contract

| Package | Purpose | Why |
|---|---|---|
| **Swashbuckle.AspNetCore** | OpenAPI 3 + Swagger UI | Feeds the frontend's `openapi-typescript` generator — types stay in sync with the API without hand-writing DTOs on the client. |
| **XML doc comments** (`GenerateDocumentationFile=true`) | Doc surface for Swagger | Same doc string ends up in Swagger UI and IntelliSense. |

### Testing

| Package | Purpose | Why |
|---|---|---|
| **xUnit** | Test runner | De-facto standard for .NET, better parallelization semantics than NUnit. |
| **FluentAssertions** | Assertion syntax | Assertions read like sentences, error messages are helpful. |
| **NSubstitute** | Mocking | Cleaner API than Moq; no lambda hell. |
| **Testcontainers.MsSql** | Real SQL Server in integration tests | See ADR-006 — real DB > in-memory to catch query filters, migrations, transactions. |
| **Microsoft.EntityFrameworkCore.InMemory** | Fast unit tests | Used only where behavior is provider-independent. |
| **NetArchTest.Rules** | Layer boundary enforcement | Turns "please respect Clean Architecture" from a wiki page into a CI check. |
| **Microsoft.AspNetCore.Mvc.Testing** | `WebApplicationFactory<T>` | Boots the real pipeline in-process for integration tests. |

### Static analysis / build

| Package | Purpose | Why |
|---|---|---|
| **SonarAnalyzer.CSharp** | Bug + code-smell rules | Catches OWASP-style issues in-editor. |
| **Meziantou.Analyzer** | Perf + reliability lints | Specifically catches async/`ConfigureAwait`, culture-sensitive string ops, etc. |
| **Roslynator.Analyzers** | Style + refactoring hints | Consistent code without prescribing everything. |
| **Directory.Build.props** | Shared backend build configuration | Enables nullable reference types and warnings as errors in Release; domain-specific and deferred analyzer findings remain non-blocking during incremental cleanup. |

## Frontend (Next.js 16)

### Framework + rendering

| Package | Purpose | Why |
|---|---|---|
| **Next.js 16 (App Router)** | React framework, RSC, routing | Standard for modern React SSR / RSC. `output: standalone` gives us a tiny Docker image. |
| **React 19** | UI runtime | Concurrent features, `useTransition`, stable Suspense, the Actions/`use()` additions Next.js 16 expects. |
| **TypeScript** (strict) | Type safety | Strict mode with bundler module resolution and `@/*` path aliases; no deprecated `baseUrl` setting. |

### UI

| Package | Purpose | Why |
|---|---|---|
| **Tailwind CSS 4** | Utility-first styling | Fast iteration, small runtime, design-token driven. Chose over CSS Modules for developer speed. |
| **Radix UI primitives** (`@radix-ui/*`) | Unstyled a11y-first components | Provides correct semantics (dialog, dropdown, avatar, label, separator, slot) — we style them with Tailwind. |
| **shadcn-style composition** (`class-variance-authority`, `clsx`, `tailwind-merge`) | Variant + className composition | Copy-paste components without a runtime UI library dependency. |
| **Lucide React** | Icon system | Per-icon tree-shakeable imports, consistent stroke. |
| **Framer Motion** | Animation | Sensible defaults, respects `prefers-reduced-motion`. |
| **Sonner** | Toast notifications | Accessible, unopinionated, one prop away from RTL. |

### State / data

| Package | Purpose | Why |
|---|---|---|
| **Axios** | HTTP client | Interceptor-friendly (auth, correlation id, 401 refresh). `fetch` was considered but interceptor ergonomics + response transformation lean toward Axios for a large API surface. |
| **Zod** | Schema validation | Validates browser-build environment variables in `src/lib/env.ts`; types are inferred from the schema. |
| **@tanstack/react-table** | Headless table logic | Pagination, sorting, filtering without a giant UI table library. |
| **date-fns** | Date utilities | Tree-shakeable (~2 KB per function) vs `moment` (~65 KB). |

### Domain-specific

| Package | Purpose | Why |
|---|---|---|
| **@microsoft/signalr** | Real-time notifications | Native to the .NET backend's SignalR hub. |
| **ckeditor5** + **@ckeditor/ckeditor5-react** | Rich text editor for knowledge base | Explicit `ClassicEditor` plugin list; avoids the discontinued predefined build package. |
| **jsPDF + jspdf-autotable** | Client-side PDF export | Avoids a server-side headless-Chrome dependency for simple reports. |
| Browser CSV export | Tabular export | Uses the native Blob download API; no spreadsheet parser is shipped to the client. |

### Testing / tooling

| Package | Purpose | Why |
|---|---|---|
| **Vitest** | Test runner | Vite-native, near-instant HMR, Jest-compatible API. |
| **@testing-library/jest-dom** + **@testing-library/react** (planned) | DOM assertion + component test | Encourages testing behavior over implementation. |
| **happy-dom** | DOM environment for Vitest | Faster than jsdom for our use cases. |
| **ESLint (flat config)** + **@typescript-eslint** | Linting | Flat config aligns with Next 15+; strict rules on `no-explicit-any`, `consistent-type-imports`, `eqeqeq`. |
| **Prettier** (via lint pipeline) | Formatting | No debate; runs on save + pre-commit. |
| **openapi-typescript** | Optional Swagger-to-TypeScript generator | Kept as a manual `npm run api:types` utility. The app currently uses hand-maintained service DTOs, so generated schema output is not part of the checked-in runtime contract. |

## Infrastructure

| Item | Purpose | Why |
|---|---|---|
| **SQL Server 2022** | Primary datastore | See [ADR-004](adr/004-sql-server-and-repository-abstraction.md). |
| **Docker** (multi-stage) | Reproducible builds + prod runtime | Backend uses the Debian-based .NET 8 runtime image as non-root; frontend uses `node:22-alpine`; both exclude local secrets and build output from image contexts. |
| **docker-compose** (dev + prod) | Local orchestration | Fastest onramp for new contributors. |
| **GitHub Actions** | CI pipeline | Free for public repos, tight integration, matrix jobs. |
| **Dependabot** | Automated dependency PRs | Weekly, grouped by ecosystem. |
| **Serilog → File / MSSQL sinks** (default) | Log delivery | Zero external dependency for dev; swappable for Seq/Loki in prod. |

## What we deliberately did **not** add

| Rejected | Reason |
|---|---|
| **AutoMapper alternatives (Mapster/manual)** | AutoMapper's explicit-map policy (v16) already prevents surprise mappings; migration cost > benefit. |
| **MediatR alternatives (Wolverine, hand-rolled)** | Simple pattern, no distributed messaging needed. |
| **Specification pattern in Application** | Pragmatic Clean Architecture (see [ADR-001](adr/001-pragmatic-clean-architecture.md)). Repository returns `IQueryable<T>`, handlers use LINQ, boundary is documented and enforced by tests. |
| **Dapper** | Change tracking + navigation graphs matter more than raw query speed for our surface area. Would introduce alongside EF for a specific hot read path if profiling ever justified it. |
| **NoSQL / event sourcing** | RBAC + audit trail need strong consistency and referential integrity. |
| **GraphQL** | REST + typed OpenAPI is enough for a single first-party client. |
| **Micro-services** | One deployable unit is faster to ship and easier to reason about; boundaries live inside the codebase (Clean Architecture). |
| **Bicep/Pulumi at repo root** | Deploy target (Azure DevOps vs AWS) is intentionally kept open; IaC lives in the deployment repo. |

## Version pins

Concrete versions live in the manifests — this document tracks *what and why*, not *which patch*.

- Backend: [`help-center-backend/HelpCenter.WebApi/HelpCenter.WebApi.csproj`](../help-center-backend/HelpCenter.WebApi/HelpCenter.WebApi.csproj) and sibling `.csproj` files
- Frontend: [`help-center-ui/package.json`](../help-center-ui/package.json)
