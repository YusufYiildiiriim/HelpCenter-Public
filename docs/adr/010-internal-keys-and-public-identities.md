# ADR-010: Move internal keys to Guid

- Date: 2026-10-17
- Status: Accepted (direction); implementation is a separate backlog item

## Context

HelpCenter domain records use `int` for `BaseEntity.Id` and relational foreign keys. `BaseEntity.PublicId` is already a Guid used as the external identity in API/UI contracts. Auxiliary tables such as audit logs and refresh tokens also have independent `long` keys.

Yusuf has decided to move the internal identity model to Guid. This records the direction; it does not mean the schema or application code has been converted.

## Decision

Domain entity primary keys and their corresponding relational foreign keys will use Guid. The conversion must preserve relationships for existing records; where feasible, existing `PublicId` Guid values should be evaluated as migration keys. API/UI identity contracts and token claims must remain compatible during the transition.

Before implementation, inventory all foreign keys, scalar ID consumers, DTOs, routes, JWT claims, audit targets, seeds and client usages. Explicitly decide the scope of audit target identifiers and refresh-token helper keys in the implementation design. Rehearse the migration against an isolated SQL Server with representative data. Do not apply it to a local/shared database until that rehearsal is verified.

## Consequences

- Guid is the target type for internal domain identities in new work.
- This ADR alone makes no code, migration, database or API change.
- Reusing existing `PublicId` values as primary keys is a candidate data-preserving path; validate relationships and uniqueness in migration design.
- The transition requires coordinated API/UI versioning, compatibility for existing JWT claims, seed updates and a database rollback plan.
- Migration and database application are separately reviewable steps; no database was changed in this decision task.
