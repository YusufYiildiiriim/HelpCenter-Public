# ADR-010: Use Guid for persisted record identities

- Date: 2026-10-17
- Status: Accepted (direction); implementation follows the vault plan

## Context

HelpCenter currently uses integer primary keys for `BaseEntity` records, `bigint` keys for `AuditLog` and `RefreshToken`, and numeric foreign/scalar identity references throughout the database and API. `BaseEntity.PublicId` is already a Guid on most business records. Status rows have stable numeric IDs used in seeds and client contracts.

Yusuf has decided that all persisted record identities will use Guid. The decision includes primary keys, relational foreign keys and scalar references to records, including status IDs, audit actor/target IDs and refresh-token IDs/references. It does not turn every number into an identity: counts, pagination, enum values and the human-facing ticket number remain their own data types.

## Decision

Use Guid as the primary-key type for every persisted entity and Guid for every corresponding relationship or record-identity reference. Preserve existing entity identity and relationship meaning through an explicit old-to-new map. Reuse a row's existing `PublicId` as its new key where available; create and persist a one-time mapping for `AuditLog` and `RefreshToken`, which do not currently have `PublicId`.

Coordinate JWT claims, API/UI DTOs, SignalR payloads, cache keys, seed data, exports and generated OpenAPI types. Existing PublicId-compatible routes must continue to resolve to the same records. The ordered execution, preflight, validation and rollback procedure lives in the shared project vault's “HelpCenter — Guid kimlik dönüşümü ana planı”.

## Consequences

- This ADR records direction only; it does not change code or a database.
- Numeric status IDs become stable Guid constants/seed keys, with coordinated API/UI migration.
- Record identities are generated as Guid in the application; IDs are not authorization secrets.
- Numeric counts, page indexes/sizes, enum ordinals, sort positions and string `TicketId` remain unchanged unless separately required.
- No database cutover occurs until inventory, migration rehearsal, performance checks, rollback rehearsal and the user-visible cutover plan pass their gates.
