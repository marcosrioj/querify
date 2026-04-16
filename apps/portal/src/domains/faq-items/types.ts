export type FaqItemAnswerDto = {
  id: string;
  shortAnswer: string;
  answer?: string | null;
  sort: number;
  voteScore: number;
  isActive: boolean;
  faqItemId: string;
};

export type FaqItemAnswerCreateRequestDto = {
  shortAnswer: string;
  answer?: string | null;
  sort: number;
  isActive: boolean;
  faqItemId: string;
};

export type FaqItemAnswerUpdateRequestDto = FaqItemAnswerCreateRequestDto;

export type FaqItemDto = {
  id: string;
  question: string;
  shortAnswer?: string | null;
  answer?: string | null;
  answers: FaqItemAnswerDto[];
  additionalInfo?: string | null;
  ctaTitle?: string | null;
  ctaUrl?: string | null;
  sort: number;
  feedbackScore: number;
  confidenceScore: number;
  isActive: boolean;
  faqId: string;
  contentRefId?: string | null;
};

export type FaqItemCreateRequestDto = {
  question: string;
  additionalInfo?: string | null;
  ctaTitle?: string | null;
  ctaUrl?: string | null;
  sort: number;
  isActive: boolean;
  faqId: string;
  contentRefId?: string | null;
};

export type FaqItemUpdateRequestDto = FaqItemCreateRequestDto;
