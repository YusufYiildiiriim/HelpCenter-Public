# API contracts

[English](README.md) · [Türkçe](README.tr.md)

`openapi/helpcenter.v1.json` is a narrow, repository-stored OpenAPI snapshot of the endpoints
adopted by the frontend. Because Swagger is disabled in production, it does not depend on a
running production or CI API.

`npm run api:types` regenerates `schema.d.ts`; `npm run api:types:check` verifies that the
artifact remains in sync with the snapshot. When a new service adopts generated types, add only
that endpoint's request/response schema to the snapshot; do not assume the file covers the entire
API surface. Backend validation remains authoritative.
