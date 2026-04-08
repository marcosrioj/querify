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
import { translateText } from '@/shared/lib/i18n-core';

export const contentRefKeys = {
  all: ['portal', 'content-refs'] as const,
  list: (params: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
    kind?: number;
    faqId?: string;
    faqItemId?: string;
  }) => [...contentRefKeys.all, 'list', params] as const,
  detail: (id: string) => [...contentRefKeys.all, 'detail', id] as const,
};

export function useContentRefList({
  page,
  pageSize,
  sorting,
  searchText,
  kind,
  faqId,
  faqItemId,
}: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  kind?: number;
  faqId?: string;
  faqItemId?: string;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();
  const params = { page, pageSize, sorting, searchText, kind, faqId, faqItemId };

  return useQuery({
    queryKey: contentRefKeys.list(params),
    queryFn: ({ signal }) =>
      listContentRefs(session?.accessToken, currentTenantId, params, signal),
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
      toast.success(translateText('Source created.'));
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
      toast.success(translateText('Source updated.'));
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
      toast.success(translateText('Source deleted.'));
      await queryClient.invalidateQueries({ queryKey: contentRefKeys.all });
    },
  });
}
