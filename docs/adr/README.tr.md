<!-- Canonical source: README.md. Keep this translation aligned with the English README. -->

# Mimari Karar Kayıtları (ADR)

[English](README.md) · [Türkçe](README.tr.md)

ADR (Architecture Decision Record), önemli bir mimari tercihin sadece sonucunu değil; hangi bağlamda
alındığını, hangi alternatiflerin değerlendirildiğini ve hangi sonuçların kabul edildiğini kaydeder.
Böylece gelecekteki bakım yapan kişi — veya gelecekteki ekip — commit geçmişinde arkeoloji yapmadan
kararın gerekçesini ve trade-off'larını anlayabilir.

Bu dizin hafif MADR biçimini kullanır. Her kayıt **bağlam**, **karar** ve **sonuçları** kaydeder.
ADR-011 PostgreSQL'i tek runtime sağlayıcısı olarak belirler; ADR-004 başlangıçtaki SQL Server
kararını tarihsel kayıt olarak korur. 003 de kaldırılmış tasarımın tarihçesidir.

| Kayıt | Türkçe karşılığı | Durum |
|---|---|---|
| [010](010-internal-keys-and-public-identities.tr.md) | İç sayısal anahtarları ve dış UUID kimliklerini koruma | Proposed |
| [011](011-postgresql-runtime-provider.tr.md) | Tek runtime sağlayıcısı olarak PostgreSQL | Kabul edildi |
| [001](001-pragmatic-clean-architecture.tr.md) | Pragmatik Clean Architecture ve Application'ın EF Core LINQ uzantıları | Kabul edildi |
| [002](002-hierarchical-rbac.tr.md) | Boolean izin kolonları yerine hiyerarşik action-list RBAC | Kabul edildi |
| [003](003-row-level-restrictions-asynclocal.tr.md) | EF Core global filter + AsyncLocal ile satır seviyesi kısıtlama | Yerine yenisi geldi / kaldırıldı |
| [004](004-sql-server-and-repository-abstraction.tr.md) | Birincil veri deposu olarak SQL Server 2022 ve DB-bağımsız repository yüzeyi | ADR-011 ile geçersiz kılındı |
| [005](005-jwt-auth-and-password-hashing.tr.md) | JWT bearer authentication, BCrypt ve Argon2id geçiş yolu | Kabul edildi |
| [006](006-testing-strategy.tr.md) | Mimari + birim + Testcontainers entegrasyon test piramidi | Kabul edildi |
| [007](007-observability-baseline.tr.md) | Serilog, correlation ID ve health check gözlemlenebilirlik temeli | Kabul edildi |
| [008](008-frontend-foundation.tr.md) | Frontend görsel yüzeyleri, veri ve UI sınırları | Kabul edildi |
| [009](009-operational-logging-and-audit-trail.tr.md) | Operasyonel loglama ile değiştirilemez audit trail sınırı | Kabul edildi |

## Yeni ADR yazmak

Kabul edilmiş bir ADR, geçmişi yeniden yazacak biçimde değiştirilmez. Karar zamanla değişirse yeni
bir ADR oluşturulur; eski kayıtta yeni numaraya açık referans verilerek `Superseded` durumuna alınır.
Tablo başlıkları ve ADR dosya adları kalıcı teknik referans oldukları için İngilizce tutulur.

1. Son ADR dosyasını kopyalayın ve numarayı artırın.
2. `Context`, `Decision` ve `Consequences` bölümlerini somut gerekçeler ve etkilerle doldurun.
3. Kaydı `Proposed` durumuyla pull request'e ekleyin.
4. Merge sonrasında `Accepted` yapın; bir önceki kararı değiştiriyorsa eski ADR'ye açıkça
   `Superseded by ADR-N` referansı koyun.
