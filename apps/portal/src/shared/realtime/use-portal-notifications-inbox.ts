import { useContext } from "react";
import { PortalNotificationsInboxContext } from "@/shared/realtime/portal-notifications-context";

export function usePortalNotificationsInbox() {
  const context = useContext(PortalNotificationsInboxContext);
  if (!context) {
    throw new Error(
      "usePortalNotificationsInbox must be used within PortalNotificationsProvider.",
    );
  }

  return context;
}
