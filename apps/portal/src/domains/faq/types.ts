import { FaqStatus } from '@/shared/constants/backend-enums';

export type FaqDto = {
  id: string;
  name: string;
  language: string;
  status: FaqStatus;
  updatedDate: string | null;
};

export type FaqCreateRequestDto = {
  name: string;
  language: string;
  status: FaqStatus;
};

export type FaqUpdateRequestDto = FaqCreateRequestDto;
