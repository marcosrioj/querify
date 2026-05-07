import { ReactNode, useState } from "react";
import { LoaderCircle, Search, SlidersHorizontal, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { translateText } from "@/shared/lib/i18n-core";

export function ListFilterToolbar({
  children,
  isLoading = false,
  className,
}: {
  children: ReactNode;
  isLoading?: boolean;
  className?: string;
}) {
  return (
    <div className={cn("relative grid w-full gap-3", className)}>
      {isLoading ? (
        <div className="pointer-events-none absolute inset-x-0 -top-1 h-1 overflow-hidden rounded-full">
          <Skeleton className="h-full w-full" />
        </div>
      ) : null}
      {children}
    </div>
  );
}

export function ListFilterSearch({
  value,
  onChange,
  placeholder,
  activeFilterCount,
  onClear,
  isLoading = false,
}: {
  value: string;
  onChange: (value: string) => void;
  placeholder: string;
  activeFilterCount: number;
  onClear: () => void;
  isLoading?: boolean;
}) {
  return (
    <div className="grid w-full gap-2 sm:grid-cols-[minmax(0,1fr)_auto]">
      <div className="relative min-w-0">
        {isLoading ? (
          <div className="pointer-events-none absolute inset-x-0 -top-1 h-1 overflow-hidden rounded-full">
            <Skeleton className="h-full w-full" />
          </div>
        ) : null}
        <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={translateText(placeholder)}
          className="w-full border-border/70 bg-background ps-9 shadow-none"
          variant="lg"
        />
      </div>
      <ListFilterClearButton
        activeFilterCount={activeFilterCount}
        onClear={onClear}
        className="sm:w-auto"
      />
    </div>
  );
}

export function ListFilterDisclosure({
  search,
  children,
  activeFilterCount,
  isLoading = false,
  defaultOpen = false,
  label = "Filters",
}: {
  search: ReactNode;
  children: ReactNode;
  activeFilterCount: number;
  isLoading?: boolean;
  defaultOpen?: boolean;
  label?: string;
}) {
  const [open, setOpen] = useState(defaultOpen);

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <div className="grid w-full min-w-0 gap-2 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-start">
        <div className="min-w-0">{search}</div>
        <CollapsibleTrigger asChild>
          <Button
            type="button"
            variant={open || activeFilterCount > 0 ? "secondary" : "outline"}
            size="lg"
            className="w-full justify-center sm:w-auto"
            aria-label={translateText(label)}
          >
            {isLoading ? (
              <LoaderCircle className="size-4 animate-spin" />
            ) : (
              <SlidersHorizontal className="size-4" />
            )}
            {translateText(label)}
            {activeFilterCount > 0 ? (
              <Badge variant="primary" appearance="outline">
                {activeFilterCount}
              </Badge>
            ) : null}
          </Button>
        </CollapsibleTrigger>
      </div>
      <CollapsibleContent>
        <div className="pt-3">{children}</div>
      </CollapsibleContent>
    </Collapsible>
  );
}

export function ListFilterClearButton({
  activeFilterCount,
  onClear,
  size = "lg",
  className,
}: {
  activeFilterCount: number;
  onClear: () => void;
  size?: "sm" | "md" | "lg";
  className?: string;
}) {
  if (activeFilterCount <= 0) {
    return null;
  }

  return (
    <Button
      type="button"
      variant="outline"
      size={size}
      onClick={onClear}
      className={cn("w-full", className)}
    >
      <X className="size-4" />
      {translateText("Clear filters")}
      <Badge variant="primary" appearance="outline">
        {activeFilterCount}
      </Badge>
    </Button>
  );
}

export function ListFilterSection({
  label,
  activeFilterCount,
  emptyLabel,
  action,
  children,
}: {
  label: string;
  activeFilterCount: number;
  emptyLabel: string;
  action?: ReactNode;
  children: ReactNode;
}) {
  const hasActiveFilters = activeFilterCount > 0;

  return (
    <div className="min-w-0 space-y-2">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          {translateText(label)}
        </p>
        <div className="flex flex-wrap items-center gap-2">
          <Badge
            variant={hasActiveFilters ? "primary" : "outline"}
            appearance={hasActiveFilters ? "outline" : "default"}
          >
            {hasActiveFilters
              ? translateText("{count} active", {
                  count: activeFilterCount,
                })
              : translateText(emptyLabel)}
          </Badge>
          {action}
        </div>
      </div>
      {children}
    </div>
  );
}

export function ListFilterChipRail({ children }: { children: ReactNode }) {
  return (
    <div className="flex min-w-0 gap-1.5 overflow-x-auto rounded-lg border border-border/70 bg-background/70 p-1 shadow-xs shadow-black/5 [scrollbar-width:none]">
      {children}
    </div>
  );
}

export function ListFilterChip({
  active,
  children,
  onClick,
}: {
  active: boolean;
  children: ReactNode;
  onClick: () => void;
}) {
  return (
    <Button
      type="button"
      variant={active ? "secondary" : "ghost"}
      size="sm"
      className={cn(
        "shrink-0 whitespace-nowrap border border-transparent",
        active && "border-border/70 shadow-xs shadow-black/5",
      )}
      onClick={onClick}
    >
      {children}
    </Button>
  );
}

export function ListFilterField({
  label,
  children,
  className,
}: {
  label: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("space-y-1.5", className)}>
      <p className="text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
        {translateText(label)}
      </p>
      {children}
    </div>
  );
}
