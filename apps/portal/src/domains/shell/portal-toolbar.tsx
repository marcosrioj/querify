import { Link, useMatches } from "react-router-dom";
import { AppRouteHandle } from "@/app/router/route-types";
import { NotificationsMenu } from "@/domains/shell/notifications-menu";
import { PortalCommandDialog } from "@/domains/shell/portal-command-dialog";
import { UserMenu } from "@/domains/shell/user-menu";
import { portalNavigation } from "@/shared/constants/navigation";
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
  const handles = useRouteHandles();
  const current = handles.at(-1);

  if (!current?.navKey) {
    return null;
  }

  const navItem = portalNavigation.find((item) => item.key === current.navKey);
  const currentLabel = current.breadcrumb ?? current.title;

  if (!navItem || navItem.label === currentLabel) {
    return null;
  }

  return (
    <div className="flex items-center gap-2 text-sm">
      <Link
        to={navItem.path}
        className="text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        {navItem.label}
      </Link>
      <span className="text-muted-foreground/60">/</span>
      <span className="text-sm text-mono">{currentLabel}</span>
    </div>
  );
}

function ToolbarHeading() {
  const handles = useRouteHandles();
  const current = handles.at(-1);
  const title = current?.title ?? "BaseFAQ Portal";

  return (
    <div className="flex flex-col flex-wrap gap-1 md:flex-row md:items-center lg:gap-5">
      <h1 className="inline-flex w-fit rounded-full border border-primary/15 bg-primary/8 px-3 py-1 text-sm font-medium uppercase tracking-[0.2em] text-primary">
        {title}
      </h1>
      <ToolbarBreadcrumbs />
    </div>
  );
}

function ToolbarActions() {
  return (
    <div className="flex items-center gap-1.5 lg:gap-3.5">
      <PortalCommandDialog />
      <NotificationsMenu />
      <UserMenu />
    </div>
  );
}

export function PortalToolbar() {
  return (
    <div className="pb-5">
      <Container className="flex flex-wrap items-center justify-between gap-3">
        <ToolbarHeading />
        <ToolbarActions />
      </Container>
    </div>
  );
}
