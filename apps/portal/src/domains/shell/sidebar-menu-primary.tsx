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
import { cn } from "@/lib/utils";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/shared/ui";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import {
  portalNavigation,
  portalNavigationGroups,
} from "@/shared/constants/navigation";

export function SidebarMenuPrimary({
  compact = false,
  onNavigate,
}: {
  compact?: boolean;
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

  const getNavigationTarget = (item: NavigationItem) =>
    item.children?.find((child) => matchPath(child.path))?.path ??
    item.children?.[0]?.path ??
    item.path;

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

  if (compact) {
    return (
      <nav
        className="flex flex-col items-center gap-4 px-2"
        aria-label={t("Primary navigation")}
      >
        {portalNavigationGroups.map((group) => (
          <div
            key={group.key}
            role="group"
            aria-label={t(group.label)}
            className="flex w-full flex-col items-center gap-1"
          >
            {group.items.map((item) => {
              const Icon = item.icon;
              const isSelected = activeRootItem?.key === item.key;
              const label = t(item.label);
              const description = t(item.description);

              return (
                <Tooltip key={item.key}>
                  <TooltipTrigger asChild>
                    <button
                      type="button"
                      className={cn(
                        "flex size-11 items-center justify-center rounded-lg border border-transparent text-accent-foreground outline-hidden transition-colors hover:border-border/60 hover:bg-background/70 hover:text-mono focus-visible:border-primary/30 focus-visible:bg-background focus-visible:text-mono",
                        isSelected &&
                          "border-primary/20 bg-primary/[0.08] text-mono",
                      )}
                      aria-label={label}
                      aria-current={isSelected ? "page" : undefined}
                      onClick={() => {
                        navigate(getNavigationTarget(item));
                        onNavigate?.();
                      }}
                    >
                      <Icon className="size-4 opacity-70" />
                    </button>
                  </TooltipTrigger>
                  <TooltipContent side="right" className="max-w-56 space-y-0.5">
                    <div className="text-xs font-semibold">{label}</div>
                    <div className="text-[0.68rem] text-muted-foreground">
                      {description}
                    </div>
                  </TooltipContent>
                </Tooltip>
              );
            })}
          </div>
        ))}
      </nav>
    );
  }

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
