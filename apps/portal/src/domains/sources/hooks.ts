import { useCallback } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  completeSourceUpload,
  createSource,
  createSourceUploadIntent,
  deleteSource,
  getSource,
  getSourceDownloadUrl,
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
import {
  PORTAL_NOTIFICATION_TYPES,
  type SourceUploadStatusChangedNotificationPayload,
} from "@/shared/realtime/portal-notification-types";
import { usePortalNotifications } from "@/shared/realtime/use-portal-notifications";
import { translateText } from "@/shared/lib/i18n-core";
import type {
  SourceCreateRequestDto,
  SourceDetailDto,
  SourceUploadCompleteRequestDto,
  SourceUploadIntentRequestDto,
  SourceUpdateRequestDto,
} from "@/domains/sources/types";

export const sourceKeys = createQnaDomainKeys("sources");

export function useSourceList(params: {
  page: number;
  pageSize: number;
  sorting?: string;
  searchText?: string;
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
    queryFn: ({ signal }) =>
      getSource(session?.accessToken, currentTenantId, id ?? "", signal),
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

export function useCreateSourceUploadIntent() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();

  return useMutation({
    mutationKey: [...sourceKeys.all(currentTenantId), "upload-intent"],
    mutationFn: (body: SourceUploadIntentRequestDto) =>
      createSourceUploadIntent(session?.accessToken, currentTenantId, body),
  });
}

export function useCompleteSourceUpload() {
  const { session } = useAuth();
  const { currentTenantId } = useTenant();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: [...sourceKeys.all(currentTenantId), "upload-complete"],
    mutationFn: ({
      body,
      id,
    }: {
      id: string;
      body: SourceUploadCompleteRequestDto;
    }) => completeSourceUpload(session?.accessToken, currentTenantId, id, body),
    onSuccess: async (_result, variables) => {
      toast.success(translateText("Source upload submitted."));
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: sourceKeys.list(currentTenantId, {}),
        }),
        queryClient.invalidateQueries({
          queryKey: sourceKeys.detail(currentTenantId, variables.id),
        }),
        queryClient.invalidateQueries({
          queryKey: qnaTenantKey(currentTenantId),
        }),
      ]);
    },
  });
}

export function useSourceUploadStatusNotifications(id: string | undefined) {
  const queryClient = useQueryClient();
  const { currentTenantId } = useTenant();

  const handleNotification = useCallback(
    (notification: {
      tenantId: string;
      resourceKind: string;
      resourceId: string;
      payload: SourceUploadStatusChangedNotificationPayload;
    }) => {
      if (
        !id ||
        !currentTenantId ||
        notification.tenantId !== currentTenantId ||
        notification.resourceKind !== "source" ||
        notification.resourceId !== id
      ) {
        return;
      }

      queryClient.setQueryData<SourceDetailDto>(
        sourceKeys.detail(currentTenantId, id),
        (current) =>
          current
            ? {
                ...current,
                uploadStatus: notification.payload.uploadStatus,
                storageKey: notification.payload.storageKey ?? current.storageKey,
                checksum: notification.payload.checksum ?? current.checksum,
              }
            : current,
      );

      void queryClient.invalidateQueries({
        queryKey: sourceKeys.detail(currentTenantId, id),
      });
      void queryClient.invalidateQueries({
        queryKey: sourceKeys.all(currentTenantId),
      });
    },
    [currentTenantId, id, queryClient],
  );

  usePortalNotifications<SourceUploadStatusChangedNotificationPayload>(
    PORTAL_NOTIFICATION_TYPES.SOURCE_UPLOAD_STATUS_CHANGED,
    handleNotification,
  );
}

export function useSourceUploadStatusListNotifications() {
  const queryClient = useQueryClient();
  const { currentTenantId } = useTenant();

  const handleNotification = useCallback(
    (notification: {
      tenantId: string;
      resourceKind: string;
      payload: SourceUploadStatusChangedNotificationPayload;
    }) => {
      if (
        !currentTenantId ||
        notification.tenantId !== currentTenantId ||
        notification.resourceKind !== "source"
      ) {
        return;
      }

      void queryClient.invalidateQueries({
        queryKey: sourceKeys.all(currentTenantId),
      });
      void queryClient.invalidateQueries({
        queryKey: qnaTenantKey(currentTenantId),
      });
    },
    [currentTenantId, queryClient],
  );

  usePortalNotifications<SourceUploadStatusChangedNotificationPayload>(
    PORTAL_NOTIFICATION_TYPES.SOURCE_UPLOAD_STATUS_CHANGED,
    handleNotification,
  );
}

export function useSourceDownloadUrl(id: string | undefined, enabled = false) {
  const { session, status } = useAuth();
  const { currentTenantId } = useTenant();

  return useQuery({
    queryKey: [
      ...sourceKeys.detail(currentTenantId, id ?? "unknown"),
      "download-url",
    ],
    queryFn: ({ signal }) =>
      getSourceDownloadUrl(
        session?.accessToken,
        currentTenantId,
        id ?? "",
        signal,
      ),
    enabled:
      enabled && status === "ready" && Boolean(currentTenantId) && Boolean(id),
  });
}
