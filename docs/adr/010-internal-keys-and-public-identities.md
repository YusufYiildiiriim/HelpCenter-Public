# ADR-010: Keep internal numeric keys alongside public UUID identities

- Date: 2026-10-16
- Status: Proposed

## Context

`BaseEntity` already creates a random `PublicId` before persistence. EF and SQL Server preserve that identity and enforce uniqueness. Internal relationships use integer foreign keys; audit records and refresh tokens use long keys. Several external routes use `PublicId`, while some administrative contracts still carry numeric identifiers.

The technical-debt review proposed replacing every internal numeric key with a UUID. No requirement for independently generated relational keys, cross-database merges or disconnected writes has been established. Public UUIDs already provide independently generated external identities. UUIDs do not replace authorization checks.

## Decision

Retain existing numeric primary/foreign keys and UUID `PublicId` identities. Do not perform a global key migration in this change. Keep public identity generation and uniqueness tests, and continue using explicit ownership and permission checks on every protected route.

This proposal does not claim that every route already uses UUIDs. Any remaining public identifier exposure should be reviewed at the API boundary separately.

## Consequences

- No key conversion, data rewrite or rollback operation is required.
- Existing foreign keys, JWT identity claims, audit targets and API consumers retain their current contracts.
- A future UUID-key migration needs a demonstrated requirement, a full FK/consumer inventory, a shadow-key backfill, dual-read/write compatibility, isolated SQL rehearsal and a recoverable cutover. Randomly replacing existing IDs is not an acceptable migration.
- Revisit this proposal if multi-writer ingestion or cross-database merging becomes a concrete requirement.

## Validation

Existing public identity lifecycle tests cover generation, persistence and explicit identity preservation. Repository pagination now supports model-defined primary keys, including long keys. No database schema change is introduced by this ADR.
