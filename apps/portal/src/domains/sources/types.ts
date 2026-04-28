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
  allowsPublicCitation: boolean;
  allowsPublicExcerpt: boolean;
  isAuthoritative: boolean;
  capturedAtUtc?: string | null;
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
  checksum: string;
  metadataJson?: string | null;
  visibility: VisibilityScope;
  allowsPublicCitation: boolean;
  allowsPublicExcerpt: boolean;
  isAuthoritative: boolean;
  capturedAtUtc?: string | null;
  markVerified: boolean;
};

export type SourceUpdateRequestDto = SourceCreateRequestDto;
