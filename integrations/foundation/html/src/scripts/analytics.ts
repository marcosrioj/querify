/**
 * BaseFAQ Foundation — Analytics Hooks
 *
 * Provides a standardized event surface for host pages to hook into
 * BaseFAQ user interaction events. No data is sent anywhere by default.
 * Integrators attach their own tracking logic via the callback API or
 * by listening for custom events on the root element.
 *
 * Custom DOM events emitted:
 *   bf:faq-view       — FAQ root entered viewport
 *   bf:item-open      — user expanded an FAQ item
 *   bf:item-close     — user collapsed an FAQ item
 *   bf:feedback-up        — user selected up on an item
 *   bf:feedback-down      — user selected down on an item
 *   bf:cta-click      — user clicked a CTA link
 *   bf:source-click   — user clicked a source/content-ref link
 *   bf:search         — user submitted a search query
 *   bf:page-change    — user navigated to another page of results
 */

export interface BfAnalyticsEvent {
  type:    string;
  faqId?:  string;
  itemId?: string;
  meta?:   Record<string, unknown>;
}

export type AnalyticsHandler = (event: BfAnalyticsEvent) => void;

export interface AnalyticsOptions {
  /** Track FAQ visibility via IntersectionObserver. Default: true. */
  trackVisibility?: boolean;
  /** Minimum milliseconds an item is visible before triggering bf:faq-view. Default: 1000. */
  visibilityThreshold?: number;
  /** Called for every tracked event. */
  onEvent?: AnalyticsHandler;
}

export class BaseFaqAnalytics {
  private readonly root:      Element;
  private readonly options:   Required<AnalyticsOptions>;
  private readonly observers: IntersectionObserver[] = [];

  constructor(root: Element, options: AnalyticsOptions = {}) {
    this.root    = root;
    this.options = {
      trackVisibility:    options.trackVisibility    ?? true,
      visibilityThreshold: options.visibilityThreshold ?? 1000,
      onEvent:            options.onEvent            ?? (() => {}),
    };
    this.init();
  }

  // ─── Init ─────────────────────────────────────────────────────────────

  private init(): void {
    if (this.options.trackVisibility && 'IntersectionObserver' in window) {
      this.trackFaqVisibility();
    }

    this.root.addEventListener('bf:open',  (e) => {
      const { itemId } = (e as CustomEvent).detail ?? {};
      this.emit({ type: 'bf:item-open', faqId: this.faqId(), itemId });
    });

    this.root.addEventListener('bf:close', (e) => {
      const { itemId } = (e as CustomEvent).detail ?? {};
      this.emit({ type: 'bf:item-close', faqId: this.faqId(), itemId });
    });

    // Feedback buttons
    this.root.addEventListener('click', (e) => {
      const target = e.target as Element;
      const feedbackBtn = target.closest<HTMLElement>('.bf-faq__feedback-btn');
      if (!feedbackBtn) return;
      const item   = feedbackBtn.closest<HTMLElement>('[data-basefaq-item-id]');
      const itemId = item?.dataset['basefaqItemId'];
      const isUp   = feedbackBtn.classList.contains('bf-faq__feedback-btn--up');
      this.emit({
        type:   isUp ? 'bf:feedback-up' : 'bf:feedback-down',
        faqId:  this.faqId(),
        itemId,
      });
    });

    // CTA clicks
    this.root.addEventListener('click', (e) => {
      const target = e.target as Element;
      const cta = target.closest<HTMLAnchorElement>('.bf-faq__cta');
      if (!cta) return;
      const item   = cta.closest<HTMLElement>('[data-basefaq-item-id]');
      const itemId = item?.dataset['basefaqItemId'];
      this.emit({
        type:   'bf:cta-click',
        faqId:  this.faqId(),
        itemId,
        meta:   { href: cta.href },
      });
    });

    // Source link clicks
    this.root.addEventListener('click', (e) => {
      const target = e.target as Element;
      const link = target.closest<HTMLAnchorElement>('.bf-faq__source-link');
      if (!link) return;
      const item   = link.closest<HTMLElement>('[data-basefaq-item-id]');
      const itemId = item?.dataset['basefaqItemId'];
      this.emit({
        type:   'bf:source-click',
        faqId:  this.faqId(),
        itemId,
        meta:   { href: link.href, label: link.textContent?.trim() },
      });
    });
  }

  // ─── Visibility tracking ──────────────────────────────────────────────

  private trackFaqVisibility(): void {
    let visibleAt: number | null = null;

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            visibleAt = Date.now();
          } else if (visibleAt !== null) {
            const duration = Date.now() - visibleAt;
            if (duration >= this.options.visibilityThreshold) {
              this.emit({ type: 'bf:faq-view', faqId: this.faqId(), meta: { durationMs: duration } });
            }
            visibleAt = null;
          }
        }
      },
      { threshold: 0.3 }
    );

    observer.observe(this.root);
    this.observers.push(observer);
  }

  // ─── Emit ─────────────────────────────────────────────────────────────

  private emit(event: BfAnalyticsEvent): void {
    // Call host callback
    this.options.onEvent(event);

    // Dispatch DOM event
    this.root.dispatchEvent(
      new CustomEvent(event.type, {
        bubbles:  true,
        composed: true,
        detail:   event,
      })
    );
  }

  // ─── Helpers ──────────────────────────────────────────────────────────

  private faqId(): string | undefined {
    return (this.root as HTMLElement).dataset['basefaqFaqId'];
  }

  /** Emit a custom event manually (for host-page use). */
  track(event: BfAnalyticsEvent): void {
    this.emit(event);
  }

  /** Clean up observers. */
  destroy(): void {
    for (const obs of this.observers) obs.disconnect();
  }
}
