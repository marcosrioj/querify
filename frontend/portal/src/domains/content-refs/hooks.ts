import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  createContentRef,
  deleteContentRef,
  getContentRef,
  listContentRefs,
  updateContentRef,
} from '@/domains/content-refs/api';
import type {
  ContentRefCreateRequestDto,
  ContentRefUpdateRequestDto,
} from '@/domains/content-refs/types';
import { useAuth } from '@/platform/auth/auth-context';
import { useTenant } from '@/platform/tenant/tenant-context';

export const contentRefKeys = {
  all: ['portal', 'content-refs'] as const,
  list: (page: number, pageSize: number, sorting?: string) =>
    [...contentRefKeys.all, 'list', page, pageSize, sorting] as const,
  detail: (id: string) => [...contentRefKeys.all, 'detail', id] as const,
};

export function useContentRefList({
  page,
  pageSize,
  sorting,
}: {
  page: number;
  pageSize: number;
  sorting?: string;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: contentRefKeys.list(page, pageSize, sorting),
    queryFn: () =>
      listContentRefs(session?.accessToken, currentTenantId, page, pageSize, sorting),
    enabled: status === 'ready' && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useContentRef(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: contentRefKeys.detail(id ?? 'unknown'),
    queryFn: () => getContentRef(session?.accessToken, currentTenantId, id ?? ''),
    enabled: status === 'ready' && Boolean(currentTenantId) && Boolean(id),
  });
}

export function useCreateContentRef() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...contentRefKeys.all, 'create'],
    mutationFn: (body: ContentRefCreateRequestDto) =>
      createContentRef(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('Content ref created.');
      await queryClient.invalidateQueries({ queryKey: contentRefKeys.all });
    },
  });
}

export function useUpdateContentRef(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...contentRefKeys.all, 'update', id],
    mutationFn: (body: ContentRefUpdateRequestDto) =>
      updateContentRef(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success('Content ref updated.');
      await queryClient.invalidateQueries({ queryKey: contentRefKeys.all });
    },
  });
}

export function useDeleteContentRef() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...contentRefKeys.all, 'delete'],
    mutationFn: (id: string) =>
      deleteContentRef(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success('Content ref deleted.');
      await queryClient.invalidateQueries({ queryKey: contentRefKeys.all });
    },
  });
}
