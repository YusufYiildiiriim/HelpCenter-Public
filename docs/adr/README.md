# Architecture Decision Records (ADR)

[English](README.md) · [Türkçe](README.tr.md)

Short, dated documents capturing the *why* behind architectural choices — so a future maintainer (including future-me) can understand the tradeoffs without archaeology through commit history.

**Format:** [MADR](https://adr.github.io/madr/) (lightweight). Each ADR has: context, decision, consequences.

| # | Title | Status |
|---|---|---|
| [001](001-pragmatic-clean-architecture.md) | Pragmatic Clean Architecture — Application may reference EF Core LINQ extensions | Accepted |
| [002](002-hierarchical-rbac.md) | Hierarchical action-list RBAC over boolean permission columns | Accepted |
| [003](003-row-level-restrictions-asynclocal.md) | Row-level restrictions via EF Core global filters + AsyncLocal ambient | Superseded/Removed (2026-09-06) |
| [004](004-sql-server-and-repository-abstraction.md) | SQL Server 2022 as primary datastore, DB-agnostic repository surface | Accepted |
| [005](005-jwt-auth-and-password-hashing.md) | JWT bearer auth, BCrypt today with Argon2id migration path | Accepted |
| [006](006-testing-strategy.md) | Testing pyramid: architecture + unit + Testcontainers integration | Accepted |
| [007](007-observability-baseline.md) | Observability baseline: Serilog + correlation id + health checks | Accepted |
| [008](008-frontend-foundation.md) | Frontend foundation: visual surfaces, data and UI boundaries | Accepted |
| [009](009-operational-logging-and-audit-trail.md) | Operational logging and immutable audit-trail boundary | Accepted |

## Writing a new ADR

1. Copy the last file, bump the number.
2. Fill Context / Decision / Consequences.
3. Set status to `Proposed`, open a PR.
4. On merge, mark `Accepted` (or `Superseded by ADR-N` if replacing another).

An ADR is **never edited after acceptance** — supersede it with a new one so the reasoning trail stays intact.
