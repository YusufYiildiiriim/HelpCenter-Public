# Guid identity cutover notes

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
