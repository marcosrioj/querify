import {
  BarChart3,
  ArrowRight,
  FolderKanban,
  Tags,
  Waypoints,
  type LucideIcon,
} from "lucide-react";
import { Link } from "react-router-dom";
import { cn } from "@/lib/utils";
import { translateText } from "@/shared/lib/i18n-core";
import { Button, Card, CardContent, ContextHint } from "@/shared/ui";

export type QnaModuleKey =
  | "dashboard"
  | "spaces"
  | "tags"
  | "sources";

const qnaModuleItems: Array<{
  key: QnaModuleKey;
  label: string;
  description: string;
  to: string;
  icon: LucideIcon;
}> = [
  {
    key: "dashboard",
    label: "Dashboard",
    description: "Act on risks",
    to: "/app/dashboard",
    icon: BarChart3,
  },
  {
    key: "spaces",
    label: "Spaces",
    description: "Daily work",
    to: "/app/spaces",
    icon: FolderKanban,
  },
  {
    key: "tags",
    label: "Tags",
    description: "Taxonomy",
    to: "/app/tags",
    icon: Tags,
  },
  {
    key: "sources",
    label: "Sources",
    description: "Evidence",
    to: "/app/sources",
    icon: Waypoints,
  },
];

export function QnaModuleNav({
  activeKey,
  intent,
}: {
  activeKey: QnaModuleKey;
  intent?: string;
}) {
  return (
    <Card className="overflow-hidden border-primary/15 bg-linear-to-br from-background via-background to-emerald-500/[0.06]">
      <CardContent className="space-y-4 p-4 lg:p-5">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="min-w-0 space-y-2">
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-300">
                {translateText("QnA module")}
              </span>
              <ContextHint
                content={translateText(
                  "Dashboard shows risk, Spaces are the operating boundary, and Tags plus Sources are reusable libraries.",
                )}
                label={translateText("QnA module details")}
              />
            </div>
            <div className="flex min-w-0 flex-col gap-1">
              <h3 className="text-lg font-semibold text-mono">
                {translateText("Operate QnA through business context")}
              </h3>
              <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
                {translateText(
                  intent ??
                    "Start with the dashboard for risks, then work inside Spaces before attaching taxonomy or evidence.",
                )}
              </p>
            </div>
          </div>
          <Button
            asChild
            className="bg-emerald-600 text-white shadow-lg shadow-emerald-600/25 hover:bg-emerald-700"
          >
            <Link to="/app/spaces">
              {translateText("Start here")}
              <ArrowRight className="size-4" />
            </Link>
          </Button>
        </div>

        <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
          {qnaModuleItems.map((item) => {
            const Icon = item.icon;
            const active = item.key === activeKey;

            return (
              <Link
                key={item.key}
                to={item.to}
                className={cn(
                  "group flex min-h-16 min-w-0 items-center gap-3 rounded-lg border px-3 py-3 text-left transition-colors",
                  active
                    ? "border-emerald-500/25 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
                    : "border-border/70 bg-background/75 text-foreground hover:border-emerald-500/20 hover:bg-emerald-500/[0.04]",
                )}
              >
                <span
                  className={cn(
                    "flex size-9 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
                    active
                      ? "bg-emerald-600 text-white ring-emerald-500/20"
                      : "bg-muted text-muted-foreground ring-border/70 group-hover:text-emerald-600",
                  )}
                >
                  <Icon className="size-4" />
                </span>
                <span className="min-w-0">
                  <span className="block truncate text-sm font-semibold">
                    {translateText(item.label)}
                  </span>
                  <span className="block truncate text-xs text-muted-foreground">
                    {translateText(item.description)}
                  </span>
                </span>
              </Link>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
