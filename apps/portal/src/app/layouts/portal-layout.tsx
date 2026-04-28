import { useIsFetching } from "@tanstack/react-query";
import { Outlet, useMatches, useNavigation } from "react-router-dom";
import { Helmet } from "react-helmet-async";
import { AppRouteHandle } from "@/app/router/route-types";
import { MobileHeader } from "@/domains/shell/mobile-header";
import { PortalFooter } from "@/domains/shell/portal-footer";
import { PortalSidebar } from "@/domains/shell/portal-sidebar";
import { PortalToolbar } from "@/domains/shell/portal-toolbar";
import { useBodyClass } from "@/hooks/use-body-class";
import { useIsMobile } from "@/hooks/use-mobile";
import { cn } from "@/lib/utils";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";

function useRouteTitle() {
  return (
    useMatches()
      .map((match) => match.handle as AppRouteHandle | undefined)
      .filter((handle): handle is AppRouteHandle => Boolean(handle?.title))
      .at(-1)?.title ?? "BaseFAQ QnA Portal"
  );
}

export function PortalLayout() {
  const { t } = usePortalI18n();
  const title = t(useRouteTitle());
  const isMobile = useIsMobile();

  useBodyClass(`
    [--header-height:var(--portal-header-height)]
    [--sidebar-width:var(--portal-sidebar-width)]
    xl:overflow-hidden
    portal-body-background
  `);

  return (
    <>
      <Helmet>
        <title>
          {title} | {t("BaseFAQ QnA Portal")}
        </title>
      </Helmet>

      <div className="flex min-h-screen min-w-0 grow">
        {!isMobile && <PortalSidebar />}
        {isMobile && <MobileHeader />}

        <div className="flex min-w-0 grow flex-col pt-[var(--header-height)] xl:ms-[var(--sidebar-width)] xl:w-[calc(100vw-var(--sidebar-width))] xl:max-w-[calc(100vw-var(--sidebar-width))] xl:grow-0 xl:pt-0">
          <div className="portal-elevated relative m-2 mt-0 flex min-w-0 grow flex-col items-stretch overflow-hidden rounded-2xl border border-border/70 bg-background/95 backdrop-blur sm:m-3 sm:mt-0 xl:mt-3">
            <PortalActivityBar />
            <div className="kt-scrollable-y-auto flex min-w-0 grow flex-col pt-5 [--kt-scrollbar-width:auto]">
              <main className="min-w-0 grow" role="main">
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
  const isBusy = navigation.state !== "idle" || isFetching > 0;

  return (
    <div
      aria-hidden="true"
      className={cn(
        "pointer-events-none absolute inset-x-0 top-0 z-30 h-1 overflow-hidden rounded-t-lg transition-opacity duration-200",
        isBusy ? "opacity-100" : "opacity-0",
      )}
    >
      <div className="h-full w-full animate-pulse bg-linear-to-r from-emerald-500 via-sky-500 to-cyan-500" />
    </div>
  );
}
