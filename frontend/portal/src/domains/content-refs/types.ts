import { ContentRefKind } from '@/shared/constants/backend-enums';

export type ContentRefDto = {
  id: string;
  kind: ContentRefKind;
  locator: string;
  label?: string | null;
  scope?: string | null;
};

export type ContentRefCreateRequestDto = {
  kind: ContentRefKind;
  locator: string;
  label?: string | null;
  scope?: string | null;
};

export type ContentRefUpdateRequestDto = ContentRefCreateRequestDto;
