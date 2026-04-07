import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  deleteTenantUser,
  getTenantUsers,
  upsertTenantUser,
} from '@/domains/members/api';
import type { TenantUserUpsertRequestDto } from '@/domains/members/types';
import { useAuth } from '@/platform/auth/auth-context';
import { useTenant } from '@/platform/tenant/tenant-context';

const membersKeys = {
  list: (tenantId?: string) =>
    ['portal', 'tenant-members', tenantId ?? 'none'] as const,
};

async function invalidateWorkspaceAccess(queryClient: ReturnType<typeof useQueryClient>) {
  await Promise.all([
    queryClient.invalidateQueries({
      queryKey: ['portal', 'tenant-context', 'tenants'],
    }),
    queryClient.invalidateQueries({
      queryKey: ['portal', 'tenant-domain'],
    }),
  ]);
}

export function useTenantMembers() {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: membersKeys.list(currentTenantId),
    queryFn: () => getTenantUsers(session?.accessToken, currentTenantId),
    enabled: status === 'ready' && Boolean(session?.accessToken) && Boolean(currentTenantId),
  });
}

export function useUpsertTenantMember() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-members', 'upsert'],
    mutationFn: (body: TenantUserUpsertRequestDto) =>
      upsertTenantUser(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('Member saved in the workspace.');
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: membersKeys.list(currentTenantId),
        }),
        invalidateWorkspaceAccess(queryClient),
      ]);
    },
  });
}

export function useDeleteTenantMember() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-members', 'delete'],
    mutationFn: (id: string) =>
      deleteTenantUser(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success('Member removed from the workspace.');
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: membersKeys.list(currentTenantId),
        }),
        invalidateWorkspaceAccess(queryClient),
      ]);
    },
  });
}
