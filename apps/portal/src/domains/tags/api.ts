import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import { toPagedQuery } from '@/shared/lib/pagination';
import type { PagedResultDto } from '@/shared/types/api';
import type {
  TagCreateRequestDto,
  TagDto,
  TagUpdateRequestDto,
} from '@/domains/tags/types';

export function listTags(
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
  return portalRequest<PagedResultDto<TagDto>>({
    service: 'qna',
    path: '/api/qna/tag',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
    }),
    signal,
  });
}

export function getTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  signal?: AbortSignal,
) {
  return portalRequest<TagDto>({
    service: 'qna',
    path: `/api/qna/tag/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    signal,
  });
}

export function createTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: TagCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: '/api/qna/tag',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: TagUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/tag/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function deleteTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/tag/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
