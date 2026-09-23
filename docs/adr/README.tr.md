<!-- Canonical source: README.md. Keep this translation aligned with the English README. -->

# Mimari Karar Kayıtları (ADR)

[English](README.md) · [Türkçe](README.tr.md)

ADR (Architecture Decision Record), önemli bir mimari tercihin sadece sonucunu değil; hangi bağlamda
alındığını, hangi alternatiflerin değerlendirildiğini ve hangi sonuçların kabul edildiğini kaydeder.
Böylece gelecekteki bakım yapan kişi — veya gelecekteki ekip — commit geçmişinde arkeoloji yapmadan
kararın gerekçesini ve trade-off'larını anlayabilir.

Bu dizin hafif MADR biçimini kullanır. Her kayıt şu üç ana unsuru taşır: **bağlam**, **karar** ve
**sonuçlar**. Mevcut kayıtlar; pragmatik Clean Architecture, hiyerarşik action-list RBAC, SQL
Server/repository sınırı, JWT ve parola hashleme, test piramidi, gözlemlenebilirlik, frontend
temeli ile operasyon logu/audit trail ayrımını kapsar. 003 numaralı kayıt kaldırılmıştır ve geçmiş
bağlamını korumak için tabloda `Superseded/Removed` durumunda bırakılmıştır.

| Kayıt | Türkçe karşılığı | Durum |
|---|---|---|
| [001](001-pragmatic-clean-architecture.tr.md) | Pragmatik Clean Architecture ve Application'ın EF Core LINQ uzantıları | Kabul edildi |
| [002](002-hierarchical-rbac.tr.md) | Boolean izin kolonları yerine hiyerarşik action-list RBAC | Kabul edildi |
| [003](003-row-level-restrictions-asynclocal.tr.md) | EF Core global filter + AsyncLocal ile satır seviyesi kısıtlama | Yerine yenisi geldi / kaldırıldı |
| [004](004-sql-server-and-repository-abstraction.tr.md) | Birincil veri deposu olarak SQL Server 2022 ve DB-bağımsız repository yüzeyi | Kabul edildi |
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
