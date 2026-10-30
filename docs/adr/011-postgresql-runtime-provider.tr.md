# ADR-011 — Tek runtime sağlayıcısı olarak PostgreSQL

**Durum:** Kabul edildi
**Tarih:** 2026-10-30
**Karar veren:** Yusuf Yıldırım

## Bağlam

HelpCenter SQL Server ile başladı; ardından temiz veritabanı geçmişiyle PostgreSQL'e geçti.
Uygulamada artık PostgreSQL initial migration'ı, PostgreSQL concurrency ve timestamp davranışı,
LINQ dashboard read-model sorguları, PostgreSQL health check'i, Docker Compose ve gerçek
PostgreSQL integration testleri bulunuyor. SQL Server'ı runtime seçeneği olarak tutmak, mevcut
ürün ihtiyacı olmadan ikinci migration hattı ve provider test matrisi gerektirir.

## Karar

- Desteklenen tek runtime veritabanı PostgreSQL 17'dir. EF Core
  `Npgsql.EntityFrameworkCore.PostgreSQL` ve `UseNpgsql` kullanır.
- Aktif migration hattı güncel EF modelinden üretilen PostgreSQL initial migration ile başlar.
  SQL Server migration dosyaları `database/legacy/sqlserver-migrations/` altında tarihsel
  referans olarak kalır ve aktif uygulama tarafından çalıştırılmaz.
- Integration testleri gerçek PostgreSQL'i `Testcontainers.PostgreSql` ile kullanır; readiness
  PostgreSQL health-check sağlayıcısını kullanır. Yerel ve production Compose PostgreSQL çalıştırır.
- Dashboard/rapor okumaları EF Core/LINQ kullanır. PostgreSQL'e özel function veya view ancak
  production benzeri profiling ihtiyacı gösterirse eklenir.
- Bu karara SQL Server verisi taşıma dahil değildir. PostgreSQL temiz veritabanıyla başlar.
- `RowVersion`, uygulamanın ürettiği GUID concurrency token'dır. Kalıcı `DateTime` değerleri
  PostgreSQL'e yazılmadan önce UTC'ye normalize edilir.
- Provider seçimi bakımından ADR-004'ün yerini bu kayıt alır. Kabul edilmiş diğer ADR'lerdeki SQL
  Server ifadeleri, şema notları ve eski uygulama kayıtları yazıldıkları dönemin tarihsel bağlamıdır.

## Sonuçlar

**Olumlu**

- Geliştirme, CI integration testleri, container, migration ve runtime tek provider kullanır.
- Temiz veritabanı kurulumu SQL Server migration geçmişini çalıştırmadan tekrarlanabilir.
- Read-model ve concurrency davranışı gerçek runtime sağlayıcısıyla sınanır.

**Olumsuz / kabul edilen riskler**

- Mevcut SQL Server verisi taşınmaz; ileride veri transferi ayrı kapsam, doğrulama ve rollback planı
  gerektirir.
- Dashboard sorgularının production ölçeğindeki gecikmesi henüz ölçülmedi. Sağlayıcıya özel
  optimizasyon ancak temsili veriyle profiling sonrasında eklenmelidir.

**Nötr**

- Tarihsel SQL Server migration'ları ve ADR metinleri inceleme için korunur; runtime/deploy hattında
  kullanılmaz.

## Doğrulama

P5 kapanış kaydı ve güncel test kanıtı PostgreSQL geçiş planında ve CI workflow'unda tutulur.
