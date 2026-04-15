---
name: implement-portal-localization
description: Maintain BaseFAQ Portal localization, language precedence, and RTL/LTR behavior as a frontend-owned capability.
category: frontend
priority: high
triggers:
  - localization
  - translate copy
  - rtl
  - language selector
owned_paths:
  - apps/portal/src/shared/lib/i18n*
  - apps/portal/src/shared/lib/language.ts
  - apps/portal/src/domains/*
collaborates_with:
  - build-portal-domain-data-flow
  - design-confirmed-actions-and-stateful-feedback
---

# Implement Portal Localization

## When to Use

- A feature introduces new user-facing copy.
- The Portal needs a new locale, translation key, or direction-aware behavior.

## Responsibilities

- Keep translation catalogs aligned.
- Route all UI copy through frontend localization helpers.
- Preserve language precedence and document direction behavior.

## Workflow

1. Add the English key first and mirror it across supported locale files.
2. Replace hardcoded strings with translation keys.
3. Prefer interpolation-based copy over string concatenation.
4. Preserve the Portal language precedence order: profile, local storage, browser language, then English.
5. Validate both authenticated and unauthenticated routes.
6. Confirm `lang` and `dir` update at the document level.

## BaseFAQ Domain Alignment

- Localization is frontend-owned; backend DTOs do not ship translated labels.
- Authenticated and unauthenticated paths resolve language differently.
- RTL support is part of the product baseline, not an afterthought.

## Collaborates With

- [`build-portal-domain-data-flow`](../build-portal-domain-data-flow/SKILL.md)
- [`design-confirmed-actions-and-stateful-feedback`](../design-confirmed-actions-and-stateful-feedback/SKILL.md)

## Done When

- All new copy is keyed and translatable.
- Document `lang` and `dir` are correct.
- The feature behaves correctly in both LTR and RTL contexts.
