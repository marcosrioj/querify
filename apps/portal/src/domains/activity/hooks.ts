import { useQuery } from '@tanstack/react-query';
import { getActivity, listActivity } from '@/domains/activity/api';
import { useAuth } from '@/platform/auth/use-auth';
import { useTenant } from '@/platform/tenant/use-tenant';

const qnaRootKey = ['portal', 'qna'] as const;

export const activityKeys = {
  all: [...qnaRootKey, 'activity'] as const,
  list: (params: Record<string, unknown>) =>
    [...activityKeys.all, 'list', params] as const,
  detail: (id: string) => [...activityKeys.all, 'detail', id] as const,
};

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
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: activityKeys.list(params),
    queryFn: ({ signal }) => listActivity(session?.accessToken, currentTenantId, params, signal),
    enabled: status === 'ready' && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useActivity(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: activityKeys.detail(id ?? 'unknown'),
    queryFn: () => getActivity(session?.accessToken, currentTenantId, id ?? ''),
    enabled: status === 'ready' && Boolean(currentTenantId) && Boolean(id),
  });
}
