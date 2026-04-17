import { SourceKind, VisibilityScope } from '@/shared/constants/backend-enums';

export type SourceDto = {
  id: string;
  tenantId: string;
  kind: SourceKind;
  locator: string;
  label?: string | null;
  scope?: string | null;
  systemName?: string | null;
  externalId?: string | null;
  language?: string | null;
  mediaType?: string | null;
  checksum?: string | null;
  metadataJson?: string | null;
  visibility: VisibilityScope;
  allowsPublicCitation: boolean;
  allowsPublicExcerpt: boolean;
  isAuthoritative: boolean;
  capturedAtUtc?: string | null;
  lastVerifiedAtUtc?: string | null;
};

export type SourceCreateRequestDto = {
  kind: SourceKind;
  locator: string;
  label?: string | null;
  scope?: string | null;
  systemName?: string | null;
  externalId?: string | null;
  language?: string | null;
  mediaType?: string | null;
  checksum?: string | null;
  metadataJson?: string | null;
  visibility: VisibilityScope;
  allowsPublicCitation: boolean;
  allowsPublicExcerpt: boolean;
  isAuthoritative: boolean;
  capturedAtUtc?: string | null;
  markVerified: boolean;
};

export type SourceUpdateRequestDto = SourceCreateRequestDto;
