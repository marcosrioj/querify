import { SpaceKind, VisibilityScope } from '@/shared/constants/backend-enums';
import type { SourceDto } from '@/domains/sources/types';
import type { TagDto } from '@/domains/tags/types';

export type SpaceDto = {
  id: string;
  tenantId: string;
  name: string;
  slug: string;
  summary?: string | null;
  language: string;
  kind: SpaceKind;
  visibility: VisibilityScope;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  publishedAtUtc?: string | null;
  questionCount: number;
};

export type SpaceDetailDto = SpaceDto & {
  tags: TagDto[];
  curatedSources: SourceDto[];
};

export type SpaceCreateRequestDto = {
  name: string;
  slug: string;
  language: string;
  summary?: string | null;
  kind: SpaceKind;
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
