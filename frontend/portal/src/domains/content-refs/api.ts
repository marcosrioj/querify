import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import type {
  ContentRefCreateRequestDto,
  ContentRefDto,
  ContentRefUpdateRequestDto,
} from '@/domains/content-refs/types';
import type { PagedResultDto } from '@/shared/types/api';
import { toPagedQuery } from '@/shared/lib/pagination';

export function listContentRefs(
  accessToken: string | undefined,
  tenantId: string | undefined,
  page: number,
  pageSize: number,
  sorting?: string,
) {
  return portalRequest<PagedResultDto<ContentRefDto>>({
    service: 'faq',
    path: '/api/faqs/content-ref',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting),
  });
}

export function getContentRef(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<ContentRefDto>({
    service: 'faq',
    path: `/api/faqs/content-ref/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createContentRef(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: ContentRefCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'faq',
    path: '/api/faqs/content-ref',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateContentRef(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: ContentRefUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'faq',
    path: `/api/faqs/content-ref/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function deleteContentRef(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'faq',
    path: `/api/faqs/content-ref/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
