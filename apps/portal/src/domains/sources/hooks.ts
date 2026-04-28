import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  createSource,
  deleteSource,
  getSource,
  listSources,
  updateSource,
} from "@/domains/sources/api";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  SourceCreateRequestDto,
  SourceUpdateRequestDto,
} from "@/domains/sources/types";

const qnaRootKey = ["portal", "qna"] as const;

export const sourceKeys = {
  all: [...qnaRootKey, "sources"] as const,
  list: (params: Record<string, unknown>) =>
    [...sourceKeys.all, "list", params] as const,
  detail: (id: string) => [...sourceKeys.all, "detail", id] as const,
};

export function useSourceList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  kind?: number;
  visibility?: number;
  isAuthoritative?: boolean;
  enabled?: boolean;
}) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();
  const { enabled = true, ...requestParams } = params;

  return useQuery({
    queryKey: sourceKeys.list(requestParams),
    queryFn: ({ signal }) =>
      listSources(session?.accessToken, currentTenantId, requestParams, signal),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: (previous) => previous,
  });
}

export function useSource(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: sourceKeys.detail(id ?? "unknown"),
    queryFn: () => getSource(session?.accessToken, currentTenantId, id ?? ""),
    enabled: status === "ready" && Boolean(currentTenantId) && Boolean(id),
  });
}

function useInvalidateQna() {
  const queryClient = useQueryClient();

  return async () => {
    await queryClient.invalidateQueries({ queryKey: qnaRootKey });
  };
}

export function useCreateSource() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...sourceKeys.all, "create"],
    mutationFn: (body: SourceCreateRequestDto) =>
      createSource(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success(translateText("Source created."));
      await invalidateQna();
    },
  });
}

export function useUpdateSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...sourceKeys.all, "update", id],
    mutationFn: (body: SourceUpdateRequestDto) =>
      updateSource(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Source updated."));
      await invalidateQna();
    },
  });
}

export function useDeleteSource() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...sourceKeys.all, "delete"],
    mutationFn: (id: string) =>
      deleteSource(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Source deleted."));
      await invalidateQna();
    },
  });
}
