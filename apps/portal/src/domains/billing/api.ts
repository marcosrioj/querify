import {
  portalRequest,
  requireAccessToken,
  requireTenantId,
} from '@/platform/api/http-client';
import type { PagedResultDto } from '@/shared/types/api';
import type {
  BillingInvoiceDto,
  BillingListRequest,
  BillingPaymentDto,
  TenantBillingSummaryDto,
  TenantSubscriptionDetailDto,
} from '@/domains/billing/types';

function resolveTenantHeader(tenantId?: string) {
  return requireTenantId(tenantId);
}

export function getBillingSummary(accessToken?: string, tenantId?: string) {
  const resolvedTenantId = resolveTenantHeader(tenantId);

  return portalRequest<TenantBillingSummaryDto>({
    service: 'tenant',
    path: '/api/tenant/billing/summary',
    accessToken: requireAccessToken(accessToken),
    tenantId: resolvedTenantId,
  });
}

export function getBillingSubscription(accessToken?: string, tenantId?: string) {
  const resolvedTenantId = resolveTenantHeader(tenantId);

  return portalRequest<TenantSubscriptionDetailDto>({
    service: 'tenant',
    path: '/api/tenant/billing/subscription',
    accessToken: requireAccessToken(accessToken),
    tenantId: resolvedTenantId,
  });
}

export function getBillingInvoices(
  accessToken: string | undefined,
  tenantId: string | undefined,
  requestDto: BillingListRequest,
) {
  const resolvedTenantId = resolveTenantHeader(tenantId);

  return portalRequest<PagedResultDto<BillingInvoiceDto>>({
    service: 'tenant',
    path: '/api/tenant/billing/invoices',
    accessToken: requireAccessToken(accessToken),
    tenantId: resolvedTenantId,
    query: requestDto,
  });
}

export function getBillingPayments(
  accessToken: string | undefined,
  tenantId: string | undefined,
  requestDto: BillingListRequest,
) {
  const resolvedTenantId = resolveTenantHeader(tenantId);

  return portalRequest<PagedResultDto<BillingPaymentDto>>({
    service: 'tenant',
    path: '/api/tenant/billing/payments',
    accessToken: requireAccessToken(accessToken),
    tenantId: resolvedTenantId,
    query: requestDto,
  });
}
