import {
  portalRequest,
  requireAccessToken,
} from '@/platform/api/http-client';
import type {
  TenantUserCreateRequestDto,
  TenantUserDto,
  TenantUserUpdateRequestDto,
} from '@/domains/members/types';

export function getTenantUsers(accessToken?: string, tenantId?: string) {
  return portalRequest<TenantUserDto[]>({
    service: 'tenant',
    path: `/api/tenant/tenant-users/GetAll?tenantId=${requireTenantIdParam(tenantId)}`,
    accessToken: requireAccessToken(accessToken),
  });
}

export function createTenantUser(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: TenantUserCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'tenant',
    path: '/api/tenant/tenant-users',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body: {
      ...body,
      tenantId: requireTenantIdParam(tenantId),
    },
  });
}

export function updateTenantUser(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: TenantUserUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'tenant',
    path: `/api/tenant/tenant-users/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    body: {
      ...body,
      tenantId: requireTenantIdParam(tenantId),
    },
  });
}

export function deleteTenantUser(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'tenant',
    path: `/api/tenant/tenant-users/${id}?tenantId=${requireTenantIdParam(tenantId)}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
  });
}

function requireTenantIdParam(tenantId: string | undefined) {
  if (!tenantId) {
    throw new Error('A workspace must be selected before calling this endpoint.');
  }

  return tenantId;
}
