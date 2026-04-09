import type { PortalPermission } from '@/platform/permissions/permissions';
import { hasPermission } from '@/platform/permissions/permissions';
import { useAuth } from '@/platform/auth/use-auth';
import { useTenant } from '@/platform/tenant/use-tenant';

export function usePermission(permission: PortalPermission) {
  const { user } = useAuth();
  const { currentTenant } = useTenant();
  return hasPermission(user?.role, currentTenant?.currentUserRole, permission);
}
