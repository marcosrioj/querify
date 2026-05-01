export const qnaRootKey = ["portal", "qna"] as const;

export function qnaTenantKey(tenantId?: string) {
  return [...qnaRootKey, tenantId ?? "none"] as const;
}

export function createQnaDomainKeys<const Domain extends string>(
  domain: Domain,
) {
  return {
    all: (tenantId?: string) => [...qnaTenantKey(tenantId), domain] as const,
    list: (tenantId: string | undefined, params: Record<string, unknown>) =>
      [...qnaTenantKey(tenantId), domain, "list", params] as const,
    detail: (tenantId: string | undefined, id: string) =>
      [...qnaTenantKey(tenantId), domain, "detail", id] as const,
  };
}

export function keepPreviousQnaTenantData<T>(tenantId?: string) {
  const tenantKey = tenantId ?? "none";

  return (
    previousData: T | undefined,
    previousQuery?: { queryKey: readonly unknown[] },
  ) => (previousQuery?.queryKey[2] === tenantKey ? previousData : undefined);
}
