import {
  AnswerKind,
  AnswerStatus,
  QuestionStatus,
  SourceGenerationRunStatus,
  SourceGenerationTagMode,
  SourceRole,
  SourceUploadStatus,
  SpaceStatus,
  VisibilityScope,
} from "@/shared/constants/backend-enums";

export type SourceDto = {
  id: string;
  tenantId: string;
  locator: string;
  storageKey?: string | null;
  label?: string | null;
  contextNote?: string | null;
  externalId?: string | null;
  language: string;
  mediaType?: string | null;
  sizeBytes?: number | null;
  checksum: string;
  metadataJson?: string | null;
  createdAtUtc?: string | null;
  lastUpdatedAtUtc?: string | null;
  uploadStatus: SourceUploadStatus;
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
  locator: string;
  label?: string | null;
  contextNote?: string | null;
  externalId?: string | null;
  language: string;
  mediaType?: string | null;
  metadataJson?: string | null;
};

export type SourceUpdateRequestDto = Omit<SourceCreateRequestDto, "locator">;

export type SourceExternalUrlInspectionRequestDto = {
  locator: string;
};

export type SourceExternalUrlInspectionDto = {
  isReachable: boolean;
  status?: number | null;
  statusText?: string | null;
  finalUrl?: string | null;
  contentType?: string | null;
  contentLengthBytes?: number | null;
  lastModified?: string | null;
};

export type SourceUploadIntentRequestDto = {
  fileName: string;
  contentType: string;
  sizeBytes: number;
  language: string;
  label?: string | null;
  contextNote?: string | null;
  externalId?: string | null;
  metadataJson?: string | null;
};

export type SourceUploadIntentResponseDto = {
  sourceId: string;
  uploadUrl: string;
  requiredHeaders: Record<string, string>;
  storageKey: string;
  expiresAtUtc: string;
};

export type SourceUploadCompleteRequestDto = {
  clientChecksum?: string | null;
};

export type SourceDownloadUrlDto = {
  url: string;
  expiresAtUtc: string;
};

export type SourceGenerateSpaceRequestDto = {
  spaceName: string;
  spaceSlug?: string | null;
  language: string;
  visibility: VisibilityScope;
  status: SpaceStatus;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  extractionGoal?: string | null;
  maxTopLevelQuestions: number;
  maxFollowUpDepth: number;
  maxAnswersPerQuestion: number;
  includeFollowUpQuestions: boolean;
  tagGenerationMode: SourceGenerationTagMode;
  sourceRole: SourceRole;
  requireEveryAnswerToCiteSource: boolean;
  contentHint?: string | null;
};

export type SourceGenerationRunDto = {
  id: string;
  sourceId: string;
  createdSpaceId?: string | null;
  status: SourceGenerationRunStatus;
  failureReason?: string | null;
  warning?: string | null;
  spaceName: string;
  spaceSlug?: string | null;
  language: string;
  visibility: VisibilityScope;
  spaceStatus: SpaceStatus;
  acceptsQuestions: boolean;
  acceptsAnswers: boolean;
  extractionGoal?: string | null;
  maxTopLevelQuestions: number;
  maxFollowUpDepth: number;
  maxAnswersPerQuestion: number;
  includeFollowUpQuestions: boolean;
  tagGenerationMode: SourceGenerationTagMode;
  sourceRole: SourceRole;
  requireEveryAnswerToCiteSource: boolean;
  contentHint?: string | null;
  createdAtUtc: string;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
};

export type SourceGenerationRunSummaryDto = {
  id: string;
  sourceId: string;
  createdSpaceId?: string | null;
  status: SourceGenerationRunStatus;
  failureReason?: string | null;
  spaceName: string;
  tagGenerationMode: SourceGenerationTagMode;
  createdAtUtc: string;
  completedAtUtc?: string | null;
};
