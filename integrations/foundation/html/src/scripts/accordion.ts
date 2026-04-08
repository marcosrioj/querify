/**
 * BaseFAQ Foundation — Accordion Enhancement
 *
 * Progressive enhancement layer on top of native <details>/<summary>.
 * The page is fully functional without this script.
 *
 * Features:
 * - Single-open accordion mode (closes siblings when one opens)
 * - Animated height transition (respects prefers-reduced-motion)
 * - Deep-link expansion via URL hash (#bf-item-{id} or #{slug})
 * - Custom events: bf:open, bf:close, bf:all-closed
 * - Host callbacks for embed integration
 *
 * Usage:
 *   const accordion = new BaseFaqAccordion(rootElement, options);
 *   accordion.open('item_123');
 *   accordion.close('item_123');
 *   accordion.openAll();
 *   accordion.closeAll();
 *   accordion.destroy();
 */

export interface AccordionOptions {
  /** Allow only one item open at a time. Default: false. */
  singleOpen?: boolean;
  /** Animate height changes. Disabled automatically when prefers-reduced-motion is active. Default: true. */
  animated?: boolean;
  /** Expand item linked by the URL hash on load. Default: true. */
  deepLinkOnLoad?: boolean;
  /** Called when an item opens. */
  onOpen?:  (itemId: string, element: HTMLDetailsElement) => void;
  /** Called when an item closes. */
  onClose?: (itemId: string, element: HTMLDetailsElement) => void;
}

export class BaseFaqAccordion {
  private readonly root:    Element;
  private readonly options: Required<AccordionOptions>;
  private readonly details: HTMLDetailsElement[];
  private readonly motionQuery: MediaQueryList;

  constructor(root: Element, options: AccordionOptions = {}) {
    this.root    = root;
    this.options = {
      singleOpen:    options.singleOpen    ?? false,
      animated:      options.animated      ?? true,
      deepLinkOnLoad: options.deepLinkOnLoad ?? true,
      onOpen:        options.onOpen        ?? (() => {}),
      onClose:       options.onClose       ?? (() => {}),
    };
    this.details     = Array.from(root.querySelectorAll<HTMLDetailsElement>('.bf-faq__item'));
    this.motionQuery = window.matchMedia('(prefers-reduced-motion: reduce)');

    this.init();
  }

  // ─── Init ─────────────────────────────────────────────────────────────

  private init(): void {
    for (const detail of this.details) {
      const summary = detail.querySelector<HTMLElement>('.bf-faq__question');
      if (!summary) continue;

      summary.addEventListener('click', (e) => {
        e.preventDefault();
        this.toggle(detail);
      });

      // Keyboard: enter/space handled by browser for summary, but we intercept
      // for custom behavior
      summary.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          this.toggle(detail);
        }
      });
    }

    if (this.options.deepLinkOnLoad) {
      this.handleDeepLink();
      window.addEventListener('hashchange', () => this.handleDeepLink());
    }
  }

  // ─── Toggle ───────────────────────────────────────────────────────────

  private toggle(detail: HTMLDetailsElement): void {
    if (detail.open) {
      this.collapseDetail(detail);
    } else {
      if (this.options.singleOpen) {
        for (const other of this.details) {
          if (other !== detail && other.open) {
            this.collapseDetail(other);
          }
        }
      }
      this.expandDetail(detail);
    }
  }

  // ─── Expand ───────────────────────────────────────────────────────────

  private expandDetail(detail: HTMLDetailsElement): void {
    detail.open = true;

    const answer = detail.querySelector<HTMLElement>('.bf-faq__answer');
    if (answer && this.shouldAnimate()) {
      answer.style.overflow = 'hidden';
      answer.style.height   = '0';
      // Force reflow
      void answer.offsetHeight;
      answer.style.height     = `${answer.scrollHeight}px`;
      answer.style.transition = `height var(--bf-duration-normal) var(--bf-easing-out)`;

      const onEnd = () => {
        answer.style.height     = '';
        answer.style.overflow   = '';
        answer.style.transition = '';
        answer.removeEventListener('transitionend', onEnd);
      };
      answer.addEventListener('transitionend', onEnd);
    }

    const itemId = detail.dataset['basefaqItemId'] ?? '';
    this.options.onOpen(itemId, detail);
    this.root.dispatchEvent(
      new CustomEvent('bf:open', { bubbles: true, detail: { itemId, element: detail } })
    );
  }

  // ─── Collapse ─────────────────────────────────────────────────────────

  private collapseDetail(detail: HTMLDetailsElement): void {
    const answer = detail.querySelector<HTMLElement>('.bf-faq__answer');

    if (answer && this.shouldAnimate()) {
      answer.style.overflow   = 'hidden';
      answer.style.height     = `${answer.scrollHeight}px`;
      // Force reflow
      void answer.offsetHeight;
      answer.style.height     = '0';
      answer.style.transition = `height var(--bf-duration-normal) var(--bf-easing-in)`;

      const onEnd = () => {
        detail.open             = false;
        answer.style.height     = '';
        answer.style.overflow   = '';
        answer.style.transition = '';
        answer.removeEventListener('transitionend', onEnd);
      };
      answer.addEventListener('transitionend', onEnd);
    } else {
      detail.open = false;
    }

    const itemId = detail.dataset['basefaqItemId'] ?? '';
    this.options.onClose(itemId, detail);
    this.root.dispatchEvent(
      new CustomEvent('bf:close', { bubbles: true, detail: { itemId, element: detail } })
    );

    if (this.details.every((d) => !d.open)) {
      this.root.dispatchEvent(new CustomEvent('bf:all-closed', { bubbles: true }));
    }
  }

  // ─── Deep link ────────────────────────────────────────────────────────

  private handleDeepLink(): void {
    const hash = window.location.hash.replace(/^#/, '');
    if (!hash) return;

    // Match data-basefaq-item-id or slug
    const target = this.details.find(
      (d) =>
        d.dataset['basefaqItemId'] === hash ||
        d.id === hash ||
        d.querySelector<HTMLElement>('.bf-faq__question')?.id === hash
    );

    if (target && !target.open) {
      this.expandDetail(target);
      // Smooth scroll
      setTimeout(() => target.scrollIntoView({ behavior: 'smooth', block: 'start' }), 100);
    }
  }

  // ─── Public API ───────────────────────────────────────────────────────

  /** Open item by data-basefaq-item-id. */
  open(itemId: string): void {
    const target = this.details.find((d) => d.dataset['basefaqItemId'] === itemId);
    if (target && !target.open) this.expandDetail(target);
  }

  /** Close item by data-basefaq-item-id. */
  close(itemId: string): void {
    const target = this.details.find((d) => d.dataset['basefaqItemId'] === itemId);
    if (target?.open) this.collapseDetail(target);
  }

  /** Open all items. */
  openAll(): void {
    for (const detail of this.details) {
      if (!detail.open) this.expandDetail(detail);
    }
  }

  /** Close all items. */
  closeAll(): void {
    for (const detail of this.details) {
      if (detail.open) this.collapseDetail(detail);
    }
  }

  /** Clean up all event listeners. */
  destroy(): void {
    window.removeEventListener('hashchange', () => this.handleDeepLink());
  }

  // ─── Helpers ──────────────────────────────────────────────────────────

  private shouldAnimate(): boolean {
    return this.options.animated && !this.motionQuery.matches;
  }
}
