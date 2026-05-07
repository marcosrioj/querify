import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  createSource,
  deleteSource,
  getSource,
  listSources,
  updateSource,
} from "@/domains/sources/api";
import {
  createQnaDomainKeys,
  keepPreviousQnaTenantData,
  qnaTenantKey,
} from "@/domains/qna/query-keys";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  SourceCreateRequestDto,
  SourceUpdateRequestDto,
} from "@/domains/sources/types";

export const sourceKeys = createQnaDomainKeys("sources");

export function useSourceList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  kind?: number;
  visibility?: number;
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
    queryKey: sourceKeys.list(currentTenantId, requestParams),
    queryFn: ({ signal }) =>
      listSources(session?.accessToken, currentTenantId, requestParams, signal),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: keepPreviousQnaTenantData(currentTenantId),
    gcTime,
    refetchOnMount,
    staleTime,
  });
}

export function useSource(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: sourceKeys.detail(currentTenantId, id ?? "unknown"),
    queryFn: () => getSource(session?.accessToken, currentTenantId, id ?? ""),
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

export function useCreateSource() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...sourceKeys.all(currentTenantId), "create"],
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
    mutationKey: [...sourceKeys.all(currentTenantId), "update", id],
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
    mutationKey: [...sourceKeys.all(currentTenantId), "delete"],
    mutationFn: (id: string) =>
      deleteSource(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Source deleted."));
      await invalidateQna();
    },
  });
}
