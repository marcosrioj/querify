import type { PortalRole } from '@/platform/auth/types';
import { TenantUserRoleType } from '@/shared/constants/backend-enums';

export type PortalPermission =
  | 'space.read'
  | 'space.write'
  | 'question.read'
  | 'question.write'
  | 'source.read'
  | 'source.write'
  | 'ai.request'
  | 'tenant.manage'
  | 'members.manage'
  | 'billing.manage';

const baseWorkspacePermissions: PortalPermission[] = [
  'space.read',
  'space.write',
  'question.read',
  'question.write',
  'source.read',
  'source.write',
  'ai.request',
];

const ownerWorkspacePermissions: PortalPermission[] = [
  ...baseWorkspacePermissions,
  'tenant.manage',
  'members.manage',
  'billing.manage',
];

const globalRolePermissions: Record<PortalRole, PortalPermission[]> = {
  Admin: ownerWorkspacePermissions,
  Member: [],
};

export function hasPermission(
  role: PortalRole | undefined,
  tenantRole: TenantUserRoleType | undefined,
  permission: PortalPermission,
) {
  if (!role && tenantRole === undefined) {
    return false;
  }

  if (role && globalRolePermissions[role].includes(permission)) {
    return true;
  }

  if (tenantRole === undefined) {
    return false;
  }

  const workspacePermissions =
    tenantRole === TenantUserRoleType.Owner ||
    tenantRole === TenantUserRoleType.Member
      ? ownerWorkspacePermissions
      : baseWorkspacePermissions;

  return workspacePermissions.includes(permission);
}
