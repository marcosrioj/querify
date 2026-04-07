import {
  portalRequest,
  requireAccessToken,
} from '@/platform/api/http-client';
import type {
  TenantAiProviderDto,
  TenantCreateOrUpdateRequestDto,
  TenantSetAiProviderCredentialsRequestDto,
} from '@/domains/tenants/types';

export function getTenantClientKey(accessToken?: string, tenantId?: string) {
  return portalRequest<string | null>({
    service: 'tenant',
    path: `/api/tenant/tenants/GetClientKey?tenantId=${requireTenantIdParam(tenantId)}`,
    accessToken: requireAccessToken(accessToken),
  });
}

export function generateTenantClientKey(accessToken?: string, tenantId?: string) {
  return portalRequest<string>({
    service: 'tenant',
    path: `/api/tenant/tenants/GenerateNewClientKey?tenantId=${requireTenantIdParam(tenantId)}`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
  });
}

export function getConfiguredAiProviders(accessToken?: string, tenantId?: string) {
  return portalRequest<TenantAiProviderDto[]>({
    service: 'tenant',
    path: `/api/tenant/tenants/GetConfiguredAiProviders?tenantId=${requireTenantIdParam(tenantId)}`,
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
    path: '/api/tenant/tenants/CreateOrUpdate',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body: {
      ...body,
      tenantId,
    },
  });
}

export function setAiProviderCredentials(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: TenantSetAiProviderCredentialsRequestDto,
) {
  return portalRequest<boolean>({
    service: 'tenant',
    path: '/api/tenant/tenants/SetAiProviderCredentials',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body: {
      ...body,
      tenantId: requireTenantIdParam(tenantId),
    },
  });
}

function requireTenantIdParam(tenantId: string | undefined) {
  if (!tenantId) {
    throw new Error('A workspace must be selected before calling this endpoint.');
  }

  return tenantId;
}
