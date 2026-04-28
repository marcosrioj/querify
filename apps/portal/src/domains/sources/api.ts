import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import { toPagedQuery } from '@/shared/lib/pagination';
import type { PagedResultDto } from '@/shared/types/api';
import type {
  SourceCreateRequestDto,
  SourceDto,
  SourceUpdateRequestDto,
} from '@/domains/sources/types';

export function listSources(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    searchText,
    kind,
    visibility,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
    kind?: number;
    visibility?: number;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<SourceDto>>({
    service: 'qna',
    path: '/api/qna/source',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
      Kind: kind,
      Visibility: visibility,
    }),
    signal,
  });
}

export function getSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<SourceDto>({
    service: 'qna',
    path: `/api/qna/source/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: SourceCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: '/api/qna/source',
    method: 'POST',
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
    service: 'qna',
    path: `/api/qna/source/${id}`,
    method: 'PUT',
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
    service: 'qna',
    path: `/api/qna/source/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
