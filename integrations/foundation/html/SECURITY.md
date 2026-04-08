# Security

## Public credential model

The BaseFAQ HTML foundation uses `X-Client-Key` — a **public tenant-safe credential**.

- It is safe to embed in browser-delivered HTML, JavaScript, and client-side configuration
- It is **not** a secret admin token or a signing key
- It cannot be used to access admin-only operations
- Treat it like a public API key, not a password

## Content safety

- `answerHtml` content from the API is already sanitized by the BaseFAQ content pipeline
- The copy-paste snippet renders `answerHtml` as raw HTML inside `.bfq__abody` — only embed from trusted BaseFAQ API responses, never from user-supplied strings
- All user-visible strings in the examples are HTML-escaped before insertion via the `esc()` helper

## XSS prevention in JSON-LD

The `buildFaqPageJsonLd` helper and all example implementations replace `</script>` with `<\/script>` inside JSON-LD `<script>` blocks to prevent script injection via injected closing tags.

## Reporting vulnerabilities

Report security issues via the project's private disclosure channel or open a GitHub security advisory at the repository level.
