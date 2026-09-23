# ADR-003 — Row-level restrictions via EF Core global filters + AsyncLocal ambient

> **Status: Superseded/Removed (2026-09-06).** The row-level data restriction system described
> below — the `[DataRestriction]` attribute registry, `DataRestrictionQueryFilterBuilder`, the
> `RestrictionAmbient` `AsyncLocal` fallback, and the `DataRestrictions` feature slice — has been
> **removed from the codebase in its entirety** (Domain, Application, Persistence, and WebApi).
> It is kept here only as the historical record of the design and the bugs it worked around. A
> replacement is planned as part of the project's known roadmap: after the backend moves from
> .NET 8 to .NET 10 and the database moves from SQL Server to PostgreSQL, row-level access
> restriction will be redesigned directly on top of EF Core's native "multiple query filter"
> support (`OnModelCreating`) rather than the bespoke builder/registry/ambient mechanism this ADR
> describes. Until that redesign lands, no row-level restriction is enforced anywhere in the app —
> an accepted, deliberate gap, since there is no production traffic yet.

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım

## Context

Some users are restricted to a subset of rows (e.g. "only projects A and B"). We wanted the restriction to be:

- **Enforced at the data layer** — not in each handler's `Where(...)`.
- **Declarative** — a table registers the restriction; entities opt in.
- **Impossible to forget** — a new endpoint automatically inherits the filter.

The natural fit is EF Core's `HasQueryFilter`. But query filters compile into the model cache; anything captured in the filter's expression is fixed at model-build time. Our first implementation captured the DbContext instance:

```csharp
var dbContextConst = Expression.Constant(this);
var check = Expression.Call(dbContextConst, isRestrictedMethod, keyConst);
```

This meant the *first* DbContext created (during startup migration, with a null restriction context) was called forever — a silent bypass.

## Decision

Move restriction state to an ambient owned by `AsyncLocal<T>`, and reference it from the query filter via a **static method call** so the filter re-evaluates per query:

```csharp
public static class RestrictionAmbient
{
    private static readonly AsyncLocal<IUserRestrictionContext?> _current = new();
    public static IUserRestrictionContext? Current { get => _current.Value; internal set => _current.Value = value; }
    public static bool IsKeyRestricted(string key) => Current?.GetAllowedIds(key) != null;
    public static List<int> GetAllowedIds(string key) => Current?.GetAllowedIds(key) ?? new();
}

// EfContext.OnModelCreating — filter builds a call to the static method
var check = Expression.Call(null, isRestrictedMethod, keyConst);
```

`EfContext`'s constructor sets `RestrictionAmbient.Current = restrictionContext` when a real context is provided (and *only* then, so nested DbContext creations don't clobber the ambient).

## Consequences

**Positive**

- Filter is re-evaluated per query — no stale captured instance.
- No handler ever forgets to scope; new endpoints inherit the behavior.
- `AsyncLocal` is safe across `await` boundaries, so the ambient survives async pipelines.

**Negative**

- Ambient state is a mild "spooky action at a distance." Testing scenarios that mutate the ambient need explicit reset.
- Not thread-safe if a single logical request forks worker threads that mutate the ambient — mitigated by treating the ambient as read-only after `EfContext` construction.

**Neutral**

- Tests can bypass with `IgnoreQueryFilters()` when needed. Every such usage is required to be documented on the call site.

## Related

- [Architecture — Row-level restrictions](../architecture.md#4-data-restrictions-row-level-security)
- Archived analysis: [Architecture review §A1](../history/architecture-review-and-hardening.md)

## Amendment (2026-09-01) — the pure-static-call design above was refined again

This ADR is kept as the historical record of *why* the AsyncLocal ambient exists, but the
implementation it describes in the Decision section — a pure static call with **no** DbContext
reference in the filter expression at all — is no longer what ships. It turned out to have its
own bug: EF Core treats a call with no per-row, non-constant argument as a row-independent
constant and evaluates it **once**, caching the result — restrictions went stale across queries
the same way the original captured-instance bug did, just via a different mechanism.

The current implementation, extracted into
`DataRestrictionQueryFilterBuilder.BuildFilter(entityType, context)`
(`HelpCenter.Persistence/Services/DataRestrictionQueryFilterBuilder.cs`), goes back to embedding
the `EfContext` instance in the filter as an `Expression.Constant(context)` and calling
**instance** methods on it (`context.IsKeyRestricted(key)` / `context.GetAllowedIds(key)`). This
does not reintroduce the original bug because EF Core has built-in "current DbContext"
substitution for query filters: a constant of the context's own type inside a filter expression is
swapped for whichever context instance is actually executing the query, every time — it is not
resolved once at model-build time. `RestrictionAmbient` (the `AsyncLocal<IUserRestrictionContext?>`
described above) is still present and still real, but now serves as a **fallback** inside
`EfContext.IsKeyRestricted`/`GetAllowedIds` (`_restrictionContext ?? RestrictionAmbient.Current`)
for `EfContext` instances created without a DI-scoped `IUserRestrictionContext` (e.g. the nested
context `UserRestrictionContext` itself creates to load a user's restrictions). The DI-scoped
`IUserRestrictionContext`, not the ambient, is the primary path for a normal request.

This is documented in depth, with the empirical tests that motivated it, directly in
`DataRestrictionQueryFilterBuilder.cs` and `DataRestrictionQueryFilterBuilderTests.cs`. Per the
"never edited after acceptance" rule in [docs/adr/README.md](README.md) this amendment is added as
a dated addendum rather than rewriting the Decision above — a formal superseding ADR can be filed
later if this area changes again.
