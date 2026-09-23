<!-- Canonical source: README.md. Keep this translation aligned with the English README. -->

<div align="center">

# HelpCenter

**Hiyerarşik RBAC ve özellik paketleri kullanan, kurumsal destek masası platformu.**

[English](README.md) · [Türkçe](README.tr.md)

![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Next.js](https://img.shields.io/badge/Next.js-16-000?logo=nextdotjs&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)
![Tailwind](https://img.shields.io/badge/Tailwind-4-06B6D4?logo=tailwindcss&logoColor=white)
![License](https://img.shields.io/badge/license-GPL--3.0--only-blue)

[**Mimari**](docs/architecture.tr.md) · [**Veritabanı Şeması**](docs/database-schema.tr.md) · [**Teknoloji Yığını**](docs/tech-stack.tr.md) · [**ADR'ler**](docs/adr/README.tr.md) · [**Güvenlik**](SECURITY.tr.md)

</div>

---

## ✨ Öne çıkanlar

HelpCenter; hiyerarşik RBAC ve özellik paketleriyle yetkilendirilmiş, kurumsal kullanıma yönelik
bir destek masası platformudur. Backend .NET 8, CQRS ve MediatR; istemci Next.js 16 ve React 19;
kalıcı veri katmanı SQL Server 2022 ve EF Core 9 kullanır. Clean Architecture sınırları yalnızca
ekip konvansiyonuna bırakılmaz: CI içinde çalışan NetArchTest kuralları katman ihlallerini yakalar.

- **Hiyerarşik action-list RBAC:** Yetkiler boolean kolonlarıyla şişirilmez; kaynak × aksiyon
  matrisiyle tanımlanır. Project Manager, Module Manager, Support Agent, Content Editor ve
  Read-Only Observer olmak üzere beş hazır özellik paketi, rol kurulumunu tek adımda başlatır.
- **Clean Architecture + CQRS + MediatR:** Domain, Application, Persistence, Infrastructure ve
  WebApi katmanları ayrıdır. Controller'lar ince tutulur; istekler validation ve loglama pipeline'ından
  geçer. Application; `IGenericRepository<T>`, `IUnitOfWork` ve `IUserContext` soyutlamalarını kullanır.
- **Güvenlik:** Access token bellekten, refresh token döndürülen `HttpOnly` cookie'den kullanılır.
  Yeni parolalar Argon2id ile hashlenir; BCrypt yalnız eski hashleri doğrulamak için korunur.
  FluentValidation, endpoint kapsamlı native rate limiting, temizlenmiş `IExceptionHandler` cevapları
  ve `SecurityHeadersMiddleware` temel koruma katmanını tamamlar. Ayrıntı için [güvenlik politikasına](SECURITY.tr.md) bakın.
- **Gözlemlenebilirlik:** Serilog, correlation ID, sağlık endpoint'leri, OpenTelemetry ve Prometheus
  bulunur. Hata cevapları `ApiResponse<T>` ile sarmalanır; başarılı cevapların çoğu ham DTO döndürür.
  Dosya tabanlı operasyon loglarıyla SQL Server'daki append-only AuditLog farklı amaçlarla tutulur.
- **Üretim teslimatı:** Çok aşamalı Dockerfile'lar, Caddy TLS, private API/veritabanı container'ları,
  GitHub Actions CI, bağımlılık/secret taraması ve Dependabot birlikte kullanılır.

## 🏗️ Mimariye kısa bakış

İstemci, API ile HTTPS, JWT ve `X-Correlation-Id` üzerinden haberleşir. WebApi katmanında correlation
ID, global exception handler ve rate limiter middleware'leri bulunur; controller'lar veri erişimi
yapmadan `IMediator` çağırır. Application CQRS handler'larını, DTO'ları ve servis soyutlamalarını;
Domain entity, value object ve domain event'leri; Persistence/Infrastructure ise EF Core, SQL Server,
JWT, parola hashleme ve SignalR uyarlamalarını içerir. Ayrıntılı diyagramlar için [mimari belgesine](docs/architecture.tr.md) bakın.

## 🚀 Hızlı başlangıç

Önce repoyu klonlayın, kökteki `.env.example` dosyasını `.env` olarak kopyalayıp gerçek değerleri
girin. Bu tek dosya .NET, Next.js ve Docker Compose'un yerel runtime yapılandırmasının kaynağıdır.
Geliştirme veritabanı Docker'da, backend ve frontend yerelde çalışır.

```bash
git clone https://github.com/<you>/HelpCenter.git
cd HelpCenter
cp .env.example .env
docker compose --env-file .env -f dockerfiles/docker-compose.dev.yml up -d db

./scripts/with-env.sh dotnet ef database update --project help-center-backend/HelpCenter.Persistence --startup-project help-center-backend/HelpCenter.WebApi
./scripts/with-env.sh dotnet run --project help-center-backend/HelpCenter.WebApi

# Yeni terminal
./scripts/with-env.sh npm --prefix help-center-ui ci
./scripts/with-env.sh npm --prefix help-center-ui run dev
```

`scripts/with-env.sh`, kökteki `.env` değerlerini yalnız başlattığı komut için environment variable
olarak export eder. Yerelde .NET ve Next.js repository kökündeki `.env` dosyasını kendiliğinden
okumadığı için bu komutlarda kullanılmalıdır; Docker Compose ise aynı dosyayı `--env-file .env` ile okur.

API varsayılan olarak `http://localhost:5005`, istemci `http://localhost:3000` adresindedir. Seed
edilen varsayılan yönetici `admin@helpcenter.com` / `Admin123!` bilgisidir. Parolası publictir;
gerçek bir dağıtımdan önce parolayı değiştirin veya seed'i kaldırın. Ayrıntı için [güvenlik politikasına](SECURITY.tr.md)
bakın. Swagger yalnız Production dışındaki
ortamlarda `/swagger` yolundadır.

## 🧪 Testler

| Paket | Komut | Doğruladığı alan |
|---|---|---|
| Mimari | `cd help-center-backend && dotnet test tests/HelpCenter.ArchitectureTests` | NetArchTest ve FluentAssertions ile katman sınırları |
| Application birim | `cd help-center-backend && dotnet test tests/HelpCenter.Application.Tests` | xUnit, NSubstitute ve EF InMemory ile handler'lar |
| WebApi entegrasyon | `cd help-center-backend && dotnet test tests/HelpCenter.WebApi.IntegrationTests -c Release --logger "trx;LogFileName=integration-results.trx" --results-directory TestResults` | Testcontainers.MsSql ve WebApplicationFactory ile gerçek veritabanı |
| Frontend birim/build | `cd help-center-ui && npm run lint && npm run type-check && npm run api:types:check && npm test && npm run build` | ESLint, TypeScript, OpenAPI type drift, Vitest ve Next.js |
| Frontend browser smoke | `cd help-center-ui && npx playwright install chromium && npm run e2e` | Playwright ile public rota ve axe WCAG A/AA (renk kontrastı hariç) |

## 🚢 Production'a dağıtım

Production compose yapısı UI ve API'yi Caddy üzerinden tek HTTPS alan adında sunar; API ve SQL
Server host portlarına yayınlanmaz. Alan adının A/AAAA kaydını sunucuya yönlendirin, 80 ve 443
portlarını açın; sonra repository kökünden `.env` dosyasında veritabanı, JWT, SMTP,
public origin (`NEXT_PUBLIC_API_URL`, `NEXT_PUBLIC_APP_URL`, `AppSettings__FrontendUrl`), `APP_DOMAIN`
ve `ACME_EMAIL` değerlerini tanımlayın:

```bash
cp .env.example .env
docker compose --env-file .env -f dockerfiles/docker-compose.prod.yml up -d --build
```

Yeni sürümden önce CI ile aynı release kontrolleri çalıştırılmalıdır:

```bash
cd help-center-backend && dotnet test HelpCenter.slnx -c Release
cd ../help-center-ui && npm audit --omit=dev --audit-level=high && npm run lint && npm run type-check && npm test && npm run build
```

Migration'lar uygulama
başlangıcında kasten otomatik uygulanmaz; private veritabanı ağına ulaşabilen güvenilir bir runner
üzerinden ayrı ve izlenebilir release adımı olarak uygulanır.

## 📚 Dokümantasyon

- [Mimari ve diyagramlar](docs/architecture.tr.md)
- [Veritabanı şema referansı](docs/database-schema.tr.md)
- [Teknoloji yığını](docs/tech-stack.tr.md)
- [Mimari karar kayıtları](docs/adr/README.tr.md)
- [Güvenlik politikası](SECURITY.tr.md)

## 📊 Proje istatistikleri

- **Backend:** .NET 8, beş production projesi, 24 controller üzerinde 90'dan fazla endpoint ve MediatR CQRS
- **Frontend:** Next.js 16 App Router, React 19, strict TypeScript, Tailwind 4 ve Radix primitive'leri
- **Veri:** SQL Server 2022, EF Core 9 ve 25'ten fazla migration
- **CI:** Her PR'da secret/vulnerability taraması, release build, mimari/birim/entegrasyon smoke testleri, lint ve frontend build
- **Operasyon:** Çok aşamalı Docker, Caddy TLS reverse proxy ve private API/veritabanı container'ları

## 📝 Lisans

GPL-3.0-only — ayrıntı için [LICENSE](LICENSE).
