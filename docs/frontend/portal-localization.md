# BaseFAQ Portal Localization

## Purpose

This document explains how `apps/portal` resolves language, applies RTL/LTR, and keeps Portal UI translations owned by the frontend.

## Language source of truth

The Portal language is resolved in this order:

1. `User.Language` from the Tenant Portal profile endpoint, when available for the authenticated user
2. the locally stored Portal language in `localStorage` under `basefaq.portal.language`
3. browser language
4. English (`en-US`)

On unauthenticated routes such as `/login`, there is no profile lookup, so the login experience resolves language from local storage first, then the browser language, then English.

The frontend never depends on backend-translated labels. Backend APIs continue returning domain data, while UI copy and fallback error messages are translated in the Portal frontend.

## User profile contract

The Tenant Portal user profile now exposes:

- `language`
- `timeZone`

`language` is a nullable string that stores a locale code such as `en-US`, `pt-BR`, or `ar-SA`.

## Supported Portal language options

The language picker is defined in:

- `apps/portal/src/shared/lib/language.ts`

That file contains the 20 built-in language options used by the Portal UI, with:

- `code`
- `label`
- `direction`

## RTL / LTR behavior

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

When adding new Portal UI copy:

1. add the key to `en-US.json`
2. keep the other 19 locale JSON files aligned with the same key set
3. route the UI copy through the Portal translation helpers
4. avoid moving translated presentation strings into backend DTOs

## Persistence note

The tenant database migration for `User.Language` already exists in `BaseFaq.Common.EntityFramework.Tenant/Migrations/20260408200842_UserLanguageAdded.cs`. Frontend localization work should therefore treat the nullable profile language field as an active backend contract, not as a pending schema change.
