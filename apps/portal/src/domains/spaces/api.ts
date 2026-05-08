import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import { toPagedQuery } from '@/shared/lib/pagination';
import type { PagedResultDto } from '@/shared/types/api';
import type {
  SpaceCreateRequestDto,
  SpaceDetailDto,
  SpaceDto,
  SpaceSourceCreateRequestDto,
  SpaceTagCreateRequestDto,
  SpaceUpdateRequestDto,
} from '@/domains/spaces/types';

export function listSpaces(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    searchText,
    visibility,
    status,
    acceptsQuestions,
    acceptsAnswers,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
    visibility?: number;
    status?: number;
    acceptsQuestions?: boolean;
    acceptsAnswers?: boolean;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<SpaceDto>>({
    service: 'qna',
    path: '/api/qna/space',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
      Visibility: visibility,
      Status: status,
      AcceptsQuestions: acceptsQuestions,
      AcceptsAnswers: acceptsAnswers,
    }),
    signal,
  });
}

export function getSpace(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  signal?: AbortSignal,
) {
  return portalRequest<SpaceDetailDto>({
    service: 'qna',
    path: `/api/qna/space/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    signal,
  });
}

export function createSpace(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: SpaceCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: '/api/qna/space',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateSpace(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: SpaceUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/space/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function deleteSpace(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/space/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function addSpaceTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: SpaceTagCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/space/${id}/tag`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function removeSpaceTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  tagId: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/space/${id}/tag/${tagId}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function addSpaceSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: SpaceSourceCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/space/${id}/source`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function removeSpaceSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  sourceId: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/space/${id}/source/${sourceId}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
