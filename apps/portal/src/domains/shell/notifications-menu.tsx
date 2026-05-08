import { Link } from "react-router-dom";
import { Bell, BellDot, CheckCheck, Inbox, Trash2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { usePortalNotificationsInbox } from "@/shared/realtime/use-portal-notifications-inbox";
import { presentPortalNotification } from "@/shared/realtime/portal-notification-presenters";
import type { PortalNotificationInboxItem } from "@/shared/realtime/portal-notifications-context";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import {
  Badge,
  Button,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/ui";

function formatNotificationTime(input: string) {
  const date = new Date(input);
  if (Number.isNaN(date.getTime())) {
    return input;
  }

  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function NotificationRow({
  item,
  onRead,
}: {
  item: PortalNotificationInboxItem;
  onRead: (id: string) => void;
}) {
  const presentation = presentPortalNotification(item.notification);
  const isUnread = !item.readAtUtc;
  const timeLabel = formatNotificationTime(
    item.notification.occurredAtUtc || item.receivedAtUtc,
  );
  const content = (
    <div className="flex min-w-0 items-start gap-3 p-3">
      <span
        className={cn(
          "mt-1 flex size-8 shrink-0 items-center justify-center rounded-md border",
          isUnread
            ? "border-primary/20 bg-primary/10 text-primary"
            : "border-border bg-muted/40 text-muted-foreground",
        )}
      >
        {isUnread ? (
          <BellDot className="size-4" />
        ) : (
          <Bell className="size-4" />
        )}
      </span>
      <span className="min-w-0 flex-1 space-y-2">
        <span className="flex min-w-0 items-start justify-between gap-2">
          <span className="min-w-0 text-sm font-medium leading-5 text-foreground">
            {presentation.title}
          </span>
          {isUnread ? (
            <span className="mt-1 size-2 shrink-0 rounded-full bg-primary" />
          ) : null}
        </span>
        <span className="block text-xs leading-5 text-muted-foreground">
          {presentation.description}
        </span>
        <span className="flex min-w-0 flex-wrap items-center gap-1.5">
          <Badge
            variant={presentation.badgeVariant}
            appearance="outline"
            size="sm"
          >
            {presentation.badgeLabel}
          </Badge>
          <span className="text-[0.7rem] text-muted-foreground">
            {presentation.resourceLabel}
          </span>
          <span className="text-[0.7rem] text-muted-foreground">/</span>
          <span className="text-[0.7rem] text-muted-foreground">
            {timeLabel}
          </span>
        </span>
      </span>
    </div>
  );

  if (presentation.href) {
    return (
      <DropdownMenuItem
        asChild
        className={cn(
          "block cursor-pointer rounded-md p-0",
          isUnread && "bg-primary/[0.035]",
        )}
        onSelect={() => onRead(item.id)}
      >
        <Link to={presentation.href}>{content}</Link>
      </DropdownMenuItem>
    );
  }

  return (
    <DropdownMenuItem
      className={cn(
        "block cursor-pointer rounded-md p-0",
        isUnread && "bg-primary/[0.035]",
      )}
      onSelect={() => onRead(item.id)}
    >
      {content}
    </DropdownMenuItem>
  );
}

export function NotificationsMenu({
  triggerVariant = "outline",
}: {
  triggerVariant?: "outline" | "ghost";
}) {
  const { t } = usePortalI18n();
  const {
    clearNotifications,
    markAllNotificationsRead,
    markNotificationRead,
    notifications,
    unreadCount,
  } = usePortalNotificationsInbox();
  const hasNotifications = notifications.length > 0;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          mode="icon"
          variant={triggerVariant}
          aria-label={t("Notifications")}
          className="relative"
        >
          {unreadCount ? (
            <BellDot className="size-4" />
          ) : (
            <Bell className="size-4" />
          )}
          {unreadCount ? (
            <span className="absolute -end-1 -top-1 flex min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[0.625rem] font-semibold leading-4 text-destructive-foreground">
              {unreadCount > 9 ? "9+" : unreadCount}
            </span>
          ) : null}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        align="end"
        className="w-[min(24rem,calc(100vw-1.5rem))] p-0"
      >
        <div className="flex items-center justify-between gap-3 px-3 py-3">
          <div className="min-w-0">
            <DropdownMenuLabel className="px-0 py-0 text-sm font-semibold text-foreground">
              {t("Notifications")}
            </DropdownMenuLabel>
            <p className="mt-0.5 text-xs text-muted-foreground">
              {unreadCount
                ? t("{count} unread", { count: unreadCount })
                : t("All caught up")}
            </p>
          </div>
          <div className="flex shrink-0 items-center gap-1">
            <Button
              mode="icon"
              variant="ghost"
              size="sm"
              disabled={!unreadCount}
              onClick={markAllNotificationsRead}
              aria-label={t("Mark all as read")}
            >
              <CheckCheck className="size-4" />
            </Button>
            <Button
              mode="icon"
              variant="ghost"
              size="sm"
              disabled={!hasNotifications}
              onClick={clearNotifications}
              aria-label={t("Clear notifications")}
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        </div>
        <DropdownMenuSeparator className="my-0" />
        {hasNotifications ? (
          <>
            <div className="max-h-[min(28rem,calc(100vh-8rem))] overflow-y-auto p-2">
              {notifications.map((item) => (
                <NotificationRow
                  key={item.id}
                  item={item}
                  onRead={markNotificationRead}
                />
              ))}
            </div>
            <DropdownMenuSeparator className="my-0" />
            <div className="px-3 py-2 text-[0.7rem] text-muted-foreground">
              {t("Showing the latest realtime events.")}
            </div>
          </>
        ) : (
          <div className="flex flex-col items-center gap-2 px-6 py-8 text-center">
            <span className="flex size-10 items-center justify-center rounded-md border border-dashed border-border text-muted-foreground">
              <Inbox className="size-5" />
            </span>
            <div className="space-y-1">
              <p className="text-sm font-medium text-foreground">
                {t("No notifications yet")}
              </p>
              <p className="text-xs leading-5 text-muted-foreground">
                {t(
                  "Realtime events received by this browser will appear here.",
                )}
              </p>
            </div>
          </div>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
