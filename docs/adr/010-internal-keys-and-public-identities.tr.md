# ADR-010: Saklanan kayıt kimliklerinde Guid kullanımı

- Tarih: 2026-10-17
- Durum: Accepted (yön kararı); uygulama vault planına göre yapılacak

## Bağlam

HelpCenter bugün `BaseEntity` kayıtlarında integer PK, `AuditLog` ve `RefreshToken` tablolarında `bigint` PK; veritabanı ve API boyunca sayısal foreign/scalar kayıt referansları kullanıyor. İş kayıtlarının çoğunda `BaseEntity.PublicId` zaten Guid. Status kayıtlarının seed ve istemci sözleşmelerinde kullanılan sabit sayısal ID’leri var.

Yusuf, saklanan tüm kayıt kimliklerinin Guid olmasına karar verdi. Buna PK’ler, ilişkisel FK’ler ve kayda işaret eden scalar alanlar; status ID’leri, audit actor/target ID’leri ve refresh-token ID/referansları dahildir. Sayaçlar, sayfalama değerleri, enum değerleri ve kullanıcıya gösterilen ticket numarası kimlik değildir; tipleri korunur.

## Karar

Saklanan her entity’nin primary key’i ve ona karşılık gelen tüm ilişki/kayıt referansları Guid olacak. Mevcut kayıt ve ilişkiler açık bir eski→yeni eşleme ile korunacak. Mevcut `PublicId` olan satırlarda bu değer yeni anahtar olarak kullanılacak; `PublicId` olmayan `AuditLog` ve `RefreshToken` satırlarına tek seferlik Guid üretilecek ve migration eşlemesinde kalıcı olarak tutulacak.

JWT claim’leri, API/UI DTO’ları, SignalR payload’ları, cache anahtarları, seed’ler, export’lar ve üretilen OpenAPI tipleri birlikte geçirilecek. Mevcut PublicId kullanan rotalar aynı kaydı bulmaya devam edecek. Sıralı uygulama, ön kontroller, doğrulama ve geri dönüş prosedürü ortak vault’taki “HelpCenter — Guid kimlik dönüşümü ana planı” belgesindedir.

## Sonuçlar

- Bu ADR yön kararını kaydeder; tek başına kodu veya veritabanını değiştirmez.
- Sayısal status ID’leri, koordineli API/UI geçişiyle sabit Guid seed kimliklerine dönüşür.
- Kayıt kimlikleri uygulamada Guid olarak üretilir; kimlikler yetkilendirme sırrı değildir.
- Sayısal sayaçlar, sayfa numarası/boyutu, enum sıra değerleri, sıralama pozisyonları ve string `TicketId` ayrı bir gereksinim yoksa değişmez.
- Envanter, migration provası, performans ve geri dönüş provası ile kullanıcıya dönük cutover planı geçmeden DB geçişi yapılmaz.
