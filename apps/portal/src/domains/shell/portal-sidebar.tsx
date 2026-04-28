import { SidebarFooter } from '@/domains/shell/sidebar-footer';
import { SidebarHeader } from '@/domains/shell/sidebar-header';
import { SidebarMenu } from '@/domains/shell/sidebar-menu';

export function PortalSidebar({
  mobile = false,
  onNavigate,
}: {
  mobile?: boolean;
  onNavigate?: () => void;
} = {}) {
  const content = (
    <div className="flex h-full flex-col">
      <SidebarHeader onNavigate={onNavigate} />
      <SidebarMenu onNavigate={onNavigate} />
      <SidebarFooter />
    </div>
  );

  if (mobile) {
    return content;
  }

  return (
    <aside className="fixed inset-y-0 z-20 hidden w-[var(--sidebar-width)] shrink-0 flex-col border-r border-border/70 bg-muted/55 backdrop-blur lg:flex">
      {content}
    </aside>
  );
}
