# Compatibility Matrix

## BaseFAQ Public API

| HTML Foundation | Public API | Status  |
|-----------------|-----------|---------|
| 0.1.x           | 1.0       | Supported |

## Browser support

The canonical HTML path (`details`/`summary` accordion) works without JavaScript in any browser that supports the HTML5 `details` element.

| Browser        | No-JS rendering | JS enhancements |
|---------------|-----------------|-----------------|
| Chrome 80+    | ✓               | ✓               |
| Firefox 79+   | ✓               | ✓               |
| Safari 14+    | ✓               | ✓               |
| Edge 80+      | ✓               | ✓               |
| IE 11         | ✗               | ✗               |

## CSS custom properties

All tokens use CSS custom properties (`var(--bf-*)`). IE 11 is not supported. All evergreen browsers are supported.

## API endpoints

| Endpoint                  | Method | Required header  | Used for               |
|--------------------------|--------|-----------------|------------------------|
| `/api/faqs/faq`          | GET    | `X-Client-Key`  | List all FAQs          |
| `/api/faqs/faq/{id}`     | GET    | `X-Client-Key`  | Get FAQ by ID          |
| `/api/faqs/vote`         | POST   | `X-Client-Key`  | Submit item vote       |

## Structured data

FAQPage JSON-LD is validated against the Google Rich Results schema.
See: https://developers.google.com/search/docs/appearance/structured-data/faqpage
