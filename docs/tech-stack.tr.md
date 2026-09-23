<!-- Canonical source: tech-stack.md. Keep this translation aligned with the English document. -->

# Teknoloji Yığını

[English](tech-stack.md) · [Türkçe](tech-stack.tr.md)

Bu belge repodaki runtime bağımlılıklarını, seçilme gerekçelerini ve bilinçli olarak reddedilen
alternatifleri açıklar; somut patch sürümleri manifestlerde tutulur.

## Backend (.NET 8)

| Alan | Seçim | Gerekçe |
|---|---|---|
| Runtime/web host | .NET 8 + ASP.NET Core 8 | LTS, native `IExceptionHandler`, `RateLimiter`, AOT hazırlığı; MVC controller'lar OpenAPI ergonomisi için kullanılır. |
| CQRS | MediatR | Küçük ve denenmiş yüzey; Wolverine saga/distributed messaging ihtiyacından ağır, elle dispatcher pipeline behavior'ları kaybettirir. |
| Mapping | AutoMapper | Convention tabanlı DTO glue kodunu azaltır; v16 explicit `CreateMap` sürpriz eşlemeyi sınırlar. Mapster daha hızlı olsa da LINQ projection desteği daha zayıftır. |
| Validation | FluentValidation | Composable/test edilebilir; service bağımlısı validator'lar için DataAnnotations'tan uygundur. |
| ORM | EF Core 9 + SQL Server provider | LINQ, migration, global filter ve change tracking gerekir; okuma hot-path'i profiling gerektirirse Dapper yeniden değerlendirilir. |
| Dış kimlik | `BaseEntity.PublicId` + value generator | Her uygun entity DB'nin ürettiği rastgele `Guid` alır; kaldırılmış reversible `HashService` yerine public route'lar bunu kullanır. |
| JWT | `Microsoft.AspNetCore.Authentication.JwtBearer` | In-box standardı; tek audience için Duende IdentityServer gereksizdir. |
| Password | Argon2 + BCrypt.Net-Next | Yeni/değişen parola Argon2id (64 MiB, 3 iterasyon, parallelism 4); BCrypt yalnız legacy verify içindir. `NeedsRehash`in login'de çağrılmaması bilinen boşluktur. |
| Observability | Serilog + File sink + SQL health check | Structured log, environment/thread enrichment, rolling fallback ve `/health/ready`. |
| API contract | Swashbuckle + XML docs | OpenAPI 3/Swagger, frontend `openapi-typescript` üretimini besler; XML hem Swagger hem IntelliSense'tedir. |
| Test | xUnit, FluentAssertions, NSubstitute, Testcontainers, EF InMemory, NetArchTest, MVC Testing | Unit/integration/layer testleri; gerçek SQL Server, provider farklarını yakalar. |
| Statik analiz | Sonar, Meziantou, Roslynator, `Directory.Build.props` | OWASP/bakım/performance lintleri; nullable ve Release warnings-as-errors; alan-adına özgü ve ertelenmiş analyzer bulguları kademeli temizlik boyunca bloklamaz. |

## Frontend (Next.js 16)

| Alan | Seçim | Gerekçe |
|---|---|---|
| Framework | Next.js 16 App Router + React 19 | Modern SSR/RSC, routing; `output: standalone` küçük Docker imajı. React concurrent özellikleri ve Next'in beklediği Actions/`use()` desteği. |
| Tip | Strict TypeScript | Bundler resolution ve `@/*` alias; deprecated `baseUrl` yok. |
| UI | Tailwind 4 + Radix UI + shadcn-style composition | Hızlı, token tabanlı stil; erişilebilir dialog/dropdown/avatar/label primitive'leri, runtime UI library bağımlılığı olmadan class/variant composition. |
| Görsel/etkileşim | Lucide, Framer Motion, Sonner | Tree-shakeable tutarlı ikon, `prefers-reduced-motion`u gözeten animasyon ve erişilebilir toast. |
| HTTP/veri | Axios, Zod, TanStack Table, date-fns | Auth/correlation/401 interceptor ergonomisi, env schema doğrulaması, headless tablo ve küçük tree-shakeable tarih fonksiyonları. |
| Alan araçları | SignalR, CKEditor 5, jsPDF + autotable, browser Blob CSV | Gerçek zamanlı bildirim, explicit ClassicEditor plugin listesi, client PDF ve spreadsheet parser taşımadan CSV. |
| Test/tooling | Vitest, happy-dom, Testing Library (planlı), ESLint, Prettier, openapi-typescript | Hızlı helper/hook testi, behavior-first DOM testleri, strict lint ve isteğe bağlı snapshot'tan type üretimi. |

## Altyapı

| Öğe | Amaç ve gerekçe |
|---|---|
| SQL Server 2022 | Birincil veri deposu; seçim için [ADR-004](adr/004-sql-server-and-repository-abstraction.tr.md). |
| Multi-stage Docker | Tekrarlanabilir build/production; backend Debian .NET 8 non-root, frontend `node:22-alpine`; local secret/build output image contextinden dışlanır. |
| docker-compose | Development ve production yerel orkestrasyonu; yeni katkıcı için en hızlı giriş. |
| GitHub Actions + Dependabot | Public repo için CI ve haftalık, ekosisteme göre gruplanmış dependency PR'ları. |
| Serilog File sink | Developmentda harici bağımlılıksız runtime teşhisi; prod observability backend'i için değiştirilebilir. AuditLog bunun yerine SQL Server'da iş/güvenlik kanıtıdır. |

## Bilinçli olarak eklenmeyenler

- **Mapster/manuel mapping:** AutoMapper'ın explicit-map politikası yeterli; migration maliyeti faydayı aşar.
- **Wolverine/elle dispatcher:** Dağıtık mesajlaşma gerekmiyor, MediatR patterni yeterli.
- **Application'da Specification pattern:** Pragmatik Clean Architectureta repository `IQueryable<T>` döndürür ve handler LINQ kullanır; sınır testlerle uygulanır. Ayrıntı: [ADR-001](adr/001-pragmatic-clean-architecture.tr.md).
- **Dapper:** Mevcut yüzeyde change tracking/navigation graph, ham query hızından önemlidir; profil gerektirirse belirli hot read path'e eklenir.
- **NoSQL/event sourcing:** RBAC ve audit trail güçlü tutarlılık ile referential integrity gerektirir.
- **GraphQL:** Tek first-party client için REST + typed OpenAPI yeterli.
- **Microservice:** Tek deployable unit daha hızlı teslim edilir; sınırlar Clean Architecture ile kod içindedir.
- **Repo kökünde Bicep/Pulumi:** Deployment hedefi açık tutulur; IaC deployment reposunda yaşar.

## Sürüm pinleri

Somut sürümler manifestlerde bulunur: backend için
[`HelpCenter.WebApi.csproj`](../help-center-backend/HelpCenter.WebApi/HelpCenter.WebApi.csproj) ve
diğer `.csproj`ler; frontend için [`package.json`](../help-center-ui/package.json).
