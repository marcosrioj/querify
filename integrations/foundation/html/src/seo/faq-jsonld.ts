/**
 * BaseFAQ Foundation — FAQPage JSON-LD Builder
 *
 * Emits a Google-compliant FAQPage structured data block.
 * Rules:
 * - Emit one FAQPage block per canonical page render when schemaMode = 'canonical'
 * - Use questionText and answerPlainText
 * - Keep item order stable
 * - Only include active, visible FAQ items
 * - Suppress emission for preview/admin/sandbox/demo contexts
 *
 * Reference: https://schema.org/FAQPage
 * Google guide: https://developers.google.com/search/docs/appearance/structured-data/faqpage
 */

// ─── Types ────────────────────────────────────────────────────────────────

export interface JsonLdFaqItem {
  questionText:    string;
  answerPlainText: string;
}

export interface JsonLdFaqInput {
  faqId:        string;
  canonicalUrl: string | null;
  items:        JsonLdFaqItem[];
  /** 'canonical' = emit. 'mirror' = emit with isBasedOn. 'off' = suppress. */
  schemaMode:   'canonical' | 'mirror' | 'off';
  /** Origin URL that owns the canonical schema. Required when schemaMode = 'mirror'. */
  canonicalOrigin?: string;
}

export interface FaqPageJsonLd {
  '@context':        'https://schema.org';
  '@type':           'FAQPage';
  mainEntity:        QuestionJsonLd[];
  url?:              string;
}

export interface QuestionJsonLd {
  '@type':           'Question';
  name:              string;
  acceptedAnswer:    AnswerJsonLd;
}

export interface AnswerJsonLd {
  '@type':    'Answer';
  text:       string;
}

// ─── Builder ──────────────────────────────────────────────────────────────

/**
 * Build a FAQPage JSON-LD object from a normalized input.
 * Returns null when emission is suppressed (schemaMode = 'off' or no items).
 */
export function buildFaqPageJsonLd(input: JsonLdFaqInput): FaqPageJsonLd | null {
  if (input.schemaMode === 'off') return null;
  if (!input.items.length) return null;

  const mainEntity: QuestionJsonLd[] = input.items.map((item) => ({
    '@type': 'Question',
    name:    item.questionText.trim(),
    acceptedAnswer: {
      '@type': 'Answer',
      text:    item.answerPlainText.trim(),
    },
  }));

  const schema: FaqPageJsonLd = {
    '@context':  'https://schema.org',
    '@type':     'FAQPage',
    mainEntity,
  };

  if (input.canonicalUrl) {
    schema.url = input.canonicalUrl;
  }

  return schema;
}

/**
 * Serialize JSON-LD to a <script type="application/ld+json"> string.
 * The output is safe to inject into a document head.
 */
export function serializeFaqPageJsonLd(schema: FaqPageJsonLd): string {
  // Escape </script> within JSON values to prevent XSS via injected closing tags
  const json = JSON.stringify(schema, null, 2)
    .replace(/<\/script>/gi, '<\\/script>');
  return `<script type="application/ld+json">\n${json}\n</script>`;
}

/**
 * Inject JSON-LD into the document <head>.
 * Idempotent: removes any existing BaseFAQ JSON-LD block for the same faqId before injecting.
 */
export function injectFaqPageJsonLd(schema: FaqPageJsonLd, faqId: string): void {
  // Remove existing block for this faqId
  const existing = document.querySelector<HTMLScriptElement>(
    `script[type="application/ld+json"][data-bf-faq-id="${faqId}"]`
  );
  existing?.remove();

  const script = document.createElement('script');
  script.type = 'application/ld+json';
  script.dataset['bfFaqId'] = faqId;
  script.textContent = JSON.stringify(schema, null, 2)
    .replace(/<\/script>/gi, '<\\/script>');

  document.head.appendChild(script);
}

/**
 * Strip all HTML tags from a string to produce plain text.
 * Used to derive answerPlainText from answerHtml when not explicitly provided.
 */
export function stripHtml(html: string): string {
  // Use a temporary element for safe, browser-native stripping
  if (typeof document !== 'undefined') {
    const el = document.createElement('div');
    el.innerHTML = html;
    return el.textContent ?? el.innerText ?? '';
  }
  // Fallback for non-browser environments
  return html.replace(/<[^>]+>/g, '').replace(/\s+/g, ' ').trim();
}
