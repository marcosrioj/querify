# Querify Portal Localization

## Purpose

This document explains how `apps/portal` resolves language, applies RTL or LTR, and keeps Portal UI translations owned by the frontend.

## Language source of truth

The Portal language is resolved in this order:

1. `User.Language` from the Tenant Portal profile endpoint, when available for the authenticated user
2. the locally stored Portal language in `localStorage` under `querify.portal.language`
3. browser language
4. English `en-US`

On unauthenticated routes such as `/login`, there is no profile lookup, so the login experience resolves language from local storage first, then the browser language, then English.

The frontend never depends on backend-translated labels. Backend APIs continue returning domain data, while UI copy and fallback error messages are translated in the Portal frontend.

## User profile contract

The Tenant Portal user profile now exposes:

- `language`
- `timeZone`

`language` is a nullable string that stores a locale code such as `en-US`, `pt-BR`, or `ar-SA`.
`timeZone` is a nullable string that stores an IANA timezone id such as `UTC` or
`America/Vancouver`.

## Timezone source of truth

Portal timestamp display resolves timezone in this order:

1. `User.TimeZone` from the Tenant Portal profile endpoint, when present and supported by the browser
2. `UTC`

The browser timezone is not a fallback for timestamp display. It is only a browser-provided IANA
timezone value that the user can explicitly save to their profile. Components must pass the resolved
Portal timezone into `Intl.DateTimeFormat` instead of letting browser locale APIs choose a timezone
implicitly.

Implementation references:

- `apps/portal/src/shared/lib/time-zone.ts`
- `apps/portal/src/domains/settings/settings-hooks.ts`
- `apps/portal/src/domains/settings/profile-settings-page.tsx`

## Supported Portal language options

The language picker is defined in:

- `apps/portal/src/shared/lib/language.ts`

That file contains the 20 built-in language options used by the Portal UI, with:

- `code`
- `label`
- `direction`

## RTL and LTR behavior

`apps/portal/src/shared/lib/i18n-provider.tsx` applies:

- `document.documentElement.lang`
- `document.documentElement.dir`
- `document.body.dir`

Direction is derived from the selected locale metadata in `language.ts`.

## UI entry points

Language can be changed from:

- the login page header selector
- profile settings
- the toolbar language selector beside notifications

Those entry points do not all persist the same way:

- the login page selector updates only the frontend-owned stored language
- the toolbar selector updates the local language immediately and also writes the preference to the authenticated user profile
- profile settings update the authenticated user profile directly

## Translation ownership

Portal translations are frontend-owned:

- locale catalogs live in `apps/portal/src/shared/lib/i18n/locales/*.json`
- locale loading lives in `apps/portal/src/shared/lib/i18n/messages.ts`
- shared translation helpers live in `apps/portal/src/shared/lib/i18n-core.ts`
- React provider wiring lives in `apps/portal/src/shared/lib/i18n-provider.tsx`
- React consumer access lives in `apps/portal/src/shared/lib/use-portal-i18n.ts`
- API fallback messages and toast copy use the same frontend translation layer

`i18n-core.ts` also matches placeholder-based keys at runtime. Prefer keys like `Delete source "{name}"?` or `Search: {value}` instead of concatenating translated fragments in components.

## API error localization

Backend APIs return request-time failures through `ApiErrorException`, serialized as
`{ ErrorCode, MessageError, Data }`. The Portal treats `MessageError` as a canonical English
message key, not as final UI copy.

All API errors shown in popups, toasts, mutation errors, or page error states must use
`toErrorMessage(...)` from `src/platform/api/api-error.ts`. That path:

- accepts both PascalCase and camelCase API error fields
- maps dynamic messages with ids, tenant ids, client keys, or headers to stable translation keys
- collapses model validation payloads to `The submitted data is invalid.`
- sends the final message key through the same locale catalogs as the rest of the UI

When adding a new backend `ApiErrorException` message that can reach the Portal:

1. prefer an existing stable frontend-translatable message
2. add the exact message key to every locale file when the message is user-facing and stable
3. add a pattern in `api-error.ts` when the backend message contains dynamic ids or operational
   details that should not appear in translated UI
4. verify the message appears through the shared toast or error-state path instead of raw component
   rendering

When adding new Portal UI copy:

1. add the key to `en-US.json`
2. keep the other locale JSON files aligned with the same key set
3. route the UI copy through the Portal translation helpers
4. avoid moving translated presentation strings into backend DTOs

## Persistence note

The tenant database migration for `User.Language` already exists in `Querify.Common.EntityFramework.Tenant/Migrations/20260408200842_UserLanguageAdded.cs`. Frontend localization work should therefore treat the nullable profile language field as an active backend contract, not as a pending schema change.
