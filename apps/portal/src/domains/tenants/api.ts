import {
  portalRequest,
  requireAccessToken,
  requireTenantId,
} from '@/platform/api/http-client';
import type {
  TenantAiProviderDto,
  TenantCreateOrUpdateRequestDto,
  TenantSetAiProviderCredentialsRequestDto,
} from '@/domains/tenants/types';

export function getTenantClientKey(accessToken?: string, tenantId?: string) {
  return portalRequest<string | null>({
    service: 'tenant',
    path: '/api/tenant/tenants/GetClientKey',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function generateTenantClientKey(accessToken?: string, tenantId?: string) {
  return portalRequest<string>({
    service: 'tenant',
    path: '/api/tenant/tenants/GenerateNewClientKey',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function getConfiguredAiProviders(accessToken?: string, tenantId?: string) {
  return portalRequest<TenantAiProviderDto[]>({
    service: 'tenant',
    path: '/api/tenant/tenants/GetConfiguredAiProviders',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
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
    tenantId,
    body,
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
    tenantId: requireTenantId(tenantId),
    body,
  });
}
