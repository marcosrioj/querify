import { Fragment } from "react";
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

  return (
    <div className="flex min-w-0 items-start gap-2">
      {pageChrome.backTo ? (
        <Button
          asChild
          mode="icon"
          variant="outline"
          size="icon"
          className="mt-0.5"
        >
          <Link to={pageChrome.backTo} aria-label={t("Go back")}>
            <ArrowLeft className="size-4" />
          </Link>
        </Button>
      ) : null}

      <Breadcrumb
        className="min-w-0 flex-1 overflow-hidden"
        aria-label={t("breadcrumb")}
      >
        <BreadcrumbList className="flex-nowrap overflow-hidden">
          {linkedItems.map((item, index) => {
            const isImmediateParent = index === linkedItems.length - 1;
            const visibilityClassName = isImmediateParent
              ? undefined
              : "hidden sm:inline-flex";
            const label = t(item.label);

            return (
              <Fragment key={item.key}>
                <BreadcrumbItem
                  className={cn("min-w-0 shrink-0", visibilityClassName)}
                >
                  <BreadcrumbLink
                    asChild
                    className="block max-w-[7rem] truncate text-xs sm:max-w-[11rem] sm:text-sm"
                  >
                    <Link to={item.path} title={label}>
                      {label}
                    </Link>
                  </BreadcrumbLink>
                </BreadcrumbItem>
                <BreadcrumbSeparator
                  className={cn("shrink-0", visibilityClassName)}
                />
              </Fragment>
            );
          })}

          <BreadcrumbItem className="min-w-0 flex-1 basis-0">
            <div className="flex min-w-0 items-center gap-1.5">
              <h1
                className="min-w-0 truncate text-lg font-semibold leading-7 tracking-normal text-mono sm:text-xl"
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

function ToolbarActions() {
  return (
    <div className="flex w-full min-w-0 flex-wrap items-center justify-end gap-2 lg:gap-3 xl:w-auto xl:flex-nowrap">
      <PortalCommandDialog />
      <LanguageSelector />
      <NotificationsMenu />
      <UserMenu />
    </div>
  );
}

export function PortalToolbar() {
  return (
    <div className="sticky top-0 z-10 min-w-0 border-b border-border/60 bg-background/88 pb-4 pt-0 backdrop-blur supports-[backdrop-filter]:bg-background/78">
      <Container className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
        <ToolbarPageTrail />
        <ToolbarActions />
      </Container>
    </div>
  );
}
