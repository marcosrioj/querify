import { SpaceStatus, VisibilityScope } from '@/shared/constants/backend-enums';
import type { SourceDto } from '@/domains/sources/types';
import type { TagDto } from '@/domains/tags/types';

export type SpaceDto = {
  id: string;
  tenantId: string;
  name: string;
  slug: string;
  summary?: string | null;
  language: string;
  status: SpaceStatus;
  visibility: VisibilityScope;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  questionCount: number;
};

export type SpaceDetailDto = SpaceDto & {
  tags: TagDto[];
  curatedSources: SourceDto[];
};

export type SpaceCreateRequestDto = {
  name: string;
  slug?: string | null;
  language: string;
  summary?: string | null;
  status: SpaceStatus;
  visibility: VisibilityScope;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
};

export type SpaceUpdateRequestDto = SpaceCreateRequestDto;

export type SpaceTagCreateRequestDto = {
  spaceId: string;
  tagId: string;
};

export type SpaceSourceCreateRequestDto = {
  spaceId: string;
  sourceId: string;
};
