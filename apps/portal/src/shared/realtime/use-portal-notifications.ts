import { useContext, useEffect } from "react";
import {
  PortalNotificationsContext,
  type PortalNotificationHandler,
} from "@/shared/realtime/portal-notifications-context";

export function usePortalNotifications<TPayload>(
  type: string,
  handler: PortalNotificationHandler<TPayload>,
) {
  const context = useContext(PortalNotificationsContext);
  if (!context) {
    throw new Error(
      "usePortalNotifications must be used within PortalNotificationsProvider.",
    );
  }

  useEffect(() => {
    return context.subscribe((notification) => {
      if (notification.type === type) {
        handler(notification as Parameters<typeof handler>[0]);
      }
    });
  }, [context, handler, type]);
}
