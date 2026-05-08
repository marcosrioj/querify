import {
  SourceUploadStatus,
  sourceUploadStatusLabels,
} from "@/shared/constants/backend-enums";
import {
  sourceUploadStatusPresentation,
  type BadgeVariant,
} from "@/shared/constants/enum-ui";
import {
  PORTAL_NOTIFICATION_TYPES,
  type PortalNotificationEnvelope,
  type SourceUploadStatusChangedNotificationPayload,
} from "@/shared/realtime/portal-notification-types";
import { translateText } from "@/shared/lib/i18n-core";

export type PortalNotificationPresentation = {
  title: string;
  badgeLabel: string;
  badgeVariant: BadgeVariant;
  resourceLabel: string;
  href?: string;
};

type PortalNotificationPresenter = (
  notification: PortalNotificationEnvelope,
) => PortalNotificationPresentation;

function toTitleCase(value: string) {
  return value
    .replace(/[-_.]+/g, " ")
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .map((part) => `${part.charAt(0).toUpperCase()}${part.slice(1)}`)
    .join(" ");
}

function getKnownResourceHref(resourceKind: string, resourceId: string) {
  if (!resourceId) {
    return undefined;
  }

  switch (resourceKind.toLowerCase()) {
    case "activity":
      return `/app/activity/${resourceId}`;
    case "answer":
      return `/app/answers/${resourceId}`;
    case "question":
      return `/app/questions/${resourceId}`;
    case "source":
      return `/app/sources/${resourceId}`;
    case "space":
      return `/app/spaces/${resourceId}`;
    default:
      return undefined;
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return Boolean(value) && typeof value === "object";
}

function isSourceUploadStatusPayload(
  value: unknown,
): value is SourceUploadStatusChangedNotificationPayload {
  return (
    isRecord(value) &&
    typeof value.uploadStatus === "number" &&
    value.uploadStatus in sourceUploadStatusLabels
  );
}

function presentSourceUploadStatusChanged(
  notification: PortalNotificationEnvelope,
) {
  const payload = isSourceUploadStatusPayload(notification.payload)
    ? notification.payload
    : undefined;
  const status = payload?.uploadStatus ?? SourceUploadStatus.Pending;
  const statusPresentation = sourceUploadStatusPresentation[status];
  const statusLabel =
    statusPresentation?.label ?? sourceUploadStatusLabels[status] ?? "Updated";

  return {
    title: translateText("Source upload {status}", {
      status: translateText(statusLabel).toLowerCase(),
    }),
    badgeLabel: statusLabel,
    badgeVariant: statusPresentation?.badgeVariant ?? "info",
    resourceLabel: translateText("Source"),
    href: getKnownResourceHref(
      notification.resourceKind,
      notification.resourceId,
    ),
  } satisfies PortalNotificationPresentation;
}

const portalNotificationPresenters: Record<
  string,
  PortalNotificationPresenter
> = {
  [PORTAL_NOTIFICATION_TYPES.SOURCE_UPLOAD_STATUS_CHANGED]:
    presentSourceUploadStatusChanged,
};

export function presentPortalNotification(
  notification: PortalNotificationEnvelope,
): PortalNotificationPresentation {
  const presenter = portalNotificationPresenters[notification.type];
  if (presenter) {
    return presenter(notification);
  }

  const resourceLabel = toTitleCase(notification.resourceKind || "resource");
  const moduleLabel = toTitleCase(notification.module || "portal");

  return {
    title: translateText("{resource} notification", {
      resource: resourceLabel,
    }),
    badgeLabel: moduleLabel,
    badgeVariant: "outline",
    resourceLabel,
    href: getKnownResourceHref(
      notification.resourceKind,
      notification.resourceId,
    ),
  };
}
