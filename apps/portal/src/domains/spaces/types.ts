import {
  ModerationPolicy,
  SearchMarkupMode,
  SpaceKind,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import type { SourceDto } from '@/domains/sources/types';
import type { TagDto } from '@/domains/tags/types';

export type SpaceDto = {
  id: string;
  tenantId: string;
  name: string;
  key: string;
  summary?: string | null;
  defaultLanguage: string;
  kind: SpaceKind;
  visibility: VisibilityScope;
  moderationPolicy: ModerationPolicy;
  searchMarkupMode: SearchMarkupMode;
  productScope?: string | null;
  journeyScope?: string | null;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  requiresQuestionReview: boolean;
  requiresAnswerReview: boolean;
  publishedAtUtc?: string | null;
  lastValidatedAtUtc?: string | null;
  questionCount: number;
};

export type SpaceDetailDto = SpaceDto & {
  tags: TagDto[];
  curatedSources: SourceDto[];
};

export type SpaceCreateRequestDto = {
  name: string;
  key: string;
  defaultLanguage: string;
  summary?: string | null;
  kind: SpaceKind;
  visibility: VisibilityScope;
  moderationPolicy: ModerationPolicy;
  searchMarkupMode: SearchMarkupMode;
  productScope?: string | null;
  journeyScope?: string | null;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  requiresQuestionReview: boolean;
  requiresAnswerReview: boolean;
  markValidated: boolean;
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
