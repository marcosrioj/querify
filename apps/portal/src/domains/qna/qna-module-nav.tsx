import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { translateText } from "@/shared/lib/i18n-core";
import { Badge, Card, CardContent, ContextHint } from "@/shared/ui";

export type QnaRelationshipItem = {
  key: string;
  label: string;
  description: string;
  icon: LucideIcon;
  count?: number | string;
  disabled?: boolean;
};

export function QnaModuleNav({
  eyebrow = "Relationships",
  items,
  activeKey,
  onActiveKeyChange,
}: {
  eyebrow?: string;
  items: QnaRelationshipItem[];
  activeKey: string;
  onActiveKeyChange: (key: string) => void;
}) {
  return (
    <Card className="overflow-hidden border-primary/15 bg-linear-to-br from-background via-background to-emerald-500/[0.045]">
      <CardContent className="space-y-3 p-3 lg:p-4">
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-xs font-semibold uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-300">
            {translateText(eyebrow)}
          </span>
          <ContextHint
            content={translateText(
              "This panel is scoped to the current record. Choose a relationship area to manage its related list without leaving this screen.",
            )}
            label={translateText("Relationship panel details")}
          />
        </div>

        <div
          className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4"
          role="tablist"
          aria-label={translateText(eyebrow)}
        >
          {items.map((item) => {
            const Icon = item.icon;
            const isActive = activeKey === item.key;
            const content = (
              <>
                <span
                  className={cn(
                    "flex size-9 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
                    isActive
                      ? "bg-primary text-primary-foreground ring-primary shadow-sm"
                      : item.disabled
                        ? "bg-muted text-muted-foreground/60 ring-border/70"
                        : "bg-primary/10 text-primary ring-primary/15 group-hover:bg-primary group-hover:text-primary-foreground",
                  )}
                >
                  <Icon className="size-4" />
                </span>
                <span className="min-w-0 flex-1">
                  <span className="flex min-w-0 items-center gap-2">
                    <span className="truncate text-sm font-semibold">
                      {translateText(item.label)}
                    </span>
                    {item.count !== undefined ? (
                      <Badge variant="outline" appearance="outline" size="sm">
                        {item.count}
                      </Badge>
                    ) : null}
                  </span>
                  <span className="mt-1 block line-clamp-2 text-xs leading-5 text-muted-foreground">
                    {translateText(item.description)}
                  </span>
                </span>
              </>
            );
            const className = cn(
              "group flex min-h-20 min-w-0 items-start gap-3 rounded-xl border px-3 py-3 text-left transition-colors",
              isActive
                ? "border-primary/30 bg-primary/[0.065] text-foreground shadow-sm"
                : item.disabled
                  ? "border-border/60 bg-muted/20 text-muted-foreground"
                  : "border-border/70 bg-background/75 text-foreground hover:border-primary/25 hover:bg-primary/[0.035]",
            );

            if (!item.disabled) {
              return (
                <button
                  key={item.key}
                  type="button"
                  className={className}
                  role="tab"
                  aria-selected={isActive}
                  onClick={() => onActiveKeyChange?.(item.key)}
                >
                  {content}
                </button>
              );
            }

            return (
              <div
                key={item.key}
                className={className}
                role="tab"
                aria-disabled="true"
                aria-selected="false"
              >
                {content}
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
