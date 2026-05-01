import { SidebarMenuPrimary } from "@/domains/shell/sidebar-menu-primary";

export function SidebarMenu({
  compact = false,
  onNavigate,
}: {
  compact?: boolean;
  onNavigate?: () => void;
}) {
  return (
    <div className="kt-scrollable-y-auto min-h-0 grow">
      <SidebarMenuPrimary compact={compact} onNavigate={onNavigate} />
    </div>
  );
}
