import type { SourceUploadStatus } from "@/shared/constants/backend-enums";

export const PORTAL_NOTIFICATION_TYPES = {
  SOURCE_UPLOAD_STATUS_CHANGED: "qna.source-upload.status-changed.v1",
} as const;

export type PortalNotificationEnvelope<TPayload = unknown> = {
  notificationId: string;
  occurredAtUtc: string;
  type: string;
  module: string;
  tenantId: string;
  resourceKind: string;
  resourceId: string;
  version: number;
  payload: TPayload;
};

export type SourceUploadStatusChangedNotificationPayload = {
  uploadStatus: SourceUploadStatus;
  storageKey?: string | null;
  checksum?: string | null;
  reason?: string | null;
};
