import { Fragment, useEffect, useRef, useState } from "react";
import { ArrowLeft } from "lucide-react";
import { Link, useMatches } from "react-router-dom";
import { AppRouteHandle } from "@/app/router/route-types";
import { LanguageSelector } from "@/domains/shell/language-selector";
import { NotificationsMenu } from "@/domains/shell/notifications-menu";
import { PortalCommandDialog } from "@/domains/shell/portal-command-dialog";
import { UserMenu } from "@/domains/shell/user-menu";
import {
  getPageChromeText,
  usePageChrome,
} from "@/shared/layout/page-chrome-context";
import { translateMaybeString } from "@/shared/lib/i18n-render";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { findPortalNavigationPath } from "@/shared/constants/navigation";
import { Container } from "@/shared/layout/container";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbSeparator,
  Button,
  ContextHint,
} from "@/shared/ui";
import { cn } from "@/lib/utils";

type RoutedHandle = AppRouteHandle;

function useRouteHandles() {
  return useMatches().reduce<RoutedHandle[]>((acc, match) => {
    const handle = match.handle as AppRouteHandle | undefined;

    if (handle?.title) {
      acc.push(handle);
    }

    return acc;
  }, []);
}

function ToolbarPageTrail() {
  const { t } = usePortalI18n();
  const pageChrome = usePageChrome();
  const handles = useRouteHandles();
  const current = handles.at(-1);
  const currentTitle =
    pageChrome.title ?? current?.breadcrumb ?? current?.title;
  const currentTitleText = getPageChromeText(currentTitle);
  const currentLabel = currentTitleText
    ? t(currentTitleText)
    : translateMaybeString(currentTitle ?? "BaseFAQ QnA Portal", t);

  const navPath = current?.navKey
    ? findPortalNavigationPath(current.navKey)
    : [];
  const lastNavItem = navPath.at(-1);
  const currentMatchesLastNavItem =
    lastNavItem !== undefined &&
    typeof currentLabel === "string" &&
    t(lastNavItem.label) === currentLabel;

  const linkedItems = currentMatchesLastNavItem
    ? navPath.slice(0, -1)
    : navPath;
  const chromeBreadcrumbs = pageChrome.breadcrumbs;
  const breadcrumbItems =
    chromeBreadcrumbs !== undefined
      ? chromeBreadcrumbs
      : linkedItems.map((item) => ({ label: item.label, to: item.path }));

  return (
    <div className="flex w-full min-w-0 items-center gap-2 overflow-hidden xl:flex-1">
      {pageChrome.backTo ? (
        <Button asChild mode="icon" variant="outline" size="icon">
          <Link to={pageChrome.backTo} aria-label={t("Go back")}>
            <ArrowLeft className="size-4" />
          </Link>
        </Button>
      ) : null}

      <Breadcrumb
        className="min-w-0 flex-1 overflow-hidden"
        aria-label={t("breadcrumb")}
      >
        <BreadcrumbList className="w-full min-w-0 flex-nowrap overflow-hidden">
          {breadcrumbItems.map((item, index) => {
            const isImmediateParent = index === breadcrumbItems.length - 1;
            const visibilityClassName = isImmediateParent
              ? undefined
              : "hidden sm:inline-flex";
            const label = translateMaybeString(item.label, t);
            const title = typeof label === "string" ? label : undefined;

            return (
              <Fragment key={`${index}-${title ?? "breadcrumb"}`}>
                <BreadcrumbItem
                  className={cn("min-w-0 shrink-0", visibilityClassName)}
                >
                  {item.to ? (
                    <BreadcrumbLink
                      asChild
                      className="block max-w-[7rem] truncate text-xs sm:max-w-[11rem] sm:text-sm"
                    >
                      <Link to={item.to} title={title}>
                        {label}
                      </Link>
                    </BreadcrumbLink>
                  ) : (
                    <span
                      className="block max-w-[7rem] truncate text-xs text-muted-foreground sm:max-w-[11rem] sm:text-sm"
                      title={title}
                    >
                      {label}
                    </span>
                  )}
                </BreadcrumbItem>
                <BreadcrumbSeparator
                  className={cn("shrink-0", visibilityClassName)}
                />
              </Fragment>
            );
          })}

          <BreadcrumbItem className="min-w-0 flex-1 basis-0 overflow-hidden">
            <div className="flex min-w-0 flex-1 items-center gap-1.5 overflow-hidden">
              <h1
                className="min-w-0 flex-1 truncate text-lg font-semibold leading-7 tracking-normal text-mono sm:text-xl"
                title={
                  typeof currentLabel === "string" ? currentLabel : undefined
                }
              >
                {currentLabel}
              </h1>
              {pageChrome.description &&
              pageChrome.descriptionMode === "hint" ? (
                <ContextHint
                  content={translateMaybeString(pageChrome.description, t)}
                  label={t("Page details")}
                  className="shrink-0"
                />
              ) : null}
            </div>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>
    </div>
  );
}

function ToolbarActions({ compact }: { compact: boolean }) {
  return (
    <div
      aria-hidden={compact}
      inert={compact ? true : undefined}
      className={cn(
        "flex w-full min-w-0 flex-wrap items-center justify-end gap-2 overflow-hidden transition-[max-height,opacity,transform,margin] duration-300 ease-out lg:gap-3 xl:w-auto xl:flex-nowrap xl:shrink-0",
        compact
          ? "max-h-0 -translate-y-2 opacity-0 pointer-events-none"
          : "max-h-24 translate-y-0 opacity-100",
      )}
    >
      <PortalCommandDialog />
      <LanguageSelector />
      <NotificationsMenu />
      <UserMenu />
    </div>
  );
}

export function PortalToolbar() {
  const toolbarRef = useRef<HTMLDivElement>(null);
  const [isScrolled, setIsScrolled] = useState(false);
  const [hasMobileHeader, setHasMobileHeader] = useState(false);

  useEffect(() => {
    const toolbar = toolbarRef.current;
    const scrollArea = toolbar?.closest<HTMLElement>("[data-portal-scroll-area]");
    const mobileHeaderViewport = window.matchMedia("(max-width: 1279px)");

    if (!scrollArea) {
      return;
    }

    const updateToolbarState = () => {
      setIsScrolled(scrollArea.scrollTop > 16);
      setHasMobileHeader(mobileHeaderViewport.matches);
    };

    updateToolbarState();
    scrollArea.addEventListener("scroll", updateToolbarState, { passive: true });
    mobileHeaderViewport.addEventListener("change", updateToolbarState);

    return () => {
      scrollArea.removeEventListener("scroll", updateToolbarState);
      mobileHeaderViewport.removeEventListener("change", updateToolbarState);
    };
  }, []);

  const hideActions = isScrolled && hasMobileHeader;

  return (
    <div
      ref={toolbarRef}
      className={cn(
        "sticky top-0 z-10 min-w-0 border-b border-border/60 bg-background/88 backdrop-blur transition-[padding,box-shadow,background-color] duration-300 ease-out supports-[backdrop-filter]:bg-background/78",
        isScrolled
          ? "pb-2 pt-2 shadow-sm shadow-black/5"
          : "pb-4 pt-5 shadow-none",
      )}
    >
      <Container
        className={cn(
          "flex min-w-0 flex-col transition-[gap] duration-300 ease-out xl:flex-row xl:items-center xl:justify-between",
          hideActions ? "gap-0" : "gap-3",
        )}
      >
        <ToolbarPageTrail />
        <ToolbarActions compact={hideActions} />
      </Container>
    </div>
  );
}
