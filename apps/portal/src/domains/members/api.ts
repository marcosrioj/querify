import {
  portalRequest,
  requireAccessToken,
  requireTenantId,
} from '@/platform/api/http-client';
import type {
  TenantUserDto,
  TenantUserUpsertRequestDto,
} from '@/domains/members/types';

export function getTenantUsers(accessToken?: string, tenantId?: string) {
  return portalRequest<TenantUserDto[]>({
    service: 'tenant',
    path: `/api/tenant/tenant-users/GetAll?tenantId=${requireTenantId(tenantId)}`,
    accessToken: requireAccessToken(accessToken),
  });
}

export function upsertTenantUser(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: TenantUserUpsertRequestDto,
) {
  return portalRequest<string>({
    service: 'tenant',
    path: '/api/tenant/tenant-users',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    body: {
      ...body,
      tenantId: requireTenantId(tenantId),
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
    path: `/api/tenant/tenant-users/${id}?tenantId=${requireTenantId(tenantId)}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
  });
}
