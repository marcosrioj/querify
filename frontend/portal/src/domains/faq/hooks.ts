import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  createFaq,
  deleteFaq,
  getFaq,
  listFaqs,
  requestFaqGeneration,
  updateFaq,
} from '@/domains/faq/api';
import type { FaqCreateRequestDto, FaqUpdateRequestDto } from '@/domains/faq/types';
import { useAuth } from '@/platform/auth/auth-context';
import { useTenant } from '@/platform/tenant/tenant-context';

export const faqKeys = {
  all: ['portal', 'faq'] as const,
  list: (page: number, pageSize: number, sorting?: string) =>
    [...faqKeys.all, 'list', page, pageSize, sorting] as const,
  detail: (id: string) => [...faqKeys.all, 'detail', id] as const,
};

export function useFaqList({
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
    queryKey: faqKeys.list(page, pageSize, sorting),
    queryFn: () =>
      listFaqs(session?.accessToken, currentTenantId, page, pageSize, sorting),
    enabled: status === 'ready' && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useFaq(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: faqKeys.detail(id ?? 'unknown'),
    queryFn: () => getFaq(session?.accessToken, currentTenantId, id ?? ''),
    enabled: status === 'ready' && Boolean(currentTenantId) && Boolean(id),
  });
}

export function useCreateFaq() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...faqKeys.all, 'create'],
    mutationFn: (body: FaqCreateRequestDto) =>
      createFaq(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success('FAQ created.');
      await queryClient.invalidateQueries({ queryKey: faqKeys.all });
    },
  });
}

export function useUpdateFaq(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...faqKeys.all, 'update', id],
    mutationFn: (body: FaqUpdateRequestDto) =>
      updateFaq(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success('FAQ updated.');
      await queryClient.invalidateQueries({ queryKey: faqKeys.all });
    },
  });
}

export function useDeleteFaq() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...faqKeys.all, 'delete'],
    mutationFn: (id: string) => deleteFaq(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success('FAQ deleted.');
      await queryClient.invalidateQueries({ queryKey: faqKeys.all });
    },
  });
}

export function useRequestFaqGeneration() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();

  return useMutation({
    mutationKey: [...faqKeys.all, 'generation-request'],
    mutationFn: (id: string) =>
      requestFaqGeneration(session?.accessToken, currentTenantId, id),
    onSuccess: (correlationId) => {
      toast.success(`Generation requested. Correlation ID: ${correlationId}`);
    },
  });
}
