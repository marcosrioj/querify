# Changelog

All notable changes to the BaseFAQ HTML Foundation integration are documented here.

## [0.2.0] — 2026-04-09

### Added

- **Answer variants** (`FaqItemAnswer[]`) — each FAQ item now renders its `answers[]` list as
  a numbered variant list (shortAnswer headline + optional full HTML body), replacing the legacy
  single-answer fallback when variants are present
- **Vote system** — toggle-vote button per answer variant
  - `POST /api/faqs/vote { faqItemAnswerId }` — creates or removes vote (toggle)
  - Returns `Guid` (voted) or `Guid.Empty` (vote removed)
  - Visual `is-voted` state, optimistic score update, in-flight disabled state
  - `voteScore` displayed on each vote button (configurable)
- **Display configuration** — all sections are independently show/hide controllable:
  - `showTags`, `showFeedback`, `showFeedbackScore`, `showCta`, `showAdditionalInfo`
  - `showSources`, `showAiConfidence`, `showAnswerVariants`, `showVotes`, `showVoteScore`
  - In the public-page playground: live checkboxes in the config panel
  - In the copy-paste snippet: `data-show-*` attributes on the container div
- **AI confidence badge** — optional `aiConfidenceScore` display per FAQ item
  (off by default; enable with `showAiConfidence: true` / `data-show-ai-confidence="true"`)
- **Copy-paste snippet v2** — fully rewritten as a Google-Analytics-style embed:
  - Single IIFE script, auto-initialises all `[data-basefaq]` containers on the page
  - Full CSS injected into `<head>` once per page (scoped to `.bfq` namespace)
  - Dark / auto theme support via `data-theme` attribute
  - Schema mode `canonical` / `mirror` / `off` configurable per container
  - All features: answer variants, vote toggle, feedback modal, JSON-LD, accordion, toasts
- **Updated JSON-LD builder** — uses `answers[0]` (first active variant) as `acceptedAnswer.text`
  when answer variants are present; falls back to `item.shortAnswer` when absent
- **Analytics events** (new in `BaseFaqAnalytics`):
  - `bf:vote-add` — user voted for an answer variant
  - `bf:vote-remove` — user removed their vote (toggle off)

### Changed

- `renderFaqItem` — now accepts display `cfg` object; all optional sections are conditionally rendered
- Feedback score update respects `cfg.showFeedbackScore`
- `injectJsonLd` — updated to extract answer text from `answers[]` first

### Guaranteed minimum output

Each rendered `FaqItem` always shows at minimum:
- `question` — the item's question text
- `answers[0]` (or `shortAnswer`) — at least one answer per item is always visible

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
