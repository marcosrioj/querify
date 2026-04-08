/**
 * BaseFAQ Public API Client
 *
 * Typed client for the BaseFAQ Public API.
 * Base URL: https://dev.faq.public.basefaq.com
 *
 * Authentication: every request must carry the X-Client-Key header.
 * This key is a public tenant-safe credential — never a secret admin token.
 */

// ─── Enums ────────────────────────────────────────────────────────────────

export const FaqStatus = {
  Draft:     0,
  Published: 1,
  Archived:  2,
} as const;
export type FaqStatus = (typeof FaqStatus)[keyof typeof FaqStatus];

export const CtaTarget = {
  Self:  0,
  Blank: 1,
} as const;
export type CtaTarget = (typeof CtaTarget)[keyof typeof CtaTarget];

export const ContentRefKind = {
  Manual:     1,
  Web:        2,
  Pdf:        3,
  Document:   4,
  Video:      5,
  Repository: 6,
  Faq:        7,
  FaqItem:    8,
  Other:      99,
} as const;
export type ContentRefKind = (typeof ContentRefKind)[keyof typeof ContentRefKind];

export const UnLikeReason = {
  ConfusingOrUnclear:     1,
  NotRelevant:            2,
  RelevantButNotHelpful:  3,
  LengthIssue:            4,
} as const;
export type UnLikeReason = (typeof UnLikeReason)[keyof typeof UnLikeReason];

// ─── DTOs ─────────────────────────────────────────────────────────────────

export interface TagDto {
  id:    string;
  value: string;
}

export interface ContentRefDto {
  id:      string;
  kind:    ContentRefKind;
  locator: string;
  label:   string | null;
  scope:   string | null;
}

export interface FaqItemDto {
  id:                string;
  question:          string;
  shortAnswer:       string;
  answer:            string | null;
  additionalInfo:    string | null;
  ctaTitle:          string | null;
  ctaUrl:            string | null;
  sort:              number;
  voteScore:         number;
  aiConfidenceScore: number;
  isActive:          boolean;
  faqId:             string;
  contentRefId:      string | null;
}

export interface FaqDetailDto {
  id:           string;
  name:         string;
  language:     string;
  status:       FaqStatus;
  sortStrategy: number;
  ctaEnabled:   boolean;
  ctaTarget:    CtaTarget;
  items:        FaqItemDto[]    | null;
  contentRefs:  ContentRefDto[] | null;
  tags:         TagDto[]        | null;
}

export interface PagedResultDto<T> {
  items:      T[];
  totalCount: number;
}

// ─── Request params ────────────────────────────────────────────────────────

export interface FaqGetRequestParams {
  includeFaqItems:    boolean;
  includeContentRefs: boolean;
  includeTags:        boolean;
}

export interface FaqGetAllRequestParams {
  searchText?:        string;
  status?:            FaqStatus;
  faqIds?:            string[];
  includeFaqItems:    boolean;
  includeContentRefs: boolean;
  includeTags:        boolean;
  skipCount?:         number;
  maxResultCount?:    number;
  sorting?:           string;
}

export interface VoteCreateRequestDto {
  like:          boolean;
  unlikeReason?: UnLikeReason;
  faqItemId:     string;
}

// ─── API error ─────────────────────────────────────────────────────────────

export interface ApiErrorResponse {
  errorCode:    number;
  messageError: string;
  data:         unknown;
}

export class BaseFaqApiError extends Error {
  constructor(
    public readonly statusCode: number,
    public readonly apiError:   ApiErrorResponse | null,
    message: string
  ) {
    super(message);
    this.name = 'BaseFaqApiError';
  }
}

// ─── Client ────────────────────────────────────────────────────────────────

export interface BaseFaqClientOptions {
  /** Base URL. Defaults to https://dev.faq.public.basefaq.com */
  baseUrl?:    string;
  /** Public tenant client key. Required for all requests. */
  clientKey:   string;
  /** Optional AbortSignal to cancel requests. */
  signal?:     AbortSignal;
  /** Optional fetch override for testing or SSR environments. */
  fetchFn?:    typeof fetch;
}

export class BaseFaqClient {
  private readonly baseUrl: string;
  private readonly clientKey: string;
  private readonly fetchFn: typeof fetch;

  constructor(options: BaseFaqClientOptions) {
    this.baseUrl   = (options.baseUrl ?? 'https://dev.faq.public.basefaq.com').replace(/\/$/, '');
    this.clientKey = options.clientKey;
    this.fetchFn   = options.fetchFn ?? globalThis.fetch.bind(globalThis);
  }

  private buildHeaders(): HeadersInit {
    return {
      'Content-Type':  'application/json',
      'Accept':        'application/json',
      'X-Client-Key':  this.clientKey,
    };
  }

  private async request<T>(path: string, init?: RequestInit): Promise<T> {
    const url      = `${this.baseUrl}${path}`;
    const response = await this.fetchFn(url, {
      ...init,
      headers: { ...this.buildHeaders(), ...init?.headers },
    });

    if (!response.ok) {
      let apiError: ApiErrorResponse | null = null;
      try { apiError = await response.json() as ApiErrorResponse; } catch { /* ignore */ }
      throw new BaseFaqApiError(
        response.status,
        apiError,
        apiError?.messageError ?? `HTTP ${response.status}`
      );
    }

    return response.json() as Promise<T>;
  }

  private buildQuery(params: Record<string, unknown>): string {
    const qs = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) {
      if (value === undefined || value === null) continue;
      if (Array.isArray(value)) {
        for (const item of value) qs.append(key, String(item));
      } else {
        qs.set(key, String(value));
      }
    }
    const str = qs.toString();
    return str ? `?${str}` : '';
  }

  /** GET /api/faqs/faq — list FAQs with optional filters and includes. */
  async getFaqs(params: FaqGetAllRequestParams): Promise<PagedResultDto<FaqDetailDto>> {
    const query = this.buildQuery({
      searchText:         params.searchText,
      status:             params.status,
      faqIds:             params.faqIds,
      includeFaqItems:    params.includeFaqItems,
      includeContentRefs: params.includeContentRefs,
      includeTags:        params.includeTags,
      skipCount:          params.skipCount ?? 0,
      maxResultCount:     params.maxResultCount ?? 10,
      sorting:            params.sorting,
    });
    return this.request<PagedResultDto<FaqDetailDto>>(`/api/faqs/faq${query}`);
  }

  /** GET /api/faqs/faq/{id} — get a single FAQ by ID. */
  async getFaqById(id: string, params: FaqGetRequestParams): Promise<FaqDetailDto> {
    const query = this.buildQuery({
      includeFaqItems:    params.includeFaqItems,
      includeContentRefs: params.includeContentRefs,
      includeTags:        params.includeTags,
    });
    return this.request<FaqDetailDto>(`/api/faqs/faq/${id}${query}`);
  }

  /** POST /api/faqs/vote — submit a vote for an FAQ item. */
  async vote(dto: VoteCreateRequestDto): Promise<string> {
    return this.request<string>('/api/faqs/vote', {
      method: 'POST',
      body:   JSON.stringify(dto),
    });
  }
}
