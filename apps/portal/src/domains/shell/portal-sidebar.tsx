import { SidebarFooter } from "@/domains/shell/sidebar-footer";
import { SidebarHeader } from "@/domains/shell/sidebar-header";
import { SidebarMenu } from "@/domains/shell/sidebar-menu";

export function PortalSidebar({
  mobile = false,
  compact = false,
  onCompactChange,
  onNavigate,
}: {
  mobile?: boolean;
  compact?: boolean;
  onCompactChange?: (compact: boolean) => void;
  onNavigate?: () => void;
} = {}) {
  const isCompact = !mobile && compact;
  const content = (
    <div className="flex h-full min-h-0 flex-col">
      <SidebarHeader
        compact={isCompact}
        onCompactChange={onCompactChange}
        onNavigate={onNavigate}
      />
      <SidebarMenu compact={isCompact} onNavigate={onNavigate} />
      <SidebarFooter compact={isCompact} />
    </div>
  );

  if (mobile) {
    return content;
  }

  return (
    <aside
      data-compact={isCompact ? "true" : undefined}
      className="fixed inset-y-0 z-20 hidden w-[var(--sidebar-width)] shrink-0 flex-col overflow-hidden border-r border-border/70 bg-muted/55 backdrop-blur transition-[width] duration-200 xl:flex"
    >
      {content}
    </aside>
  );
}
