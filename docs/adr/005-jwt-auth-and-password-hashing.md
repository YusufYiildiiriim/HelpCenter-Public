# ADR-005 — JWT bearer auth, BCrypt today with Argon2id migration path

**Status:** Accepted
**Date:** 2026-08-09
**Deciders:** Yusuf Yıldırım

## Context

Single first-party SPA (`help-center-ui`) + one API. No third-party OAuth clients. We need:

- Stateless auth that scales horizontally.
- Password hashing that resists offline cracking on stolen dumps.
- A clear story for token expiry and revocation.

## Decision

### Token format

- **JWT bearer**, HS256 today (single-signer, symmetric key held in `JwtSettings:Secret` via `IOptions<JwtOptions>`, validated on start).
- Claims: `UserId`, `Email`, `Role`, standard `exp`, `iat`, `iss`, `aud`.
- Validated by `Microsoft.AspNetCore.Authentication.JwtBearer`; `ClockSkew` set to `TimeSpan.Zero` (default 5-minute grace is wider than we want).
- Access token lifetime: **15 minutes** (short).

### Refresh strategy (implemented — see amendment below)

- Refresh tokens stored **hashed** (`SHA-256`) in a `RefreshTokens` table with `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`, `ReplacedByTokenHash`, `CreatedByIp`, `UserAgent`.
- **Rotation on every use**: each refresh returns a new refresh token, marks the previous as `Rotated`.
- **Reuse detection**: if a previously-rotated refresh token is presented again, all descendants in the chain are revoked (assume theft).

### Password hashing

- **Today:** `BCrypt.Net-Next` with work factor 12.
- **Target:** `Argon2id` (`Konscious.Security.Cryptography.Argon2`) with `MemorySize=64MB, Iterations=3, Parallelism=4`.
- **Migration:** opportunistic rehash. On successful login, if the stored hash is BCrypt, re-hash the plaintext with Argon2id and update the row.

### Token storage on the client

- Access token issued as an **HttpOnly, Secure, SameSite=Strict** cookie (short path scope).
- Refresh token cookie scoped to `/api/auth`.
- `localStorage` is not used for auth material. XSS-stolen tokens are the #1 real-world compromise vector; HttpOnly cookies eliminate the direct-JS-access path.

## Consequences

**Positive**

- Refresh reuse detection catches token theft that a plain rotation scheme would miss.
- Argon2id makes offline cracking infeasible at commodity GPU costs.
- HttpOnly cookies remove the `document.cookie` exfiltration path for XSS.

**Negative**

- Cookie-based auth invites CSRF as a concern. Addressed by SameSite=Strict for auth cookies + antiforgery token on state-changing endpoints.
- Argon2 is slower per hash than BCrypt at same security level → login endpoint has a hard rate limit (see rate-limiting middleware).

**Neutral**

- Symmetric HS256 is fine while we own the single audience. If a second first-party service ever needs to verify tokens without the signing key, migrate to **RS256** (asymmetric) with a JWKS endpoint.

## Related

- [SECURITY.md](../../SECURITY.md)
- Roadmap detail (archived): [2.1 Authentication](../history/roadmap/2.1-authentication.md)

## Amendment (2026-09-01) — implementation status vs. this ADR

Two parts of this ADR describe a future state that has since landed, and one describes a target
that was never actually built. Recorded here rather than editing the Decision above, per this
project's "ADRs are never edited after acceptance" rule.

**Implemented as designed:**

- **Refresh tokens** — `AuthTokenService` (`HelpCenter.Infrastructure/Services/Jwt/AuthTokenService.cs`)
  stores refresh tokens hashed with SHA-256, rotates on every use, and detects reuse by revoking
  the full chain for that user when an already-rotated token is presented again. This matches the
  ADR's design.
- **Password hashing** — `PasswordService` (`HelpCenter.Infrastructure/Services/PasswordService.cs`)
  hashes all new/changed passwords with **Argon2id** using exactly the OWASP-2024 parameters this
  ADR specifies (memory 64 MiB, iterations 3, parallelism 4), and can still `Verify()` legacy
  BCrypt hashes. Argon2id is the *current* default, not a future target.

**Not implemented — do not assume this from the ADR text above:**

- **Opportunistic rehash on login.** `IPasswordService.NeedsRehash()` exists and correctly flags
  BCrypt hashes (and Argon2id hashes with stale parameters) as needing a rehash, but **no login
  handler calls it**. A user whose password was hashed with BCrypt before the Argon2id migration
  keeps a BCrypt hash indefinitely; it is never opportunistically upgraded. This is a real gap
  between design and code, not a documentation lag.
- **HttpOnly cookie token storage.** The backend never sets a `Set-Cookie` header for the access
  token — it returns the token in the JSON response body. The frontend (`AuthService`) stores it
  itself via `js-cookie` (`Secure`, `SameSite=Strict`, **not `HttpOnly`** — a client-set cookie
  cannot be) and mirrors it into `sessionStorage`. This means the token *is* reachable from
  JavaScript, including an XSS payload — the exact risk this ADR's cookie design was meant to
  close. See `SECURITY.md` for the accepted-limitation writeup.

## Amendment (2026-09-10) — browser token storage implementation completed

The preceding September 1 status note is historical. The token-storage gap was closed in the
subsequent session-security change:

- The short-lived access token is now kept only in frontend module memory (`authSession`), never
  in `localStorage`, `sessionStorage`, or a JavaScript-managed cookie.
- The API sets the rotating `refresh_token` as an `HttpOnly`, `Secure`, `SameSite=Lax` cookie.
  Browser JavaScript cannot read it; refresh and logout send it as a request cookie.
- Login and refresh DTOs suppress the raw refresh token from JSON serialization. A reload obtains
  a new access token through the refresh flow.

The opportunistic BCrypt-to-Argon2id rehash gap described above remains open.
