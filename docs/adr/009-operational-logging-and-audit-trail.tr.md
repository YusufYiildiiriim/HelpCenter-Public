<!-- Canonical source: 009-operational-logging-and-audit-trail.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-009 — Operasyonel loglama ve değiştirilemez audit trail sınırı

[English](009-operational-logging-and-audit-trail.md) · [Türkçe](009-operational-logging-and-audit-trail.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-09-19
**Karar verici:** Yusuf Yıldırım

## Bağlam

Operasyonel teşhis ile iş/güvenlik kanıtı farklı soruları yanıtlar. Runtime logları bir request'i,
dependency failure'ı veya beklenmeyen exception'ı teşhis eder. Audit kayıtları ise önemli iş veya
güvenlik olayının gerçekleştiğini, bunu kimin yaptığını ve güvenle kaydedilebilecek hangi durumun
değiştiğini kanıtlar.

Eski AppLog/`ILogService` pipeline'ı MediatR request verisini JSON dosyalarına serialize edip admin
log ekranında gösteriyordu. Bu, loglama pipeline'ını kopyalıyor, teşhis ile audit sorumluluklarını
karıştırıyor ve secret veya ilgisiz kişisel veri taşıyabilecek request payload'larının gereğinden
uzun tutulmasını kolaylaştırıyordu. Audit kayıtları SQL Server veritabanında yaşar.

## Karar

- Serilog ile birlikte `Microsoft.Extensions.Logging` üzerinden `ILogger` kullanılır; runtime
  teşhisi için rolling file sink korunur.
- `AppLog`, `ILogService`, özel JSON dosya yazımları ve admin Logs API/UI kaldırılır. Application
  request'leri operasyonel veya audit loglarına topluca serialize edilmez.
- Denetlenen iş ve güvenlik olayları SQL Server `AuditLogs` tablosuna `IAuditLogWriter` ile yazılır.
  Metadata güvenli, amaca özgü before/after değerleri ile target/actor context taşıyabilir; request
  body, credential, token veya secret içeremez.
- `AuditLogs` append-only kabul edilir. `20260919092938_RemoveAppLogsAndHardenAuditTrail` migration'ı,
  insert'e izin verip `UPDATE` ve `DELETE`yi reddeden `dbo.TR_AuditLogs_AppendOnly` trigger'ını kurar.
- Trigger WORM storage değildir; yeterince yetkili DBA database object'lerini değiştirebilir. Threat
  model gerekirse privileged access sınırlandırılmalı ve harici immutable retention kullanılmalıdır.
- AuditLog read API'si ve yönetim UI'ı bu kararın kapsamı dışındadır; henüz yoktur.

## Sonuçlar

**Olumlu:** Runtime teşhisi structured logging ve correlation bilgisini korur; iş/güvenlik kanıtı
SQL Server'da kalıcıdır ve sıradan update/delete işlemlerine karşı korunur; audit payload'larının
açık data-minimization sınırı vardır.

**Olumsuz:** İnceleme yapanların audit kayıtlarını sorgulamak için şimdilik database erişimi gerekir.
File logları yalnız operasyonel kanıttır; retention/availability sınırları audit trail'den farklıdır.
Append-only trigger, yetkili database yönetimine karşı koruma sağlayamaz.

**Nötr:** `ILogger` çağrıları audit write'ların yanında doğru kalır: ilki runtime davranışını,
ikincisi seçilmiş iş/güvenlik olgularını raporlar.

## İlgili

- [ADR-007 — Gözlemlenebilirlik temeli](007-observability-baseline.tr.md). Bu ADR, ADR-007'nin
  yalnız operasyonel sink ile dayanıklı audit sınırını açıklar ve o kısmını geçersiz kılar;
  ADR-007 correlation, health check, structured runtime logging ve gözlemlenebilirlik için kabul
  edilmiş olarak kalır.
- [Veritabanı şeması referansı](../database-schema.tr.md#denetim-kanıtı)
- Migration `20260919092938_RemoveAppLogsAndHardenAuditTrail`
