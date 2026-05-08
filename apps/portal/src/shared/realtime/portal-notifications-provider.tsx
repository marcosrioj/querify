import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { PropsWithChildren } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuth } from "@/platform/auth/use-auth";
import { logger } from "@/platform/telemetry/logger";
import { useTenant } from "@/platform/tenant/use-tenant";
import { buildPortalNotificationsHubUrl } from "@/shared/realtime/portal-notifications-client";
import {
  PortalNotificationsContext,
  PortalNotificationsInboxContext,
  type PortalNotificationHandler,
  type PortalNotificationInboxItem,
  type PortalNotificationsConnectionState,
} from "@/shared/realtime/portal-notifications-context";
import type { PortalNotificationEnvelope } from "@/shared/realtime/portal-notification-types";

const RECONNECT_DELAYS_MS = [0, 2000, 5000, 10000, 30000];
const NOTIFICATION_INBOX_LIMIT = 50;

function getNotificationInboxId(notification: PortalNotificationEnvelope) {
  return (
    notification.notificationId ||
    [
      notification.type,
      notification.tenantId,
      notification.resourceKind,
      notification.resourceId,
      notification.occurredAtUtc,
    ].join(":")
  );
}

export function PortalNotificationsProvider({ children }: PropsWithChildren) {
  const { getAccessToken, status, user } = useAuth();
  const { currentTenantId } = useTenant();
  const handlersRef = useRef(new Set<PortalNotificationHandler>());
  const getAccessTokenRef = useRef(getAccessToken);
  const userIdRef = useRef<string | undefined>(undefined);
  const [notifications, setNotifications] = useState<
    PortalNotificationInboxItem[]
  >([]);
  const [connectionState, setConnectionState] =
    useState<PortalNotificationsConnectionState>("idle");

  useEffect(() => {
    getAccessTokenRef.current = getAccessToken;
  }, [getAccessToken]);

  useEffect(() => {
    if (status !== "ready") {
      if (status === "unauthenticated") {
        setNotifications([]);
      }
      userIdRef.current = undefined;
      return;
    }

    if (userIdRef.current && user?.id && userIdRef.current !== user.id) {
      setNotifications([]);
    }

    userIdRef.current = user?.id;
  }, [status, user?.id]);

  const subscribe = useCallback((handler: PortalNotificationHandler) => {
    handlersRef.current.add(handler);
    return () => {
      handlersRef.current.delete(handler);
    };
  }, []);

  const markNotificationRead = useCallback((id: string) => {
    const readAtUtc = new Date().toISOString();
    setNotifications((current) =>
      current.map((item) =>
        item.id === id && !item.readAtUtc ? { ...item, readAtUtc } : item,
      ),
    );
  }, []);

  const markAllNotificationsRead = useCallback(() => {
    const readAtUtc = new Date().toISOString();
    setNotifications((current) =>
      current.map((item) => (item.readAtUtc ? item : { ...item, readAtUtc })),
    );
  }, []);

  const clearNotifications = useCallback(() => {
    setNotifications([]);
  }, []);

  useEffect(() => {
    if (status !== "ready" || !currentTenantId) {
      setConnectionState("idle");
      return;
    }

    let disposed = false;
    const connection = new HubConnectionBuilder()
      .withUrl(buildPortalNotificationsHubUrl(currentTenantId), {
        accessTokenFactory: async () =>
          (await getAccessTokenRef.current()) ?? "",
      })
      .withAutomaticReconnect(RECONNECT_DELAYS_MS)
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on(
      "portalNotification",
      (notification: PortalNotificationEnvelope) => {
        const id = getNotificationInboxId(notification);
        const receivedAtUtc = new Date().toISOString();

        setNotifications((current) => {
          const existing = current.find((item) => item.id === id);
          const nextItem: PortalNotificationInboxItem = {
            id,
            notification,
            receivedAtUtc: existing?.receivedAtUtc ?? receivedAtUtc,
            readAtUtc: existing?.readAtUtc,
          };
          const nextNotifications = [
            nextItem,
            ...current.filter((item) => item.id !== id),
          ];

          return nextNotifications.slice(0, NOTIFICATION_INBOX_LIMIT);
        });

        for (const handler of handlersRef.current) {
          handler(notification);
        }
      },
    );

    connection.onreconnecting(() => {
      if (!disposed) {
        setConnectionState("reconnecting");
      }
    });
    connection.onreconnected(() => {
      if (!disposed) {
        setConnectionState("connected");
      }
    });
    connection.onclose((error) => {
      if (!disposed) {
        setConnectionState("disconnected");
        if (error) {
          logger.warn("Portal notifications connection closed", error);
        }
      }
    });

    setConnectionState("connecting");
    void connection
      .start()
      .then(() => {
        if (!disposed) {
          setConnectionState("connected");
        }
      })
      .catch((error) => {
        if (!disposed) {
          setConnectionState("disconnected");
          logger.warn("Portal notifications connection failed", error);
        }
      });

    return () => {
      disposed = true;
      connection.off("portalNotification");
      void connection.stop();
    };
  }, [currentTenantId, status]);

  const value = useMemo(
    () => ({
      connectionState,
      subscribe,
    }),
    [connectionState, subscribe],
  );

  const unreadCount = useMemo(
    () => notifications.filter((item) => !item.readAtUtc).length,
    [notifications],
  );

  const inboxValue = useMemo(
    () => ({
      notifications,
      unreadCount,
      markNotificationRead,
      markAllNotificationsRead,
      clearNotifications,
    }),
    [
      clearNotifications,
      markAllNotificationsRead,
      markNotificationRead,
      notifications,
      unreadCount,
    ],
  );

  return (
    <PortalNotificationsContext.Provider value={value}>
      <PortalNotificationsInboxContext.Provider value={inboxValue}>
        {children}
      </PortalNotificationsInboxContext.Provider>
    </PortalNotificationsContext.Provider>
  );
}
