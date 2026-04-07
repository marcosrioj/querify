import {
  PropsWithChildren,
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { useQuery } from '@tanstack/react-query';
import type { TenantSummaryDto } from '@/domains/tenants/types';
import { portalRequest, requireAccessToken } from '@/platform/api/http-client';
import { useAuth } from '@/platform/auth/auth-context';
import { PortalApp } from '@/shared/constants/backend-enums';

type TenantContextValue = {
  tenants: TenantSummaryDto[];
  currentTenantId?: string;
  currentTenant?: TenantSummaryDto;
  isLoading: boolean;
  setCurrentTenantId: (tenantId: string) => void;
  refreshTenants: () => Promise<void>;
};

const TenantContext = createContext<TenantContextValue | undefined>(undefined);

const STORAGE_KEY = 'basefaq.portal.currentTenantId';

async function fetchTenants(accessToken?: string) {
  return portalRequest<TenantSummaryDto[]>({
    service: 'tenant',
    path: '/api/tenant/tenants/get-all',
    accessToken: requireAccessToken(accessToken),
  });
}

export function PortalTenantProvider({ children }: PropsWithChildren) {
  const { session, status } = useAuth();
  const [currentTenantId, setCurrentTenantIdState] = useState<string>();

  const tenantsQuery = useQuery({
    queryKey: ['portal', 'tenant-context', 'tenants'],
    queryFn: () => fetchTenants(session?.accessToken),
    enabled: status === 'ready' && Boolean(session?.accessToken),
  });

  const tenantOptions = useMemo(
    () => (tenantsQuery.data ?? []).filter((tenant) => tenant.app === PortalApp.Faq),
    [tenantsQuery.data],
  );

  useEffect(() => {
    const storedTenantId = window.localStorage.getItem(STORAGE_KEY);
    if (!storedTenantId) {
      return;
    }

    setCurrentTenantIdState(storedTenantId);
  }, []);

  useEffect(() => {
    if (!tenantOptions.length) {
      setCurrentTenantIdState(undefined);
      return;
    }

    const matchedTenant = tenantOptions.find(
      (tenant) => tenant.id === currentTenantId,
    );
    if (matchedTenant) {
      return;
    }

    const nextTenant = tenantOptions[0];
    setCurrentTenantIdState(nextTenant.id);
    window.localStorage.setItem(STORAGE_KEY, nextTenant.id);
  }, [currentTenantId, tenantOptions]);

  const value = useMemo<TenantContextValue>(
    () => ({
      tenants: tenantOptions,
      currentTenantId,
      currentTenant: tenantOptions.find((tenant) => tenant.id === currentTenantId),
      isLoading: tenantsQuery.isLoading,
      setCurrentTenantId(tenantId) {
        setCurrentTenantIdState(tenantId);
        window.localStorage.setItem(STORAGE_KEY, tenantId);
      },
      async refreshTenants() {
        await tenantsQuery.refetch();
      },
    }),
    [currentTenantId, tenantOptions, tenantsQuery],
  );

  return (
    <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
  );
}

export function useTenant() {
  const context = useContext(TenantContext);
  if (!context) {
    throw new Error('useTenant must be used within PortalTenantProvider.');
  }

  return context;
}
