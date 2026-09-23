# ADR-009 — Operational logging and immutable audit-trail boundary

**Status:** Accepted
**Date:** 2026-09-19
**Deciders:** Yusuf Yıldırım

## Context

Operational diagnostics and business/security evidence answer different questions. Runtime logs
help diagnose a request, dependency failure, or unexpected exception. Audit records establish
that an important business or security event happened, who performed it, and what safely
recordable state changed.

The former AppLog/`ILogService` pipeline serialized MediatR request data into JSON files and
exposed an admin log screen. It duplicated the logging pipeline, mixed diagnostic and audit
concerns, and made it too easy to retain request payloads that may contain secrets or unrelated
personal data. Audit records already belong in the SQL Server database because they must remain
available to authorized database users during an investigation.

## Decision

- Use `ILogger` through `Microsoft.Extensions.Logging` with Serilog as the operational logging
  implementation. Keep the rolling file sink for runtime diagnostics.
- Remove `AppLog`, `ILogService`, custom JSON-file writes, and the admin Logs API/UI. Application
  requests must not be serialized wholesale into operational or audit logs.
- Record audited business and security events in the SQL Server `AuditLogs` table through
  `IAuditLogWriter`. Audit metadata may contain safe, purpose-specific before/after values and
  target/actor context; it must not contain request bodies, credentials, tokens, or secrets.
- Treat `AuditLogs` as append-only. Migration
  `20260919092938_RemoveAppLogsAndHardenAuditTrail` installs
  `dbo.TR_AuditLogs_AppendOnly`, which rejects `UPDATE` and `DELETE` while allowing inserts.
- A database trigger is not WORM storage: a sufficiently privileged DBA can alter database
  objects. Restrict privileged access and use external immutable retention if that threat model
  requires it.
- An AuditLog read API and administration UI are deliberately out of scope for this decision;
  they do not exist yet.

## Consequences

**Positive**

- Runtime diagnostics retain structured logging and correlation without duplicating all request
  data in custom JSON files.
- Business/security evidence is durable in SQL Server and protected against ordinary application
  or database-client updates and deletes.
- Audit payloads have an explicit data-minimization boundary.

**Negative**

- Investigators currently need database access to query audit records; a controlled application
  read surface remains future work.
- File logs are operational evidence only and have retention/availability limits distinct from
  the audit trail.
- The append-only trigger cannot protect against privileged database administration.

**Neutral**

- `ILogger` calls remain appropriate alongside audit writes: they report runtime behaviour,
  whereas audit records report selected business/security facts.

## Related

- [ADR-007 — Observability baseline](007-observability-baseline.md). This ADR clarifies and
  supersedes only ADR-007's operational-sink versus durable-audit boundary; ADR-007 remains
  accepted for correlation, health checks, structured runtime logging, and observability.
- [Database schema reference](../database-schema.md#audit-evidence)
- Migration `20260919092938_RemoveAppLogsAndHardenAuditTrail`
