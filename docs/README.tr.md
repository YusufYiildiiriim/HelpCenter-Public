<!-- Canonical source: README.md. Keep this translation aligned with the English README. -->

# HelpCenter — Dokümantasyon

[English](README.md) · [Türkçe](README.tr.md)

Bu klasör, projenin yayımlanan ayrıntılı teknik dokümantasyonudur. Kök dizindeki
[`README.tr.md`](../README.tr.md), projenin genel tanıtımı ve tüm sistemi yerelde ayağa kaldırmak için
hızlı başlangıç noktasıdır; burası ise mimariyi, kalıcı veri modelini ve karar geçmişini derinlemesine
incelemek için teknik referanstır.

| Dosya | Amaç |
|---|---|
| [`architecture.tr.md`](architecture.tr.md) | Mermaid diyagramlarıyla katman haritası, bir isteğin yaşam döngüsü, RBAC modeli ve dağıtım topolojisi |
| [`database-schema.tr.md`](database-schema.tr.md) | SQL Server tablo referansı; ilişkiler, foreign key davranışları, kısıtlar, indeksler, yaşam döngüsü ve audit sınırı |
| [`tech-stack.tr.md`](tech-stack.tr.md) | Kullanılan her paket, seçilme gerekçesi ve bilinçli olarak reddedilen alternatifler |
| [`adr/README.tr.md`](adr/README.tr.md) | 9 mimari karar kaydı: 8'i kabul edilmiş, 1'i kaldırılmış/yerine yenisi gelmiş |

Dahili çalışma belgeleri — sistem mimarisi raporu, commit çalışma günlüğü, refactoring planları ve
arşivlenmiş production-readiness yol haritası — bu public repoda tutulmaz. Bunlar ayrı çalışma
alanında izlenir; buradaki belgeler ise ürünü kullanan veya katkı veren kişinin ihtiyaç duyacağı
doğrulanabilir teknik kaynaktır.
