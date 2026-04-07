import {
  portalRequest,
  requireAccessToken,
  requireTenantId,
} from '@/platform/api/http-client';
import type {
  AddTenantMemberRequestDto,
  TenantUserDto,
} from '@/domains/members/types';

export function getTenantUsers(accessToken?: string, tenantId?: string) {
  return portalRequest<TenantUserDto[]>({
    service: 'tenant',
    path: `/api/tenant/tenant-users/get-all?tenantId=${requireTenantId(tenantId)}`,
    accessToken: requireAccessToken(accessToken),
  });
}

export function addTenantMember(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: AddTenantMemberRequestDto,
) {
  return portalRequest<string>({
    service: 'tenant',
    path: '/api/tenant/tenant-users/add-tenant-member',
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
