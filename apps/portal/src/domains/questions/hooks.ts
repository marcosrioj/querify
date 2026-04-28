import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  addQuestionSource,
  addQuestionTag,
  approveQuestion,
  createQuestion,
  deleteQuestion,
  escalateQuestion,
  getQuestion,
  listQuestions,
  rejectQuestion,
  removeQuestionSource,
  removeQuestionTag,
  submitQuestion,
  updateQuestion,
} from "@/domains/questions/api";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  QuestionCreateRequestDto,
  QuestionSourceLinkCreateRequestDto,
  QuestionTagCreateRequestDto,
  QuestionUpdateRequestDto,
} from "@/domains/questions/types";

const qnaRootKey = ["portal", "qna"] as const;

export const questionKeys = {
  all: [...qnaRootKey, "questions"] as const,
  list: (params: Record<string, unknown>) =>
    [...questionKeys.all, "list", params] as const,
  detail: (id: string) => [...questionKeys.all, "detail", id] as const,
};

export function useQuestionList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  spaceId?: string;
  acceptedAnswerId?: string;
  duplicateOfQuestionId?: string;
  status?: number;
  visibility?: number;
  spaceKey?: string;
  includeAnswers?: boolean;
  includeTags?: boolean;
  includeSources?: boolean;
  includeActivity?: boolean;
  enabled?: boolean;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();
  const { enabled = true, ...requestParams } = params;

  return useQuery({
    queryKey: questionKeys.list(requestParams),
    queryFn: ({ signal }) =>
      listQuestions(
        session?.accessToken,
        currentTenantId,
        requestParams,
        signal,
      ),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useQuestion(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: questionKeys.detail(id ?? "unknown"),
    queryFn: () => getQuestion(session?.accessToken, currentTenantId, id ?? ""),
    enabled: status === "ready" && Boolean(currentTenantId) && Boolean(id),
  });
}

function useInvalidateQna() {
  const queryClient = useQueryClient();

  return async () => {
    await queryClient.invalidateQueries({ queryKey: qnaRootKey });
  };
}

export function useCreateQuestion() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "create"],
    mutationFn: (body: QuestionCreateRequestDto) =>
      createQuestion(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success(translateText("Question created."));
      await invalidateQna();
    },
  });
}

export function useUpdateQuestion(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "update", id],
    mutationFn: (body: QuestionUpdateRequestDto) =>
      updateQuestion(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Question updated."));
      await invalidateQna();
    },
  });
}

export function useDeleteQuestion() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "delete"],
    mutationFn: (id: string) =>
      deleteQuestion(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Question deleted."));
      await invalidateQna();
    },
  });
}

export function useSubmitQuestion() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "workflow", "submit"],
    mutationFn: (id: string) =>
      submitQuestion(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Question submitted for review."));
      await invalidateQna();
    },
  });
}

export function useApproveQuestion() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "workflow", "approve"],
    mutationFn: (id: string) =>
      approveQuestion(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Question approved."));
      await invalidateQna();
    },
  });
}

export function useRejectQuestion() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "workflow", "reject"],
    mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
      rejectQuestion(session?.accessToken, currentTenantId, id, notes),
    onSuccess: async () => {
      toast.success(translateText("Question rejected."));
      await invalidateQna();
    },
  });
}

export function useEscalateQuestion() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "workflow", "escalate"],
    mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
      escalateQuestion(session?.accessToken, currentTenantId, id, notes),
    onSuccess: async () => {
      toast.success(translateText("Question escalated."));
      await invalidateQna();
    },
  });
}

export function useAddQuestionTag(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "tag", id, "add"],
    mutationFn: (body: QuestionTagCreateRequestDto) =>
      addQuestionTag(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Question tag added."));
      await invalidateQna();
    },
  });
}

export function useRemoveQuestionTag(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "tag", id, "remove"],
    mutationFn: (tagId: string) =>
      removeQuestionTag(session?.accessToken, currentTenantId, id, tagId),
    onSuccess: async () => {
      toast.success(translateText("Question tag removed."));
      await invalidateQna();
    },
  });
}

export function useAddQuestionSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "source", id, "add"],
    mutationFn: (body: QuestionSourceLinkCreateRequestDto) =>
      addQuestionSource(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Question source added."));
      await invalidateQna();
    },
  });
}

export function useRemoveQuestionSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...questionKeys.all, "source", id, "remove"],
    mutationFn: (sourceLinkId: string) =>
      removeQuestionSource(
        session?.accessToken,
        currentTenantId,
        id,
        sourceLinkId,
      ),
    onSuccess: async () => {
      toast.success(translateText("Question source removed."));
      await invalidateQna();
    },
  });
}
