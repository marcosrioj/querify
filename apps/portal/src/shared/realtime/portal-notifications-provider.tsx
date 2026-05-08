import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { PropsWithChildren } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuth } from "@/platform/auth/use-auth";
import { logger } from "@/platform/telemetry/logger";
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
const NOTIFICATION_INBOX_STORAGE_KEY_PREFIX =
  "querify.portal.notifications.inbox";

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

function getNotificationInboxStorageKey(userId?: string) {
  if (!userId) {
    return undefined;
  }

  return `${NOTIFICATION_INBOX_STORAGE_KEY_PREFIX}:${userId}`;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function isStoredNotificationInboxItem(
  value: unknown,
): value is PortalNotificationInboxItem {
  if (
    !isRecord(value) ||
    typeof value.id !== "string" ||
    typeof value.receivedAtUtc !== "string" ||
    (value.readAtUtc !== undefined && typeof value.readAtUtc !== "string")
  ) {
    return false;
  }

  const notification = value.notification;

  return (
    isRecord(notification) &&
    (notification.notificationId === undefined ||
      typeof notification.notificationId === "string") &&
    typeof notification.occurredAtUtc === "string" &&
    typeof notification.type === "string" &&
    typeof notification.module === "string" &&
    typeof notification.tenantId === "string" &&
    typeof notification.resourceKind === "string" &&
    typeof notification.resourceId === "string" &&
    typeof notification.version === "number"
  );
}

function readNotificationInboxFromStorage(
  storageKey: string,
): PortalNotificationInboxItem[] {
  try {
    const rawNotifications = globalThis.localStorage?.getItem(storageKey);
    if (!rawNotifications) {
      return [];
    }

    const parsedNotifications: unknown = JSON.parse(rawNotifications);
    if (!Array.isArray(parsedNotifications)) {
      return [];
    }

    return parsedNotifications
      .filter(isStoredNotificationInboxItem)
      .slice(0, NOTIFICATION_INBOX_LIMIT);
  } catch (error) {
    logger.warn("Portal notifications inbox could not be restored", error);
    return [];
  }
}

function writeNotificationInboxToStorage(
  storageKey: string,
  notifications: PortalNotificationInboxItem[],
) {
  try {
    if (notifications.length === 0) {
      globalThis.localStorage?.removeItem(storageKey);
      return;
    }

    globalThis.localStorage?.setItem(
      storageKey,
      JSON.stringify(notifications.slice(0, NOTIFICATION_INBOX_LIMIT)),
    );
  } catch (error) {
    logger.warn("Portal notifications inbox could not be persisted", error);
  }
}

export function PortalNotificationsProvider({ children }: PropsWithChildren) {
  const { getAccessToken, status, user } = useAuth();
  const handlersRef = useRef(new Set<PortalNotificationHandler>());
  const getAccessTokenRef = useRef(getAccessToken);
  const hydratedStorageKeyRef = useRef<string | undefined>(undefined);
  const [notifications, setNotifications] = useState<
    PortalNotificationInboxItem[]
  >([]);
  const [notificationStorageKey, setNotificationStorageKey] = useState<
    string | undefined
  >(undefined);
  const [connectionState, setConnectionState] =
    useState<PortalNotificationsConnectionState>("idle");

  useEffect(() => {
    getAccessTokenRef.current = getAccessToken;
  }, [getAccessToken]);

  useEffect(() => {
    if (status !== "ready") {
      if (status === "unauthenticated") {
        hydratedStorageKeyRef.current = undefined;
        setNotificationStorageKey(undefined);
        setNotifications([]);
      }
      return;
    }

    const nextStorageKey = getNotificationInboxStorageKey(user?.id);
    if (!nextStorageKey) {
      hydratedStorageKeyRef.current = undefined;
      setNotificationStorageKey(undefined);
      setNotifications([]);
      return;
    }

    if (hydratedStorageKeyRef.current === nextStorageKey) {
      return;
    }

    hydratedStorageKeyRef.current = nextStorageKey;
    setNotificationStorageKey(nextStorageKey);
    setNotifications(readNotificationInboxFromStorage(nextStorageKey));
  }, [status, user?.id]);

  useEffect(() => {
    if (
      !notificationStorageKey ||
      hydratedStorageKeyRef.current !== notificationStorageKey
    ) {
      return;
    }

    writeNotificationInboxToStorage(notificationStorageKey, notifications);
  }, [notificationStorageKey, notifications]);

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
    if (status !== "ready") {
      setConnectionState("idle");
      return;
    }

    let disposed = false;
    const connection = new HubConnectionBuilder()
      .withUrl(buildPortalNotificationsHubUrl(), {
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
  }, [status]);

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
