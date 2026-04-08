# BaseFAQ Portal Localization

## Purpose

This document explains how `apps/portal` resolves language, applies RTL/LTR, and keeps Portal UI translations owned by the frontend.

## Language source of truth

The Portal language is resolved in this order:

1. `User.Language` from the Tenant Portal profile endpoint
2. browser language
3. English (`en-US`)

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

`apps/portal/src/shared/lib/i18n.tsx` applies:

- `document.documentElement.lang`
- `document.documentElement.dir`
- `document.body.dir`

Direction is derived from the selected locale metadata in `language.ts`.

## UI entry points

Language can be changed from:

- profile settings
- the toolbar language selector beside notifications

Both entry points persist through the same user profile update endpoint.

## Translation ownership

Portal translations are frontend-owned:

- locale catalogs live in `apps/portal/src/shared/lib/i18n/locales/*.json`
- locale loading lives in `apps/portal/src/shared/lib/i18n/messages.ts`
- shared translation helpers live in `apps/portal/src/shared/lib/i18n-core.ts`
- React access lives in `apps/portal/src/shared/lib/i18n.tsx`
- API fallback messages and toast copy use the same frontend translation layer

When adding new Portal UI copy:

1. add the key to `en-US.json`
2. keep the other 19 locale JSON files aligned with the same key set
3. route the UI copy through the Portal translation helpers
4. avoid moving translated presentation strings into backend DTOs

## Implementation note

This change intentionally does not include the EF migration. Add the migration separately when promoting the `User.Language` column to the database.
