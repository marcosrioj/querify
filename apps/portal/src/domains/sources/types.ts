import {
  AnswerKind,
  AnswerStatus,
  QuestionStatus,
  SourceKind,
  SourceRole,
  SpaceStatus,
  VisibilityScope,
} from '@/shared/constants/backend-enums';

export type SourceDto = {
  id: string;
  tenantId: string;
  kind: SourceKind;
  locator: string;
  label?: string | null;
  contextNote?: string | null;
  externalId?: string | null;
  language: string;
  mediaType?: string | null;
  checksum: string;
  metadataJson?: string | null;
  visibility: VisibilityScope;
  lastVerifiedAtUtc?: string | null;
  spaceUsageCount: number;
  questionUsageCount: number;
  answerUsageCount: number;
};

export type SourceSpaceRelationshipDto = {
  id: string;
  spaceId: string;
  name: string;
  slug: string;
  summary?: string | null;
  status: SpaceStatus;
  visibility: VisibilityScope;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  questionCount: number;
};

export type SourceQuestionRelationshipDto = {
  id: string;
  questionId: string;
  spaceId: string;
  spaceSlug: string;
  title: string;
  summary?: string | null;
  status: QuestionStatus;
  visibility: VisibilityScope;
  role: SourceRole;
  order: number;
  lastActivityAtUtc?: string | null;
};

export type SourceAnswerRelationshipDto = {
  id: string;
  answerId: string;
  questionId: string;
  questionTitle: string;
  headline: string;
  kind: AnswerKind;
  status: AnswerStatus;
  visibility: VisibilityScope;
  role: SourceRole;
  order: number;
  isAccepted: boolean;
};

export type SourceDetailDto = SourceDto & {
  spaces: SourceSpaceRelationshipDto[];
  questions: SourceQuestionRelationshipDto[];
  answers: SourceAnswerRelationshipDto[];
};

export type SourceCreateRequestDto = {
  kind: SourceKind;
  locator: string;
  label?: string | null;
  contextNote?: string | null;
  externalId?: string | null;
  language: string;
  mediaType?: string | null;
  metadataJson?: string | null;
  visibility: VisibilityScope;
  markVerified: boolean;
};

export type SourceUpdateRequestDto = SourceCreateRequestDto;
