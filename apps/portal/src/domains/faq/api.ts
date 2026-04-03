import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import type { FaqCreateRequestDto, FaqDto, FaqUpdateRequestDto } from '@/domains/faq/types';
import type { PagedResultDto } from '@/shared/types/api';
import { toPagedQuery } from '@/shared/lib/pagination';

export function listFaqs(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    searchText,
    status,
    faqIds,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
    status?: number;
    faqIds?: string[];
  },
) {
  return portalRequest<PagedResultDto<FaqDto>>({
    service: 'faq',
    path: '/api/faqs/faq',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
      Status: status,
      FaqIds: faqIds,
    }),
  });
}

export function getFaq(accessToken: string | undefined, tenantId: string | undefined, id: string) {
  return portalRequest<FaqDto>({
    service: 'faq',
    path: `/api/faqs/faq/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createFaq(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: FaqCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'faq',
    path: '/api/faqs/faq',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateFaq(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: FaqUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'faq',
    path: `/api/faqs/faq/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function deleteFaq(accessToken: string | undefined, tenantId: string | undefined, id: string) {
  return portalRequest<void>({
    service: 'faq',
    path: `/api/faqs/faq/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function requestFaqGeneration(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<string>({
    service: 'faq',
    path: `/api/faqs/faq/${id}/generation-request`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
