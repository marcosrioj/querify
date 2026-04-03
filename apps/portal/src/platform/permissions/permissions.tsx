import { PropsWithChildren } from 'react';
import { PortalRole } from '@/platform/auth/types';
import { useAuth } from '@/platform/auth/auth-context';

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

const rolePermissions: Record<PortalRole, PortalPermission[]> = {
  Admin: [
    'faq.read',
    'faq.write',
    'faq-item.read',
    'faq-item.write',
    'content-ref.read',
    'content-ref.write',
    'ai.request',
    'tenant.manage',
    'members.manage',
    'billing.manage',
  ],
  Member: [
    'faq.read',
    'faq.write',
    'faq-item.read',
    'faq-item.write',
    'content-ref.read',
    'content-ref.write',
    'ai.request',
  ],
};

export function hasPermission(role: PortalRole | undefined, permission: PortalPermission) {
  if (!role) {
    return false;
  }

  return rolePermissions[role].includes(permission);
}

export function usePermission(permission: PortalPermission) {
  const { user } = useAuth();
  return hasPermission(user?.role, permission);
}

export function Can({
  permission,
  children,
  fallback = null,
}: PropsWithChildren<{ permission: PortalPermission; fallback?: React.ReactNode }>) {
  const allowed = usePermission(permission);
  return allowed ? children : fallback;
}
