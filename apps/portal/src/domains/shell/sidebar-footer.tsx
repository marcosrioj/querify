import { LanguageSelector } from "@/domains/shell/language-selector";
import { NotificationsMenu } from "@/domains/shell/notifications-menu";
import { PortalCommandDialog } from "@/domains/shell/portal-command-dialog";
import { UserMenu } from "@/domains/shell/user-menu";

export function SidebarFooter({ compact = false }: { compact?: boolean }) {
  if (compact) {
    return (
      <div className="flex shrink-0 flex-col items-center gap-2 px-2 py-3">
        <UserMenu variant="compact" />
        <div className="h-px w-8 bg-border/70" />
        <LanguageSelector iconOnly variant="compact" />
        <NotificationsMenu triggerVariant="ghost" />
        <PortalCommandDialog variant="icon" />
      </div>
    );
  }

  return (
    <div className="flex h-14 shrink-0 items-center justify-between ps-4 pe-3.5">
      <UserMenu variant="compact" />

      <div className="flex items-center gap-1.5">
        <LanguageSelector variant="compact" />
        <NotificationsMenu triggerVariant="ghost" />
        <PortalCommandDialog variant="icon" />
      </div>
    </div>
  );
}
