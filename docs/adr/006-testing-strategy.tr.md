<!-- Canonical source: 006-testing-strategy.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-006 — Test piramidi: mimari + birim + Testcontainers entegrasyon testleri

[English](006-testing-strategy.md) · [Türkçe](006-testing-strategy.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım

## Bağlam

Test paketleri iki uca da kayabilir. Tamamı birim testiyse persistence refactor'larının kıracağı
query filter, migration ve transaction'lar yeşil geçer. Tamamı entegrasyon testiyse yüksek doğruluk
pahasına yavaşlar ve hatanın kaynağı ayrışmaz. Yapısal çürümeyi, iş kuralı ve entegrasyon hatalarını
ait oldukları katmanda yakalayan bir şekil gerekir.

## Karar

Üç backend katmanı ve frontend katmanı her pull request'te CI'da çalışır.

### 1. Mimari testler — `HelpCenter.ArchitectureTests`

- **Araç:** `NetArchTest.Rules`.
- **Korunan kurallar:** `Domain` dış referans taşımaz; `Application`, `Persistence` veya `WebApi`ye
  referans vermez; controller'lar `DbContext` veya `Microsoft.EntityFrameworkCore` kullanmaz;
  MediatR handler'ları adlandırma/modifier kurallarına uyar.
- **Çalışma süresi:** Saniyeler.
- **Değer:** “Yanlış şeyi import ettin” türündeki review yorumu derleyici seviyesinde yanıtlanır.

### 2. Application birim testleri — `HelpCenter.Application.Tests`

- **Araç:** `xUnit` + `NSubstitute` + `FluentAssertions`.
- **DB:** Provider'dan bağımsız davranışlar için `Microsoft.EntityFrameworkCore.InMemory`.
- **Kapsam:** Command/query handler iş kuralları, validation hataları ve edge case'ler.
- **Kural:** Her handler için en az üç test: happy path, validation failure ve edge case.

### 3. WebApi entegrasyon testleri — `HelpCenter.WebApi.IntegrationTests`

- **Araç:** `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) +
  `Testcontainers.MsSql`.
- **DB:** Her test çalışmasında Docker container'ında açılan gerçek SQL Server 2022.
- **Kapsam:** Gerçek migration ve SQL Server üzerinde health readiness, refresh rotation/logout,
  SignalR bearer authentication ve public FAQ/guide filtreleme.
- **Çalışma süresi:** Container başlangıcı nedeniyle yaklaşık 30 saniye daha yavaştır; ana pipeline'ı
  yavaşlatmamak için ayrı job'da tutulur.

### 4. Frontend

- Hook'lar ve saf helper'lar için `Vitest` + `happy-dom`.
- E2E smoke kapsamı için Playwright: login → dashboard → role oluşturma.

## Sonuçlar

**Olumlu:** Katman sınırları sessizce bozulamaz; gerçek SQL Server testleri query filter, unique
index ve transaction semantiğini production'a eşdeğer sınar; InMemory testleri saf iş kuralları için
hızlı kalır.

**Olumsuz:** Testcontainers CI runner'ında Docker ister ve container boot süre ekler. GitHub Actions
Linux runner'larında vardır; Windows self-hosted ortamlarda olmayabilir.

## Ek (2026-09-11) — CI sonuç sözleşmesi

Entegrasyon job'ı TRX dosyasını açıkça `TestResults/integration-results.trx` yoluna yazar; dosya
yoksa veya sıfır keşfedilmiş test raporlarsa başarısız olur. Böylece discovery'si kırılmış bir
çalışma, kapsama olmadan yeşil görünemez.

**Nötr:** Test sayısı kalite için geriden gelen göstergedir. Coverage belirli eşikte gate edilmez;
pull request gözden geçirmesi sayıdan çok *neyin* test edildiğine bakar.
