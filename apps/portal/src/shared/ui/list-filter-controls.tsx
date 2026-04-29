import { ReactNode } from "react";
import { X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
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
    <div className={cn("relative grid w-full gap-4", className)}>
      {isLoading ? (
        <div className="pointer-events-none absolute inset-x-0 -top-2 h-1 overflow-hidden rounded-full">
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
        <Input
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={translateText(placeholder)}
          className="w-full"
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
        <p className="text-xs font-semibold text-muted-foreground">
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
    <div className="flex min-w-0 gap-1.5 overflow-x-auto rounded-md border border-border/70 bg-muted/30 p-1 [scrollbar-width:none]">
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
      className="shrink-0 whitespace-nowrap"
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
      <p className="text-xs font-medium text-muted-foreground">
        {translateText(label)}
      </p>
      {children}
    </div>
  );
}
