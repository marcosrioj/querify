import { SourceKind, VisibilityScope } from '@/shared/constants/backend-enums';

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
