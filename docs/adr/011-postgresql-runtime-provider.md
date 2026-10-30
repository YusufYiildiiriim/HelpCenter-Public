# ADR-011 — PostgreSQL as the sole runtime provider

**Status:** Accepted
**Date:** 2026-10-30
**Deciders:** Yusuf Yıldırım

## Context

HelpCenter began with SQL Server, then moved to PostgreSQL using a clean database history. The
application now has a PostgreSQL initial migration, PostgreSQL-specific concurrency and timestamp
handling, LINQ dashboard read models, PostgreSQL health checks, Docker Compose, and real
PostgreSQL integration tests. Keeping SQL Server as a runtime option would require a second
migration line and provider-specific test matrix without a current product requirement.

## Decision

- PostgreSQL 17 is the only supported runtime database provider. EF Core uses
  `Npgsql.EntityFrameworkCore.PostgreSQL` and `UseNpgsql`.
- The active migration line starts from the current EF model with the PostgreSQL initial
  migration. SQL Server migration files remain under `database/legacy/sqlserver-migrations/` as
  historical reference and must never be applied by the active application.
- Integration tests use real PostgreSQL through `Testcontainers.PostgreSql`; readiness checks
  use the PostgreSQL health-check provider. Local and production Compose definitions use
  PostgreSQL.
- Dashboard/report reads use EF Core/LINQ. PostgreSQL-specific functions or views may be added
  only when production-like profiling demonstrates a need.
- No SQL Server data migration is part of this decision. PostgreSQL starts with a clean database.
- `RowVersion` is an application-generated GUID concurrency token. Persisted `DateTime` values
  are normalized to UTC before PostgreSQL writes.
- ADR-004 is superseded for provider selection. Other SQL Server references in accepted ADRs,
  schema notes, and old implementation records describe their original historical context.

## Consequences

**Positive**

- Development, CI integration tests, containers, migrations, and runtime use one provider.
- A clean database setup is reproducible without applying the SQL Server migration history.
- Read models and concurrency behavior are exercised against the actual runtime provider.

**Negative / accepted risks**

- Existing SQL Server data is not migrated; a future data transfer requires a separately scoped,
  verified migration and rollback plan.
- Production-scale dashboard query latency has not yet been measured. Add provider-specific
  optimization only after profiling representative data.

**Neutral**

- Historical SQL Server migrations and ADR text remain available for archaeology but are not part
  of the runtime or deployment path.

## Verification

The P5 completion record and current test evidence are maintained in the repository's PostgreSQL
transition plan and CI workflow.
