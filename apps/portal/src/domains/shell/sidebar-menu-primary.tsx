import { useCallback } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  AccordionMenu,
  AccordionMenuClassNames,
  AccordionMenuGroup,
  AccordionMenuItem,
  AccordionMenuLabel,
  AccordionMenuSub,
  AccordionMenuSubContent,
  AccordionMenuSubTrigger,
} from "@/components/ui/accordion-menu";
import type { NavigationItem } from "@/shared/constants/navigation";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import {
  portalNavigation,
  portalNavigationGroups,
} from "@/shared/constants/navigation";

export function SidebarMenuPrimary({
  onNavigate,
}: {
  onNavigate?: () => void;
}) {
  const { t } = usePortalI18n();
  const { pathname } = useLocation();
  const navigate = useNavigate();

  const matchPath = useCallback(
    (path: string): boolean =>
      path === pathname || (path.length > 1 && pathname.startsWith(path)),
    [pathname],
  );
  const activeRootItem = portalNavigation.find((item) =>
    item.children
      ? item.children.some((child) => matchPath(child.path)) ||
        item.activePaths?.some(matchPath)
      : item.activePaths?.some(matchPath)
        ? true
        : matchPath(item.path),
  );
  const activeRootValue = activeRootItem?.children
    ? `${activeRootItem.key}:${pathname}`
    : (activeRootItem?.key ?? pathname);
  const selectedNavigationValue = activeRootItem?.children
    ? (activeRootItem.children.find((child) => matchPath(child.path))?.path ??
      (activeRootItem.key === "qna" ? "/app/spaces" : pathname))
    : (activeRootItem?.path ?? pathname);

  const classNames: AccordionMenuClassNames = {
    root: "space-y-5 px-3.5",
    group: "gap-px",
    label:
      "pt-1 pb-1 text-[0.68rem] font-semibold uppercase tracking-[0.18em] text-muted-foreground/75",
    item: "h-9.5 rounded-lg border border-transparent bg-transparent text-accent-foreground hover:border-border/60 hover:bg-background/70 hover:text-mono data-[selected=true]:border-primary/20 data-[selected=true]:bg-primary/[0.08] data-[selected=true]:font-semibold data-[selected=true]:text-mono",
    subTrigger:
      "h-9.5 rounded-lg border border-transparent bg-transparent text-accent-foreground hover:border-border/60 hover:bg-background/70 hover:text-mono data-[state=open]:border-primary/20 data-[state=open]:bg-primary/[0.08] data-[state=open]:font-semibold data-[state=open]:text-mono",
  };

  const renderNavigationItem = (item: NavigationItem) => {
    const Icon = item.icon;

    if (item.children?.length) {
      return (
        <AccordionMenuSub key={item.key} value={item.key}>
          <AccordionMenuSubTrigger className="text-sm font-medium">
            <Icon data-slot="accordion-menu-icon" />
            <span data-slot="accordion-menu-title">{t(item.label)}</span>
          </AccordionMenuSubTrigger>
          <AccordionMenuSubContent
            type="single"
            collapsible
            parentValue={item.key}
          >
            {item.children.map(renderNavigationItem)}
          </AccordionMenuSubContent>
        </AccordionMenuSub>
      );
    }

    return (
      <AccordionMenuItem
        key={item.key}
        value={item.path}
        className="text-sm font-medium"
      >
        <Icon data-slot="accordion-menu-icon" />
        <span data-slot="accordion-menu-title">{t(item.label)}</span>
      </AccordionMenuItem>
    );
  };

  return (
    <AccordionMenu
      key={activeRootValue}
      type="single"
      selectedValue={selectedNavigationValue}
      matchPath={matchPath}
      collapsible
      classNames={classNames}
      onItemClick={(value) => {
        navigate(value);
        onNavigate?.();
      }}
    >
      {portalNavigationGroups.map((group) => (
        <AccordionMenuGroup key={group.key}>
          <AccordionMenuLabel>{t(group.label)}</AccordionMenuLabel>
          {group.items.map(renderNavigationItem)}
        </AccordionMenuGroup>
      ))}
    </AccordionMenu>
  );
}
