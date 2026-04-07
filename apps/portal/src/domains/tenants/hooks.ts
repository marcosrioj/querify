import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  createOrUpdateTenant,
  generateTenantClientKey,
  getConfiguredAiProviders,
  getTenantClientKey,
  setAiProviderCredentials,
} from '@/domains/tenants/api';
import type {
  TenantCreateOrUpdateRequestDto,
  TenantSetAiProviderCredentialsRequestDto,
} from '@/domains/tenants/types';
import { useAuth } from '@/platform/auth/auth-context';
import { useTenant } from '@/platform/tenant/tenant-context';

const tenantKeys = {
  workspace: (tenantId?: string) =>
    ['portal', 'tenant-domain', 'workspace', tenantId ?? 'none'] as const,
  aiProviders: (tenantId?: string) =>
    ['portal', 'tenant-domain', 'ai-providers', tenantId ?? 'none'] as const,
};

export function useTenantWorkspace() {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  const clientKeyQuery = useQuery({
    queryKey: tenantKeys.workspace(currentTenantId),
    queryFn: () => getTenantClientKey(session?.accessToken, currentTenantId),
    enabled: status === 'ready' && Boolean(session?.accessToken) && Boolean(currentTenantId),
  });

  const aiProvidersQuery = useQuery({
    queryKey: tenantKeys.aiProviders(currentTenantId),
    queryFn: () => getConfiguredAiProviders(session?.accessToken, currentTenantId),
    enabled: status === 'ready' && Boolean(session?.accessToken) && Boolean(currentTenantId),
  });

  return {
    clientKeyQuery,
    aiProvidersQuery,
  };
}

export function useUpdateTenantWorkspace() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-domain', 'update-workspace'],
    mutationFn: (body: TenantCreateOrUpdateRequestDto) =>
      createOrUpdateTenant(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('Workspace settings saved.');
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ['portal', 'tenant-context', 'tenants'],
        }),
        queryClient.invalidateQueries({
          queryKey: ['portal', 'tenant-domain'],
        }),
      ]);
    },
  });
}

export function useGenerateClientKey() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-domain', 'generate-client-key'],
    mutationFn: () => generateTenantClientKey(session?.accessToken, currentTenantId),
    onSuccess: async () => {
      toast.success('A new public client key was generated.');
      await queryClient.invalidateQueries({
        queryKey: tenantKeys.workspace(currentTenantId),
      });
    },
  });
}

export function useSetAiProviderCredentials() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-domain', 'set-ai-provider-credentials'],
    mutationFn: (body: TenantSetAiProviderCredentialsRequestDto) =>
      setAiProviderCredentials(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('AI provider credentials stored for the current workspace.');
      await queryClient.invalidateQueries({
        queryKey: tenantKeys.aiProviders(currentTenantId),
      });
    },
  });
}

export function useCurrentWorkspace() {
  return useTenant().currentTenant;
}
