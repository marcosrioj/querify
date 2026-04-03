import { Link } from 'react-router-dom';
import { toAbsoluteUrl } from '@/lib/helpers';
import { useTenant } from '@/platform/tenant/tenant-context';
import { TenantEditionBadge } from '@/shared/ui/status-badges';
import { PortalCommandDialog } from '@/domains/shell/portal-command-dialog';

export function SidebarHeader({
  onNavigate,
}: {
  onNavigate?: () => void;
}) {
  const { currentTenant } = useTenant();

  return (
    <div className="mb-3.5">
      <div className="flex h-[70px] items-center justify-between gap-2.5 px-3.5">
        <Link to="/app/dashboard" onClick={onNavigate}>
          <img
            src={toAbsoluteUrl('/media/app/default-logo.svg')}
            className="default-logo h-[28px] dark:hidden"
            alt="BaseFAQ Portal"
          />
          <img
            src={toAbsoluteUrl('/media/app/default-logo-dark.svg')}
            className="default-logo hidden h-[28px] dark:inline-block"
            alt="BaseFAQ Portal"
          />
          <img
            src={toAbsoluteUrl('/media/app/mini-logo-circle-primary.svg')}
            className="small-logo hidden h-[32px] dark:hidden"
            alt="BaseFAQ Portal"
          />
          <img
            src={toAbsoluteUrl('/media/app/mini-logo-circle-primary-dark.svg')}
            className="small-logo hidden h-[32px] dark:inline-block"
            alt="BaseFAQ Portal"
          />
        </Link>

        {currentTenant ? (
          <TenantEditionBadge edition={currentTenant.edition} />
        ) : null}
      </div>

      <div className="px-3.5 pt-2.5">
        <div className="rounded-xl border border-border bg-background px-3.5 py-3">
          <div className="text-sm font-medium text-mono">
            {currentTenant?.name ?? 'No active workspace'}
          </div>
          <div className="mt-1 text-xs text-muted-foreground">
            {currentTenant?.slug
              ? `Tenant slug: ${currentTenant.slug}`
              : 'Select a tenant workspace to activate Portal features.'}
          </div>
        </div>
      </div>

      <div className="mb-1 px-3.5 pt-2.5">
        <PortalCommandDialog variant="sidebar" />
      </div>
    </div>
  );
}
