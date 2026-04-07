import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  createTenantUser,
  deleteTenantUser,
  getTenantUsers,
  updateTenantUser,
} from '@/domains/members/api';
import type {
  TenantUserCreateRequestDto,
  TenantUserUpdateRequestDto,
} from '@/domains/members/types';
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

export function useCreateTenantMember() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-members', 'create'],
    mutationFn: (body: TenantUserCreateRequestDto) =>
      createTenantUser(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('Member added to the workspace.');
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: membersKeys.list(currentTenantId),
        }),
        invalidateWorkspaceAccess(queryClient),
      ]);
    },
  });
}

export function useUpdateTenantMember() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-members', 'update'],
    mutationFn: ({ id, body }: { id: string; body: TenantUserUpdateRequestDto }) =>
      updateTenantUser(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success('Workspace role updated.');
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
