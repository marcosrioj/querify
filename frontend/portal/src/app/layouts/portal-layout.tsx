import { Outlet, useMatches } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { AppRouteHandle } from '@/app/router/route-types';
import { MobileHeader } from '@/domains/shell/mobile-header';
import { PortalFooter } from '@/domains/shell/portal-footer';
import { PortalSidebar } from '@/domains/shell/portal-sidebar';
import { PortalToolbar } from '@/domains/shell/portal-toolbar';
import { useBodyClass } from '@/hooks/use-body-class';
import { useIsMobile } from '@/hooks/use-mobile';

function useRouteTitle() {
  return (
    useMatches()
      .map((match) => match.handle as AppRouteHandle | undefined)
      .filter((handle): handle is AppRouteHandle => Boolean(handle?.title))
      .at(-1)?.title ?? 'BaseFAQ Portal'
  );
}

export function PortalLayout() {
  const title = useRouteTitle();
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
        <title>{title} | BaseFAQ Portal</title>
      </Helmet>

      <div className="flex grow">
        {!isMobile && <PortalSidebar />}
        {isMobile && <MobileHeader />}

        <div className="flex grow flex-col pt-[var(--header-height)] lg:flex-row lg:pt-0">
          <div className="m-[15px] mt-0 flex grow flex-col items-stretch rounded-xl border border-input bg-background lg:ms-[var(--sidebar-width)] lg:mt-[15px]">
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
