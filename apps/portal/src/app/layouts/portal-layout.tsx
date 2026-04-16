import { useIsFetching } from '@tanstack/react-query';
import { Outlet, useMatches, useNavigation } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { AppRouteHandle } from '@/app/router/route-types';
import { MobileHeader } from '@/domains/shell/mobile-header';
import { PortalFooter } from '@/domains/shell/portal-footer';
import { PortalSidebar } from '@/domains/shell/portal-sidebar';
import { PortalToolbar } from '@/domains/shell/portal-toolbar';
import { useBodyClass } from '@/hooks/use-body-class';
import { useIsMobile } from '@/hooks/use-mobile';
import { cn } from '@/lib/utils';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';

function useRouteTitle() {
  return (
    useMatches()
      .map((match) => match.handle as AppRouteHandle | undefined)
      .filter((handle): handle is AppRouteHandle => Boolean(handle?.title))
      .at(-1)?.title ?? 'BaseFAQ QnA Portal'
  );
}

export function PortalLayout() {
  const { t } = usePortalI18n();
  const title = t(useRouteTitle());
  const isMobile = useIsMobile();

  useBodyClass(`
    [--header-height:60px]
    [--sidebar-width:270px]
    lg:overflow-hidden
    bg-muted!
  `);

  return (
    <>
      <Helmet>
        <title>{title} | {t('BaseFAQ QnA Portal')}</title>
      </Helmet>

      <div className="flex grow">
        {!isMobile && <PortalSidebar />}
        {isMobile && <MobileHeader />}

        <div className="flex grow flex-col pt-[var(--header-height)] lg:flex-row lg:pt-0">
          <div className="relative m-[15px] mt-0 flex grow flex-col items-stretch rounded-xl border border-input bg-background lg:ms-[var(--sidebar-width)] lg:mt-[15px]">
            <PortalActivityBar />
            <div className="kt-scrollable-y-auto flex grow flex-col pt-5 [--kt-scrollbar-width:auto]">
              <main className="grow" role="content">
                <PortalToolbar />
                <Outlet />
              </main>

              <PortalFooter />
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

function PortalActivityBar() {
  const navigation = useNavigation();
  const isFetching = useIsFetching();
  const isBusy = navigation.state !== 'idle' || isFetching > 0;

  return (
    <div
      aria-hidden="true"
      className={cn(
        'pointer-events-none absolute inset-x-0 top-0 z-30 h-1 overflow-hidden rounded-t-xl transition-opacity duration-200',
        isBusy ? 'opacity-100' : 'opacity-0',
      )}
    >
      <div className="h-full w-full animate-pulse bg-linear-to-r from-emerald-500 via-sky-500 to-cyan-500" />
    </div>
  );
}
