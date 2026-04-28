import {
  Activity,
  BarChart3,
  CheckCircle2,
  MessageSquareText,
  MessagesSquare,
  PanelsTopLeft,
  Tags,
  Waypoints,
} from "lucide-react";
import { Link } from "react-router-dom";
import { cn } from "@/lib/utils";
import { translateText } from "@/shared/lib/i18n-core";
import { Button, Card, CardContent, ContextHint } from "@/shared/ui";

export type QnaModuleKey =
  | "dashboard"
  | "spaces"
  | "questions"
  | "answers"
  | "sources"
  | "tags"
  | "activity";

const qnaModuleItems: Array<{
  key: QnaModuleKey;
  label: string;
  description: string;
  to: string;
  icon: typeof PanelsTopLeft;
}> = [
  {
    key: "spaces",
    label: "Spaces",
    description: "Start here",
    to: "/app/spaces",
    icon: PanelsTopLeft,
  },
  {
    key: "questions",
    label: "Questions",
    description: "Threads",
    to: "/app/questions",
    icon: MessagesSquare,
  },
  {
    key: "answers",
    label: "Answers",
    description: "Resolutions",
    to: "/app/answers",
    icon: MessageSquareText,
  },
  {
    key: "sources",
    label: "Sources",
    description: "Evidence",
    to: "/app/sources",
    icon: Waypoints,
  },
  {
    key: "tags",
    label: "Tags",
    description: "Taxonomy",
    to: "/app/tags",
    icon: Tags,
  },
  {
    key: "activity",
    label: "Activity",
    description: "Signals",
    to: "/app/activity",
    icon: Activity,
  },
  {
    key: "dashboard",
    label: "Dashboard",
    description: "Macro view",
    to: "/app/dashboard",
    icon: BarChart3,
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
    <Card className="overflow-hidden border-primary/20 bg-linear-to-br from-primary/[0.08] via-background to-cyan-500/[0.05]">
      <CardContent className="space-y-4 p-4 lg:p-5">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="min-w-0 space-y-2">
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-primary">
                {translateText("QnA module")}
              </span>
              <ContextHint
                content={translateText(
                  "Spaces are the parent boundary. Questions, answers, sources, tags, and activity should always be read through that operating context.",
                )}
                label={translateText("QnA module details")}
              />
            </div>
            <div className="flex min-w-0 flex-col gap-1">
              <h3 className="text-lg font-semibold text-mono">
                {translateText("Start with Spaces")}
              </h3>
              <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
                {translateText(
                  intent ??
                    "Use the module map to keep every child record tied to its parent space and operational outcome.",
                )}
              </p>
            </div>
          </div>
          <Button asChild className="shadow-lg shadow-primary/20">
            <Link to="/app/spaces">
              <CheckCircle2 className="size-4" />
              {translateText("Start here")}
            </Link>
          </Button>
        </div>

        <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-4 xl:grid-cols-7">
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
                    ? "border-primary/25 bg-primary/10 text-primary"
                    : "border-border/70 bg-background/75 text-foreground hover:border-primary/20 hover:bg-primary/[0.04]",
                )}
              >
                <span
                  className={cn(
                    "flex size-9 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
                    active
                      ? "bg-primary text-primary-foreground ring-primary/20"
                      : "bg-muted text-muted-foreground ring-border/70 group-hover:text-primary",
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
