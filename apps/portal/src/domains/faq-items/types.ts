export type FaqItemDto = {
  id: string;
  question: string;
  shortAnswer: string;
  answer?: string | null;
  additionalInfo?: string | null;
  ctaTitle?: string | null;
  ctaUrl?: string | null;
  sort: number;
  voteScore: number;
  aiConfidenceScore: number;
  isActive: boolean;
  faqId: string;
  contentRefId?: string | null;
};

export type FaqItemCreateRequestDto = {
  question: string;
  shortAnswer: string;
  answer?: string | null;
  additionalInfo?: string | null;
  ctaTitle?: string | null;
  ctaUrl?: string | null;
  sort: number;
  voteScore: number;
  aiConfidenceScore: number;
  isActive: boolean;
  faqId: string;
  contentRefId?: string | null;
};

export type FaqItemUpdateRequestDto = FaqItemCreateRequestDto;
