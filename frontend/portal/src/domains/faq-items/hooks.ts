import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  createFaqItem,
  deleteFaqItem,
  getFaqItem,
  listFaqItems,
  updateFaqItem,
} from '@/domains/faq-items/api';
import type {
  FaqItemCreateRequestDto,
  FaqItemUpdateRequestDto,
} from '@/domains/faq-items/types';
import { useAuth } from '@/platform/auth/auth-context';
import { useTenant } from '@/platform/tenant/tenant-context';

export const faqItemKeys = {
  all: ['portal', 'faq-items'] as const,
  list: (page: number, pageSize: number, sorting?: string) =>
    [...faqItemKeys.all, 'list', page, pageSize, sorting] as const,
  detail: (id: string) => [...faqItemKeys.all, 'detail', id] as const,
};

export function useFaqItemList({
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
    queryKey: faqItemKeys.list(page, pageSize, sorting),
    queryFn: () =>
      listFaqItems(session?.accessToken, currentTenantId, page, pageSize, sorting),
    enabled: status === 'ready' && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useFaqItem(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: faqItemKeys.detail(id ?? 'unknown'),
    queryFn: () => getFaqItem(session?.accessToken, currentTenantId, id ?? ''),
    enabled: status === 'ready' && Boolean(currentTenantId) && Boolean(id),
  });
}

export function useCreateFaqItem() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...faqItemKeys.all, 'create'],
    mutationFn: (body: FaqItemCreateRequestDto) =>
      createFaqItem(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('FAQ item created.');
      await queryClient.invalidateQueries({ queryKey: faqItemKeys.all });
    },
  });
}

export function useUpdateFaqItem(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...faqItemKeys.all, 'update', id],
    mutationFn: (body: FaqItemUpdateRequestDto) =>
      updateFaqItem(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success('FAQ item updated.');
      await queryClient.invalidateQueries({ queryKey: faqItemKeys.all });
    },
  });
}

export function useDeleteFaqItem() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...faqItemKeys.all, 'delete'],
    mutationFn: (id: string) =>
      deleteFaqItem(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success('FAQ item deleted.');
      await queryClient.invalidateQueries({ queryKey: faqItemKeys.all });
    },
  });
}
