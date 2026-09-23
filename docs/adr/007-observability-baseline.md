# ADR-007 — Observability baseline: Serilog + correlation id + health checks

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım

## Context

Production issues have exactly one useful shape: *"a request came in, it did something, it either finished or failed, and someone needs to know what happened."* The minimum to answer that:

- **Structured** logs (JSON-shaped, not plaintext lines) so they're queryable.
- A **correlation id** that ties frontend, backend, and downstream log lines to the same request.
- **Health endpoints** distinguishing "process alive" from "ready to serve traffic".
- A path to **OpenTelemetry** without rewriting the log pipeline later.

## Decision

### Logging

- **Serilog** (`Serilog.AspNetCore`) as the sole logger. Microsoft.Extensions.Logging is the interface; Serilog is the sink.
- Enrichers: `WithMachineName`, `WithThreadId`, `FromLogContext` (correlation id + user id land here).
- Sinks configured in `appsettings.json`:
  - Console (JSON) for containers.
  - File (rolling daily) as fallback.
  - Optional MSSQL sink for durable local trace during dev.

### Correlation id

- `CorrelationIdMiddleware` reads `X-Correlation-Id` from the request or generates one, sets `HttpContext.TraceIdentifier`, and pushes it into `LogContext` for every downstream log line.
- Response echoes the same header, so the frontend can display it in error toasts.
- Frontend Axios interceptor sends a fresh id per request (or reuses the current page's id when set).

### Errors

- `ExceptionMiddleware` catches every unhandled exception, maps by type to an HTTP status + safe message, wraps in `ApiResponse<T>.ErrorResult(...)`.
- Stack traces are logged, not returned.

### Health checks

- `/health/live` — returns 200 if the process is up. Used for container HEALTHCHECK and load balancer liveness.
- `/health/ready` — returns 200 only if SQL Server is reachable (`AspNetCore.HealthChecks.SqlServer`). Used to gate traffic; a rolling deploy waits for `ready` before flipping.

### OpenTelemetry (implemented, not just a future path — see amendment below)

- Log format is already structured; OTLP export can be added in parallel without touching call sites.
- `CorrelationId` maps cleanly to the OTel trace id when we adopt it.

## Consequences

**Positive**

- Every prod log line is queryable by `CorrelationId`, `UserId`, `RequestPath`, `StatusCode`.
- Health probes give the reverse proxy a clean signal for traffic gating.
- No client ever sees an unformatted stack trace.

**Negative**

- MSSQL sink adds write pressure to the primary DB. Optional; disabled in prod deployments that use an external log store.
- Multiple sinks mean careful log-level configuration to avoid duplicate cost.

**Neutral**

- `Sentry`, `Seq`, `Loki`, or an OTel collector are drop-in additions — one sink configuration change.

## Related

- [Architecture — cross-cutting concerns](../architecture.md#5-cross-cutting-concerns)
- Roadmap (archived): [1.4 Loglama & observability](../history/roadmap/1.4-loglama-hata-observability.md)

## Amendment (2026-09-01) — OpenTelemetry has already landed

`OpenTelemetryServiceRegistration.AddOpenTelemetryServices` (`HelpCenter.WebApi/ServiceRegistration/`)
wires the OpenTelemetry SDK in full, not just a "path to" it:

- **Tracing:** ASP.NET Core, `HttpClient`, and EF Core instrumentation are always on;
  `AddOtlpExporter` is added conditionally, only when `Otel:OtlpEndpoint` is configured.
- **Metrics:** ASP.NET Core, `HttpClient`, runtime, and process instrumentation feed a
  **Prometheus exporter**, scraped at `/metrics` (`app.MapPrometheusScrapingEndpoint()` in
  `Program.cs`) — this endpoint is always exposed, independent of the OTLP setting above.

So "OTEL collector optional" in the deployment diagram is only true for the trace-export leg; the
Prometheus metrics endpoint has no on/off switch and is live in every environment.

Also worth correcting here: the error pipeline in this ADR's "Errors" section says
`ExceptionMiddleware` — the actual class is `GlobalExceptionHandler`, a .NET `IExceptionHandler`
implementation (`HelpCenter.WebApi/Errors/GlobalExceptionHandler.cs`), registered via
`AddGlobalExceptionHandling()` / `app.UseExceptionHandler()`. There is no middleware class by that
name in the codebase.
