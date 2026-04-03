import { portalRequest, requireAccessToken } from '@/platform/api/http-client';
import type {
  TenantAiProviderDto,
  TenantCreateOrUpdateRequestDto,
  TenantSetAiProviderCredentialsRequestDto,
} from '@/domains/tenants/types';

export function getTenantClientKey(accessToken?: string) {
  return portalRequest<string | null>({
    service: 'tenant',
    path: '/api/tenant/tenants/GetClientKey',
    accessToken: requireAccessToken(accessToken),
  });
}

export function generateTenantClientKey(accessToken?: string) {
  return portalRequest<string>({
    service: 'tenant',
    path: '/api/tenant/tenants/GenerateNewClientKey',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
  });
}

export function getConfiguredAiProviders(accessToken?: string) {
  return portalRequest<TenantAiProviderDto[]>({
    service: 'tenant',
    path: '/api/tenant/tenants/GetConfiguredAiProviders',
    accessToken: requireAccessToken(accessToken),
  });
}

export function createOrUpdateTenant(
  accessToken: string | undefined,
  body: TenantCreateOrUpdateRequestDto,
) {
  return portalRequest<boolean>({
    service: 'tenant',
    path: '/api/tenant/tenants/CreateOrUpdate',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body,
  });
}

export function setAiProviderCredentials(
  accessToken: string | undefined,
  body: TenantSetAiProviderCredentialsRequestDto,
) {
  return portalRequest<boolean>({
    service: 'tenant',
    path: '/api/tenant/tenants/SetAiProviderCredentials',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body,
  });
}
