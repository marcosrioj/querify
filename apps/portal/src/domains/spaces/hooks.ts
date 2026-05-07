import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  addSpaceSource,
  addSpaceTag,
  createSpace,
  deleteSpace,
  getSpace,
  listSpaces,
  removeSpaceSource,
  removeSpaceTag,
  updateSpace,
} from "@/domains/spaces/api";
import {
  createQnaDomainKeys,
  keepPreviousQnaTenantData,
  qnaTenantKey,
} from "@/domains/qna/query-keys";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  SpaceCreateRequestDto,
  SpaceSourceCreateRequestDto,
  SpaceTagCreateRequestDto,
  SpaceUpdateRequestDto,
} from "@/domains/spaces/types";

export const spaceKeys = createQnaDomainKeys("spaces");

export function useSpaceList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
  visibility?: number;
  status?: number;
  acceptsQuestions?: boolean;
  acceptsAnswers?: boolean;
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
    queryKey: spaceKeys.list(currentTenantId, requestParams),
    queryFn: ({ signal }) =>
      listSpaces(session?.accessToken, currentTenantId, requestParams, signal),
    enabled: enabled && status === "ready" && Boolean(currentTenantId),
    placeholderData: keepPreviousQnaTenantData(currentTenantId),
    gcTime,
    refetchOnMount,
    staleTime,
  });
}

export function useSpace(id: string | undefined) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: spaceKeys.detail(currentTenantId, id ?? "unknown"),
    queryFn: () => getSpace(session?.accessToken, currentTenantId, id ?? ""),
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

export function useCreateSpace() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "create"],
    mutationFn: (body: SpaceCreateRequestDto) =>
      createSpace(session?.accessToken, currentTenantId, body),
    onSuccess: async () => {
      toast.success(translateText("Space created."));
      await invalidateQna();
    },
  });
}

export function useUpdateSpace(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "update", id],
    mutationFn: (body: SpaceUpdateRequestDto) =>
      updateSpace(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Space updated."));
      await invalidateQna();
    },
  });
}

export function useDeleteSpace() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "delete"],
    mutationFn: (id: string) =>
      deleteSpace(session?.accessToken, currentTenantId, id),
    onSuccess: async () => {
      toast.success(translateText("Space deleted."));
      await invalidateQna();
    },
  });
}

export function useAddSpaceTag(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "tag", id, "add"],
    mutationFn: (body: SpaceTagCreateRequestDto) =>
      addSpaceTag(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Space tag added."));
      await invalidateQna();
    },
  });
}

export function useRemoveSpaceTag(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "tag", id, "remove"],
    mutationFn: (tagId: string) =>
      removeSpaceTag(session?.accessToken, currentTenantId, id, tagId),
    onSuccess: async () => {
      toast.success(translateText("Space tag removed."));
      await invalidateQna();
    },
  });
}

export function useAddSpaceSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "source", id, "add"],
    mutationFn: (body: SpaceSourceCreateRequestDto) =>
      addSpaceSource(session?.accessToken, currentTenantId, id, body),
    onSuccess: async () => {
      toast.success(translateText("Curated source added."));
      await invalidateQna();
    },
  });
}

export function useRemoveSpaceSource(id: string) {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const invalidateQna = useInvalidateQna();

  return useMutation({
    mutationKey: [...spaceKeys.all(currentTenantId), "source", id, "remove"],
    mutationFn: (sourceId: string) =>
      removeSpaceSource(session?.accessToken, currentTenantId, id, sourceId),
    onSuccess: async () => {
      toast.success(translateText("Curated source removed."));
      await invalidateQna();
    },
  });
}
