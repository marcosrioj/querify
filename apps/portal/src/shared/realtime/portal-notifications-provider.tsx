import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { PropsWithChildren } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuth } from "@/platform/auth/use-auth";
import { logger } from "@/platform/telemetry/logger";
import { useTenant } from "@/platform/tenant/use-tenant";
import { buildPortalNotificationsHubUrl } from "@/shared/realtime/portal-notifications-client";
import {
  PortalNotificationsContext,
  type PortalNotificationHandler,
  type PortalNotificationsConnectionState,
} from "@/shared/realtime/portal-notifications-context";
import type { PortalNotificationEnvelope } from "@/shared/realtime/portal-notification-types";

const RECONNECT_DELAYS_MS = [0, 2000, 5000, 10000, 30000];

export function PortalNotificationsProvider({ children }: PropsWithChildren) {
  const { getAccessToken, status } = useAuth();
  const { currentTenantId } = useTenant();
  const handlersRef = useRef(new Set<PortalNotificationHandler>());
  const [connectionState, setConnectionState] =
    useState<PortalNotificationsConnectionState>("idle");

  const subscribe = useCallback((handler: PortalNotificationHandler) => {
    handlersRef.current.add(handler);
    return () => {
      handlersRef.current.delete(handler);
    };
  }, []);

  useEffect(() => {
    if (status !== "ready" || !currentTenantId) {
      setConnectionState("idle");
      return;
    }

    let disposed = false;
    const connection = new HubConnectionBuilder()
      .withUrl(buildPortalNotificationsHubUrl(currentTenantId), {
        accessTokenFactory: async () => (await getAccessToken()) ?? "",
      })
      .withAutomaticReconnect(RECONNECT_DELAYS_MS)
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on(
      "portalNotification",
      (notification: PortalNotificationEnvelope) => {
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
  }, [currentTenantId, getAccessToken, status]);

  const value = useMemo(
    () => ({
      connectionState,
      subscribe,
    }),
    [connectionState, subscribe],
  );

  return (
    <PortalNotificationsContext.Provider value={value}>
      {children}
    </PortalNotificationsContext.Provider>
  );
}
