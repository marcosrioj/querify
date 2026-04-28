import { Fragment } from "react";
import { Link, useMatches } from "react-router-dom";
import { AppRouteHandle } from "@/app/router/route-types";
import { LanguageSelector } from "@/domains/shell/language-selector";
import { NotificationsMenu } from "@/domains/shell/notifications-menu";
import { PortalCommandDialog } from "@/domains/shell/portal-command-dialog";
import { UserMenu } from "@/domains/shell/user-menu";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { findPortalNavigationPath } from "@/shared/constants/navigation";
import { Container } from "@/shared/layout/container";

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

function ToolbarBreadcrumbs() {
  const { t } = usePortalI18n();
  const handles = useRouteHandles();
  const current = handles.at(-1);

  if (!current?.navKey) {
    return null;
  }

  const navPath = findPortalNavigationPath(current.navKey);
  const currentLabel = t(current.breadcrumb ?? current.title);
  const lastNavItem = navPath.at(-1);
  const currentMatchesLastNavItem =
    lastNavItem !== undefined && t(lastNavItem.label) === currentLabel;

  if (
    navPath.length === 0 ||
    (navPath.length === 1 && currentMatchesLastNavItem)
  ) {
    return null;
  }

  const linkedItems = currentMatchesLastNavItem
    ? navPath.slice(0, -1)
    : navPath;

  return (
    <div className="flex items-center gap-2 text-sm">
      {linkedItems.map((item) => (
        <Fragment key={item.key}>
          <Link
            to={item.path}
            className="text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            {t(item.label)}
          </Link>
          <span className="text-muted-foreground/60">/</span>
        </Fragment>
      ))}
      <span className="text-sm text-mono">{currentLabel}</span>
    </div>
  );
}

function ToolbarHeading() {
  const { t } = usePortalI18n();
  const handles = useRouteHandles();
  const current = handles.at(-1);
  const title = t(current?.title ?? "BaseFAQ QnA Portal");

  return (
    <div className="flex min-w-0 flex-col gap-1.5">
      <h1 className="truncate text-xl font-semibold tracking-normal text-mono">
        {title}
      </h1>
      <ToolbarBreadcrumbs />
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
        <ToolbarHeading />
        <ToolbarActions />
      </Container>
    </div>
  );
}
