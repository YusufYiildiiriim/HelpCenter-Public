<!-- Canonical source: 005-jwt-auth-and-password-hashing.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-005 — JWT bearer authentication, BCrypt ve Argon2id geçiş yolu

[English](005-jwt-auth-and-password-hashing.md) · [Türkçe](005-jwt-auth-and-password-hashing.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım

## Bağlam

Tek bir first-party SPA (`help-center-ui`) ve tek API vardır; üçüncü taraf OAuth istemcisi yoktur.
Yatayda ölçeklenen stateless authentication, çalınmış dump'larda offline cracking'e dayanıklı parola
hashleme ve token süresi/iptali için açık bir model gerekir.

## Karar

### Token biçimi

- Bugün **JWT bearer**, HS256: tek imzalayıcı, `IOptions<JwtOptions>` üzerinden başlangıçta
  doğrulanan `JwtSettings:Secret` içindeki simetrik anahtar.
- Claim'ler: `UserId`, `Email`, `Role`, standart `exp`, `iat`, `iss`, `aud`.
- `Microsoft.AspNetCore.Authentication.JwtBearer` ile doğrulanır; varsayılan beş dakikalık grace
  geniş olduğu için `ClockSkew`, `TimeSpan.Zero` yapılır.
- Access token ömrü **15 dakikadır**.

### Refresh stratejisi (uygulandı; aşağıdaki eke bakın)

- Refresh token'lar `RefreshTokens` tablosunda hashlenmiş (`SHA-256`) tutulur: `UserId`,
  `TokenHash`, `ExpiresAt`, `RevokedAt`, `ReplacedByTokenHash`, `CreatedByIp`, `UserAgent`.
- **Her kullanımda rotation:** Her refresh yeni token döndürür ve öncekini `Rotated` işaretler.
- **Reuse detection:** Daha önce rotate edilmiş token yeniden sunulursa tüm alt zincir iptal edilir;
  token çalınmış varsayılır.

### Parola hashleme

- **Bugün:** `BCrypt.Net-Next`, work factor 12.
- **Hedef:** `Konscious.Security.Cryptography.Argon2` ile Argon2id:
  `MemorySize=64MB`, `Iterations=3`, `Parallelism=4`.
- **Geçiş:** Başarılı girişte fırsatçı rehash; saklı hash BCrypt ise plaintext Argon2id ile yeniden
  hashlenir ve satır güncellenir.

### İstemcide token saklama

- Access token, kısa path scope'lu **HttpOnly, Secure, SameSite=Strict** cookie olarak verilir.
- Refresh token cookie'si `/api/auth` ile sınırlanır.
- `localStorage`, auth materyali için kullanılmaz. HttpOnly cookie, XSS ile doğrudan JavaScript
  erişiminden token sızdırma yolunu kapatır.

## Sonuçlar

**Olumlu:** Refresh reuse detection token hırsızlığını yakalar; Argon2id offline cracking maliyetini
yükseltir; HttpOnly cookie'ler `document.cookie` sızdırma yolunu kapatır.

**Olumsuz:** Cookie tabanlı auth CSRF'i gündeme getirir; auth cookie'leri için `SameSite=Strict` ve
state değiştiren endpoint'lerde antiforgery token gerekir. Argon2 daha yavaş olduğundan login'de dar
rate limit gerekir.

**Nötr:** Tek audience için HS256 yeterlidir. İmzalama anahtarını paylaşmadan token doğrulamak isteyen
ikinci first-party servis gerekirse JWKS endpoint'iyle **RS256**ya geçilir.

## İlgili

- [Güvenlik politikası](../../SECURITY.tr.md)

## Ek (2026-09-01) — Tasarım ve uygulama durumu

Kabul edilmiş ADR gövdesi değiştirilmeden sonraki durum kayda alınır.

**Uygulananlar:** `AuthTokenService`, refresh token'ları SHA-256 ile hashlenmiş tutar, her kullanımda
rotate eder ve yeniden kullanımdaysa kullanıcı için tüm zinciri iptal eder. `PasswordService`, yeni
ve değiştirilen parolaları ADR'deki OWASP-2024 parametreleriyle Argon2id kullanarak hashler; legacy
BCrypt hash'lerini de doğrular. Argon2id artık gelecek hedef değil, varsayılandır.

**Uygulanmayanlar:** `IPasswordService.NeedsRehash()` BCrypt ve eski Argon2id parametrelerini doğru
işaretler; fakat hiçbir login handler'ı bunu çağırmaz. Eski BCrypt hash'leri başarılı girişte otomatik
yükseltilmez. O tarihte backend access token için `Set-Cookie` göndermez, frontend token'ı
JavaScript'in erişebildiği `js-cookie` ve `sessionStorage`da tutardı; bu XSS riskiydi.

## Ek (2026-09-10) — Tarayıcı token saklama tamamlandı

Önceki token-saklama boşluğu kapandı:

- Kısa ömürlü access token yalnız frontend module memory'sinde (`authSession`) tutulur; ne
  `localStorage`, ne `sessionStorage`, ne de JavaScript'in yönettiği cookie kullanılır.
- API, rotate edilen `refresh_token`ı `HttpOnly`, `Secure`, `SameSite=Lax` cookie olarak ayarlar.
  JavaScript bunu okuyamaz; refresh ve logout request cookie'sini gönderir.
- Login ve refresh DTO'ları raw refresh token'ı JSON serialization dışında tutar. Sayfa yenilemesi,
  refresh akışıyla yeni access token alır.

Fırsatçı BCrypt→Argon2id rehash boşluğu açıktır.
