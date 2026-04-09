# Changelog

All notable changes to the BaseFAQ HTML Foundation integration are documented here.

## [0.1.0] — 2026-04-07

### Added

- Canonical HTML rendering foundation for BaseFAQ public content
- `FaqDetailDto` API response model mapped to semantic HTML5 (`details`/`summary`)
- CSS design token layer (`tokens.css`) with full `--bf-*` custom property system
- Structural styles (`faq.css`) with BEM naming under `bf-` namespace
- Light theme (`theme-light.css`) and neutral/dark theme (`theme-neutral.css`)
- `BaseFaqClient` TypeScript API client for `https://dev.faq.public.basefaq.com`
  - `GET /api/faqs/faq` — list with pagination, status filter, and includes
  - `GET /api/faqs/faq/{id}` — single FAQ by ID
  - `POST /api/faqs/feedback` — thumbs up / down with `UnLikeReason`
- `BaseFaqAccordion` progressive enhancement: single-open mode, animated height, deep-link
- `BaseFaqAnalytics` event surface: visibility, open/close, feedback, CTA, source clicks
- `buildFaqPageJsonLd` / `injectFaqPageJsonLd` — FAQPage JSON-LD builder (schema.org)
- i18n message files for English (`messages.en.json`) and Arabic (`messages.ar.json`)
- Public page example (`examples/public-page/index.html`) — full API integration with:
  - X-Client-Key configuration panel (supports `?key=` URL param)
  - Theme switcher (light / neutral / dark)
  - Skeleton loader, error state, empty state
  - Voting with reason modal
  - Pagination
  - JSON-LD injection
  - Deep-link via URL hash
- Embedded block example (`examples/embedded-block/index.html`)
- RTL example (`examples/rtl/index.html`) with Arabic UI strings
- Copy-paste snippet (`examples/copy-paste-snippet/snippet.html`) — zero-dependency, ~5 KB
