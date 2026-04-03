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
  workspace: ['portal', 'tenant-domain', 'workspace'] as const,
  aiProviders: ['portal', 'tenant-domain', 'ai-providers'] as const,
};

export function useTenantWorkspace() {
  const { session, status } = useAuth();

  const clientKeyQuery = useQuery({
    queryKey: tenantKeys.workspace,
    queryFn: () => getTenantClientKey(session?.accessToken),
    enabled: status === 'ready',
  });

  const aiProvidersQuery = useQuery({
    queryKey: tenantKeys.aiProviders,
    queryFn: () => getConfiguredAiProviders(session?.accessToken),
    enabled: status === 'ready',
  });

  return {
    clientKeyQuery,
    aiProvidersQuery,
  };
}

export function useUpdateTenantWorkspace() {
  const { session } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-domain', 'update-workspace'],
    mutationFn: (body: TenantCreateOrUpdateRequestDto) =>
      createOrUpdateTenant(session?.accessToken, body),
    onSuccess: async () => {
      toast.success('Workspace settings saved.');
      await queryClient.invalidateQueries({
        queryKey: ['portal', 'tenant-context', 'tenants'],
      });
    },
  });
}

export function useGenerateClientKey() {
  const { session } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-domain', 'generate-client-key'],
    mutationFn: () => generateTenantClientKey(session?.accessToken),
    onSuccess: async () => {
      toast.success('A new public client key was generated.');
      await queryClient.invalidateQueries({ queryKey: tenantKeys.workspace });
    },
  });
}

export function useSetAiProviderCredentials() {
  const { session } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'tenant-domain', 'set-ai-provider-credentials'],
    mutationFn: (body: TenantSetAiProviderCredentialsRequestDto) =>
      setAiProviderCredentials(session?.accessToken, body),
    onSuccess: async () => {
      toast.success('AI provider credentials stored for the current workspace.');
      await queryClient.invalidateQueries({ queryKey: tenantKeys.aiProviders });
    },
  });
}

export function useCurrentWorkspace() {
  return useTenant().currentTenant;
}
