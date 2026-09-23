# ADR-008 — Frontend foundation: visual surfaces, data and UI boundaries

**Status:** Accepted
**Date:** 2026-09-12
**Deciders:** Yusuf Yıldırım

## Context

The frontend has grown around route-local components and service modules, which is a useful
starting point for an App Router application. Its next changes need shared defaults so that new
work does not introduce another theme, modal implementation, server-state pattern, or hand-written
copy of an API contract.

The public support portal and the authenticated administration experience serve different jobs.
The portal benefits from a focused, high-contrast dark identity; the dense operational dashboard
benefits from a light working surface. Treating that difference as an accidental collection of
colour utilities would make a later token migration harder.

## Decision

### Visual identity

- The public portal is intentionally dark.
- The admin/dashboard is intentionally a light shell for now.
- This slice does not add an end-user theme toggle. A future product requirement may supersede this
  ADR with a token-backed multi-theme design.

### Application boundaries

- Keep App Router route entry points in `src/app`. Use `page.tsx`, `layout.tsx`, `loading.tsx`,
  `error.tsx`, and `not-found.tsx` only for their Next.js routing responsibilities.
- Co-locate a route's private components below its route segment in `components/`. Put reusable,
  route-agnostic UI in `src/components/`, cross-cutting client providers in `src/context/`, and
  transport code in `src/services/`.
- Use the `@/` import alias for code under `src/`; use relative imports only within the same small
  feature or route folder.

### Incremental standards

- TanStack Query is the standard for client-side server state. It will be introduced through a
  pilot rather than a repository-wide rewrite; local React state remains appropriate for ephemeral
  UI state.
- Accessible dialogs are standardized on the existing Radix Dialog primitive and its shared
  wrapper. New dialogs must not recreate modal semantics with generic `div` elements.
- Generated OpenAPI types are the source of truth for API contracts, adopted incrementally at
  touched service boundaries.
- React Hook Form plus Zod is the standard for new or substantially revised complex forms, also
  adopted incrementally. Small, local forms do not need a speculative migration.

## Consequences

**Positive**

- The two product surfaces have explicit intent, leaving a clear seam for semantic design tokens.
- New work has predictable homes and imports without reshaping existing routes.
- Server-state, dialogs, API types, and form validation can improve one feature at a time with
  limited regression risk.

**Negative**

- The codebase will temporarily contain legacy and standard patterns side by side.
- The eventual theme-token migration must replace hard-coded colour utilities across both surfaces.
- TanStack Query, generated contracts, and React Hook Form add dependencies and conventions that
  require focused tests and documentation when each is introduced.

**Neutral**

- Existing UI is not restyled or rewritten by this ADR; follow-up slices implement these standards.

## Related

- [Frontend README](../../help-center-ui/README.md#frontend-conventions)
- [Next.js App Router project structure](https://nextjs.org/docs/app/getting-started/project-structure)
