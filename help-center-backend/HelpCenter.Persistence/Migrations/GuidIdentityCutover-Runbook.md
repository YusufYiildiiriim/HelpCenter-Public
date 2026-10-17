# Guid identity cutover notes

## Staged migration status

`20261017090000_PrepareGuidIdentity` is ordered after `20261016194811_AddAuditLogMenu` and before `20261017122147_GuidIdentityCutover`. It writes only migration-owned `GuidIdentityPrepare*` tables: entity Guid mappings, source/prepared row counts, and identity references. It does not update application rows or alter existing keys, columns, indexes, triggers, defaults, or constraints. It captures database FK references and an explicit scalar-reference manifest including `RequestHistories.ActorAccountId` and polymorphic `AuditLogs.TargetId`. Unknown or empty identifiers, unresolved references, and unknown non-null audit target types fail the migration transaction.

The prepared mapping is a point-in-time snapshot. Apply Prepare first, freeze writes before Validate, keep them frozen through Finalize, and recheck/catch up references immediately before key replacement. A future dual-write scheme is acceptable only if Validate proves catch-up complete. Finalize must recheck row counts and reference parity in its cutover transaction. Never finalize from a stale preparation snapshot.

Repository evidence: the source branch contains the cutover locally and the only references found are generic EF migration commands and the integration-test migration runner. The existing runbook says no SQL Server was migrated or rehearsed; source status is local and unpushed. Repository inspection cannot prove external database history. Confirm no external application before changing the retained migration implementation.

The `20261017122147_GuidIdentityCutover` migration uses a SQL Server shadow-column cutover. Review it against a restored production-like database before treating it as deployable.

## Migration behavior

- The manifest explicitly covers all 30 persisted entity tables: 28 `BaseEntity` tables plus `AuditLogs` and `RefreshTokens`.
- `GuidIdentityMap` stores `(EntityName, LegacyId) -> NewId` and each row's prior `PublicId`. `BaseEntity` rows reuse the existing `PublicId`; statuses 1–4 are the canonical-ID exception. A known legacy system-seed key may use its fixed seed Guid only when its legacy `PublicId` is empty. Any other empty `PublicId`, missing table, unsupported index/FK shape, duplicate map value, or unresolved reference stops the migration before key swap.
- FK/scalar columns are shadowed and mapped before legacy identity columns are dropped. Catalog metadata is recorded for primary keys, single-column identity FKs, and rowstore indexes so those definitions can be recreated on Guid columns.
- The append-only audit trigger is disabled only for the migration transaction and re-enabled after the swap. Status-dependent stored procedures are recreated with canonical status Guids; status-change history strings are converted.
- The migration keeps the identity map and captured schema metadata after cutover. Runtime system-default seeding uses the map to find migrated rows and preserve administrator and permission edits.

## Rollback

`Down` fails immediately. It does not convert rows back to numeric keys. The rollback path is restoring and verifying the pre-cutover backup, then running the old application/schema. After Guid writes begin, backup restore is the only supported rollback in this implementation.

## Verification limits

`dotnet ef migrations script --idempotent` generates deployment SQL, and source-level checks can inspect it. No SQL Server database has been migrated or rehearsed in this worktree. Before cutover, validate the generated script against a restored production-like copy, record row counts and FK/index checks, rehearse backup restore, and review the schema manifest against that database. Script generation alone is not enough to approve applying the migration.

## Seed identity note

Existing SystemDefaults rows keep their migrated `PublicId`/Guid identity and administrator edits. Missing rows on a new database use fixed seed Guids. Existing installations can retain different seed Guids between environments; statuses 1–4 use the same canonical IDs everywhere.
