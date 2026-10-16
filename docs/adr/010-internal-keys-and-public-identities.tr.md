# ADR-010: İç sayısal anahtarları ve dış UUID kimliklerini koruma

- Tarih: 2026-10-16
- Durum: Proposed

## Bağlam

`BaseEntity`, kayıt oluşturulurken rastgele `PublicId` üretiyor; EF ve SQL Server bu kimliği koruyor ve benzersizliğini denetliyor. İç ilişkiler integer FK, audit ve refresh token kayıtları long anahtar kullanıyor. Dış rotaların bir kısmında PublicId, bazı yönetim sözleşmelerinde sayısal kimlik var.

Tüm iç anahtarların Guid olması için bağımsız veri yazıcıları, veritabanları arası birleştirme veya çevrimdışı kayıt üretimi gibi somut bir ihtiyaç belirlenmedi. Dış UUID kimliği zaten mevcut. Guid kullanımı erişim kontrolünün yerini almıyor.

## Karar önerisi

İç sayısal PK/FK düzenini ve dış `PublicId` kimliklerini koru; bu değişiklikte toplu anahtar migration'ı yapma. Kimlik üretimi ve benzersizlik testlerini, her korumalı rotadaki sahiplik ve izin denetimlerini sürdür.

Bu öneri bütün rotaların Guid kullandığını iddia etmez. Kalan dış kimlik sözleşmeleri API sınırında ayrı incelenebilir.

## Sonuçlar

Anahtar dönüştürme, veri yeniden yazımı ve geri dönüş işlemi gerekmiyor. Mevcut FK'ler, JWT claim'leri, audit hedefleri ve istemciler aynı sözleşmeyi kullanır.

İleride Guid anahtara geçiş gerekirse FK/tüketici envanteri, gölge anahtar backfill'i, geçiş dönemi çift okuma/yazma, izole SQL provası ve geri alınabilir geçiş gerekir. Mevcut ID'leri rastgele yeniden üretmek uygun değildir.

PublicId lifecycle testleri üretim, kalıcılık ve açıkça verilen kimliğin korunmasını kapsıyor. Repository sayfalaması modeldeki anahtar tipini, long dahil, destekliyor. Bu ADR şema değiştirmiyor.
