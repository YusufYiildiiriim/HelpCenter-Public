# HelpCenter localization inventory (2026-10-31)

## Source-code inventory

| Area | Current contract | Localization state |
| --- | --- | --- |
| Public portal | `/{tr,en}` routes; public project/module/FAQ/guide APIs | Routes, portal chrome, FAQ and guide body/text support `tr-TR` and `en-US`. Project/module labels still use legacy single-language fields. |
| Admin and customer routes | Existing `/admin` and `/dashboard` route shapes | Language selector and account preference are wired. Most page copy, form validation, status/subject labels and ARIA text remain Turkish-only. |
| Account/company preference | `Account.PreferredLocale`, `Company.DefaultLocale`, `Accept-Language` | Preference endpoints and company setting exist. Public FAQ/guide response includes requested and content locale. |
| FAQ and guide editing | Legacy `Title`/`Description` fields plus per-locale records | Source locale and manual translations are supported. Guide attachments have locale-specific records. |
| Request conversation | User-authored message body and attachments | Original text is retained as authored; request subjects/status display labels are not translated yet. |
| Email | In-memory queue and SMTP templates | Password reset, new request, and expert assignment templates use recipient account preference (unset defaults to Turkish). |
| API errors | `ApiResponse.Error.Code` stable codes | Known business codes map to Turkish/English client messages. Fluent validation detail strings are not yet individually translated. |

## Existing content data inventory

The legacy single-language fields that need classification are:

- `FAQs.Title` / `FAQs.Description`
- `Guides.Title` / `Guides.Description` and existing `GuideDocuments`
- `Projects.Name` / `Projects.Description`
- `Modules.Name` / `Modules.Description`
- `RequestSubjects.Name` / `RequestSubjects.Description`
- `CustomerRequestStatuses.Name` / `CustomerRequestStatuses.Description`
- `MenuItems.Label` / `MenuItems.GroupTitle`

No live or local PostgreSQL content inventory was run in this change. The API is not running on `localhost:5005`, and Docker/PostgreSQL test infrastructure is unavailable in this environment. Existing text language therefore remains unclassified. The FAQ/guide migration adds nullable source-locale fields and translation tables only; it contains no backfill and has not been applied to a database. Do not assign a source locale or backfill translations until a non-production database inventory has been exported and reviewed.

## Remaining implementation and acceptance work

1. Add locale-backed labels and admin editors for project/module names, request subjects, statuses, and menu labels.
2. Complete static UI translation for admin/customer pages, validation details, plural forms, and ARIA text; verify catalog key parity.
3. Extend public locale negotiation and fallback/content-locale reporting to project and module metadata.
4. Export existing content and attachments from a disposable non-production PostgreSQL database; classify each record, review the inventory, then prepare a deliberate backfill migration.
5. Add PostgreSQL tests for migration, uniqueness, source classification, fallback order, permissions, and locale-specific attachments.
6. Run complete Playwright acceptance with API + disposable PostgreSQL + test email outbox, including admin CRUD, account/company preferences, mobile, SEO, `html lang`, network and console checks.

## Validation completed so far

- Backend solution build: 0 warnings, 0 errors.
- Application unit suite: 317/317 passed.
- Frontend type-check passed; selected locale tests passed.
- Playwright MCP opened `/en`, verified `html lang="en-US"`, canonical/hreflang metadata, and confirmed an explicit `/en` route wins over an earlier Turkish cookie.
- Full CRUD/email-outbox browser acceptance was not possible because the local API/database are unavailable; the browser reported connection/CORS errors for `localhost:5005`.
