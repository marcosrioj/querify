# BaseFAQ Integrations Architecture

## Purpose

This document defines the future `/integrations` scope for BaseFAQ. It is intentionally forward-looking and repository-oriented: it establishes the architectural boundaries, folder taxonomy, standards, release discipline, and platform-by-platform design that should guide later scaffolding and implementation.

The scope of this document is limited to external delivery channels and ecosystem integrations that consume BaseFAQ publicly. It does not redesign BaseFAQ internal domain services, databases, or internal workspace applications beyond the minimum context required to define integration contracts.

## 1. Executive Summary

`/integrations` should be the single architectural umbrella for every external-facing BaseFAQ delivery mechanism:

- canonical HTML rendering
- embeddable widgets and script loaders
- framework adapters and component libraries
- CMS plugins and extensions
- e-commerce storefront integrations
- language SDKs and server-side helpers
- static-site and documentation-site adapters
- low-code and copy-paste installation channels

This centralization matters because BaseFAQ will be consumed through many channels that all need to preserve the same guarantees:

- correct FAQ semantics
- SEO-safe rendering
- FAQPage JSON-LD behavior
- accessibility defaults
- tenant-safe public consumption
- versioned compatibility with BaseFAQ public APIs
- consistent documentation and release quality

Without a unified `/integrations` scope, the repository will drift into disconnected one-off plugins, duplicated schema builders, inconsistent rendering contracts, and incompatible release practices. With a unified scope, BaseFAQ can treat external rendering as a disciplined product surface instead of a loose collection of adapters.

The design recommendation in this document is category-first at the top of `/integrations`, with platform-first naming underneath each category. That gives BaseFAQ a scalable information architecture while keeping each platform folder named after the ecosystem it serves.

## 2. Architectural Principles for `/integrations`

The `/integrations` area should follow these non-negotiable principles.

### 2.1 Pure HTML is the canonical rendering foundation

Every rendering integration should ultimately map back to the same canonical FAQ render contract and the same HTML semantics. Framework wrappers are conveniences, not the source of truth.

### 2.2 Public rendering contracts come before platform implementation details

Each integration should consume a normalized BaseFAQ public render model rather than reaching directly into internal service details. Platform code should adapt the contract, not redefine it.

### 2.3 SEO is first-class

FAQ integrations exist to render discoverable public content. Server-rendered markup, crawlable content, canonical URL discipline, and structured data support are product requirements, not optional enhancements.

### 2.4 FAQPage JSON-LD is mandatory where applicable

Every integration that renders indexable FAQ content should support FAQPage JSON-LD emission or intentionally document why it is suppressed. Structured data ownership must be explicit to avoid duplicate schema.

### 2.5 Accessibility is the default, not a theme option

Accordion semantics, heading structure, keyboard behavior, focus management, color contrast, and reduced-motion behavior should be built into the base rendering model.

### 2.6 Performance budgets apply to every integration

The canonical HTML path should work without JavaScript. JavaScript should only enhance behavior. Bundles, CSS, hydration, and API round-trips should be budgeted per platform and documented per integration.

### 2.7 Public-consumption safety and tenant isolation are core constraints

No browser-delivered integration may require tenant admin secrets. Embed and SDK flows should use tenant-safe public credentials, signed public configuration, or server-side mediation where needed.

### 2.8 Versioning is explicit and integration-specific

A WordPress plugin, React package, CDN embed loader, and .NET SDK do not evolve at the same cadence. Versioning should be independently managed while staying tied to a shared BaseFAQ public contract policy.

### 2.9 Composition beats duplication

JSON-LD builders, normalized render contracts, shared fixtures, schema validators, release templates, and compatibility manifests should be shared within `/integrations` to reduce drift.

### 2.10 Platform conventions must be respected

BaseFAQ standards should not force unnatural integration patterns. WordPress should look like a WordPress plugin, Shopify should look like a Shopify app, Nuxt should look like a Nuxt module, and .NET should look like a NuGet package.

### 2.11 Documentation is part of the product surface

Every integration should carry local documentation, examples, compatibility data, security notes, and migration guidance. Central docs should aggregate and cross-link them.

### 2.12 Rendering ownership must be explicit

For any page that includes BaseFAQ content, exactly one layer should own:

- canonical HTML output
- JSON-LD emission
- cache invalidation responsibility
- theming boundary
- telemetry ownership

### 2.13 Embed is the universal fallback channel

If BaseFAQ does not yet have a deep native integration for a platform, the supported fallback should be the canonical embed/web delivery stack, not an undocumented custom snippet.

### 2.14 Headless adapters should prioritize contracts over heavy coupling

For headless CMS and low-code platforms, BaseFAQ should prefer stable fetch/render contracts and deployment guidance over prematurely building deep bespoke apps that are expensive to maintain.

## 3. Integration Taxonomy

BaseFAQ should organize integrations into clear categories because different integration types solve different delivery problems.

### A. Canonical foundation

Scope:

- pure HTML renderer
- design tokens and CSS baseline
- structured data helpers
- canonical render contracts

Why it exists:

This is the source of truth for FAQ rendering semantics. Every other integration should either reuse it directly or map to it.

How it differs:

It is not a platform wrapper. It defines the rendering contract and baseline output that other channels inherit.

### B. Embeddable web delivery

Scope:

- vanilla JavaScript loader
- copy-paste snippet
- CDN bundle
- iframe fallback renderer
- Web Component variant

Why it exists:

This is the lowest-friction install path for arbitrary websites and the universal fallback for platforms without deeper native integrations.

How it differs:

It optimizes for host-page installation simplicity, cross-site delivery, CSS isolation, and runtime configurability.

### C. Frontend frameworks

Scope:

- React
- Next.js
- Vue
- Nuxt
- Angular
- Svelte
- SvelteKit
- Astro
- Remix
- Qwik if justified later

Why it exists:

These integrations give application developers native building blocks that fit the idioms of their framework instead of forcing generic script embeds.

How it differs:

The framework package owns developer ergonomics, SSR/CSR patterns, composables/hooks, and framework metadata integration.

### D. CMS platforms

Scope:

- WordPress
- Drupal
- Joomla
- headless CMS adapters for Contentful, Strapi, Sanity, and Hygraph

Why it exists:

CMS users need authoring-time installation, admin settings, cache behavior, theme compatibility, and SEO coexistence with site plugins.

How it differs:

CMS integrations live inside another product’s extension model and must respect that product’s packaging, permissions, and templating conventions.

### E. E-commerce platforms

Scope:

- WooCommerce
- Shopify
- Magento / Adobe Commerce
- BigCommerce later if justified

Why it exists:

Storefront FAQ delivery has unique constraints around product pages, category templates, theme performance, and coexistence with commerce schemas.

How it differs:

These integrations center on storefront rendering, merchandising templates, product/category associations, and transaction-sensitive performance concerns.

### F. Language and server-side SDKs

Scope:

- JavaScript / TypeScript
- Node server SDK
- PHP
- .NET
- Python

Why it exists:

These packages give developers direct programmatic access to BaseFAQ public APIs, typed DTOs, resilience policies, and server-side rendering helpers.

How it differs:

SDKs are not platform plugins. They are code-facing interfaces and transport abstractions.

### G. Static site generators and content frameworks

Scope:

- Eleventy
- Hugo
- Jekyll
- Docusaurus
- Gatsby
- MkDocs optionally

Why it exists:

These channels favor build-time fetching and pre-rendered SEO output, which is highly aligned with FAQ content.

How it differs:

They optimize for build pipelines, cache freshness, and static publishing rather than live runtime rendering.

### H. API-first and headless adapters

Scope:

- OpenAPI-generated client patterns
- SSR template examples
- middleware adapters
- edge runtime adapters

Why it exists:

Some consumers do not want an opinionated UI package. They want transport contracts, server templates, or edge middleware examples.

How it differs:

These assets are integration primitives for teams building their own renderers and deployments.

### I. Low-code / no-code / embedded channels

Scope:

- Webflow
- Wix
- GTM snippet compatibility
- generic HTML snippets
- copy-paste installation kits

Why it exists:

These platforms often do not justify deep native integrations early, but they still represent strong acquisition channels.

How it differs:

They are install-pattern integrations rather than full code packages, and they should stay close to the embed foundation.

## 4. Recommended Top-Level `/integrations` Folder Structure

### Recommendation

Use a category-first structure at the top level of `/integrations`, with platform-first names beneath each category.

### Why category-first is the better choice

Category-first is the better long-term monorepo design because:

- it prevents a flat root with dozens of unrelated platform folders
- it keeps shared governance visible: `foundation`, `embed`, `frameworks`, `cms`, `commerce`, `sdks`, `static`, `headless`, `lowcode`
- it makes ownership and rollout planning clearer
- it avoids confusion between a platform integration and a shared library
- it supports different packaging systems within a single monorepo cleanly

Platform-first at the root would be acceptable for a smaller repository, but BaseFAQ’s intended breadth makes category-first more maintainable.

### Recommended top-level structure

```text
/integrations
  /docs
  /foundation
  /embed
  /frameworks
  /cms
  /commerce
  /sdks
  /static
  /headless
  /lowcode
  /shared
  /templates
  /tooling
```

### Category overview

| Folder | Responsibility |
|---|---|
| `docs` | central docs, standards, generated indexes, release playbooks |
| `foundation` | canonical HTML, baseline CSS tokens, schema helpers |
| `embed` | universal embed delivery: script loader, CDN bundle, iframe fallback, Web Component |
| `frameworks` | developer-facing packages for frontend frameworks |
| `cms` | plugins, modules, and adapters for CMS ecosystems |
| `commerce` | commerce-platform specific storefront integrations |
| `sdks` | language and server-side packages |
| `static` | static-site and documentation-site integrations |
| `headless` | OpenAPI, SSR templates, middleware, and edge examples |
| `lowcode` | low-code and embed-first installation channels |
| `shared` | shared contracts, fixtures, SEO utilities, themes, and test harnesses |
| `templates` | integration scaffolds, docs templates, release templates |
| `tooling` | build, release, validation, package graph, compatibility automation |

## 5. Canonical Naming Rules for All Integration Folders

### 5.1 Top-level rules

- Use category names only for the first directory under `/integrations`.
- Use platform or ecosystem names for the next directory.
- Do not use generic artifact names like `plugin`, `package`, `client`, or `sdk` as platform folder names.
- Avoid abbreviations unless they are the official ecosystem name.

Examples:

- use `frameworks/nextjs`, not `frameworks/next`
- use `sdks/dotnet`, not `sdks/csharp`
- use `cms/wordpress`, not `cms/wp`
- use `commerce/woocommerce`, not `commerce/wc`
- use `embed/web`, not `embed/widget`

### 5.2 Naming rules for specific ecosystems

- Prefer `nextjs` over `next`
- Prefer `dotnet` over `csharp`
- Prefer `wordpress` over `wp`
- Prefer `woocommerce` as its own platform folder, not a nested alias inside `wordpress`
- Prefer `webflow` and `wix` as low-code platform names
- Prefer `javascript` for the browser/isomorphic SDK and `node` for server-only additions
- Prefer `sveltekit` over `svelte-kit`
- Prefer `magento` for the directory, with Adobe Commerce discussed in docs and compatibility metadata

### 5.3 Source folder naming

- Use `src` for authored source code.
- Use `generated` only for committed generated assets that are meant to be reviewed and versioned.
- Use `dist` only for build outputs and do not commit it unless a marketplace requires committed artifacts.
- Use `scripts` for local integration-specific build or release helpers.
- Use `fixtures` for local static test/example inputs.

### 5.4 Documentation folder naming

- Use `docs` inside every integration.
- Keep filenames task-oriented and stable:
  - `getting-started.md`
  - `installation.md`
  - `configuration.md`
  - `rendering-modes.md`
  - `seo.md`
  - `structured-data.md`
  - `troubleshooting.md`
  - `upgrade-guide.md`
  - `migration.md`
  - `developer-faq.md`

### 5.5 Examples and tests

- Use `examples` for runnable or copy-paste sample projects.
- Use `tests` for automated tests.
- Use subfolders by test tier only when needed:
  - `tests/unit`
  - `tests/integration`
  - `tests/e2e`
  - `tests/accessibility`
  - `tests/structured-data`
  - `tests/visual`

### 5.6 Release and versioning artifacts

- Use `VERSION.json` for the machine-readable version manifest.
- Use `COMPATIBILITY.md` for the human-readable support matrix.
- Use `CHANGELOG.md` for release history.
- Use `SECURITY.md` for security notes and reporting policy.
- Use `.release` for committed release configuration and notes templates.
- Use `artifacts` only for generated release assets in CI and keep it gitignored.

### 5.7 Nested subfolder rules

Nested platform subfolders are justified only when they represent real platform extension points, not arbitrary technical layers.

Examples:

- `cms/wordpress/src/blocks`
- `cms/wordpress/src/shortcodes`
- `cms/wordpress/src/template-tags`
- `cms/wordpress/src/admin`
- `commerce/shopify/theme-extension`
- `commerce/magento/src/view/frontend`
- `frameworks/react/src/hooks`

Do not create nested folders like `plugin/plugin-core/plugin-shared` when a flatter module layout will do.

## 6. Canonical Pure HTML Foundation

The pure HTML foundation must be the first implemented integration. It is the canonical rendering baseline for all other channels.

### 6.1 Recommended folder

`/integrations/foundation/html`

### 6.2 Purpose

This integration defines the canonical render contract and the canonical HTML, CSS, and JSON-LD output for BaseFAQ public content. It should be usable:

- as raw server-rendered HTML
- as a copy-paste block
- as the semantic baseline for framework components
- as the payload target for embed delivery

### 6.3 Canonical render contract

Every rendering integration should map BaseFAQ public API responses into a normalized `FaqRenderModel`.

Recommended contract shape:

```text
FaqRenderModel
  tenantKey
  faqId
  faqSlug
  locale
  direction
  title
  description
  canonicalUrl
  lastModified
  schemaMode
  items[]
    id
    slug
    questionText
    answerHtml
    answerPlainText
    position
    isExpandedByDefault
    canonicalUrl
    contentRefs[]
      label
      href
      rel
```

Rules:

- `answerHtml` must already be sanitized by the public content pipeline or sanitized again by the integration boundary before render.
- `answerPlainText` exists for structured data and text-only environments.
- `direction` is `ltr` or `rtl`.
- `schemaMode` should be `canonical`, `mirror`, or `off`.
- `contentRefs` should be rendered consistently across all integrations.

### 6.4 Semantic HTML5 structure

The pure HTML output should favor native semantics and progressive enhancement.

Recommended baseline:

- outer `<section>` with `data-basefaq-root`
- accessible heading for the FAQ title
- each FAQ item rendered as `<details>` for zero-JS disclosure behavior
- `<summary>` for the question
- answer content inside a child container
- optional sources list after the answer body

Suggested outline:

```html
<section
  class="bf-faq"
  data-basefaq-root
  data-basefaq-faq-id="faq_123"
  data-basefaq-locale="en-US"
  lang="en-US"
  dir="ltr"
>
  <header class="bf-faq__header">
    <h2 class="bf-faq__title">Shipping FAQ</h2>
  </header>

  <div class="bf-faq__items">
    <details class="bf-faq__item" data-basefaq-item-id="item_1">
      <summary class="bf-faq__question">How long does shipping take?</summary>
      <div class="bf-faq__answer">
        <div class="bf-faq__answer-body">
          <p>Standard shipping usually arrives within 3 to 5 business days.</p>
        </div>
      </div>
    </details>
  </div>
</section>
```

Why `details` and `summary` are recommended:

- native keyboard behavior
- no-JS fallback
- strong semantic fit for FAQ disclosure patterns
- low hydration cost

Optional enhancement logic can layer on single-open accordion behavior, analytics hooks, and host-controlled state sync without replacing the no-JS core.

### 6.5 Accessibility requirements

The HTML foundation must define and test:

- logical heading order
- visible focus styles
- keyboard-operable disclosure controls
- sufficient color contrast in baseline themes
- reduced motion support
- descriptive labels for source links
- no reliance on hover-only interactions

If a custom button/region accordion variant is needed for specific platforms, it must preserve the semantics established by the canonical HTML specification and be documented as an alternate rendering mode, not a replacement.

### 6.6 CSS architecture and theme baseline

The HTML foundation should ship a minimal CSS layer based on CSS custom properties rather than opinionated component theming.

Recommended CSS layers:

- `tokens.css`: spacing, typography, radius, border, color, focus, motion tokens
- `faq.css`: structural styles only
- `theme-light.css` and `theme-neutral.css`: optional theme examples

Rules:

- no dependency on external CSS frameworks
- no global element resets beyond the component root
- class namespace prefix `bf-`
- support `prefers-reduced-motion`
- support `color-scheme` without forcing dark mode assumptions

### 6.7 Progressive enhancement strategy

The HTML foundation must work fully without JavaScript.

Optional JavaScript should only enhance:

- single-open accordion mode
- analytics events
- animated height transitions when motion is allowed
- deep-link expansion to a specific FAQ item
- host callbacks for embed integration

Hydration-safe attributes should be standardized:

- `data-basefaq-root`
- `data-basefaq-faq-id`
- `data-basefaq-item-id`
- `data-basefaq-schema-mode`
- `data-basefaq-render-mode`

### 6.8 FAQPage JSON-LD generation

The foundation should own the canonical JSON-LD builder used across integrations.

Rules:

- emit one `FAQPage` block per canonical page render when `schemaMode=canonical`
- use `questionText` and `answerPlainText`
- keep item order stable
- include only visible, indexable FAQs
- suppress emission for preview/admin/sandbox/demo pages
- allow host integrations to disable emission explicitly

### 6.9 Schema validation

The HTML integration should include:

- JSON schema validation for the normalized render contract
- structured data shape validation for generated FAQPage payloads
- snapshot tests for script block output
- documentation for testing against Rich Results validation workflows during CI and release QA

### 6.10 SSR-friendly markup conventions

The canonical markup should be safe for:

- ASP.NET SSR
- PHP server templates
- React/Next SSR
- Astro and static generators
- CDN-delivered pre-rendered fragments

Conventions:

- stable IDs only when needed for anchoring or analytics
- no random IDs on each render
- deterministic class and data attribute output
- language and direction attributes on the root
- no client-only placeholders for content-bearing answers

### 6.11 No-JS fallback and lightweight JS

The base package should ship:

- zero-JS HTML/CSS path
- optional `accordion.js` that enhances behavior in under a strict size budget

Recommended budget:

- base CSS under `10 KB` minified
- optional JS under `6 KB` minified and gzipped

### 6.12 Example deliverables

The HTML foundation should include:

- example public FAQ page
- example embedded FAQ block
- copy-paste static HTML snippet
- sample FAQPage JSON-LD output
- RTL example
- i18n example with locale-specific headings

### 6.13 Tenant-safe embed assumptions

The HTML foundation itself should not assume access to privileged credentials. It should render from:

- normalized data already fetched server-side
- signed public render payloads
- public API results using tenant-safe client keys or equivalent public tokens

### 6.14 i18n and RTL readiness

Requirements:

- `lang` and `dir` on the root
- no hard-coded English UI strings in JS enhancements
- source-link labels and affordances localizable
- token and layout rules that tolerate right-to-left rendering

### 6.15 Documentation structure

Recommended docs inside the HTML integration:

- `docs/getting-started.md`
- `docs/markup-contract.md`
- `docs/accessibility.md`
- `docs/styling.md`
- `docs/seo.md`
- `docs/structured-data.md`
- `docs/i18n.md`
- `docs/security.md`

### 6.16 Tests

Required tests:

- render contract validation
- HTML snapshot tests
- JSON-LD snapshot tests
- accessibility audits
- visual regression against baseline themes
- no-JS example smoke tests

### 6.17 Release strategy

The HTML foundation should be versioned independently and published as:

- source package for reuse by framework integrations
- optional npm package for CSS/JS distribution
- GitHub release assets containing minified CSS, optional JS, and examples

### 6.18 Recommended internal structure

```text
/integrations/foundation/html
  README.md
  CHANGELOG.md
  COMPATIBILITY.md
  SECURITY.md
  VERSION.json
  /docs
    getting-started.md
    markup-contract.md
    accessibility.md
    styling.md
    seo.md
    structured-data.md
    i18n.md
    security.md
  /src
    /contracts
      faq-render-model.schema.json
      faq-render-model.ts
    /templates
      faq-page.html.ts
      faq-block.html.ts
    /styles
      tokens.css
      faq.css
      theme-light.css
      theme-neutral.css
    /scripts
      accordion.ts
      analytics.ts
    /seo
      faq-jsonld.ts
      faq-jsonld.schema.json
    /i18n
      messages.en.json
      messages.ar.json
  /examples
    /public-page
    /embedded-block
    /copy-paste-snippet
    /rtl
  /fixtures
    faq-render-model.en.json
    faq-render-model.ar.json
  /tests
    /unit
    /integration
    /accessibility
    /structured-data
    /visual
```

## 7. Shared Standards Across All Integrations

Every integration folder must follow a common baseline so that contributors can navigate any platform integration predictably.

### 7.1 Required files at each integration root

Every integration should include:

- `README.md`
- `CHANGELOG.md`
- `VERSION.json`
- `COMPATIBILITY.md`
- `SECURITY.md`
- `docs/`
- `examples/`
- `tests/`

Platform-specific manifest files should also exist when relevant:

- `package.json` for npm packages
- `composer.json` for PHP packages or WordPress bridges
- `pyproject.toml` for Python
- `.csproj` and solution metadata for .NET
- `shopify.app.toml` for Shopify app code
- `module.xml` or equivalent platform manifests for Magento

### 7.2 Required docs inside every integration

At minimum:

- `docs/getting-started.md`
- `docs/installation.md`
- `docs/configuration.md`
- `docs/rendering-modes.md`
- `docs/seo.md`
- `docs/structured-data.md`
- `docs/examples.md`
- `docs/troubleshooting.md`
- `docs/upgrade-guide.md`
- `docs/migration.md`
- `docs/developer-faq.md`

### 7.3 Required policy content

Each integration README or docs set must explicitly state:

- intended audience
- supported BaseFAQ public API versions
- supported platform version range
- rendering modes supported
- structured data support statement
- security model
- deprecation policy
- release channel status

### 7.4 Required tests

Every integration must have automation for:

- contract verification against the normalized render model or API DTOs
- at least one example smoke test
- structured data behavior where applicable
- accessibility checks when rendering UI

### 7.5 Canonical integration skeleton

```text
/integrations/<category>/<platform>
  README.md
  CHANGELOG.md
  COMPATIBILITY.md
  SECURITY.md
  VERSION.json
  /docs
  /src
  /examples
  /fixtures
  /tests
  /scripts
  /.release
```

Notes:

- `fixtures` is required when the integration consumes shared payloads or snapshots.
- `scripts` and `.release` are required when packaging or marketplace publication is non-trivial.
- `src` may contain platform-native layouts instead of a language-specific convention if the platform expects it.

### 7.6 VERSION manifest requirements

`VERSION.json` should include:

- integration identifier
- current version
- release channel
- supported BaseFAQ API version range
- supported platform version range
- artifact list
- last release date

Suggested shape:

```json
{
  "id": "cms/wordpress",
  "version": "1.0.0",
  "channel": "stable",
  "basefaqApi": {
    "min": "v1",
    "maxTested": "v1"
  },
  "platform": {
    "wordpress": "6.6 - 6.x",
    "php": "8.1 - 8.3"
  },
  "artifacts": [
    "wordpress-plugin-zip"
  ]
}
```

### 7.7 Compatibility matrix requirements

`COMPATIBILITY.md` should document:

- supported BaseFAQ public API versions
- minimum runtime and language versions
- supported framework or CMS versions
- known incompatible versions
- upgrade notes for breaking platform releases

### 7.8 Changelog and release notes pattern

Every release note must capture:

- added
- changed
- fixed
- deprecated
- removed
- security

Marketplace-specific release text can be derived from the changelog, but the repository changelog is the source of truth.

### 7.9 Deprecation and migration pattern

Every breaking release must ship:

- `docs/upgrade-guide.md`
- `docs/migration.md`
- explicit deprecated configuration options
- removal timeline if another major version still receives support

## 8. Detailed Platform-by-Platform Integration Design

## A. Embed / Web Delivery

### Purpose

This is the universal delivery path for third-party websites and the fallback channel for platforms without a deep native integration.

### Recommended folder structure

```text
/integrations/embed
  /web
  /web-component
  /iframe
  /cdn
```

### Rendering models

- `web`: script loader fetches data or signed render payload and injects canonical HTML
- `web-component`: custom element wrapping the canonical HTML foundation for teams that want declarative usage
- `iframe`: isolation mode for hostile CSS environments or stricter tenant separation
- `cdn`: versioned public bundle entrypoints and manifest metadata

### SEO strategy

- prefer direct server-rendered HTML or static HTML when possible
- use embed script for pages where client injection is acceptable
- document that pure client-side embed is weaker for SEO than SSR
- offer pre-rendered HTML snippet mode for SEO-sensitive hosts

### FAQPage structured data strategy

- script embed should default to `schemaMode=off` unless explicitly enabled and the host page designates BaseFAQ as schema owner
- iframe mode should not emit top-page JSON-LD that search engines cannot associate reliably with the host page
- pre-rendered HTML snippet mode may emit JSON-LD if the host includes the snippet server-side

### Extensibility and customization

- configuration via data attributes for copy-paste installs
- configuration via JavaScript object for application installs
- event hooks:
  - `onLoad`
  - `onRender`
  - `onItemToggle`
  - `onError`
- CSS custom properties for light styling
- `renderMode` options:
  - `inline`
  - `shadow-dom`
  - `iframe`

### Versioning approach

Independent semver. The embed loader should prioritize stability and treat configuration contract changes as high-cost breaking changes.

### Packaging and distribution

- npm package for bundler consumers
- CDN bundles with versioned immutable paths
- optional self-host bundle for enterprise customers
- signed release assets with checksum publication

### Documentation structure

- copy-paste snippet guide
- data attributes reference
- JavaScript API reference
- host page CSS isolation guide
- SEO limitations guide
- CSP guide

### Testing strategy

- browser compatibility matrix for current evergreen browsers
- host page CSS collision tests
- no-conflict tests with global CSS resets
- event hook integration tests
- iframe messaging tests
- structured data emission tests

### Security considerations

- no secret keys in browser config
- origin-aware script configuration where applicable
- CSP-safe bundle path documentation
- XSS-safe HTML insertion only from sanitized canonical payloads
- postMessage origin validation in iframe mode

### Release notes and compatibility policy

- document browser support changes explicitly
- document deprecated config keys for at least one minor line before removal
- keep a compatibility table for supported browsers and BaseFAQ public API version

### Example use cases

- marketing site FAQ section
- Webflow or Wix embed
- legacy CMS page with custom HTML block
- third-party documentation site without a native plugin

### Priority

Phase 1. This is the universal fallback and one of the highest ROI integrations.

## B. JavaScript / TypeScript SDK

### Purpose

Provide the primary code-facing API for browser and server-side JavaScript ecosystems.

### Recommended folder structure

```text
/integrations/sdks/javascript
  /src
    /client
    /models
    /render
    /seo
  /examples
    /browser
    /node
```

### Rendering models

- browser fetch client
- server-side fetch client
- framework-agnostic render helpers that map API data to the canonical HTML contract

### SEO strategy

The SDK should not force CSR-only rendering. It should expose SSR-safe helpers and normalized data mappers so frameworks can render on the server.

### FAQPage structured data strategy

Ship a shared JSON-LD builder that maps normalized FAQ data to `FAQPage` payloads and can be reused by React, Next.js, Astro, Node SSR, and embed tooling.

### Extensibility and customization

- pluggable fetch implementation
- middleware/interceptor pipeline
- optional cache adapter interface
- serializer hooks for custom transports
- transform hooks before rendering

### Versioning approach

Independent semver. The data model and transport surface are the primary breaking-change boundaries.

### Packaging and distribution

- npm package
- ESM first, with CJS compatibility only if demand requires it
- type declarations included

### Documentation structure

- installation
- browser usage
- node usage
- SSR guide
- API reference
- migration notes

### Testing strategy

- unit tests for client behavior
- contract tests against mocked BaseFAQ public APIs
- integration tests against live preview endpoints in CI
- type tests

### Security considerations

- public tokens only in browser scenarios
- secret-bearing use cases delegated to server environments
- SSR docs must warn against leaking secret config through serialized props

### Release notes and compatibility policy

- Node and browser compatibility table
- fetch/runtime requirements documented
- breaking transport changes require migration examples

### Example use cases

- custom React or Vue app
- server-side rendered Node app
- middleware that fetches FAQs and injects them into templates

### Priority

Phase 1. This SDK underpins most other JavaScript-based integrations.

## C. React

### Purpose

Offer idiomatic React components and hooks built on the canonical HTML foundation and JS SDK.

### Recommended folder structure

```text
/integrations/frameworks/react
  /src
    /components
    /hooks
    /providers
    /seo
    /theme
```

### Rendering models

- pure component rendering from provided data
- data-fetching hooks for CSR
- SSR-safe presentational components
- optional provider for shared config and fetch defaults

### SEO strategy

React package should bias toward server-rendered usage. CSR-only usage should be supported but documented as secondary for SEO-sensitive FAQ pages.

### FAQPage structured data strategy

Ship:

- `FaqJsonLd` component
- helper that deduplicates against page-level schema ownership flags
- normalized answer-to-text conversion from shared SEO utilities

### Extensibility and customization

- theme via CSS variables or className slots
- render overrides for question, answer, and source link sections
- hooks for telemetry and item-toggle events
- provider for defaults, not for required state

### Versioning approach

Independent semver with explicit React peer dependency ranges.

### Packaging and distribution

- npm package with peer dependencies on React and React DOM
- tree-shakeable exports
- CSS optional import, not required global reset

### Documentation structure

- quick start
- SSR vs CSR guide
- theming guide
- JSON-LD guide
- accessibility notes

### Testing strategy

- component unit tests
- SSR render snapshot tests
- hydration tests
- accessibility tests with React renderers

### Security considerations

- guardrails around `dangerouslySetInnerHTML` and sanitized answer content
- no secret config in browser providers

### Release notes and compatibility policy

- explicit React peer range changes
- migration notes for prop or hook signature changes

### Example use cases

- FAQ section inside an SPA
- reusable design-system wrapper
- marketing page with server-rendered FAQs

### Priority

Phase 1. React remains the most common frontend consumption surface.

## D. Next.js

### Purpose

Deliver a Next.js-first integration optimized for SEO-heavy FAQ pages, App Router usage, and server-side rendering strategies.

### Recommended folder structure

```text
/integrations/frameworks/nextjs
  /src
    /server
    /client
    /components
    /metadata
    /edge
  /examples
    /app-router-page
    /embedded-section
```

### Rendering models

- App Router is the primary target
- server components for FAQ data fetch and markup generation
- client components only for optional interactivity
- support SSR, SSG, and ISR

### SEO strategy

- server-render by default
- provide metadata helpers for canonical URLs, titles, and page descriptions
- expose revalidation guidance aligned with BaseFAQ content freshness expectations

### FAQPage structured data strategy

- server component helper returns JSON-LD payload
- companion component emits `<script type="application/ld+json">`
- guard against duplicate emission across layouts and nested routes

### Extensibility and customization

- server-side fetch config wrappers
- overrideable section and item components
- optional route handler utilities for edge cache tags and revalidation

### Versioning approach

Independent semver with explicit Next.js and React support ranges.

### Packaging and distribution

- npm package
- examples for App Router
- Pages Router only if customer demand justifies the maintenance cost

### Documentation structure

- App Router quick start
- SSR / SSG / ISR guide
- metadata guide
- edge runtime guide
- deployment checklist

### Testing strategy

- example app smoke tests
- server component snapshot tests
- ISR revalidation tests
- edge compatibility tests where code is edge-safe

### Security considerations

- secret tokens only in server contexts
- serialized props must not include privileged config
- cache and revalidation endpoints must validate signatures where applicable

### Release notes and compatibility policy

- App Router is the supported primary mode
- Pages Router support, if any, should be marked secondary
- compatibility matrix must list Next.js major versions and runtime constraints

### Example use cases

- dedicated FAQ landing pages
- product help routes
- embedded FAQ sections inside statically generated pages

### Priority

Phase 1. Next.js is one of the highest-value SEO-capable application ecosystems.

## E. Vue

### Purpose

Provide Vue-native components and composables for applications that need BaseFAQ rendering without using React.

### Recommended folder structure

```text
/integrations/frameworks/vue
  /src
    /components
    /composables
    /seo
```

### Rendering models

- presentational components
- composables for fetching and state handling
- SSR-compatible rendering helpers

### SEO strategy

Prefer SSR or static generation when Vue is used via Nuxt or another SSR host. Standalone CSR Vue usage is supported but documented as secondary for SEO.

### FAQPage structured data strategy

Component and composable helpers should mirror React behavior using shared SEO utilities.

### Extensibility and customization

- slots for question and answer rendering
- CSS variable theming
- composable overrides for fetch and cache

### Versioning approach

Independent semver with Vue peer dependency ranges.

### Packaging and distribution

- npm package
- ESM-first build

### Documentation structure

- installation
- component usage
- composable usage
- SSR notes
- theming

### Testing strategy

- component tests
- slot rendering tests
- SSR snapshot coverage

### Security considerations

- sanitized HTML rules
- browser-safe public config only

### Release notes and compatibility policy

- explicit peer dependency notes
- migration guidance for prop/slot changes

### Example use cases

- Vue SPA FAQ section
- custom storefront app

### Priority

Phase 2. Valuable, but behind React and Next.js in ROI.

## F. Nuxt

### Purpose

Support SSR and static generation in the Vue ecosystem through a Nuxt-native module or plugin.

### Recommended folder structure

```text
/integrations/frameworks/nuxt
  /src
    /module
    /runtime
    /components
    /seo
```

### Rendering models

- Nuxt module for runtime config and component registration
- SSR and static generation support
- composables for route-level fetching

### SEO strategy

- SSR and static generation are first-class
- provide route metadata helpers and JSON-LD composables

### FAQPage structured data strategy

- server-rendered JSON-LD helper integrated with Nuxt head management
- duplicate suppression across page/layout usage

### Extensibility and customization

- module options
- runtime config integration
- theme overrides

### Versioning approach

Independent semver with Nuxt version matrix.

### Packaging and distribution

- npm package as Nuxt module

### Documentation structure

- module install
- configuration
- SSR and static site guide
- SEO and head integration guide

### Testing strategy

- Nuxt example smoke tests
- SSR output validation
- module config tests

### Security considerations

- public runtime config only on the client
- secret values server-only

### Release notes and compatibility policy

- explicit Nuxt major compatibility
- migration notes for module options

### Example use cases

- marketing sites
- content-heavy Nuxt platforms

### Priority

Phase 2.

## G. Angular

### Purpose

Support Angular teams with a library that fits standalone components, dependency injection, and Angular SSR.

### Recommended folder structure

```text
/integrations/frameworks/angular
  /projects/basefaq
    /src
      /lib
        /components
        /services
        /directives
        /seo
```

### Rendering models

- standalone components
- service-based fetch layer
- directives for progressive enhancement
- Angular Universal / SSR compatibility

### SEO strategy

Prefer Angular SSR for FAQ pages. CSR-only rendering should be documented as non-ideal for index-critical routes.

### FAQPage structured data strategy

Provide a service and component helper for server-side JSON-LD injection.

### Extensibility and customization

- injection tokens for defaults
- template inputs for rendering overrides
- service interceptors for transport customization

### Versioning approach

Independent semver with Angular major compatibility declared.

### Packaging and distribution

- npm package using Angular library packaging

### Documentation structure

- install
- standalone component usage
- SSR notes
- SEO guide

### Testing strategy

- Angular component tests
- SSR tests
- DI token tests

### Security considerations

- sanitized HTML bindings
- no secrets in browser environment files

### Release notes and compatibility policy

- Angular major support table
- migration notes for DI tokens or component selectors

### Example use cases

- enterprise Angular portals
- documentation portals using Angular SSR

### Priority

Phase 3.

## H. Svelte / SvelteKit

### Purpose

Support lightweight framework consumers and SSR-first SvelteKit deployments.

### Recommended folder structure

```text
/integrations/frameworks/svelte
/integrations/frameworks/sveltekit
```

### Rendering models

- `svelte`: components and stores
- `sveltekit`: load-function helpers, SSR and SSG patterns, route examples

### SEO strategy

SvelteKit should prefer SSR or prerendered FAQ routes. Plain Svelte package supports client apps but does not define the SEO story alone.

### FAQPage structured data strategy

- store/helper to generate stable JSON-LD payloads
- SvelteKit examples for injecting JSON-LD server-side

### Extensibility and customization

- slots
- store adapters
- theme variable support

### Versioning approach

Independent semver with Svelte and SvelteKit compatibility declared separately.

### Packaging and distribution

- npm packages

### Documentation structure

- package usage
- SvelteKit route examples
- prerender notes

### Testing strategy

- component tests
- SSR output tests for SvelteKit examples

### Security considerations

- safe HTML rendering guidance
- no secret leakage through client loads

### Release notes and compatibility policy

- separate compatibility tables for Svelte and SvelteKit

### Example use cases

- content microsites
- lightweight FAQ widgets in Svelte apps

### Priority

Phase 3.

## I. Astro

### Purpose

Astro is an excellent fit for FAQ pages because it is SEO-first, static-first, and can render mostly HTML with near-zero client overhead.

### Recommended folder structure

```text
/integrations/static/astro
  /src
    /components
    /content
    /seo
  /examples
    /faq-page
    /faq-section
```

### Rendering models

- build-time fetch
- SSR where needed
- islands only for optional interactivity
- content collections or page components that map to the canonical HTML renderer

### SEO strategy

Astro should lean heavily on pre-rendered HTML. This is one of the strongest channels for FAQ landing pages.

### FAQPage structured data strategy

- server-side JSON-LD injection by default
- no client dependency for structured data

### Extensibility and customization

- component props
- content collection adapters
- optional client directives only for interaction layers

### Versioning approach

Independent semver with Astro support ranges.

### Packaging and distribution

- npm package with Astro components and helpers

### Documentation structure

- Astro install
- static build guide
- partial island guide
- SEO guide

### Testing strategy

- static build smoke tests
- HTML snapshot tests
- structured data validation

### Security considerations

- server-side token use only
- sanitized answer content

### Release notes and compatibility policy

- explicit Astro version range
- migration notes for content collection or component API changes

### Example use cases

- SEO-first marketing site FAQ pages
- documentation hubs using Astro

### Priority

Phase 2. Astro deserves early attention because of its strong FAQ fit, but it still sits behind the universal embed path, JS SDK, React/Next, and WordPress in total ROI.

## J. WordPress

### Purpose

WordPress should be one of the most mature BaseFAQ integrations because it combines broad market reach with strong SEO expectations and multiple rendering modes.

### Strategic position

WordPress should be a Phase 1 integration.

Recommendations:

- Gutenberg blocks are the primary authoring model.
- Shortcodes remain supported for compatibility and migration.
- Legacy widgets, if shipped at all, are secondary compatibility surfaces and should not drive architecture.
- Full-page templates and PHP template tags are important for theme developers.
- Do not model BaseFAQ data as a first-party WordPress custom post type in the initial version.

The reason to avoid a CPT-first strategy initially is simple: it duplicates canonical content, creates synchronization drift, complicates moderation, and blurs content ownership. The WordPress plugin should treat BaseFAQ as the content source of truth and render externally owned FAQ content through tenant-safe public APIs or signed server-side fetches.

### Recommended folder structure

```text
/integrations/cms/wordpress
  README.md
  CHANGELOG.md
  COMPATIBILITY.md
  SECURITY.md
  VERSION.json
  composer.json
  /docs
    getting-started.md
    installation.md
    configuration.md
    shortcodes.md
    blocks.md
    widgets.md
    templates.md
    theme-developers.md
    seo.md
    structured-data.md
    troubleshooting.md
    migration.md
  /src
    /plugin
    /admin
    /api
    /blocks
    /shortcodes
    /widgets
    /template-tags
    /templates
    /cache
    /seo
    /compatibility
    /i18n
    /security
  /assets
    /js
    /css
    /images
  /examples
    /classic-theme
    /block-theme
    /elementor
  /tests
    /unit
    /integration
    /e2e
    /compatibility
```

### Rendering models

Support all of the following:

- Gutenberg block rendering
- shortcode rendering
- PHP template tag rendering
- optional full-page FAQ template routing
- legacy widget rendering only as a compatibility mode

Gutenberg blocks should be the editorial-first experience. Template tags should be the developer-first experience. Shortcodes should exist as the durable fallback for classic editors and builder tools.

### Admin settings UI

The plugin should include an admin settings area for:

- BaseFAQ public API base URL
- tenant public client key or site-scoped signed public token
- default FAQ identifier or slug mapping
- cache TTL
- schema ownership defaults
- styling mode:
  - inherit theme
  - use BaseFAQ baseline styles
  - minimal unstyled
- logging/debug mode

Settings should support environment-variable overrides so enterprise deploys can avoid storing everything in the database.

### Tenant/API credentials storage model

Rules:

- browser-visible blocks and shortcodes must never require a secret admin token
- use public client keys or signed public tokens for read-only public FAQ consumption
- if a privileged server-side flow is ever needed, store secrets server-side only in WordPress options with `autoload = no` and document optional constant-based overrides in `wp-config.php`
- never embed secrets into post content, block attributes, or localized front-end scripts

### Shortcodes

Shortcodes should exist primarily for:

- legacy content
- classic editor pages
- page builders that accept shortcode text
- migration from older plugin ecosystems

Recommended shortcode shape:

`[basefaq faq="shipping" locale="en-US" theme="inherit" schema="auto"]`

The shortcode handler should map attributes into the same render pipeline used by blocks and template tags.

### Gutenberg blocks

Blocks should be the primary editing experience.

Recommended block set:

- `basefaq/faq`
- `basefaq/faq-item-list` only if a granular list mode is needed later

Block features:

- server-side rendered block output for SEO
- inspector controls for FAQ selection and display options
- block styles and style variations
- preview mode that can render sample content safely
- theme.json-compatible styles

The block should store configuration only, not replicated FAQ content.

### Legacy widgets

Recommendation:

- ship legacy widgets only if the maintenance cost is low
- document them as compatibility features, not strategic features
- do not invest in advanced widget-only behavior

Modern WordPress prioritizes blocks. BaseFAQ should follow that direction.

### Full-page templates

The plugin should support:

- a dedicated FAQ archive/page template mode for sites that want standalone FAQ pages
- theme override files under a documented template path
- plugin-provided fallback templates that reuse the canonical HTML foundation

### Template tags and PHP helpers

Template tags are critical for theme and agency adoption.

Recommended helpers:

- `basefaq_render_faq( array $args = [] )`
- `basefaq_get_faq_html( array $args = [] )`
- `basefaq_get_faq_jsonld( array $args = [] )`

These helpers should be stable and documented for theme developers.

### REST API usage

The WordPress integration should consume BaseFAQ public APIs over HTTP and normalize payloads into the canonical render contract.

WordPress-local REST endpoints may be added only for:

- preview proxies
- cached render endpoints
- debug tooling

Do not create a full duplicate internal REST surface unless it materially improves the editor experience.

### Caching strategy

Cache strategy should include:

- transients keyed by tenant, FAQ identifier, locale, render mode, and theme mode
- optional object cache support when available
- invalidation hooks on settings changes
- webhook or manual purge endpoint support later
- stale-while-revalidate behavior when operationally safe

Cache keys must include versioned render contract identifiers so markup changes do not serve stale incompatible fragments.

### Custom post type versus external API rendering

Recommendation:

- external API rendering is the default architecture
- do not implement CPT sync in the initial scope
- if a future sync mode is required, ship it as a separate opt-in extension, not as the default plugin model

Reasoning:

- external ownership preserves BaseFAQ as source of truth
- duplication harms SEO and governance
- CPT sync introduces editorial drift and invalidation complexity

### Theme compatibility

The plugin should support:

- block themes
- classic themes
- popular general-purpose theme patterns without hard dependencies

Style strategy:

- default to scoped classes and CSS variables
- avoid aggressive global resets
- provide unstyled or minimally styled modes for design-system-heavy sites

### Block styles and variations

Support:

- default style
- minimal style
- bordered style
- compact style

All style variations should still reuse the same semantic markup and structured data behavior.

### Elementor and page builder compatibility

Recommendation:

- Phase 1: support via shortcode and server-rendered HTML blocks
- Phase 2+: evaluate dedicated Elementor widget only if demand justifies it

Do not make a page-builder-specific widget the primary integration model.

### Multisite considerations

The plugin must clarify:

- whether settings are per site or network-global
- whether each site can map to a different tenant key
- how caches are isolated per site
- how capabilities apply under network admin

Default recommendation:

- settings are site-specific by default
- optional network defaults may be inherited but locally overridden

### Plugin activation and upgrade routines

Activation should:

- register settings
- initialize defaults
- verify minimum PHP and WordPress versions
- schedule cleanup or cache tasks only if needed

Upgrade routines should:

- version database or options schema changes explicitly
- migrate settings idempotently
- record the last migrated plugin version

### Settings validation and sanitization

All admin inputs must be:

- validated against strict shapes
- sanitized before persistence
- escaped on render
- nonce-protected on save

### Capability and permission model

Recommended capability boundaries:

- global settings: `manage_options`
- block usage: normal post editing capabilities
- troubleshooting or diagnostics screens: admin-only

Do not expose raw credential values to editors who only place blocks.

### Localization / i18n

The plugin should:

- localize all admin UI strings
- support FAQ locale overrides per block instance
- be translation-ready through standard WordPress patterns

### Security

The WordPress integration must explicitly implement:

- nonce protection for admin actions
- strict escaping for all output
- sanitization for settings and block attributes
- remote HTTP allowlist guidance
- protection against leaking tokens to public HTML

### Asset loading strategy

Rules:

- load editor assets only in block editor contexts
- load front-end assets only when a BaseFAQ block, shortcode, widget, or template is present
- avoid global asset enqueueing
- support separate editor and front-end bundles

### SEO plugin coexistence

WordPress frequently includes Yoast, Rank Math, AIOSEO, or similar plugins. BaseFAQ should not blindly duplicate schema or fight ownership.

Recommended strategy:

- BaseFAQ emits FAQPage JSON-LD by default when it owns the page’s FAQ rendering and no site-level setting disables it
- site-wide and per-instance settings allow `schema=off`, `schema=auto`, or `schema=force`
- provide filters so third-party SEO plugins or themes can suppress BaseFAQ schema cleanly
- do not attempt brittle deep integration with every SEO plugin on day one

### FAQPage JSON-LD injection strategy

Preferred model:

- server-rendered block output injects JSON-LD into the page once
- deduplicate by page request context
- shortcodes and template tags share the same dedupe registry
- if multiple BaseFAQ blocks appear on a page, emit one merged `FAQPage` only when that page is the canonical FAQ owner

### Documentation segmentation

WordPress docs must explicitly contain:

- install
- shortcode usage
- block usage
- widget usage
- template usage
- theme developer docs
- troubleshooting
- migration
- changelog

### Release packaging

Ship:

- WordPress installable zip
- GitHub release asset
- optional WordPress.org path only when operational processes are ready for that ecosystem

The repo should not prematurely optimize for WordPress.org publishing if enterprise zip distribution is the first commercial path.

### Versioning and support policy

WordPress plugin versions should be independent. This integration should be considered an LTS candidate because WordPress users upgrade conservatively. Support windows should be longer than for fast-moving JS wrappers.

### Example use cases

- marketing site FAQ page
- help center embedded into a WordPress theme
- editor-managed landing pages using blocks
- developer-managed theme integration with template tags

### Priority

Phase 1.

## K. WooCommerce

### Purpose

Support product and category FAQ rendering in WooCommerce storefronts without collapsing that scope into the generic WordPress plugin.

### Strategic position

WooCommerce should be a dedicated integration under `commerce/woocommerce`, not merely a mode inside the generic WordPress plugin. It can reuse WordPress shared code, but it deserves its own compatibility matrix, release discipline, and storefront-specific feature set.

### Recommended folder structure

```text
/integrations/commerce/woocommerce
  /src
    /extension
    /product
    /category
    /seo
    /cache
```

### Rendering models

- product page FAQ injection
- product category FAQ injection
- optional account/help pages for store operations content

### SEO strategy

- server-render FAQs on product and category pages
- ensure FAQ markup is visible and relevant to the page context
- avoid rendering identical global FAQ blocks across every product page without clear content ownership

### FAQPage structured data strategy

- allow coexistence with Product schema
- emit FAQPage only when the FAQ content is genuinely part of the page and not hidden behind tabs that never load for bots or users
- deduplicate against WordPress plugin-level schema settings

### Extensibility and customization

- product/category placement hooks
- template overrides
- theme location filters
- per-product FAQ source mapping

### Versioning approach

Independent semver with explicit WooCommerce and WordPress support ranges.

### Packaging and distribution

- installable WordPress/WooCommerce extension zip
- possible dependency on the generic WordPress plugin runtime or shared library

### Documentation structure

- install
- product mapping
- category mapping
- template overrides
- performance and caching guide

### Testing strategy

- WooCommerce version matrix tests
- product page render tests
- structured data coexistence tests
- theme compatibility smoke tests

### Security considerations

- same credential rules as WordPress
- no storefront leakage of privileged data

### Release notes and compatibility policy

- clearly state supported WooCommerce versions
- document theme and builder caveats

### Example use cases

- FAQ below add-to-cart
- shipping FAQ on category pages
- return-policy FAQ on product templates

### Priority

Phase 2. High-value commerce channel, but it depends on core WordPress integration maturity.

## L. Shopify

### Purpose

Provide a modern Shopify integration that works with Online Store 2.0 themes and app distribution patterns.

### Recommended folder structure

```text
/integrations/commerce/shopify
  /app
  /theme-extension
  /blocks
  /snippets
  /docs
```

### Rendering models

Recommended primary model:

- theme app extension with app blocks and sections

Secondary models:

- app embed for lightweight storefront injection
- Liquid snippets for legacy or controlled themes

### SEO strategy

Prefer storefront-rendered HTML through theme extension or app proxy patterns over purely client-side script injection. Product FAQs should be present in rendered HTML whenever they are intended to contribute to search visibility.

### FAQPage structured data strategy

- theme app extension should own server-renderable schema snippets where possible
- deduplicate per template render
- avoid client-only schema injection when server-side Liquid or proxy render is available

### Extensibility and customization

- app block settings
- section/block placement options
- Liquid snippet fallback
- store admin mapping UI for product/category to BaseFAQ FAQ identifiers

### Versioning approach

Independent semver for the integration code, plus compatibility metadata for Shopify platform requirements.

### Packaging and distribution

- Shopify app distribution
- theme app extension package
- GitHub release artifacts for self-hosted/private install flows if relevant

### Documentation structure

- install and auth
- theme app extension setup
- app block usage
- Liquid snippet fallback
- SEO guidance

### Testing strategy

- theme preview smoke tests
- app block rendering tests
- product template integration tests
- Online Store 2.0 compatibility tests

### Security considerations

- Shopify app secrets remain server-side
- storefront code only receives safe public configuration
- signed app proxy requests where applicable

### Release notes and compatibility policy

- document theme requirements
- separate compatibility notes for public app distribution versus private/custom apps

### Example use cases

- product detail FAQ block
- shipping FAQ section on collection pages
- theme-managed FAQ landing page

### Priority

Phase 2.

## M. Magento / Adobe Commerce

### Purpose

Support enterprise storefronts that rely on Magento module architecture and server-rendered commerce pages.

### Recommended folder structure

```text
/integrations/commerce/magento
  /src
    /Block
    /Controller
    /Helper
    /Model
    /view
      /frontend
```

### Rendering models

- layout XML insertion
- block and template rendering
- full-page FAQ blocks on product and category pages

### SEO strategy

Server-rendered blocks are the default. Magento deployments often care about crawlable product support content and cache behavior.

### FAQPage structured data strategy

- inject JSON-LD at page render
- coordinate with Product schema and any existing SEO module configuration

### Extensibility and customization

- layout XML handles
- admin config for placement and FAQ mappings
- template override capability

### Versioning approach

Independent semver with Magento and PHP version compatibility explicitly declared.

### Packaging and distribution

- Magento module package
- Composer distribution
- zip artifact for controlled enterprise installs if needed

### Documentation structure

- install
- admin configuration
- theme/template customization
- cache and indexing notes

### Testing strategy

- integration tests against Magento module behavior
- layout XML render tests
- cache interaction tests

### Security considerations

- server-side secret storage only
- escaped template output
- safe admin config validation

### Release notes and compatibility policy

- Magento and Adobe Commerce support ranges documented separately if needed

### Example use cases

- product detail FAQ
- category support sections
- enterprise support microsites inside commerce platform

### Priority

Phase 3.

## N. Drupal

### Purpose

Provide a Drupal module that uses render arrays, Twig templates, cache metadata, and block placement idioms correctly.

### Recommended folder structure

```text
/integrations/cms/drupal
  /src
    /Plugin
      /Block
    /Controller
    /Service
    /Cache
  /templates
```

### Rendering models

- blocks
- controller-routed pages
- Twig templates
- render arrays as the main output abstraction

### SEO strategy

Prefer render arrays and Twig server rendering. Drupal site builders often expect cacheability metadata to preserve performance and correctness.

### FAQPage structured data strategy

- attach JSON-LD via render arrays
- deduplicate per response render
- align schema ownership with Drupal page composition rules

### Extensibility and customization

- block configuration
- theme overrides through Twig
- service decoration for advanced implementers

### Versioning approach

Independent semver with Drupal core and PHP version compatibility.

### Packaging and distribution

- Drupal module packaging
- Composer-friendly distribution

### Documentation structure

- install
- block placement
- Twig overrides
- cache metadata guide

### Testing strategy

- Drupal functional tests
- render array tests
- cache tag and cache context tests

### Security considerations

- sanitized output
- secure config storage
- permission-gated admin settings

### Release notes and compatibility policy

- Drupal core compatibility table
- module upgrade instructions

### Example use cases

- content-managed FAQ pages
- reusable FAQ blocks in site builder layouts

### Priority

Phase 3.

## O. Joomla

### Purpose

Support Joomla sites through the platform’s component/module/plugin distinction.

### Recommended folder structure

```text
/integrations/cms/joomla
  /component
  /module
  /plugin
```

### Rendering models

Recommendation:

- module is the primary delivery surface for page placement
- plugin provides content embedding hooks
- component is only needed if BaseFAQ full-page routing is required

### SEO strategy

Prefer server-rendered module output and route-level component pages when canonical FAQ pages are needed.

### FAQPage structured data strategy

- schema emitted by the module/component when configured as canonical owner

### Extensibility and customization

- template overrides
- module placement configuration
- plugin hooks for content insertion

### Versioning approach

Independent semver with Joomla and PHP version compatibility.

### Packaging and distribution

- Joomla extension packages

### Documentation structure

- install
- module usage
- plugin usage
- template override notes

### Testing strategy

- Joomla sandbox install tests
- module render tests
- template override smoke tests

### Security considerations

- admin config validation
- escaped output
- safe token handling

### Release notes and compatibility policy

- Joomla version matrix

### Example use cases

- FAQ modules inside template positions
- content-article embeds

### Priority

Phase 4.

## P. Webflow

### Purpose

Support Webflow primarily through embed-first patterns that are designer-friendly and low-friction.

### Recommended folder structure

```text
/integrations/lowcode/webflow
  /embed-kit
  /examples
  /docs
```

### Rendering models

- custom code embed block
- site-wide custom code injection
- optional CMS collection reference pattern using embed placeholders

### SEO strategy

If SEO is important, prefer pre-rendered HTML snippet mode or static injected HTML over client-only fetch-and-render. Client-side embed remains acceptable for non-canonical placements.

### FAQPage structured data strategy

- disabled by default for client-only embed
- enabled only for server-side or static snippet installs where Webflow page is the canonical owner

### Extensibility and customization

- copy-paste snippet parameters
- theme tokens via CSS variables
- data attributes for FAQ selection

### Versioning approach

Independent semver for the embed kit and docs examples.

### Packaging and distribution

- copy-paste install kit
- CDN snippet
- optional npm package for advanced teams

### Documentation structure

- designer install guide
- custom code placement guide
- CMS collection notes
- SEO caveats

### Testing strategy

- install smoke tests in Webflow sandbox projects
- CSS collision tests

### Security considerations

- public tokens only
- origin restrictions when supported

### Release notes and compatibility policy

- Webflow is documented as embed-first, not native-app-first

### Example use cases

- marketing site FAQ section
- collection page support block

### Priority

Phase 2.

## Q. Wix

### Purpose

Support Wix through Velo custom code and embed strategies without overcommitting to a costly native app too early.

### Recommended folder structure

```text
/integrations/lowcode/wix
  /velo
  /embed-kit
  /docs
```

### Rendering models

- Velo custom code fetch and render
- HTML embed or iframe fallback
- app-market path deferred until demand proves it

### SEO strategy

Wix integration should document SEO limitations clearly. Where possible, use server-capable Wix rendering or static HTML injection rather than purely dynamic client updates for canonical FAQ pages.

### FAQPage structured data strategy

- disabled by default for purely client-side widget installs
- documented workaround paths when Wix page output can include server-known JSON-LD

### Extensibility and customization

- Velo script examples
- copy-paste embed configs
- theme hooks through CSS variables

### Versioning approach

Independent semver.

### Packaging and distribution

- example code kits
- CDN and embed artifacts
- app market distribution is Phase 4+ only

### Documentation structure

- Velo install
- HTML embed guide
- SEO limitations and workarounds

### Testing strategy

- sandbox site smoke tests
- embed rendering tests

### Security considerations

- safe public config only
- server-side secrets only in Wix backend code if ever used

### Release notes and compatibility policy

- clearly mark app market support as future, not assumed

### Example use cases

- service business FAQ page
- embedded FAQ block in Wix site editor

### Priority

Phase 3.

## R. Headless CMS Adapters

### Purpose

Headless CMS adapters should help teams decide whether BaseFAQ content is fetched live at render time or synced into CMS-managed content models for editorial workflows.

### Strategic recommendation

Focus first on fetch/render contracts and cache invalidation guidance, not heavyweight deep plugins. These ecosystems vary too much for one-size-fits-all native apps.

### Recommended folder structure

```text
/integrations/cms/headless
  /contentful
  /strapi
  /sanity
  /hygraph
```

### Rendering models

For each adapter, support two patterns:

- live fetch: the app/site fetches BaseFAQ content directly at request or build time
- sync projection: selected FAQ metadata is mirrored into the CMS only when editorial workflows truly require local references

### SEO strategy

Ownership should usually stay with the frontend renderer, not the CMS adapter itself. These adapters should help normalize content and invalidation, while the consuming site controls final HTML and schema output.

### FAQPage structured data strategy

Adapters should expose normalized FAQ data and schema helper utilities, but they should not silently inject schema unless they also own final page rendering.

### Extensibility and customization

- mapper interfaces from BaseFAQ contracts to CMS-specific content models
- webhook handlers for cache invalidation
- field mapping transforms

### Versioning approach

Independent semver per adapter because CMS APIs evolve independently.

### Packaging and distribution

- npm packages or example kits depending on ecosystem norms
- webhook utilities and examples

### Documentation structure

- live fetch guide
- sync guide
- invalidation guide
- content ownership guide

### Testing strategy

- contract tests for data mapping
- webhook payload tests
- preview environment smoke tests

### Security considerations

- sync jobs use server-side secrets only
- preview tokens kept separate from production tokens

### Release notes and compatibility policy

- each adapter documents the CMS SDK/API versions it targets

### Contentful

- purpose: map FAQ content into page models or fetch it during Next.js/Astro builds
- recommendation: live fetch first, sync only for editorial references and preview cards
- priority: Phase 3

### Strapi

- purpose: use Strapi as orchestration layer or projection target for aggregated content sites
- recommendation: prefer live fetch or server-side middleware over syncing full answer bodies by default
- priority: Phase 3

### Sanity

- purpose: allow references from portable-text documents or page builders to BaseFAQ entries
- recommendation: ship schema snippets and GROQ-friendly reference patterns, not full content duplication by default
- priority: Phase 3

### Hygraph

- purpose: support GraphQL-driven frontends that want FAQ references in graph models
- recommendation: adapter should focus on fetch normalization and cache invalidation examples
- priority: Phase 3

## S. Static Site Generators

### Purpose

Support build-time fetching and pre-rendered FAQ output for sites where SEO and performance are the dominant concerns.

### Recommended folder structure

```text
/integrations/static
  /eleventy
  /hugo
  /jekyll
  /docusaurus
  /gatsby
  /mkdocs
```

### Shared rendering model

Each generator integration should:

- fetch BaseFAQ content at build time
- normalize into the canonical render contract
- emit stable HTML and FAQPage JSON-LD
- document cache freshness and rebuild triggers

### SEO strategy

Static output is ideal for FAQ pages. These integrations should be treated as SEO-first and should prefer fully rendered HTML over runtime widgets.

### FAQPage structured data strategy

- emit JSON-LD at build time
- ensure only one canonical page owns the FAQ schema
- rebuild or revalidate when FAQ content changes

### Extensibility and customization

- filters/shortcodes/partials depending on generator
- template overrides
- data-fetch adapters

### Versioning approach

Independent semver per generator.

### Packaging and distribution

- plugin packages or starter kits depending on ecosystem norms

### Documentation structure

- build-time fetch
- caching and freshness
- deployment and rebuild hooks
- structured data notes

### Testing strategy

- example site build tests
- output snapshot tests
- structured data validation

### Security considerations

- build-time secrets only
- no secret leakage into generated static payloads

### Release notes and compatibility policy

- generator version compatibility tables required

### Eleventy

- best fit: simple static marketing/help sites
- implementation: data files + shortcode or Nunjucks/Liquid includes
- priority: Phase 2

### Hugo

- best fit: high-performance docs and marketing sites
- implementation: data fetch pipeline plus shortcodes/partials
- priority: Phase 3

### Jekyll

- best fit: GitHub Pages-adjacent or Ruby-based static sites
- implementation: plugin or data ingestion pattern
- priority: Phase 3

### Docusaurus

- best fit: product documentation portals
- implementation: MDX components and build-time content fetch plugin
- priority: Phase 3

### Gatsby

- best fit: existing Gatsby estates only
- implementation: source plugin and React wrappers
- priority: Phase 4 because the ecosystem is less strategic than Next.js and Astro for new work

### MkDocs

- best fit: Python-heavy docs portals
- implementation: markdown extension or theme component examples
- priority: Phase 4

## T. Language SDKs

### Purpose

Language SDKs provide direct programmatic access to BaseFAQ public APIs and normalized render/SEO helpers outside the JavaScript frontend ecosystem.

### Shared SDK architecture rules

All SDKs should include:

- typed DTOs/models for public FAQ payloads
- configurable BaseFAQ base URL
- public-token authentication support only for browser-equivalent public flows
- retry and timeout configuration
- normalized render contract mapping where appropriate
- structured data helpers where server-side render use cases are common

### PHP

#### Recommended folder structure

```text
/integrations/sdks/php
  /src
    /Client
    /Model
    /Render
    /Seo
```

#### Client architecture

- PSR-friendly HTTP client abstraction
- DTO mapping for FAQs, FAQ items, and render models
- template helper for server-side PHP rendering

#### Auth strategy

- public token support for public FAQ reads
- secret-bearing flows only if future privileged APIs are exposed, which is out of current scope

#### SSR helpers

- helper to produce canonical HTML fragments from normalized data
- helper to emit FAQPage JSON-LD safely

#### Retry and resilience

- exponential backoff for transient failures
- configurable timeout
- pluggable PSR cache support

#### Packaging

- Composer / Packagist

#### Docs and compatibility

- PHP version range
- supported HTTP clients
- template integration examples

#### Priority

Phase 2. Strong dependency synergy with WordPress and server-rendered PHP estates.

### .NET

#### Recommended folder structure

```text
/integrations/sdks/dotnet
  /src
    /BaseFaq.Integrations.DotNet
    /BaseFaq.Integrations.DotNet.Tests
```

#### Client architecture

- typed `HttpClient`-based client
- options-based configuration
- DTOs and render-model mapping
- optional TagHelper or Razor helper layer later

#### Auth strategy

- public token support for public FAQ reads
- server configuration through options and secret providers

#### SSR helpers

- Razor-friendly HTML and JSON-LD helpers
- server-side caching adapters

#### Retry and resilience

- Polly or standard resilience pipeline integration
- cancellation token support

#### Packaging

- NuGet

#### Docs and compatibility

- target frameworks
- ASP.NET integration examples
- Blazor or Razor Pages guidance only if added later

#### Priority

Phase 3.

### Python

#### Recommended folder structure

```text
/integrations/sdks/python
  /src/basefaq_integrations_python
```

#### Client architecture

- typed client using modern Python HTTP stack
- models for FAQ payloads
- optional Jinja rendering helpers

#### Auth strategy

- public token support
- secret-bearing server-side config only

#### SSR helpers

- helper to produce JSON-LD
- optional Jinja macro examples

#### Retry and resilience

- configurable retry and timeout
- async support only if justified later

#### Packaging

- PyPI via `pyproject.toml`

#### Docs and compatibility

- supported Python versions
- Flask/Django/FastAPI examples if later added

#### Priority

Phase 3.

### Node server SDK

#### Recommended folder structure

```text
/integrations/sdks/node
  /src
    /client
    /cache
    /render
    /seo
```

#### Client architecture

- built on the JavaScript core SDK but server-optimized
- caching adapters
- secret-safe server config
- middleware helpers for Express/Fastify and generic Node runtime

#### Auth strategy

- public token support
- server-side private signing helpers only if later public render-token signing is introduced

#### SSR helpers

- Node template helpers
- static HTML fragment renderer
- JSON-LD helper

#### Retry and resilience

- retry policies
- abort signals
- cache adapters

#### Packaging

- npm

#### Docs and compatibility

- supported Node versions
- Express/Fastify examples
- server caching guide

#### Priority

Phase 1. This supports middleware, SSR, and server-managed embed use cases early.

## 9. Cross-Integration Shared Libraries / Templates / Tooling

Shared assets inside `/integrations` are necessary to prevent ecosystem drift.

### Recommended shared folders

```text
/integrations/shared
  /contracts
  /seo
  /rendering
  /fixtures
  /themes
  /testing
  /schemas
  /compatibility
```

### Shared asset definitions

#### `shared/contracts`

Contains:

- canonical public FAQ render model definitions
- DTO normalization helpers
- schema files for API-facing contract validation

Why it belongs here:

It is the stable language-neutral integration boundary used by HTML, SDKs, framework adapters, embed loaders, and CMS plugins.

#### `shared/seo`

Contains:

- canonical FAQPage JSON-LD builders
- deduplication logic contracts
- answer-to-plain-text helpers
- structured data validation helpers

Why it belongs here:

SEO consistency is one of the highest-risk drift areas across integrations.

#### `shared/rendering`

Contains:

- shared semantic render helpers
- class naming constants
- optional template fragments that can be ported across stacks

#### `shared/fixtures`

Contains:

- sample FAQ payloads
- locale variants
- large FAQ sets for stress tests
- malicious/sanitization edge cases

#### `shared/themes`

Contains:

- design tokens
- baseline CSS variables
- example themes for public FAQ rendering

#### `shared/testing`

Contains:

- contract test harness
- accessibility test helpers
- visual regression baselines
- cross-browser smoke utilities

#### `shared/schemas`

Contains:

- JSON schemas for render models
- schema docs for config manifests and `VERSION.json`

#### `shared/compatibility`

Contains:

- support policy templates
- platform matrix generators
- release-note fragments for compatibility announcements

### Templates

Recommended:

```text
/integrations/templates
  /integration-skeleton
  /docs
  /release
  /examples
```

These templates should be scaffold blueprints, not copy-paste trash. Each should be minimal, opinionated, and synchronized with the shared standards in this document.

### Tooling

Recommended:

```text
/integrations/tooling
  /build
  /release
  /validation
  /generators
  /catalog
```

Responsibilities:

- changed-integration detection
- version manifest validation
- compatibility index generation
- docs index generation
- release note validation
- package graph inventory

## 10. Versioning Strategy

### Recommendation

Use independent versioning per integration, not a locked monorepo version for all of `/integrations`.

### Why independent versioning is the right choice

- ecosystem cadence differs drastically
- WordPress, Shopify, React, and .NET support windows do not align
- breaking changes in one integration should not force major version bumps everywhere else
- some integrations will be experimental while others need long-term support

### Semantic versioning rules

- patch: bug fixes, compatibility fixes, docs-only code examples that do not change runtime contracts
- minor: backward-compatible features, new extension points, new optional config
- major: breaking API changes, dropped platform support, rendering contract changes that require integrator action

### Compatibility policy with BaseFAQ public APIs

Every integration must declare:

- minimum supported BaseFAQ public API version
- latest tested BaseFAQ public API version
- whether it depends on specific endpoints, fields, or contract versions

Breaking BaseFAQ public API changes must be announced with:

- a deprecation window
- explicit migration notes
- updated compatibility matrices

### Recommended deprecation windows

- JS/framework integrations: at least 6 months or two minor release cycles
- CMS and commerce integrations: at least 12 months for major-version transitions when feasible
- SDK transport deprecations: at least one minor release with warnings before removal

### LTS recommendations

LTS should be considered for:

- `cms/wordpress`
- `embed/web`
- `sdks/javascript`
- `sdks/php`

These represent broad adoption surfaces where conservative upgrades are common.

### Breaking-change communication

Breaking changes must be communicated through:

- `CHANGELOG.md`
- `docs/upgrade-guide.md`
- `docs/migration.md`
- release notes in GitHub Releases
- compatibility matrix updates

### Compatibility metadata location

Each integration root should contain:

- `VERSION.json`
- `COMPATIBILITY.md`

Optional machine-readable compatibility manifests may also be generated centrally under `/integrations/docs/generated`.

### Platform version declaration rules

Examples:

- WordPress integration declares supported WordPress and PHP versions
- React integration declares React and React DOM peer ranges
- .NET SDK declares target frameworks
- Shopify integration declares Online Store 2.0 requirements
- Magento integration declares Magento and PHP versions

## 11. Documentation Strategy

### Local docs per integration

Every integration must provide:

- `README.md`
- `docs/getting-started.md`
- `docs/installation.md`
- `docs/configuration.md`
- `docs/rendering-modes.md`
- `docs/seo.md`
- `docs/structured-data.md`
- `docs/examples.md`
- `docs/troubleshooting.md`
- `docs/upgrade-guide.md`
- `docs/migration.md`
- `docs/developer-faq.md`
- `CHANGELOG.md`
- `COMPATIBILITY.md`
- `SECURITY.md`

### Central aggregated docs

The future `/integrations/docs` root should aggregate:

- platform catalog
- standards and policies
- shared release guidance
- generated compatibility indexes
- rollout status and support tiers

Recommended structure:

```text
/integrations/docs
  README.md
  /architecture
  /standards
  /platforms
  /release
  /generated
```

### Docs navigation strategy

- central docs should index every integration by category and platform
- each integration README should link to central standards and its local detailed docs
- platform docs should cross-link to shared SEO and security policies instead of duplicating them

### Versioned docs strategy

- docs versioning follows the integration release line, not a global repository doc version
- major integrations should preserve upgrade guides across at least the last supported major
- generated docs indexes should surface current stable and LTS lines

### Keeping docs in sync with releases

Release automation should fail if:

- `VERSION.json` changed but changelog did not
- compatibility matrix changed but docs were not updated
- a releaseable integration lacks required docs files

## 12. Packaging & Distribution Strategy

### Distribution channels by integration type

| Integration type | Primary distribution |
|---|---|
| JS frameworks and SDKs | npm |
| CDN embed bundles | CDN plus GitHub Release assets |
| WordPress and WooCommerce | installable zip, optional WordPress.org later |
| PHP SDK | Composer / Packagist |
| .NET SDK | NuGet |
| Python SDK | PyPI |
| Magento | Composer package, optional zip artifact |
| Shopify | Shopify app distribution plus repo release evidence |
| Low-code kits | GitHub Releases and CDN snippets |

### Registry versus release-only rules

- use public registries for mainstream developer packages
- use marketplace distribution when the host ecosystem expects it
- use GitHub release-only distribution for example kits, signed artifacts, or enterprise private packages
- use private feeds only for enterprise-only channels, not as the default public story

### CDN bundle policy

CDN artifacts should:

- use immutable versioned paths
- publish checksums
- support SRI where feasible
- avoid unversioned mutable production URLs except stable channel aliases that clearly map to a specific version

### Signed artifacts and verification

Recommended for released binaries and zips:

- checksum files
- signed release notes or provenance metadata when tooling matures

### Marketplace considerations

- WordPress.org should be optional, not assumed at day one
- Shopify distribution must respect app review and theme extension constraints
- Magento and Packagist ecosystems may require additional metadata and packaging rules

## 13. Testing Strategy

### Required test layers

Every integration should adopt the relevant subset of:

- unit tests
- contract tests against BaseFAQ public APIs
- integration tests
- end-to-end tests
- accessibility tests
- structured data validation tests
- example smoke tests
- visual regression tests
- cross-browser tests when rendering in browsers

### Contract tests

Contract tests should verify:

- payload mapping to the canonical render contract
- tolerance for additive API fields
- failure behavior for missing required fields

### Rendering tests

UI integrations should include:

- snapshot tests for canonical HTML output
- hydration or progressive-enhancement tests where relevant
- theme compatibility checks for scoped CSS behavior

### Structured data tests

All SEO-capable integrations should validate:

- valid FAQPage payload shape
- dedupe behavior
- suppression behavior when schema ownership is external

### Accessibility tests

Required for all UI renderers:

- keyboard navigation
- focus visibility
- screen-reader-meaningful semantics
- color contrast in baseline themes

### Visual regression

Apply especially to:

- foundation/html
- embed/web
- React
- Next.js examples
- WordPress block output

### CMS and commerce sandbox validation

Recommended:

- WordPress sandbox site
- WooCommerce sandbox store
- Shopify preview store
- Drupal sandbox site
- Magento reference store

### Smoke tests for examples

Every example app or starter should be buildable and smoke-tested in CI or release validation.

## 14. Security & Compliance Considerations

### Public token versus secret token rules

- browser-delivered integrations use public client keys or signed public render tokens only
- admin secrets never ship to the browser
- secret-bearing server-side config is allowed only in server integrations and admin/plugin backends

### Tenant-safe embed patterns

Recommended:

- tenant-scoped public identifiers
- signed configuration payloads for embed installs
- optional origin allowlists for high-trust embed scenarios

### CSP considerations

All browser integrations should document:

- required script sources
- required style policies
- whether inline JSON-LD or inline config is used
- nonce/hash guidance when inline scripts are unavoidable

### XSS hardening

- answer HTML must be sanitized before render
- integrations must treat remote HTML as untrusted unless it comes from the canonical sanitized public contract
- all platform template outputs must escape non-HTML fields by default

### SSR injection safety

- JSON serialization must escape closing tags and unsafe sequences
- template helpers must not interpolate raw unescaped configuration into scripts or attributes

### CMS admin credential handling

- use host-platform secret/config storage correctly
- restrict access by capability
- never expose secrets in diagnostics screens without intentional redaction

### Dependency hygiene

- track dependency updates per integration
- run vulnerability scans
- minimize transitive dependencies in browser-delivered packages

### Supply chain risk

- signed artifacts and provenance should be introduced as the release system matures
- avoid pulling runtime code from mutable third-party CDNs during build and test

## 15. CI/CD & Release Strategy

### Build and release architecture

`/integrations` should have path-aware and manifest-aware CI.

Recommended pipeline stages:

1. detect changed integrations
2. validate manifests and docs completeness
3. run category-specific test matrices
4. build release artifacts
5. publish to registries or marketplaces
6. create GitHub release evidence

### Changed-package detection

Use:

- path filters for coarse detection
- `VERSION.json` and manifest graph validation for fine-grained release decisions

### Matrix builds

Matrix dimensions should include:

- integration path
- runtime version
- platform version where applicable
- release channel

### Docs generation

CI should generate:

- compatibility indexes
- docs navigation indexes
- package catalog summaries

### Publishing

Platform-specific publish jobs:

- npm publish
- Packagist/Composer packaging
- NuGet publish
- PyPI publish
- zip artifact upload
- Shopify or marketplace deployment workflows where supported

### GitHub Releases

GitHub Releases should aggregate:

- changelog excerpt
- artifacts
- checksums
- compatibility notes

### Compatibility checks

Before stable release:

- validate supported BaseFAQ public API versions
- run example smoke tests
- validate schema behavior
- run sandbox install checks for CMS/commerce platforms when applicable

### Release channels

Recommended channels:

- `canary`
- `beta`
- `stable`
- `lts` for select mature integrations

## 16. SEO & Structured Data Strategy Across Platforms

This policy is critical. Every integration must follow the same structured-data ownership rules.

### 16.1 Canonical ownership rule

Exactly one renderer on a page should own FAQPage JSON-LD emission.

Supported ownership modes:

- `canonical`: this integration emits schema
- `mirror`: this integration renders FAQ UI but does not emit schema
- `off`: this integration renders no schema and may even suppress SEO-specific extras

### 16.2 When FAQPage JSON-LD should be emitted

Emit FAQPage when:

- the FAQ content is visible to end users
- the page is indexable
- the page materially owns the FAQ content
- schema is not already emitted elsewhere on the page

### 16.3 When FAQPage JSON-LD should not be emitted

Do not emit when:

- the page is preview, admin, sandbox, or noindex
- the FAQ block is only a mirrored copy on a non-canonical page
- the same page already emits equivalent FAQPage data
- the content is lazy-loaded in a way that the page does not truly own it

### 16.4 Server-rendered preferred behavior

Server-rendered schema is the preferred mode across platforms. It is the most reliable approach for:

- crawlability
- deterministic page output
- deduplication
- compatibility with strict CSP environments

### 16.5 Client-injected fallback behavior

Client-side schema injection is a fallback only when:

- the platform cannot reasonably emit server-side HTML or JSON-LD
- the host explicitly accepts weaker SEO guarantees

Client-injected schema must still dedupe and must be configurable off by default for embed-first low-code scenarios.

### 16.6 CMS coexistence with SEO plugins

CMS integrations must expose clear schema ownership settings and suppression hooks. BaseFAQ should not assume it can outsmart Yoast, Rank Math, or other SEO systems automatically.

### 16.7 Avoiding duplicate schema

Deduplication should be based on:

- request-level schema registry in server environments
- root instance registry in browser environments
- page-level ownership config in CMS plugins

### 16.8 Content freshness and caching

If HTML is cached but JSON-LD reflects stale FAQ items, the integration is broken. Structured data must follow the same invalidation or revalidation path as visible content.

### 16.9 Canonical content ownership

BaseFAQ should remain the canonical FAQ content source. Integrations may render, cache, or project that content, but they should not silently fork ownership.

## 17. Extensibility Strategy

Integrations need extension points, but those extension points must not allow consumers to break content semantics or structured data integrity accidentally.

### Approved extension patterns

- hooks and lifecycle callbacks in SDKs and embeds
- middleware/interceptors in transport layers
- slots and render overrides in framework packages
- theme tokens and CSS variables
- template overrides in CMS and commerce plugins
- filters and actions in WordPress
- adapter interfaces for CMS mappings

### Guardrails

Do not allow extension points to override:

- required FAQ semantic structure without explicit advanced mode
- sanitized content boundaries
- structured data dedupe rules
- tenant or credential safety rules

### Recommended boundaries

- content transforms may adjust presentation but not mutate canonical IDs silently
- theme extensions may style, not rewrite content contracts
- transport middleware may augment headers and caching, not bypass security policies
- telemetry hooks may observe render lifecycle, not alter schema ownership implicitly

### Telemetry hooks

Optional hooks are useful for:

- render success/failure
- FAQ item expansion
- API latency
- cache hits/misses

Telemetry should never require extra data collection to render FAQs successfully.

## 18. Prioritized Rollout Roadmap

### Phase 1: foundation and highest ROI channels

- `foundation/html`
- `shared/*`
- `embed/web`
- `embed/iframe`
- `embed/cdn`
- `sdks/javascript`
- `sdks/node`
- `frameworks/react`
- `frameworks/nextjs`
- `cms/wordpress`

Rationale:

- establishes canonical rendering and shared SEO rules
- covers universal embed
- covers dominant JS ecosystems
- covers WordPress early because it is one of the biggest FAQ distribution targets

### Phase 2: high-value expansion

- `embed/web-component`
- `commerce/woocommerce`
- `commerce/shopify`
- `frameworks/vue`
- `frameworks/nuxt`
- `static/astro`
- `static/eleventy`
- `lowcode/webflow`
- `sdks/php`

Rationale:

- extends strong ecosystems after the core contract is stable
- prioritizes commerce and SEO-first static channels
- adds PHP SDK after WordPress proves the server-side PHP demand

### Phase 3: broader ecosystem and enterprise channels

- `frameworks/angular`
- `frameworks/svelte`
- `frameworks/sveltekit`
- `commerce/magento`
- `cms/drupal`
- `cms/headless/*`
- `sdks/dotnet`
- `sdks/python`
- `static/hugo`
- `static/jekyll`
- `static/docusaurus`
- `lowcode/wix`

Rationale:

- worthwhile platforms, but each has lower immediate ROI or higher maintenance cost
- by this phase the shared contracts, test harnesses, and release tooling should be mature

### Phase 4: optional and demand-led expansion

- `cms/joomla`
- `frameworks/remix`
- `frameworks/qwik`
- `commerce/bigcommerce`
- `static/gatsby`
- `static/mkdocs`

Rationale:

- preserve naming and architectural slots
- implement only when there is clear customer demand or partner pull

## 19. Final Recommended `integrations/` Tree

The tree below is the authoritative repository shape for future scaffolding.

```text
/integrations
  README.md
  /docs
    README.md
    /architecture
    /standards
    /platforms
    /release
    /generated
  /foundation
    /html
      README.md
      CHANGELOG.md
      COMPATIBILITY.md
      SECURITY.md
      VERSION.json
      /docs
      /src
        /contracts
        /templates
        /styles
        /scripts
        /seo
        /i18n
      /examples
      /fixtures
      /tests
  /embed
    /web
      README.md
      CHANGELOG.md
      COMPATIBILITY.md
      SECURITY.md
      VERSION.json
      /docs
      /src
      /examples
      /fixtures
      /tests
      /scripts
      /.release
    /web-component
    /iframe
    /cdn
  /frameworks
    /react
    /nextjs
    /vue
    /nuxt
    /angular
    /svelte
    /sveltekit
    /remix
    /qwik
  /cms
    /wordpress
      README.md
      CHANGELOG.md
      COMPATIBILITY.md
      SECURITY.md
      VERSION.json
      composer.json
      /docs
      /src
        /plugin
        /admin
        /api
        /blocks
        /shortcodes
        /widgets
        /template-tags
        /templates
        /cache
        /seo
        /compatibility
        /i18n
        /security
      /assets
      /examples
      /tests
      /scripts
      /.release
    /drupal
    /joomla
    /headless
      /contentful
      /strapi
      /sanity
      /hygraph
  /commerce
    /woocommerce
    /shopify
    /magento
    /bigcommerce
  /sdks
    /javascript
    /node
    /php
    /dotnet
    /python
  /static
    /astro
    /eleventy
    /hugo
    /jekyll
    /docusaurus
    /gatsby
    /mkdocs
  /headless
    /openapi
    /ssr-templates
    /middleware
    /edge
  /lowcode
    /webflow
    /wix
    /gtm
    /generic-html
  /shared
    /contracts
    /seo
    /rendering
    /fixtures
    /themes
    /testing
    /schemas
    /compatibility
  /templates
    /integration-skeleton
    /docs
    /release
    /examples
  /tooling
    /build
    /release
    /validation
    /generators
    /catalog
```

Implementation rule for the authoritative tree:

- scaffold only Phase 1 folders initially
- reserve the rest through documentation and templates first
- do not create empty placeholder code packages unless they are actively being implemented

## 20. Implementation Notes / Assumptions / Risks

### Assumptions

- BaseFAQ public APIs can provide stable, tenant-safe FAQ content contracts for public consumption
- FAQ answer HTML can be delivered sanitized or can be re-sanitized at the integration boundary
- BaseFAQ wants external channels to preserve SEO and accessibility quality, not just display content somehow

### Intentionally deferred

- deep marketplace automation for every ecosystem
- BigCommerce, Qwik, and Joomla implementation
- full CPT-style WordPress content sync
- native Wix App Market and broad page-builder-specific widgets

### What is optional early

- Web Component variant
- Magento and Drupal
- MkDocs and Gatsby
- advanced edge adapters

### What may be too costly early

- heavy native apps for low-code platforms
- maintaining deep CMS-specific sync models in multiple ecosystems
- supporting multiple authoring models when one primary model already exists

### What should remain internal versus public

Internal:

- tenant admin credentials
- internal management APIs
- unpublished API fields
- internal rendering heuristics or moderation states

Public:

- stable public FAQ render contracts
- public-token read patterns
- normalized render helpers and schema tools

### What may split into separate repositories later

If adoption grows materially, the following could eventually justify separate repositories while keeping `/integrations` as the canonical architecture source:

- `cms/wordpress`
- `commerce/shopify`
- `sdks/javascript`
- `sdks/dotnet`

Until then, keeping them in the monorepo is better for consistency, shared testing, and contract governance.

