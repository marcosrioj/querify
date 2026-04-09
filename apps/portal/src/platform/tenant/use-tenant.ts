import { useContext } from 'react';
import { TenantContext } from '@/platform/tenant/tenant-context';

export function useTenant() {
  const context = useContext(TenantContext);
  if (!context) {
    throw new Error('useTenant must be used within PortalTenantProvider.');
  }

  return context;
}
