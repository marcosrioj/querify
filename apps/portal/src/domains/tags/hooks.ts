import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  createTag,
  deleteTag,
  getTag,
  listTags,
  updateTag,
} from "@/domains/tags/api";
import {
  createQnaDomainKeys,
  keepPreviousQnaTenantData,
  qnaTenantKey,
} from "@/domains/qna/query-keys";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  TagCreateRequestDto,
  TagUpdateRequestDto,
} from "@/domains/tags/types";

export const tagKeys = createQnaDomainKeys("tags");

export function useTagList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  enabled?: boolean;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();
  const { enabled = true, ...requestParams } = params;

  return useQuery({
    queryKey: tagKeys.list(currentTenantId, requestParams),
    queryFn: ({ signal }) =>
      listTags(session?.accessToken, currentTenantId, requestParams, signal),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: keepPreviousQnaTenantData(currentTenantId),
  });
}

export function useTag(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: tagKeys.detail(currentTenantId, id ?? "unknown"),
    queryFn: () => getTag(session?.accessToken, currentTenantId, id ?? ""),
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

export function useCreateTag() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...tagKeys.all(currentTenantId), "create"],
    mutationFn: (body: TagCreateRequestDto) =>
      createTag(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success(translateText("Tag created."));
      await invalidateQna();
    },
  });
}

export function useUpdateTag(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...tagKeys.all(currentTenantId), "update", id],
    mutationFn: (body: TagUpdateRequestDto) =>
      updateTag(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Tag updated."));
      await invalidateQna();
    },
  });
}

export function useDeleteTag() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...tagKeys.all(currentTenantId), "delete"],
    mutationFn: (id: string) =>
      deleteTag(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Tag deleted."));
      await invalidateQna();
    },
  });
}
