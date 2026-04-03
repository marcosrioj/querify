import { SidebarMenuPrimary } from '@/domains/shell/sidebar-menu-primary';

export function SidebarMenu({
  onNavigate,
}: {
  onNavigate?: () => void;
}) {
  return (
    <div className="kt-scrollable-y-auto grow max-h-[calc(100vh-11.5rem)]">
      <SidebarMenuPrimary onNavigate={onNavigate} />
    </div>
  );
}
