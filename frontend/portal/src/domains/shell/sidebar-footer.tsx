import { NotificationsMenu } from '@/domains/shell/notifications-menu';
import { PortalCommandDialog } from '@/domains/shell/portal-command-dialog';
import { UserMenu } from '@/domains/shell/user-menu';

export function SidebarFooter() {
  return (
    <div className="flex h-14 shrink-0 items-center justify-between ps-4 pe-3.5">
      <UserMenu variant="compact" />

      <div className="flex items-center gap-1.5">
        <NotificationsMenu triggerVariant="ghost" />
        <PortalCommandDialog variant="icon" />
      </div>
    </div>
  );
}
