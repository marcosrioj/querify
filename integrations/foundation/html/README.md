# BaseFAQ HTML Foundation

Canonical HTML rendering integration for BaseFAQ public content.

This is the rendering baseline that all other integrations (React, Vue, WordPress, Shopify, etc.) inherit from. It defines the HTML semantics, CSS token system, structured data contract, and progressive enhancement strategy.

## Quick start

Open `examples/public-page/index.html` in a browser, enter your **X-Client-Key**, and click **Load FAQ**.

Or pass it via URL:

```
examples/public-page/index.html?key=YOUR_CLIENT_KEY
examples/public-page/index.html?key=YOUR_CLIENT_KEY&faqId=YOUR_FAQ_UUID
```

## API

All data is fetched from the BaseFAQ Public API:

```
Base URL: https://dev.faq.public.basefaq.com
Required header: X-Client-Key: <your public tenant key>
```

| Endpoint | Method | Description |
|---|---|---|
| `/api/faqs/faq` | GET | List FAQs (paginated, with status/tag filters) |
| `/api/faqs/faq/{id}` | GET | Get a single FAQ by UUID |
| `/api/faqs/vote` | POST | Submit a vote for a specific FAQ item answer |
| `/api/faqs/feedback` | POST | Submit a thumbs-up or thumbs-down feedback |

### Query parameters

**GET /api/faqs/faq/{id}**

| Param | Type | Default | Description |
|---|---|---|---|
| `includeFaqItems` | bool | false | Include FAQ items in response |
| `includeContentRefs` | bool | false | Include source/content references |
| `includeTags` | bool | false | Include tag values |

**GET /api/faqs/faq**

Same include params plus:

| Param | Type | Description |
|---|---|---|
| `status` | int | 0=Draft, 1=Published, 2=Archived |
| `maxResultCount` | int | Page size (default 10) |
| `skipCount` | int | Pagination offset |
| `sorting` | string | Sort field and direction |
| `searchText` | string | Text search filter |
| `faqIds` | guid[] | Filter to specific FAQ IDs |

### Vote payload

```json
{
  "faqItemAnswerId": "uuid"
}
```

### Feedback payload

```json
{
  "faqItemId": "uuid",
  "like": true,
  "unlikeReason": 1
}
```

`unlikeReason` values: `1` = Confusing/unclear, `2` = Not relevant, `3` = Relevant but not helpful, `4` = Length issue

## What is rendered

For each FAQ item (`FaqItemDto`), the HTML foundation renders:

| Field | Rendered as |
|---|---|
| `question` | `<summary>` disclosure trigger |
| `shortAnswer` | Top-ranked answer summary |
| `answer` | Top-ranked full answer body (pre-sanitized by API) |
| `answers[]` | Ordered answer variants available for vote-aware rendering |
| `additionalInfo` | Highlighted info block below the answer |
| `ctaTitle` + `ctaUrl` | Call-to-action button (when present on the item) |
| `contentRefs` | Source links with kind icons |
| `feedbackScore` | Live feedback score with up/down buttons |
| `tags` | Tag badges on the FAQ header |

## HTML structure

```html
<section
  class="bf-faq"
  data-basefaq-root
  data-basefaq-faq-id="{faqId}"
  data-basefaq-schema-mode="canonical"
  lang="en"
  dir="ltr"
>
  <header class="bf-faq__header">
    <h2 class="bf-faq__title">FAQ Title</h2>
    <div class="bf-faq__tags">…</div>
  </header>
  <div class="bf-faq__items" role="list">
    <details class="bf-faq__item" data-basefaq-item-id="{itemId}" role="listitem">
      <summary class="bf-faq__question">
        <span class="bf-faq__question-text">Question text</span>
        <svg class="bf-faq__icon">…chevron…</svg>
      </summary>
      <div class="bf-faq__answer">
        <div class="bf-faq__answer-body">…answer HTML…</div>
        <div class="bf-faq__additional-info">…optional…</div>
        <a class="bf-faq__cta">…optional CTA…</a>
        <div class="bf-faq__sources">…optional sources…</div>
        <div class="bf-faq__feedback">…feedback buttons…</div>
      </div>
    </details>
  </div>
</section>
```

## CSS

```
src/styles/
  tokens.css        — all --bf-* custom properties
  faq.css           — structural BEM styles
  theme-light.css   — light theme overrides
  theme-neutral.css — neutral/gray theme + dark theme via [data-bf-theme="dark"]
```

Apply a theme via class or data attribute:

```html
<section class="bf-faq bf-theme-light">…</section>
<section class="bf-faq" data-bf-theme="dark">…</section>
```

## Scripts

```
src/scripts/
  basefaq-client.ts  — typed API client (X-Client-Key auth)
  accordion.ts       — single-open mode, animated height, deep-link
  analytics.ts       — custom DOM events for host-page tracking
src/seo/
  faq-jsonld.ts      — FAQPage JSON-LD builder + injector
```

## Examples

| Example | Path | Description |
|---|---|---|
| Public page | `examples/public-page/index.html` | Full page with config panel, all features |
| Embedded block | `examples/embedded-block/index.html` | Scoped embed within a host page |
| RTL | `examples/rtl/index.html` | Arabic/RTL layout |
| Copy-paste snippet | `examples/copy-paste-snippet/snippet.html` | Zero-dependency ~5 KB inline embed |

## Data attributes

| Attribute | Element | Purpose |
|---|---|---|
| `data-basefaq-root` | `section.bf-faq` | Root marker for JS enhancement |
| `data-basefaq-faq-id` | `section.bf-faq` | FAQ UUID for JSON-LD dedup |
| `data-basefaq-item-id` | `details.bf-faq__item` | Item UUID for deep-link and feedback |
| `data-basefaq-schema-mode` | `section.bf-faq` | `canonical`, `mirror`, or `off` |
| `data-basefaq-render-mode` | `section.bf-faq` | `public-page`, `embedded-block`, etc. |

## Structured data

JSON-LD is injected as `<script type="application/ld+json" data-bf-faq-id="{id}">` in the document `<head>`. Re-renders replace the existing block for the same FAQ ID. Suppress with `data-basefaq-schema-mode="off"`.

## Accessibility

- Native `details`/`summary` keyboard behavior (Enter, Space, click)
- `aria-expanded` managed by the accordion enhancement
- `role="list"` / `role="listitem"` on items container
- `aria-label` on section, feedback group, and external links
- `aria-live="polite"` on loading states
- `aria-pressed` on feedback buttons
- Focus ring: `3px solid var(--bf-color-focus-ring)` with `outline-offset: 2px`
- `prefers-reduced-motion` respected by accordion animation

## RTL

Set `dir="rtl"` and `lang="ar"` (or appropriate locale) on the root `section`. CSS uses `border-inline-start` and `padding-inline-*` logical properties so layout mirrors automatically.

## Security

See `SECURITY.md`. Never use an admin/secret token as `X-Client-Key`.

## Versioning

See `VERSION.json` and `CHANGELOG.md`.
