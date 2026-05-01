import { useQuery } from "@tanstack/react-query";
import { getActivity, listActivity } from "@/domains/activity/api";
import {
  createQnaDomainKeys,
  keepPreviousQnaTenantData,
} from "@/domains/qna/query-keys";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";

export const activityKeys = createQnaDomainKeys("activity");

export function useActivityList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  spaceId?: string;
  questionId?: string;
  answerId?: string;
  kind?: number;
  actorKind?: number;
  enabled?: boolean;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();
  const { enabled = true, ...requestParams } = params;

  return useQuery({
    queryKey: activityKeys.list(currentTenantId, requestParams),
    queryFn: ({ signal }) =>
      listActivity(
        session?.accessToken,
        currentTenantId,
        requestParams,
        signal,
      ),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: keepPreviousQnaTenantData(currentTenantId),
  });
}

export function useActivity(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: activityKeys.detail(currentTenantId, id ?? "unknown"),
    queryFn: () => getActivity(session?.accessToken, currentTenantId, id ?? ""),
    enabled: status === "ready" && Boolean(currentTenantId) && Boolean(id),
  });
}
