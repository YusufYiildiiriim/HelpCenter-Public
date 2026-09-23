# ADR-006 — Testing pyramid: architecture + unit + Testcontainers integration

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım

## Context

Test suites can drift toward either extreme:

- **All unit, no integration** — fast, but every refactor of the persistence layer sails through green with no coverage of what actually breaks in prod (query filters, migrations, transactions).
- **All integration** — high fidelity but slow, and failures don't localize the fault.

We need a shape that catches structural rot, business-logic bugs, and integration bugs at the tier where each lives.

## Decision

Three tiers, all in CI on every PR:

### 1. Architecture tests — `HelpCenter.ArchitectureTests`

- **Tool:** `NetArchTest.Rules`.
- **What they enforce:**
  - `Domain` has no external references.
  - `Application` doesn't reference `Persistence` or `WebApi`.
  - `Controllers` don't reference `DbContext` or `Microsoft.EntityFrameworkCore`.
  - MediatR handlers follow naming and modifier conventions.
- **Runtime:** seconds.
- **Value:** kills entire classes of PR review comments. "You imported the wrong thing" becomes a compiler-level answer.

### 2. Application unit tests — `HelpCenter.Application.Tests`

- **Tool:** `xUnit` + `NSubstitute` + `FluentAssertions`.
- **DB:** `Microsoft.EntityFrameworkCore.InMemory` for behavior that is provider-independent.
- **What they cover:** command / query handler business rules, validation failures, edge cases.
- **Rule:** every handler ≥ 3 tests — happy path, validation failure, one edge case.

### 3. WebApi integration tests — `HelpCenter.WebApi.IntegrationTests`

- **Tool:** `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) + `Testcontainers.MsSql`.
- **DB:** real SQL Server 2022 booted in a Docker container per test run.
- **What they cover:** health readiness, refresh rotation/logout, SignalR bearer authentication and public FAQ/guide filtering with real migrations and SQL Server.
- **Runtime:** slower (~30 s of container boot per suite) — tolerable in CI.

### 4. Frontend

- **Vitest** + `happy-dom` for hooks and pure helpers.
- **Playwright** planned for E2E smoke coverage (login → dashboard → create role).

## Consequences

**Positive**

- Structural rules are self-enforcing — new contributors can't silently break the layering.
- Integration tests hit *real* SQL Server, so query filter behavior, unique index violations, and transaction semantics are exercised exactly as prod sees them.
- InMemory unit tests stay fast for pure business-logic assertions.

**Negative**

- Testcontainers requires Docker in CI runners. GitHub Actions Linux runners have it; Windows self-hosted may not.
- Container boot adds ~30 s to the integration suite. Kept isolated in a separate job so it doesn't slow the main pipeline.

## Amendment (2026-09-11) — CI result contract

The integration job writes its TRX file to `TestResults/integration-results.trx` explicitly and
fails if the file is absent or reports zero discovered tests. This makes a skipped/discovery-broken
run visible instead of allowing a green job with no integration coverage.

**Neutral**

- Test count is a lagging indicator of quality — coverage % is monitored but not gated at a specific threshold; PR review looks at *what* is tested, not just how much.

## Related

- Roadmap (archived): [1.2 Test altyapısı](../history/roadmap/1.2-test-altyapisi.md)
- Roadmap (archived): [1.1 Katman disiplini (arch tests)](../history/roadmap/1.1-katman-disiplini-ve-statik-analiz.md)
