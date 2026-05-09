import {
  portalRequest,
  requireAccessToken,
  requireTenantId,
} from "@/platform/api/http-client";
import { toPagedQuery } from "@/shared/lib/pagination";
import type { PagedResultDto } from "@/shared/types/api";
import type {
  SourceCreateRequestDto,
  SourceDownloadUrlDto,
  SourceDetailDto,
  SourceDto,
  SourceExternalUrlInspectionDto,
  SourceExternalUrlInspectionRequestDto,
  SourceUpdateRequestDto,
  SourceUploadCompleteRequestDto,
  SourceUploadIntentRequestDto,
  SourceUploadIntentResponseDto,
} from "@/domains/sources/types";

export function listSources(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    searchText,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<SourceDto>>({
    service: "qna",
    path: "/api/qna/source",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
    }),
    signal,
  });
}

export function getSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  signal?: AbortSignal,
) {
  return portalRequest<SourceDetailDto>({
    service: "qna",
    path: `/api/qna/source/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    signal,
  });
}

export function createSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: SourceCreateRequestDto,
) {
  return portalRequest<string>({
    service: "qna",
    path: "/api/qna/source",
    method: "POST",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: SourceUpdateRequestDto,
) {
  return portalRequest<string>({
    service: "qna",
    path: `/api/qna/source/${id}`,
    method: "PUT",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function deleteSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: "qna",
    path: `/api/qna/source/${id}`,
    method: "DELETE",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createSourceUploadIntent(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: SourceUploadIntentRequestDto,
) {
  return portalRequest<SourceUploadIntentResponseDto>({
    service: "qna",
    path: "/api/qna/source/upload-intent",
    method: "POST",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function inspectSourceExternalUrl(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: SourceExternalUrlInspectionRequestDto,
  signal?: AbortSignal,
) {
  return portalRequest<SourceExternalUrlInspectionDto>({
    service: "qna",
    path: "/api/qna/source/external-url-inspection",
    method: "POST",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
    signal,
  });
}

export function completeSourceUpload(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: SourceUploadCompleteRequestDto,
) {
  return portalRequest<string>({
    service: "qna",
    path: `/api/qna/source/${id}/upload-complete`,
    method: "POST",
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function getSourceDownloadUrl(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  signal?: AbortSignal,
) {
  return portalRequest<SourceDownloadUrlDto>({
    service: "qna",
    path: `/api/qna/source/${id}/download-url`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    signal,
  });
}
