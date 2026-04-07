import { PropsWithChildren } from 'react';
import { PortalRole } from '@/platform/auth/types';
import { useAuth } from '@/platform/auth/auth-context';
import { useTenant } from '@/platform/tenant/tenant-context';
import { TenantUserRoleType } from '@/shared/constants/backend-enums';

export type PortalPermission =
  | 'faq.read'
  | 'faq.write'
  | 'faq-item.read'
  | 'faq-item.write'
  | 'content-ref.read'
  | 'content-ref.write'
  | 'ai.request'
  | 'tenant.manage'
  | 'members.manage'
  | 'billing.manage';

const baseWorkspacePermissions: PortalPermission[] = [
  'faq.read',
  'faq.write',
  'faq-item.read',
  'faq-item.write',
  'content-ref.read',
  'content-ref.write',
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
    tenantRole === TenantUserRoleType.Owner || tenantRole === TenantUserRoleType.Member
      ? ownerWorkspacePermissions
      : baseWorkspacePermissions;

  return workspacePermissions.includes(permission);
}

export function usePermission(permission: PortalPermission) {
  const { user } = useAuth();
  const { currentTenant } = useTenant();
  return hasPermission(user?.role, currentTenant?.currentUserRole, permission);
}

export function Can({
  permission,
  children,
  fallback = null,
}: PropsWithChildren<{ permission: PortalPermission; fallback?: React.ReactNode }>) {
  const allowed = usePermission(permission);
  return allowed ? children : fallback;
}
