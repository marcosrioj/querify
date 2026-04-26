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
  summary?: string | null;
  contextNote?: string | null;
  status: QuestionStatus;
  visibility: VisibilityScope;
  originChannel: ChannelKind;
  aiConfidenceScore: number;
  feedbackScore: number;
  sort: number;
  acceptedAnswerId?: string | null;
  duplicateOfQuestionId?: string | null;
  answeredAtUtc?: string | null;
  resolvedAtUtc?: string | null;
  validatedAtUtc?: string | null;
  lastActivityAtUtc?: string | null;
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
  summary?: string | null;
  contextNote?: string | null;
  status: QuestionStatus;
  visibility: VisibilityScope;
  originChannel: ChannelKind;
  aiConfidenceScore: number;
  feedbackScore: number;
  sort: number;
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
