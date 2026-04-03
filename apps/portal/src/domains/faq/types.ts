import {
  CtaTarget,
  FaqSortStrategy,
  FaqStatus,
} from '@/shared/constants/backend-enums';

export type FaqDto = {
  id: string;
  name: string;
  language: string;
  status: FaqStatus;
  sortStrategy: FaqSortStrategy;
  ctaEnabled: boolean;
  ctaTarget: CtaTarget;
};

export type FaqCreateRequestDto = {
  name: string;
  language: string;
  status: FaqStatus;
  sortStrategy: FaqSortStrategy;
  ctaEnabled: boolean;
  ctaTarget: CtaTarget;
};

export type FaqUpdateRequestDto = FaqCreateRequestDto;
