import {
  ChannelKind,
  QuestionStatus,
  SourceRole,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import type { ActivityDto } from '@/domains/activity/types';
import type { AnswerDto } from '@/domains/answers/types';
import type { SourceDto } from '@/domains/sources/types';
import type { TagDto } from '@/domains/tags/types';

export type QuestionDto = {
  id: string;
  tenantId: string;
  spaceId: string;
  spaceKey: string;
  title: string;
  key: string;
  summary?: string | null;
  contextNote?: string | null;
  status: QuestionStatus;
  visibility: VisibilityScope;
  originChannel: ChannelKind;
  language?: string | null;
  productScope?: string | null;
  journeyScope?: string | null;
  audienceScope?: string | null;
  contextKey?: string | null;
  originUrl?: string | null;
  originReference?: string | null;
  threadSummary?: string | null;
  confidenceScore: number;
  revisionNumber: number;
  acceptedAnswerId?: string | null;
  duplicateOfQuestionId?: string | null;
  answeredAtUtc?: string | null;
  resolvedAtUtc?: string | null;
  validatedAtUtc?: string | null;
  lastActivityAtUtc?: string | null;
  feedbackScore: number;
};

export type QuestionSourceLinkDto = {
  id: string;
  questionId: string;
  sourceId: string;
  role: SourceRole;
  order: number;
  source?: SourceDto | null;
};

export type QuestionDetailDto = QuestionDto & {
  acceptedAnswer?: AnswerDto | null;
  answers: AnswerDto[];
  tags: TagDto[];
  sources: QuestionSourceLinkDto[];
  activity: ActivityDto[];
};

export type QuestionCreateRequestDto = {
  spaceId: string;
  title: string;
  key: string;
  summary?: string | null;
  contextNote?: string | null;
  threadSummary?: string | null;
  status: QuestionStatus;
  visibility: VisibilityScope;
  originChannel: ChannelKind;
  language?: string | null;
  productScope?: string | null;
  journeyScope?: string | null;
  audienceScope?: string | null;
  contextKey?: string | null;
  originUrl?: string | null;
  originReference?: string | null;
  confidenceScore: number;
};

export type QuestionUpdateRequestDto = QuestionCreateRequestDto & {
  acceptedAnswerId?: string | null;
  duplicateOfQuestionId?: string | null;
};

export type QuestionTagCreateRequestDto = {
  questionId: string;
  tagId: string;
};

export type QuestionSourceLinkCreateRequestDto = {
  questionId: string;
  sourceId: string;
  role: SourceRole;
  order: number;
};
