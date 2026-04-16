import {
  portalRequest,
  requireAccessToken,
  requireTenantId,
} from '@/platform/api/http-client';
import type { TenantCreateOrUpdateRequestDto } from '@/domains/tenants/types';

export function getTenantClientKey(accessToken?: string, tenantId?: string) {
  return portalRequest<string | null | undefined>({
    service: 'tenant',
    path: `/api/tenant/tenants/get-client-key?tenantId=${requireTenantId(tenantId)}`,
    accessToken: requireAccessToken(accessToken),
  }).then((clientKey) => clientKey ?? null);
}

export function generateTenantClientKey(accessToken?: string, tenantId?: string) {
  return portalRequest<string>({
    service: 'tenant',
    path: `/api/tenant/tenants/generate-new-client-key?tenantId=${requireTenantId(tenantId)}`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
  });
}

export function createOrUpdateTenant(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: TenantCreateOrUpdateRequestDto,
) {
  return portalRequest<boolean>({
    service: 'tenant',
    path: '/api/tenant/tenants/create-or-update',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body: {
      ...body,
      tenantId,
    },
  });
}

export function refreshAllowedTenantCache(accessToken: string | undefined) {
  return portalRequest<boolean>({
    service: 'tenant',
    path: '/api/tenant/tenants/refresh-allowed-tenant-cache',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
  });
}
