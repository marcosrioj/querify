import { useEffect, useMemo, useState } from 'react';
import type { PropsWithChildren } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { TenantSummaryDto } from '@/domains/tenants/types';
import { portalRequest, requireAccessToken } from '@/platform/api/http-client';
import { useAuth } from '@/platform/auth/use-auth';
import { TenantContext, type TenantContextValue } from '@/platform/tenant/tenant-context';
import { ModuleEnum } from '@/shared/constants/backend-enums';

const STORAGE_KEY = 'basefaq.portal.currentTenantId';

function getStoredTenantId() {
  if (typeof window === 'undefined') {
    return undefined;
  }

  try {
    return window.localStorage.getItem(STORAGE_KEY) ?? undefined;
  } catch {
    return undefined;
  }
}

function setStoredTenantId(tenantId: string) {
  if (typeof window === 'undefined') {
    return;
  }

  try {
    window.localStorage.setItem(STORAGE_KEY, tenantId);
  } catch {
    // Ignore storage failures and keep the in-memory tenant selection.
  }
}

async function fetchTenants(accessToken?: string) {
  return portalRequest<TenantSummaryDto[]>({
    service: 'tenant',
    path: '/api/tenant/tenants/get-all',
    accessToken: requireAccessToken(accessToken),
  });
}

export function PortalTenantProvider({ children }: PropsWithChildren) {
  const { session, status } = useAuth();
  const [currentTenantId, setCurrentTenantIdState] = useState<
    string | undefined
  >(getStoredTenantId);

  const tenantsQuery = useQuery({
    queryKey: ['portal', 'tenant-context', 'tenants'],
    queryFn: () => fetchTenants(session?.accessToken),
    enabled: status === 'ready' && Boolean(session?.accessToken),
  });

  const tenantOptions = useMemo(
    () =>
      (tenantsQuery.data ?? []).filter((tenant) => tenant.module === ModuleEnum.QnA),
    [tenantsQuery.data],
  );

  useEffect(() => {
    if (tenantsQuery.isLoading || tenantsQuery.data === undefined) {
      return;
    }

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
    setStoredTenantId(nextTenant.id);
  }, [currentTenantId, tenantOptions, tenantsQuery.isLoading, tenantsQuery.data]);

  const value = useMemo<TenantContextValue>(
    () => ({
      tenants: tenantOptions,
      currentTenantId,
      currentTenant: tenantOptions.find((tenant) => tenant.id === currentTenantId),
      isLoading: tenantsQuery.isLoading,
      setCurrentTenantId(tenantId) {
        setCurrentTenantIdState(tenantId);
        setStoredTenantId(tenantId);
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
