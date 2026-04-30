import {
  AnswerKind,
  AnswerStatus,
  SourceRole,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import type { SourceDto } from '@/domains/sources/types';

export type AnswerSourceLinkDto = {
  id: string;
  answerId: string;
  sourceId: string;
  role: SourceRole;
  order: number;
  source?: SourceDto | null;
};

export type AnswerDto = {
  id: string;
  tenantId: string;
  questionId: string;
  headline: string;
  body?: string | null;
  kind: AnswerKind;
  status: AnswerStatus;
  visibility: VisibilityScope;
  contextNote?: string | null;
  authorLabel?: string | null;
  aiConfidenceScore: number;
  score: number;
  sort: number;
  isAccepted: boolean;
  isOfficial: boolean;
  activatedAtUtc?: string | null;
  retiredAtUtc?: string | null;
  lastUpdatedAtUtc?: string | null;
  voteScore: number;
  sources: AnswerSourceLinkDto[];
};

export type AnswerCreateRequestDto = {
  questionId: string;
  headline: string;
  body?: string | null;
  kind: AnswerKind;
  status: AnswerStatus;
  visibility: VisibilityScope;
  contextNote?: string | null;
  authorLabel?: string | null;
  sort: number;
};

export type AnswerUpdateRequestDto = Omit<AnswerCreateRequestDto, 'questionId'>;

export type AnswerSourceLinkCreateRequestDto = {
  answerId: string;
  sourceId: string;
  role: SourceRole;
  order: number;
};
