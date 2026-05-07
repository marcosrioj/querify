import { ReactNode } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { translateMaybeString } from "@/shared/lib/i18n-render";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { ContextHint } from "@/shared/ui/context-hint";

type ListResultSummaryItem = {
  key?: string;
  label: ReactNode;
  value: ReactNode;
  description?: ReactNode;
  tone?: "default" | "primary" | "success" | "info" | "warning";
};

const toneClassNames: Record<
  NonNullable<ListResultSummaryItem["tone"]>,
  string
> = {
  default: "border-border/70 bg-background/75 text-foreground",
  primary:
    "border-primary/20 bg-primary/[0.045] text-[var(--color-primary-accent,var(--color-blue-700))]",
  success:
    "border-[var(--color-success-alpha,var(--color-green-200))] bg-[var(--color-success-soft,var(--color-green-50))] text-[var(--color-success-accent,var(--color-green-700))]",
  info: "border-cyan-500/20 bg-cyan-500/[0.055] text-cyan-700 dark:text-cyan-300",
  warning:
    "border-[var(--color-warning-alpha,var(--color-yellow-200))] bg-[var(--color-warning-soft,var(--color-yellow-50))] text-[var(--color-warning-accent,var(--color-yellow-700))]",
};

export function ListResultSummary({
  items,
  isLoading = false,
  className,
}: {
  items: ListResultSummaryItem[];
  isLoading?: boolean;
  className?: string;
}) {
  const { t } = usePortalI18n();

  return (
    <div
      className={cn(
        "flex min-w-0 flex-wrap content-start items-center justify-start gap-1.5",
        className,
      )}
      aria-label={t("Result summary")}
    >
      {isLoading
        ? Array.from(
            { length: Math.min(Math.max(items.length, 2), 4) },
            (_, index) => (
              <div
                key={`list-result-summary-skeleton-${index}`}
                className="inline-flex h-8 min-w-24 max-w-full shrink-0 items-center gap-2 rounded-md border border-border/70 bg-background/75 px-2.5"
              >
                <Skeleton className="h-3 w-12" />
                <Skeleton className="h-4 w-8" />
              </div>
            ),
          )
        : items.map((item, index) => (
            <div
              key={
                item.key ??
                (typeof item.label === "string"
                  ? item.label
                  : `list-result-summary-${index}`)
              }
              className={cn(
                "inline-flex min-h-8 max-w-full shrink-0 flex-wrap items-center gap-x-1.5 gap-y-1 rounded-md border px-2.5 py-1 leading-none",
                toneClassNames[item.tone ?? "default"],
              )}
            >
              <div className="inline-flex min-h-4 min-w-0 max-w-full items-center gap-1 text-[0.6875rem] font-semibold uppercase tracking-[0.1em] opacity-75">
                <span className="block min-w-0 break-words leading-4 [overflow-wrap:anywhere]">
                  {translateMaybeString(item.label, t)}
                </span>
                {item.description ? (
                  <ContextHint
                    content={translateMaybeString(item.description, t)}
                    label={t("Metric details")}
                    className="size-3.5 shrink-0 -translate-y-px p-0 text-[inherit] md:size-3.5 [&_i]:block [&_i]:text-[0.75rem] [&_i]:leading-none"
                  />
                ) : null}
              </div>
              <div className="flex min-h-4 shrink-0 items-center text-sm font-semibold leading-4 text-mono">
                {translateMaybeString(item.value, t)}
              </div>
            </div>
          ))}
    </div>
  );
}
