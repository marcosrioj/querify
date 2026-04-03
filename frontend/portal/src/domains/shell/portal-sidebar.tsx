import { Sparkles } from 'lucide-react';
import { NavLink } from 'react-router-dom';
import { portalNavigation } from '@/shared/constants/navigation';
import { useTenant } from '@/platform/tenant/tenant-context';
import { TenantEditionBadge } from '@/shared/ui/status-badges';
import { toAbsoluteUrl } from '@/lib/helpers';
import { cn } from '@/lib/utils';

export function PortalSidebar({ onNavigate }: { onNavigate?: () => void } = {}) {
  const { currentTenant } = useTenant();

  return (
    <div className="flex w-full flex-col">
      <div className="border-b border-border px-6 py-5">
        <div className="flex items-center gap-3">
          <img
            src={toAbsoluteUrl('/media/app/mini-logo-primary.svg')}
            alt="BaseFAQ"
            className="size-9"
          />
          <div>
            <p className="text-sm font-semibold text-mono">BaseFAQ Portal</p>
            <p className="text-xs text-muted-foreground">
              Tenant-facing SaaS workspace
            </p>
          </div>
        </div>
      </div>

      <div className="border-b border-border px-6 py-5">
        <div className="rounded-2xl border border-border bg-muted/40 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">
                Workspace
              </p>
              <p className="mt-2 text-sm font-semibold text-mono">
                {currentTenant?.name ?? 'No active tenant'}
              </p>
              <p className="mt-1 text-xs text-muted-foreground">
                {currentTenant?.slug
                  ? `Slug: ${currentTenant.slug}`
                  : 'Select or create a tenant workspace to start using Portal features.'}
              </p>
            </div>
            {currentTenant ? (
              <TenantEditionBadge edition={currentTenant.edition} />
            ) : null}
          </div>
        </div>
      </div>

      <nav className="flex-1 px-3 py-4">
        <div className="space-y-1">
          {portalNavigation.map((item) => {
            const Icon = item.icon;

            return (
              <NavLink
                key={item.key}
                to={item.path}
                onClick={onNavigate}
                className={({ isActive }) =>
                  cn(
                    'group flex items-start gap-3 rounded-2xl px-3 py-3 transition-colors',
                    isActive
                      ? 'bg-primary text-primary-foreground shadow-sm'
                      : 'text-secondary-foreground hover:bg-muted hover:text-foreground',
                  )
                }
              >
                {({ isActive }) => (
                  <>
                    <span
                      className={cn(
                        'mt-0.5 rounded-xl border p-2',
                        isActive
                          ? 'border-white/20 bg-white/10'
                          : 'border-border bg-background/60',
                      )}
                    >
                      <Icon className="size-4" />
                    </span>
                    <span className="min-w-0">
                      <span className="block truncate text-sm font-medium">
                        {item.label}
                      </span>
                      <span
                        className={cn(
                          'mt-1 block text-xs leading-5',
                          isActive
                            ? 'text-primary-foreground/80'
                            : 'text-muted-foreground',
                        )}
                      >
                        {item.description}
                      </span>
                    </span>
                  </>
                )}
              </NavLink>
            );
          })}
        </div>
      </nav>

      <div className="border-t border-border px-6 py-5">
        <div className="rounded-2xl bg-mono p-4 text-mono-foreground">
          <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-[0.2em] text-mono-foreground/70">
            <Sparkles className="size-4" />
            Portal foundation
          </div>
          <p className="mt-3 text-sm leading-6 text-mono-foreground/80">
            This shell is wired only to the confirmed Portal APIs in the
            repository and keeps BackOffice concerns outside the app boundary.
          </p>
        </div>
      </div>
    </div>
  );
}
