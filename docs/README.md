# HelpCenter — Documentation

[English](README.md) · [Türkçe](README.tr.md)

This folder contains the published project documentation. Repo-root [`README.md`](../README.md) is the overview and quick-start entry; this is the detailed reference.

| File | Purpose |
|---|---|
| [`architecture.md`](architecture.md) | Layer map, request lifecycle, RBAC model, deployment topology (Mermaid diagrams) |
| [`database-schema.md`](database-schema.md) | Historical SQL Server schema reference; active PostgreSQL schema is defined by the EF model and initial migration |
| [`tech-stack.md`](tech-stack.md) | Every package + why it's here + what was rejected |
| [`adr/`](adr/) | Architecture Decision Records (11 records; PostgreSQL runtime decision in ADR-011) |

Internal working docs (system architecture report, commit worklog, refactoring plans, archived
production-readiness roadmap) are tracked outside this repo, not published here.
