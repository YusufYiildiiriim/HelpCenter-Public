# ADR-002 — Hierarchical action-list RBAC

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım
**Supersedes:** initial boolean-column RBAC model

## Context

The original permission model stored one boolean column per action on the `RolePermission` table: `CanRead`, `CanCreate`, `CanUpdate`, `CanDelete`, `CanExport`, `CanPrint`, ...

Two problems appeared as the surface grew:

1. **Adding a contextual action** — e.g. `ManageMembers` on Projects, `LookupSelect` on any resource — required a schema change plus updates in the entity, DTO, mapper, migration, handler, and frontend. Every new action = five-file touch.
2. **Feature presets** — "give this role the Project Manager package" — became copy-paste code that drifted from one place to another.

## Decision

Replace boolean columns with an **action-list model**:

- `RolePermission (RoleId, ResourceKey)` — one row per (role, resource) pair.
- `RolePermissionAction (RolePermissionId, Action)` — one row per granted action, string-valued.
- Resource metadata (`AppResourceDefinitions.cs`) declares available actions per resource.
- Feature packages (`FeaturePackages.cs`) are named tuples of `(ResourceKey, Action[])` applied atomically.

Actions currently in use: `Read`, `Create`, `Update`, `Delete`, `ManageMembers`, `ManageExperts`, `ManageModules`, `ManageProjects`, `LookupSelect`.

## Consequences

**Positive**

- New action = **one entry** in `AppResourceDefinitions`, zero schema changes.
- Feature packages become a single dictionary applied by one handler; presets stay consistent.
- Frontend gets a normalized `permissions.modules[].actions[]` shape — one `hasPermission()` helper, no per-resource branches.
- Authorization policies resolve dynamically (`Perm:{ResourceKey}:{Action}`) — no policy-registration explosion.

**Negative**

- Query cost slightly higher: joining to `RolePermissionAction` vs reading columns on the base row. Mitigated with a covering index and eager loading on the permission query.
- Migration from the old model needed a two-phase deployment (backfill + drop) with a `Down` script that reversed the shape faithfully.

**Neutral**

- Actions are opaque strings; there is no compile-time check that a controller-side `[HasPermission("X","Y")]` matches a defined action. A validation startup task could close that gap later.

## Related

- [Legacy cleanup notes (archived)](../history/rbac-legacy-cleanup.md)
- [Hierarchical RBAC refactor plan (archived)](../history/hierarchical-rbac-refactor.md)
