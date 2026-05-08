import { createContext } from "react";
import type { PortalNotificationEnvelope } from "@/shared/realtime/portal-notification-types";

export type PortalNotificationHandler<TPayload = unknown> = (
  notification: PortalNotificationEnvelope<TPayload>,
) => void;

export type PortalNotificationsConnectionState =
  | "idle"
  | "connecting"
  | "connected"
  | "reconnecting"
  | "disconnected";

export type PortalNotificationsContextValue = {
  connectionState: PortalNotificationsConnectionState;
  subscribe: (handler: PortalNotificationHandler) => () => void;
};

export type PortalNotificationInboxItem<TPayload = unknown> = {
  id: string;
  notification: PortalNotificationEnvelope<TPayload>;
  receivedAtUtc: string;
  readAtUtc?: string;
};

export type PortalNotificationsInboxContextValue = {
  notifications: PortalNotificationInboxItem[];
  unreadCount: number;
  markNotificationRead: (id: string) => void;
  markAllNotificationsRead: () => void;
  clearNotifications: () => void;
};

export const PortalNotificationsContext = createContext<
  PortalNotificationsContextValue | undefined
>(undefined);

export const PortalNotificationsInboxContext = createContext<
  PortalNotificationsInboxContextValue | undefined
>(undefined);
