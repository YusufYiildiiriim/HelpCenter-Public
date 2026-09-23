# ADR-004 — SQL Server 2022 as primary datastore, DB-agnostic repository surface

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım

## Context

The database choice affects hosting economics, tooling, and (surprisingly often) how a project reads in a portfolio. Candidates:

- **PostgreSQL** — de-facto default in modern SaaS, cheap managed hosting (Neon, Supabase), rich features (JSONB, arrays).
- **SQL Server 2022** — first-class .NET tooling, Azure SQL integration, dominant in enterprise / Turkish market.
- **SQLite** — great for the tests, not the prod story.
- **NoSQL** (Mongo, Cosmos) — rejected: strong consistency + referential integrity are non-negotiable for RBAC and audit.

## Decision

Use **SQL Server 2022** in production with `Microsoft.EntityFrameworkCore.SqlServer` as the provider. Keep the *application code* provider-agnostic:

- Repositories return `IQueryable<T>` and generic aggregate methods.
- No provider-specific LINQ (`DATEPART`, TSQL raw fragments) leaks into handlers.
- Migrations are the only place where SQL Server dialect is inescapable — those are versioned and reviewed.

Integration tests exercise the real provider via **Testcontainers.MsSql** (see ADR-006), so we catch dialect-sensitive regressions at commit time, not in prod.

## Consequences

**Positive**

- Zero-friction on Azure SQL / Managed Instance for prod hosting.
- Existing tooling (SSMS, Azure Data Studio, `sqlcmd`) works out of the box.
- Enterprise interview conversations don't stall on "why Postgres?".
- Testcontainers proves the code runs against a real MSSQL server, not just in-memory.

**Negative**

- Managed SQL Server hosting is more expensive than managed Postgres. Mitigated for demo by Azure SQL Free tier (100k vCore-seconds/month).
- If we ever want Postgres-only features (JSONB, `NOTIFY/LISTEN`), we'd have to reconsider or dual-support.

**Neutral**

- Because handlers stick to `IQueryable<T>` + standard LINQ, swapping providers is a connection-string + migration regen job, not a rewrite. This is a portfolio talking point in itself.

## Alternatives considered

- **PostgreSQL** — stronger ecosystem fit for greenfield SaaS. Would revisit if the target audience shifts to remote / international SaaS roles. The migration cost is now bounded to Persistence + migrations because of the repository abstraction.
- **Dual support (SQL Server + Postgres)** — rejected: doubles the migration and test surface for no current business value.
