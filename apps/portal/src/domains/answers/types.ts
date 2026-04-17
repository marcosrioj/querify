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
  language?: string | null;
  contextKey?: string | null;
  applicabilityRulesJson?: string | null;
  trustNote?: string | null;
  evidenceSummary?: string | null;
  authorLabel?: string | null;
  confidenceScore: number;
  rank: number;
  revisionNumber: number;
  isAccepted: boolean;
  isOfficial: boolean;
  publishedAtUtc?: string | null;
  validatedAtUtc?: string | null;
  acceptedAtUtc?: string | null;
  retiredAtUtc?: string | null;
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
  language?: string | null;
  contextKey?: string | null;
  applicabilityRulesJson?: string | null;
  trustNote?: string | null;
  evidenceSummary?: string | null;
  authorLabel?: string | null;
  confidenceScore: number;
  rank: number;
};

export type AnswerUpdateRequestDto = AnswerCreateRequestDto;

export type AnswerSourceLinkCreateRequestDto = {
  answerId: string;
  sourceId: string;
  role: SourceRole;
  order: number;
};
