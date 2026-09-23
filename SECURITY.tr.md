<!-- Canonical source: SECURITY.md. Keep this translation aligned with the English document. -->

# Güvenlik Politikası

[English](SECURITY.md) · [Türkçe](SECURITY.tr.md)

## Desteklenen sürümler

| Sürüm | Desteklenir |
|---|---|
| 1.x | :white_check_mark: |

## Güvenlik açığı bildirimi

Güvenlik açıklarını **yusuf.yiildiiriim@gmail.com** adresine bildirin. Güvenlik açığı için public
GitHub issue oluşturmayın.

## Güvenlik önlemleri

### Kimlik doğrulama

- JWT Bearer kimlik doğrulama (HS256).
- Yeni/değişen parolalar için Argon2id (OWASP-2024: 64 MiB bellek, 3 iterasyon, parallelism 4).
  BCrypt yalnız migration öncesi hash'leri `Verify()` etmek için korunur. Bilinen boşluk:
  `IPasswordService.NeedsRehash()` var olsa da login handler'larında çağrılmaz; eski BCrypt hash'leri
  otomatik Argon2id'ye yükseltilmez.
- Issuer, audience, lifetime ve signing key doğrulaması.
- Login'de 5 istek/30 saniye; reset, public/write/upload/heavy endpoint sınıflarında native
  ASP.NET Core `RateLimiter` sınırları.
- Refresh token'lar SHA-256 hash'i olarak tutulur, her kullanımda döner; dönmüş token yeniden
  kullanılırsa tüm token zinciri iptal edilir. Ayrıntı: [ADR-005](docs/adr/005-jwt-auth-and-password-hashing.tr.md).

### Yetkilendirme

- Aksiyon tabanlı hiyerarşik RBAC.
- Policy-temelli authorization handler'ları.

### Token saklama

- API kısa ömürlü access token'ı JSON gövdesinde döndürür. Frontend onu yalnız modül belleğindeki
  `authSession`da tutar; `localStorage`, `sessionStorage` veya JavaScript'in yönettiği cookie'ye yazmaz.
  Tam sayfa yenileme refresh akışıyla yeni access token alır.
- Uzun ömürlü `refresh_token`, `HttpOnly`, `Secure`, `SameSite=Lax` cookie olarak API tarafından
  yazılır. Tarayıcı JavaScript'i okuyamaz; refresh/logout isteğin cookie'sinden alır, rotation yenisini yazar.
- Login ve refresh DTO'ları ham refresh token'ı sadece cookie kuracak kadar taşır; `JsonIgnore`,
  tokenın JSON cevabına serileşmesini önler.

### Demo SSO köprüsü

- `/dashboard/sso`, build sırasında `NEXT_PUBLIC_DEMO_SSO_ENABLED=true` verilmedikçe kapalıdır.
- Açıksa yalnız mevcut browser bellek oturumuna önceden üretilmiş JWT kabul eden CV/demo köprüsüdür;
  identity provider trust, issuer discovery, authorization-code exchange veya backend OAuth endpointi
  olmadığı için OIDC/SAML değildir.
- URL tokenını navigation'dan önce kaldırır; browser storage'a ya da JS cookie'sine yazmaz. Production
  SSO çözümü olarak sunulmamalıdır.

### Taşıma güvenliği ve başlıklar

Development dışındaki ortamlarda HTTPS redirect çalışır. `SecurityHeadersMiddleware`, her istekte
`nosniff`, `DENY`, `strict-origin-when-cross-origin`, XSS koruması, `default-src 'self'` CSP,
kamera/mikrofon/geolocation'ı kapatan Permissions-Policy ve 1 yıllık HSTS gönderir.

### Girdi ve veri koruması

- Tüm command/query nesnelerinde FluentValidation, tüm endpointlerde sunucu tarafı validation ve
  production'da stack trace içermeyen hata mesajları.
- EF Core parameterized query, soft delete, audit alanları (`CreatedAt`, `UpdatedAt`, `CreatedBy`,
  `LastModifiedBy`) ve `RowVersion` concurrency token'ları.
- Dış API kimliği, reversible integer maskeleme değil DB'nin ürettiği gerçek `Guid PublicId`dir.
  `RequestSubject`/`Faq`/`AdminCloseRequest` ham integer Id göstermeyi sürdüren, kapsam dışı eski tutarsızlıklardır.

### Secret yönetimi

- Gitignore'daki kök `.env`, .NET, Next.js ve Docker Compose için tek yerel runtime yapılandırma
  dosyasıdır. `.env.example` tüm gerekli anahtarları listeler; gerçek secret repoya girmez.
- Yerel .NET/Next.js komutları için `scripts/with-env.sh` kök `.env` dosyasını export eder.
  Compose, `docker compose --env-file .env` ile çalışır; veritabanı, JWT veya SMTP değeri yoksa
  iki compose dosyası da açık hatayla durur.
- `appsettings.json` connection string veya SMTP credential fallback'i taşımaz. API, zayıf default
  kabul etmek yerine JWT key ve SMTP seçeneklerini startup'ta doğrular.
- Kök `.env` gitignore'dadır; Dependabot açık bağımlılıkları tarar.
- Seed/demo hesap parolaları publictir; gerçek dağıtımdan önce her hesap değiştirilmeli ya da seed kaldırılmalıdır.
  `admin@helpcenter.com` / `Admin123!` CV/demo bootstrap'ıdır, secret veya production bootstrap değildir.

### Loglama

- Serilog structured logging, istekler arası correlation ID ve PII/secret içermeyen güvenli hata mesajları.

## Bağımlılık yönetimi

- Dependabot, NuGet + npm + GitHub Actions için haftalık tarama yapar.
- CI, runtime npm bağımlılıklarındaki high/critical bulguda `npm audit --omit=dev --audit-level=high`
  ile CV/demo release kapısını kırar. Development/transitive tooling dahil tam npm audit sonucu ayrıca
  raporlanır ama tek başına blocker değildir. NuGet taraması transitive paketleri de kapsar ve blocker kalır.
