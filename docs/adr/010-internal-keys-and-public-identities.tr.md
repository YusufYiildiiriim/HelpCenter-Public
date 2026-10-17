# ADR-010: İç anahtarları Guid’e dönüştürme yönü

- Tarih: 2026-10-17
- Durum: Accepted (yön kararı); uygulama ayrı backlog işi

## Bağlam

HelpCenter domain kayıtları `BaseEntity.Id` ve ilişkisel foreign key’ler için `int` kullanıyor. `BaseEntity.PublicId` zaten Guid ve dış kimlik olarak API/UI’da kullanılıyor. Audit log ve refresh token gibi yardımcı tablolarda bağımsız `long` anahtarlar da bulunuyor.

Yusuf, iç kimlik modelini Guid’e taşıma kararını kesinleştirdi. Bu kayıt karar yönünü sabitler; şema veya uygulama kodunun dönüştürüldüğü anlamına gelmez.

## Karar

Domain entity’lerinin primary key’leri ve bunlara karşılık gelen ilişkisel foreign key’ler Guid olacak. Dönüşüm mevcut kayıtların ilişkilerini koruyacak şekilde tasarlanmalı; mümkün olan yerlerde mevcut `PublicId` Guid değerleri geçiş anahtarı olarak değerlendirilmelidir. API/UI kimlik sözleşmeleri ve token claim’leri geçiş boyunca uyumlu tutulmalıdır.

Uygulama işi başlamadan önce tüm FK, scalar ID tüketicisi, DTO, route, JWT claim’i, audit hedefi, seed ve istemci kullanım noktaları envanterlenecek. Audit log hedef kimlikleri ve RefreshToken yardımcı anahtarlarının kapsamı uygulama tasarımında açıkça karara bağlanacak. Migration gerçek verinin kopyasıyla izole SQL Server’da prova edilip doğrulanmadan yerel/ortak veritabanına uygulanmayacak.

## Sonuçlar

- Yeni geliştirmelerde iç domain kimlikleri için hedef tip Guid’dir.
- Bu ADR tek başına kod, migration, veritabanı veya API değişikliği yapmaz.
- Mevcut `PublicId` değerlerini primary key’e dönüştürmek olası veri koruma yoludur; ilişki ve benzersizlik kontrolleriyle migration tasarımında doğrulanmalıdır.
- Geçiş API/UI birlikte sürümleme, JWT eski claim uyumluluğu, seed ve veritabanı geri alma planı gerektirir.
- Migration ve DB apply ayrı incelenebilir adımlardır; bu oturumda DB’ye dokunulmadı.
