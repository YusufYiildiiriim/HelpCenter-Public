# ADR-001 — Pragmatic Clean Architecture

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım

## Context

Clean Architecture insists the Application layer stay framework-agnostic: no ORM imports, no ASP.NET types, no third-party leakage. In practice, two schools coexist:

- **Strict CA** — Application uses `ISpecification<T>` and gets back `Task<Result<T>>`; EF Core lives only in Persistence.
- **Pragmatic CA** — Application may use EF Core LINQ extensions (`Include`, `ToListAsync`, `AsNoTracking`); repositories return `IQueryable<T>` so handlers can compose queries lazily.

Both are defensible. The choice affects handler ergonomics, test setup, and how many patterns a new contributor has to learn to be productive.

## Decision

Adopt **pragmatic Clean Architecture**:

- Application handlers may reference `Microsoft.EntityFrameworkCore` and use its LINQ extensions.
- Repositories expose `IQueryable<T>` via `IGenericRepository<T>.Query()`.
- Domain remains pure (no external references).
- WebApi controllers may only use `IMediator` — never `DbContext` or repositories directly (enforced by `NetArchTest`).

## Consequences

**Positive**

- Handlers stay small; no `SpecificationEvaluator` boilerplate per query.
- Deferred execution + `AsNoTracking` remain trivial one-liners.
- New contributors learn one pattern (LINQ), not two (LINQ + specifications).
- Aggregate-specific repository methods (`GetByIdWithPermissionsAsync`) still encapsulate multi-`Include` graphs.

**Negative**

- Application layer depends on EF Core. Swapping ORMs would require handler edits, not just Persistence changes.
- Application unit tests need either InMemory provider or Testcontainers to exercise LINQ that translates.

**Neutral**

- The boundary between Application and Persistence is defined by *which types return `IQueryable`*, not by *which package is imported*. Enforced structurally by architecture tests.

## When to revisit

- A second data source (document store, search index) enters the picture.
- We ship a pure unit-test suite that must run without a DB.
- The team grows past one primary maintainer.

If any of those happen, migrate handler-by-handler to specification pattern (planned but not scheduled).

## Related

- [Architecture overview](../architecture.md)
- [Tech stack](../tech-stack.md)
