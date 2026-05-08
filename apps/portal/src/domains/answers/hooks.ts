import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  addAnswerSource,
  activateAnswer,
  archiveAnswer,
  createAnswer,
  deleteAnswer,
  getAnswer,
  listAnswers,
  removeAnswerSource,
  updateAnswer,
} from "@/domains/answers/api";
import {
  createQnaDomainKeys,
  keepPreviousQnaTenantData,
  qnaTenantKey,
} from "@/domains/qna/query-keys";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import {
  AnswerStatus,
  VisibilityScope,
} from "@/shared/constants/backend-enums";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  AnswerCreateRequestDto,
  AnswerDto,
  AnswerSourceLinkCreateRequestDto,
  AnswerUpdateRequestDto,
} from "@/domains/answers/types";

export const answerKeys = createQnaDomainKeys("answers");

export function useAnswerList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  spaceId?: string;
  sourceId?: string;
  questionId?: string;
  status?: number;
  visibility?: number;
  isAccepted?: boolean;
  enabled?: boolean;
  staleTime?: number;
  gcTime?: number;
  refetchOnMount?: boolean | "always";
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();
  const {
    enabled = true,
    gcTime,
    refetchOnMount,
    staleTime,
    ...requestParams
  } = params;

  return useQuery({
    queryKey: answerKeys.list(currentTenantId, requestParams),
    queryFn: ({ signal }) =>
      listAnswers(session?.accessToken, currentTenantId, requestParams, signal),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: keepPreviousQnaTenantData(currentTenantId),
    gcTime,
    refetchOnMount,
    staleTime,
  });
}

export function useAnswer(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: answerKeys.detail(currentTenantId, id ?? "unknown"),
    queryFn: ({ signal }) =>
      getAnswer(session?.accessToken, currentTenantId, id ?? "", signal),
    enabled: status === "ready" && Boolean(currentTenantId) && Boolean(id),
  });
}

function useInvalidateQna() {
  const queryClient = useQueryClient();
  const { currentTenantId } = useTenant();

  return async () => {
    await queryClient.invalidateQueries({
      queryKey: qnaTenantKey(currentTenantId),
    });
  };
}

export function useCreateAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "create"],
    mutationFn: (body: AnswerCreateRequestDto) =>
      createAnswer(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success(translateText("Answer created."));
      await invalidateQna();
    },
  });
}

export function useUpdateAnswer(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "update", id],
    mutationFn: (body: AnswerUpdateRequestDto) =>
      updateAnswer(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Answer updated."));
      await invalidateQna();
    },
  });
}

export function useUpdateAnswerStatus() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "workflow", "status"],
    mutationFn: ({
      answer,
      status,
      visibility,
    }: {
      answer: AnswerDto;
      status: AnswerStatus;
      visibility?: VisibilityScope;
    }) =>
      updateAnswer(session?.accessToken, currentTenantId, answer.id, {
        headline: answer.headline,
        body: answer.body ?? undefined,
        kind: answer.kind,
        status,
        visibility:
          visibility ??
          (status === AnswerStatus.Active
            ? answer.visibility
            : VisibilityScope.Internal),
        contextNote: answer.contextNote ?? undefined,
        authorLabel: answer.authorLabel ?? undefined,
        sort: answer.sort,
      }),
    onSuccess: async () => {
      toast.success(translateText("Answer status updated."));
      await invalidateQna();
    },
  });
}

export function useDeleteAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "delete"],
    mutationFn: (id: string) =>
      deleteAnswer(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Answer deleted."));
      await invalidateQna();
    },
  });
}

export function useActivateAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "workflow", "activate"],
    mutationFn: (id: string) =>
      activateAnswer(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Answer activated."));
      await invalidateQna();
    },
  });
}

export function useArchiveAnswer() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "workflow", "archive"],
    mutationFn: (id: string) =>
      archiveAnswer(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Answer archived."));
      await invalidateQna();
    },
  });
}

export function useAddAnswerSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "source", id, "add"],
    mutationFn: (body: AnswerSourceLinkCreateRequestDto) =>
      addAnswerSource(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Answer source added."));
      await invalidateQna();
    },
  });
}

export function useRemoveAnswerSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...answerKeys.all(currentTenantId), "source", id, "remove"],
    mutationFn: (sourceLinkId: string) =>
      removeAnswerSource(
        session?.accessToken,
        currentTenantId,
        id,
        sourceLinkId,
      ),
    onSuccess: async () => {
      toast.success(translateText("Answer source removed."));
      await invalidateQna();
    },
  });
}
