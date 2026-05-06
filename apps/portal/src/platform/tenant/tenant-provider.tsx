import { useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";
import { useQuery } from "@tanstack/react-query";
import type { TenantSummaryDto } from "@/domains/tenants/types";
import { portalRequest, requireAccessToken } from "@/platform/api/http-client";
import { useAuth } from "@/platform/auth/use-auth";
import {
  TenantContext,
  type TenantContextValue,
} from "@/platform/tenant/tenant-context";
import { ModuleEnum } from "@/shared/constants/backend-enums";

const STORAGE_KEY = "querify.portal.currentTenantId";

function getStoredTenantId() {
  if (typeof window === "undefined") {
    return undefined;
  }

  try {
    return window.localStorage.getItem(STORAGE_KEY) ?? undefined;
  } catch {
    return undefined;
  }
}

function setStoredTenantId(tenantId: string) {
  if (typeof window === "undefined") {
    return;
  }

  try {
    window.localStorage.setItem(STORAGE_KEY, tenantId);
  } catch {
    // Ignore storage failures and keep the in-memory tenant selection.
  }
}

function removeStoredTenantId() {
  if (typeof window === "undefined") {
    return;
  }

  try {
    window.localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Ignore storage failures and keep the in-memory tenant selection.
  }
}

async function fetchTenants(accessToken?: string) {
  return portalRequest<TenantSummaryDto[]>({
    service: "tenant",
    path: "/api/tenant/tenants/get-all",
    accessToken: requireAccessToken(accessToken),
  });
}

export function PortalTenantProvider({ children }: PropsWithChildren) {
  const { session, status, user } = useAuth();
  const [selectedTenantId, setSelectedTenantId] = useState<string | undefined>(
    getStoredTenantId,
  );
  const canUseTenants =
    status === "ready" && Boolean(session?.accessToken) && Boolean(user?.id);

  const tenantsQuery = useQuery({
    queryKey: ["portal", "tenant-context", "tenants", user?.id ?? "anonymous"],
    queryFn: () => fetchTenants(session?.accessToken),
    enabled: canUseTenants,
  });

  const tenantOptions = useMemo(
    () =>
      (tenantsQuery.data ?? []).filter(
        (tenant) => tenant.module === ModuleEnum.QnA,
      ),
    [tenantsQuery.data],
  );

  useEffect(() => {
    if (
      !canUseTenants ||
      tenantsQuery.isLoading ||
      tenantsQuery.data === undefined
    ) {
      return;
    }

    if (!tenantOptions.length) {
      setSelectedTenantId(undefined);
      removeStoredTenantId();
      return;
    }

    const matchedTenant = tenantOptions.find(
      (tenant) => tenant.id === selectedTenantId,
    );
    if (matchedTenant) {
      return;
    }

    const nextTenant = tenantOptions[0];
    setSelectedTenantId(nextTenant.id);
    setStoredTenantId(nextTenant.id);
  }, [
    canUseTenants,
    selectedTenantId,
    tenantOptions,
    tenantsQuery.isLoading,
    tenantsQuery.data,
  ]);

  const currentTenant =
    canUseTenants && tenantsQuery.data !== undefined
      ? tenantOptions.find((tenant) => tenant.id === selectedTenantId)
      : undefined;
  const currentTenantId = currentTenant?.id;
  const isLoading =
    canUseTenants &&
    (tenantsQuery.isLoading ||
      (tenantsQuery.data !== undefined &&
        tenantOptions.length > 0 &&
        currentTenant === undefined));

  const value = useMemo<TenantContextValue>(
    () => ({
      tenants: tenantOptions,
      currentTenantId,
      currentTenant,
      isLoading,
      setCurrentTenantId(tenantId) {
        setSelectedTenantId(tenantId);
        setStoredTenantId(tenantId);
      },
      async refreshTenants() {
        await tenantsQuery.refetch();
      },
    }),
    [currentTenant, currentTenantId, isLoading, tenantOptions, tenantsQuery],
  );

  return (
    <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
  );
}
