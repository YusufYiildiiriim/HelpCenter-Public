<!-- Canonical source: 007-observability-baseline.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-007 — Gözlemlenebilirlik temeli: Serilog + correlation ID + health check'ler

[English](007-observability-baseline.md) · [Türkçe](007-observability-baseline.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım

## Bağlam

Production sorununun anlamlı biçimi şudur: “Bir request geldi, bir iş yaptı, tamamlandı veya
başarısız oldu ve birinin ne olduğunu bilmesi gerekiyor.” Bunun için sorgulanabilir structured
loglar, frontend/backend/downstream satırlarını bağlayan correlation ID, “process ayakta” ile
“trafik almaya hazır” durumlarını ayıran health endpoint'leri ve OpenTelemetryye geçiş yolu gerekir.

## Karar

### Loglama

- Tek logger olarak `Serilog` (`Serilog.AspNetCore`) kullanılır. Arayüz
  `Microsoft.Extensions.Logging`, sink ise Serilog'dur.
- Enricher'lar: `WithMachineName`, `WithThreadId`, `FromLogContext`; correlation ID ve user ID
  burada loga iner.
- `appsettings.json` sink'leri: container'lar için Console (JSON), fallback olarak günlük rolling
  file, development'ta dayanıklı yerel trace için isteğe bağlı MSSQL sink.

### Correlation ID

- `CorrelationIdMiddleware`, request'ten `X-Correlation-Id` okur veya üretir; bunu
  `HttpContext.TraceIdentifier`a atar ve her downstream log satırı için `LogContext`e iter.
- Response aynı header'ı echo eder; frontend bunu hata toast'ında gösterebilir.
- Frontend Axios interceptor'ı request başına yeni ID gönderir; sayfada mevcutsa aynı ID'yi kullanır.

### Hatalar

- `ExceptionMiddleware`, yakalanmamış her exception'ı ele alır; türe göre HTTP status ve güvenli
  mesaja dönüştürür, `ApiResponse<T>.ErrorResult(...)` ile sarar.
- Stack trace loglanır, response'a dönmez.

### Health check'ler

- `/health/live`: Process ayaktaysa 200 döner; container `HEALTHCHECK` ve load balancer liveness
  için kullanılır.
- `/health/ready`: Yalnız SQL Server erişilebiliyorsa 200 döner
  (`AspNetCore.HealthChecks.SqlServer`). Rolling deploy, trafiği çevirmeden önce ready olmasını bekler.

### OpenTelemetry (uygulandı; aşağıdaki eke bakın)

- Log biçimi zaten structured olduğundan OTLP export, call-site'lara dokunmadan paralel eklenebilir.
- `CorrelationId`, OTel trace ID ile temiz eşleşir.

## Sonuçlar

**Olumlu:** Production logları `CorrelationId`, `UserId`, `RequestPath`, `StatusCode` ile
sorgulanabilir; health probe'ları reverse proxy'ye açık trafik sinyali verir; istemciye stack trace
dönmez.

**Olumsuz:** MSSQL sink birincil DB'ye yazma yükü ekler; dış log deposu kullanan production'da
opsiyonel ve kapalıdır. Birden çok sink, maliyet tekrarını önlemek için dikkatli log-level ayarı ister.

**Nötr:** Sentry, Seq, Loki veya OTel collector tek sink yapılandırması değişikliğiyle eklenebilir.

## İlgili

- [Mimari — kesitsel konular](../architecture.tr.md#5-kesitsel-konular)

## Ek (2026-09-01) — OpenTelemetry uygulamaya alındı

`OpenTelemetryServiceRegistration.AddOpenTelemetryServices`
(`HelpCenter.WebApi/ServiceRegistration/`) OpenTelemetry SDK'yı tam olarak bağlar:

- **Tracing:** ASP.NET Core, `HttpClient` ve EF Core instrumentation'ı her zaman açıktır;
  `AddOtlpExporter`, yalnız `Otel:OtlpEndpoint` yapılandırılmışsa eklenir.
- **Metrics:** ASP.NET Core, `HttpClient`, runtime ve process instrumentation'ı,
  `Program.cs` içindeki `app.MapPrometheusScrapingEndpoint()` ile `/metrics`ten scrape edilen
  **Prometheus exporter**a besler. Endpoint, OTLP ayarından bağımsız her ortamda açıktır.

Hatalar bölümündeki `ExceptionMiddleware` adı tarihsel olarak yanlıştır. Gerçek sınıf,
`AddGlobalExceptionHandling()` / `app.UseExceptionHandler()` ile kayıtlı .NET `IExceptionHandler`
uygulaması `GlobalExceptionHandler`dır; kod tabanında bu isimde middleware yoktur.
