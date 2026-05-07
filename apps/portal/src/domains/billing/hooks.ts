import { useQuery } from "@tanstack/react-query";
import {
  getBillingInvoices,
  getBillingPayments,
  getBillingSubscription,
  getBillingSummary,
} from "@/domains/billing/api";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";

const billingKeys = {
  summary: (tenantId?: string) =>
    ["portal", "billing", "summary", tenantId ?? "none"] as const,
  subscription: (tenantId?: string) =>
    ["portal", "billing", "subscription", tenantId ?? "none"] as const,
  invoices: (tenantId?: string) =>
    ["portal", "billing", "invoices", tenantId ?? "none"] as const,
  payments: (tenantId?: string) =>
    ["portal", "billing", "payments", tenantId ?? "none"] as const,
};

export function useBillingSummary(options?: {
  staleTime?: number;
  gcTime?: number;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: billingKeys.summary(currentTenantId),
    queryFn: () => getBillingSummary(session?.accessToken, currentTenantId),
    enabled:
      status === "ready" &&
      Boolean(session?.accessToken) &&
      Boolean(currentTenantId),
    staleTime: options?.staleTime,
    gcTime: options?.gcTime,
  });
}

export function useBillingWorkspace() {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  const enabled =
    status === "ready" &&
    Boolean(session?.accessToken) &&
    Boolean(currentTenantId);

  const summaryQuery = useQuery({
    queryKey: billingKeys.summary(currentTenantId),
    queryFn: () => getBillingSummary(session?.accessToken, currentTenantId),
    enabled,
  });

  const subscriptionQuery = useQuery({
    queryKey: billingKeys.subscription(currentTenantId),
    queryFn: () =>
      getBillingSubscription(session?.accessToken, currentTenantId),
    enabled,
  });

  const invoicesQuery = useQuery({
    queryKey: billingKeys.invoices(currentTenantId),
    queryFn: () =>
      getBillingInvoices(session?.accessToken, currentTenantId, {
        maxResultCount: 8,
        sorting: "UpdatedDateUtc DESC",
      }),
    enabled,
  });

  const paymentsQuery = useQuery({
    queryKey: billingKeys.payments(currentTenantId),
    queryFn: () =>
      getBillingPayments(session?.accessToken, currentTenantId, {
        maxResultCount: 8,
        sorting: "UpdatedDateUtc DESC",
      }),
    enabled,
  });

  return {
    summaryQuery,
    subscriptionQuery,
    invoicesQuery,
    paymentsQuery,
  };
}
