import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import type {
  FaqItemCreateRequestDto,
  FaqItemDto,
  FaqItemUpdateRequestDto,
} from '@/domains/faq-items/types';
import type { PagedResultDto } from '@/shared/types/api';
import { toPagedQuery } from '@/shared/lib/pagination';

export function listFaqItems(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    searchText,
    faqId,
    contentRefId,
    isActive,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
    faqId?: string;
    contentRefId?: string;
    isActive?: boolean;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<FaqItemDto>>({
    service: 'faq',
    path: '/api/faqs/faq-item',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
      FaqId: faqId,
      ContentRefId: contentRefId,
      IsActive: isActive,
    }),
    signal,
  });
}

export function getFaqItem(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<FaqItemDto>({
    service: 'faq',
    path: `/api/faqs/faq-item/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createFaqItem(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: FaqItemCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'faq',
    path: '/api/faqs/faq-item',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateFaqItem(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: FaqItemUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'faq',
    path: `/api/faqs/faq-item/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function deleteFaqItem(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'faq',
    path: `/api/faqs/faq-item/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
