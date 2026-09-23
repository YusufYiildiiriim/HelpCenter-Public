# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | :white_check_mark: |

## Reporting a Vulnerability

Please report security vulnerabilities to **yusuf.yiildiiriim@gmail.com**.

Do not create public GitHub issues for security vulnerabilities.

## Security Measures

### Authentication
- JWT Bearer token authentication (HS256)
- Argon2id password hashing (OWASP-2024 params: 64 MiB memory, 3 iterations, parallelism 4) for
  all current/changed passwords. BCrypt is kept only to `Verify()` passwords that were hashed
  before the migration — **known gap:** the opportunistic-rehash-on-login step
  (`IPasswordService.NeedsRehash()`) is implemented but not currently called from any login
  handler, so pre-existing BCrypt hashes are never automatically upgraded to Argon2id.
- Token validation (issuer, audience, lifetime, signing key)
- Rate limiting on login endpoints (5 req/30s window; several other endpoint classes — password
  reset, public/write/upload/heavy traffic — have their own limits via ASP.NET Core's native
  `RateLimiter`)
- Refresh tokens are stored hashed (SHA-256), rotated on every use, with reuse detection that
  revokes the whole token chain if an already-rotated token is replayed (see ADR-005).

### Authorization
- Hierarchical RBAC with action-based permissions
- Policy-based authorization handlers

### Token storage

- The API returns the short-lived access token in the JSON response body. The frontend keeps it
  only in module memory (`authSession`); it is not written to `localStorage`, `sessionStorage`,
  or a JavaScript-managed cookie. A full page reload therefore obtains a new access token through
  the refresh flow.
- The API sets the long-lived `refresh_token` as a `HttpOnly`, `Secure`, `SameSite=Lax` cookie.
  Browser JavaScript cannot read this cookie. Refresh and logout endpoints read it from the
  request cookie, and refresh rotation replaces it with a new cookie.
- The server-side login and refresh DTOs carry the raw refresh token only long enough to set the
  cookie. `System.Text.Json.Serialization.JsonIgnore` prevents `RefreshToken` from being
  serialized into those JSON responses. See [ADR-005](docs/adr/005-jwt-auth-and-password-hashing.md)
  for the design and implementation-status history.

### Demo SSO bridge

- `/dashboard/sso` is disabled unless `NEXT_PUBLIC_DEMO_SSO_ENABLED=true` is set at build time.
- When enabled, it is a CV/demo bridge that accepts a pre-issued JWT only for the current browser
  memory session. It is not an OIDC or SAML implementation: there is no identity-provider trust,
  issuer discovery, authorization-code exchange, or backend OAuth endpoint.
- The bridge removes its query token from the URL before navigating and never writes it to browser
  storage or a JavaScript-managed cookie. It must not be presented as a production SSO solution.

### Transport Security
- HTTPS redirection in non-development environments (`app.UseHttpsRedirection()`)
- HSTS and the headers below are sent by the running app — see "Headers" below.

### Headers
`SecurityHeadersMiddleware` (`HelpCenter.WebApi/Middleware/SecurityHeadersMiddleware.cs`) is
registered in `Program.cs` and runs on every request, emitting:
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- Referrer-Policy: strict-origin-when-cross-origin
- X-XSS-Protection: 1; mode=block
- Content-Security-Policy: default-src 'self'
- Permissions-Policy: camera=(), microphone=(), geolocation=()
- Strict-Transport-Security: max-age=31536000; includeSubDomains

### Input Validation
- FluentValidation on all command/query objects
- Server-side validation on all endpoints
- Safe error messages (no stack traces in production)

### Data Protection
- Entity Framework Core with parameterized queries (SQL injection prevention)
- Soft delete pattern for data retention
- Audit fields on all entities (CreatedAt, UpdatedAt, CreatedBy, LastModifiedBy)
- RowVersion concurrency tokens
- Internal integer IDs are masked in API responses as a real `Guid PublicId` per entity
  (`BaseEntity.PublicId`, `HelpCenter.Persistence/ValueGeneration/PublicIdValueGenerator.cs`) —
  a database-generated random identifier, not a reversible encoding of the integer Id.
  `RequestSubject`/`Faq`/`AdminCloseRequest` still expose raw integer Ids — a pre-existing,
  intentionally out-of-scope inconsistency, not a regression.

### Secrets Management
- The ignored root `.env` is the single local runtime configuration file for .NET, Next.js, and
  Docker Compose. `.env.example` lists every required key; no real secret is committed.
- `scripts/with-env.sh` exports root `.env` for local .NET/Next.js commands. Compose is run with
  `docker compose --env-file .env`; both compose files fail fast when database, JWT, or SMTP
  values are missing.
- `appsettings.json` has no connection string or SMTP credential fallback. The API validates its
  JWT key and SMTP options at startup instead of accepting weak defaults.
- Root `.env` is excluded from git via `.gitignore`.
- Dependabot alerts for vulnerable dependencies
- **Seed/demo account passwords are public and must be rotated before any real deployment.**
  `AdminAccountSeed.cs` and `SampleUserSeed.cs` seed fixed demo accounts (e.g.
  `admin@helpcenter.com` / `Admin123!`) whose bcrypt hashes are committed to this repository. These
  are intentionally well-known CV/demo bootstrap data, not a production bootstrap mechanism. They
  must **never** be treated as usable secrets, and every seeded account's password must be changed
  (or the seed disabled/removed entirely) before deploying this app anywhere beyond a local/demo
  environment.

### Logging
- Serilog structured logging
- No PII in production logs
- Correlation ID tracking across requests
- Safe error messages (no secrets in logs)

## Dependency Management

- Weekly Dependabot scans (NuGet + npm + GitHub Actions)
- CI fails the CV/demo release gate for high or critical findings in runtime npm dependencies via
  `npm audit --omit=dev --audit-level=high`. It also reports the full npm audit result, including
  development and transitive tooling findings, without treating those findings alone as a release
  blocker. NuGet vulnerability scanning includes transitive packages and remains blocking.
