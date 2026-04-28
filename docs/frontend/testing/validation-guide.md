# Frontend Validation Guide

## Purpose

This document defines how the Portal frontend is currently validated and what must be checked before merging frontend changes.

## Current state

The repository does not currently contain a dedicated frontend automated test suite such as Vitest, Playwright, or Cypress.

Today the required validation gates are:

- `npm run lint`
- `npm run build`
- targeted manual regression checks in the browser

That means frontend quality depends on disciplined manual verification, not on pretending automated coverage already exists.

## Required validation commands

```bash
cd apps/portal
npm run lint
npm run build
```

Use `npm run dev` or `npm run dev:polling` for the actual browser verification pass.

## Manual regression matrix

### Auth and session changes

Verify:

- login redirect succeeds
- logout returns to the expected route
- protected routes behave correctly when unauthenticated
- authenticated routes render with the correct tenant context

### Domain data-flow changes

Verify:

- list pages show loading, empty, success, and error states correctly
- detail pages load the expected record and related sections
- mutations revalidate the affected data
- API failures surface user-facing feedback instead of silent breakage

### Destructive or long-running actions

Verify:

- `ConfirmAction` appears where required
- pending state is visible
- success and error feedback are explicit
- irreversible actions are not triggered accidentally

### Localization changes

Verify:

- the new key exists in `en-US`
- all locale files remain aligned
- language resolution still works on authenticated and unauthenticated routes
- `lang` and `dir` update correctly for RTL and LTR locales

### Layout and shared UI changes

Verify:

- the page still uses the intended shared layout
- responsive behavior works on narrow and wide viewports
- no affected route creates horizontal page overflow at 320, 360, 375, 414, 768, 1024, 1279, 1280, and desktop widths
- tablet widths below `xl` use the header and drawer shell, not the fixed desktop sidebar
- list pages use stacked card rows below `xl` and table layout from `xl` upward unless the page intentionally owns a different responsive pattern
- root, shell, page, cards, filters, tables, dialogs, sheets, popovers, and flex children can shrink with `min-w-0` or equivalent constraints
- long unbroken content such as URLs, external ids, checksums, user agents, and generated tokens wraps inside cards and cells
- skeletons, empty states, and error states still fit the surrounding layout
- focus flow and keyboard activation remain intact for dialogs, buttons, and confirms
- light and dark themes both use readable contrast and token-based surfaces
- `ActionPanel` and `ActionButton` surfaces keep the square, lightly rounded action identity
- relationship sections behave as local tabs, not anchors or global-list redirects
- child lists with more than five items use the local 5, 10, and 20 item pagination pattern
- API-backed relationship selects use `SearchSelect` or `SearchSelectField`

### Responsive list regression check

Run this check whenever a change touches the Portal shell, `ListLayout`, `DataTable`, `Card`, `SearchSelect`, pagination, top-level list pages, or shared filter/action surfaces.

Required routes:

- `/app/questions`
- `/app/sources`
- `/app/tags`
- `/app/activity`

Required viewport widths:

- `320`
- `360`
- `375`
- `414`
- `768`
- `1024`
- `1279`
- `1280`
- at least one wider desktop size, such as `1440`

For each route and width:

- verify `document.documentElement.scrollWidth` and `document.body.scrollWidth` do not exceed the viewport, except for an explicitly local scroll container
- verify visible elements do not extend beyond the viewport bounds
- verify the mobile/tablet shell is active below `1280px`
- verify the desktop sidebar and table are active at `1280px` and wider
- use representative data with long unbroken strings so URLs, ids, checksums, and user agents cannot hide wrapping defects

## Change-driven checklists

### If you changed a domain page

- the domain route renders
- data loads through the expected hook or API path
- empty and error states are explicit
- navigation back to list and detail flows still works
- relationship tabs stay scoped to the current record
- destructive actions require confirmation
- right-rail actions, metadata, workflow rules, and timing summaries remain readable in dark mode
- top-level list pages were checked across the responsive list regression matrix when their list, filters, toolbar, pagination, or row content changed

### If you changed shared UI primitives

- at least one representative consumer page was manually checked
- no layout regression was introduced in list, detail, and settings contexts
- lint and build still pass
- light and dark theme behavior was checked on every affected primitive
- responsive list consumers were checked at mobile, tablet, and desktop widths when the primitive affects width, wrapping, popovers, tables, cards, dialogs, sheets, filters, or actions

### If you changed localization logic

- login flow respects the unauthenticated resolution order
- authenticated flow respects profile language when present
- toolbar and profile selectors still update language consistently

## When to add automation

Automation should be introduced only when a repeated regression pattern justifies it.

Good candidates:

- auth redirects and protected route handling
- localization and direction switching
- critical list, detail, and mutation flows

Until then, treat the manual checklist as required engineering work, not as optional polish.

## Frontend validation checklist

- `npm run lint` passes
- `npm run build` passes
- affected browser flows were exercised manually
- affected shell, list, shared UI, and filter changes were checked against the responsive list regression matrix
- loading, empty, error, and confirmation states were checked
- localization was checked when user-facing copy changed
- documentation was updated if the runtime or validation workflow changed

## Related documents

- [`../architecture/portal-app.md`](../architecture/portal-app.md)
- [`../tools/portal-runtime.md`](../tools/portal-runtime.md)
- [`../../backend/testing/integration-testing-strategy.md`](../../backend/testing/integration-testing-strategy.md)
