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

export const PortalNotificationsContext = createContext<
  PortalNotificationsContextValue | undefined
>(undefined);
