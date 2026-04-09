import type { PropsWithChildren, ReactNode } from 'react';
import type { PortalPermission } from '@/platform/permissions/permissions';
import { usePermission } from '@/platform/permissions/use-permission';

type CanProps = PropsWithChildren<{
  permission: PortalPermission;
  fallback?: ReactNode;
}>;

export function Can({ permission, children, fallback = null }: CanProps) {
  const allowed = usePermission(permission);
  return allowed ? children : fallback;
}
