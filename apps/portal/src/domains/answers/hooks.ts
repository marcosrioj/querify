import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  addAnswerSource,
  activateAnswer,
  createAnswer,
  deleteAnswer,
  getAnswer,
  listAnswers,
  removeAnswerSource,
  retireAnswer,
  updateAnswer,
} from '@/domains/answers/api';
import { useAuth } from '@/platform/auth/use-auth';
import { useTenant } from '@/platform/tenant/use-tenant';
import { translateText } from '@/shared/lib/i18n-core';
import type {
  AnswerCreateRequestDto,
  AnswerSourceLinkCreateRequestDto,
  AnswerUpdateRequestDto,
} from '@/domains/answers/types';

const qnaRootKey = ['portal', 'qna'] as const;

export const answerKeys = {
  all: [...qnaRootKey, 'answers'] as const,
  list: (params: Record<string, unknown>) => [...answerKeys.all, 'list', params] as const,
  detail: (id: string) => [...answerKeys.all, 'detail', id] as const,
};

export function useAnswerList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  spaceId?: string;
  questionId?: string;
  status?: number;
  visibility?: number;
  isAccepted?: boolean;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: answerKeys.list(params),
    queryFn: ({ signal }) => listAnswers(session?.accessToken, currentTenantId, params, signal),
    enabled: status === 'ready' && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useAnswer(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: answerKeys.detail(id ?? 'unknown'),
    queryFn: () => getAnswer(session?.accessToken, currentTenantId, id ?? ''),
    enabled: status === 'ready' && Boolean(currentTenantId) && Boolean(id),
  });
}

function useInvalidateQna() {
  const queryClient = useQueryClient();

  return async () => {
    await queryClient.invalidateQueries({ queryKey: qnaRootKey });
  };
}

export function useCreateAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'create'],
    mutationFn: (body: AnswerCreateRequestDto) =>
      createAnswer(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success(translateText('Answer created.'));
      await invalidateQna();
    },
  });
}

export function useUpdateAnswer(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'update', id],
    mutationFn: (body: AnswerUpdateRequestDto) =>
      updateAnswer(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText('Answer updated.'));
      await invalidateQna();
    },
  });
}

export function useDeleteAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'delete'],
    mutationFn: (id: string) => deleteAnswer(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText('Answer deleted.'));
      await invalidateQna();
    },
  });
}

export function useActivateAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'workflow', 'activate'],
    mutationFn: (id: string) => activateAnswer(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText('Answer activated.'));
      await invalidateQna();
    },
  });
}

export function useRetireAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'workflow', 'retire'],
    mutationFn: (id: string) => retireAnswer(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText('Answer retired.'));
      await invalidateQna();
    },
  });
}

export function useAddAnswerSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'source', id, 'add'],
    mutationFn: (body: AnswerSourceLinkCreateRequestDto) =>
      addAnswerSource(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText('Answer source added.'));
      await invalidateQna();
    },
  });
}

export function useRemoveAnswerSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all, 'source', id, 'remove'],
    mutationFn: (sourceLinkId: string) =>
      removeAnswerSource(session?.accessToken, currentTenantId, id, sourceLinkId),
    onSuccess: async () => {
      toast.success(translateText('Answer source removed.'));
      await invalidateQna();
    },
  });
}
