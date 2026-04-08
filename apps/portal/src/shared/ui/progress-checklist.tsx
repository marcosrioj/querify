import { type ReactNode } from "react";
import { ArrowRight, CheckCircle2, CircleDashed } from "lucide-react";
import { Link } from "react-router-dom";
import { cn } from "@/lib/utils";
import { translateMaybeString, usePortalI18n } from "@/shared/lib/i18n";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";

export type ProgressChecklistStep = {
  id: string;
  label: string;
  description: string;
  complete: boolean;
};

export function ProgressChecklistCard({
  eyebrow = "Start here",
  title,
  description,
  steps,
  action,
  secondaryAction,
  className,
  hideWhenComplete = true,
}: {
  eyebrow?: string;
  title: ReactNode;
  description: ReactNode;
  steps: ProgressChecklistStep[];
  action?: { label: string; to: string };
  secondaryAction?: { label: string; to: string };
  className?: string;
  hideWhenComplete?: boolean;
}) {
  const { t } = usePortalI18n();
  const completeCount = steps.filter((step) => step.complete).length;
  const allStepsComplete = steps.length > 0 && completeCount === steps.length;
  const progressValue =
    steps.length > 0 ? Math.round((completeCount / steps.length) * 100) : 0;
  const nextStep = steps.find((step) => !step.complete);

  if (hideWhenComplete && allStepsComplete) {
    return null;
  }

  return (
    <Card
      className={cn(
        "border-emerald-500/20 bg-linear-to-br from-emerald-500/[0.08] via-background to-background",
        className,
      )}
    >
      <CardContent className="space-y-5 p-5 lg:p-6">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="min-w-0 space-y-2">
            <p className="text-xs font-medium uppercase tracking-[0.22em] text-emerald-700 dark:text-emerald-400">
              {t(eyebrow)}
            </p>
            <div className="space-y-1">
              <h3 className="text-xl font-semibold tracking-tight text-mono">
                {translateMaybeString(title, t)}
              </h3>
              <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
                {translateMaybeString(description, t)}
              </p>
            </div>
          </div>

          <div className="rounded-2xl border border-emerald-500/20 bg-background/90 px-4 py-3 text-left shadow-xs sm:min-w-[180px] sm:text-right">
            <p className="text-2xl font-semibold tracking-tight text-mono">
              {completeCount}/{steps.length}
            </p>
            <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              {t("steps complete")}
            </p>
          </div>
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3 text-sm">
            <span className="font-medium text-foreground">
              {nextStep
                ? t("Next: {label}", { label: t(nextStep.label) })
                : t("Everything is in place")}
            </span>
            <span className="font-semibold text-emerald-700 dark:text-emerald-400">
              {progressValue}%
            </span>
          </div>
          <Progress
            value={progressValue}
            className="h-2 bg-emerald-500/12"
            indicatorClassName="bg-emerald-500"
          />
        </div>

        <div className="grid gap-3 lg:grid-cols-2">
          {steps.map((step) => (
            <div
              key={step.id}
              className={cn(
                "rounded-2xl border p-4",
                step.complete
                  ? "border-emerald-500/20 bg-emerald-500/[0.06]"
                  : "border-border/80 bg-background/80",
              )}
            >
              <div className="flex items-start gap-3">
                <div
                  className={cn(
                    "mt-0.5 flex size-6 shrink-0 items-center justify-center rounded-full",
                    step.complete
                      ? "bg-emerald-500/12 text-emerald-600 dark:text-emerald-400"
                      : "bg-muted text-muted-foreground",
                  )}
                >
                  {step.complete ? (
                    <CheckCircle2 className="size-4" />
                  ) : (
                    <CircleDashed className="size-4" />
                  )}
                </div>
                <div className="min-w-0 space-y-1">
                  <p className="font-medium text-foreground">{t(step.label)}</p>
                  <p className="text-sm leading-6 text-muted-foreground">
                    {t(step.description)}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>

        {action || secondaryAction ? (
          <div className="flex flex-wrap gap-3">
            {action ? (
              <Button asChild variant="mono">
                <Link to={action.to}>
                  {t(action.label)}
                  <ArrowRight className="size-4" />
                </Link>
              </Button>
            ) : null}
            {secondaryAction ? (
              <Button asChild variant="outline">
                <Link to={secondaryAction.to}>{t(secondaryAction.label)}</Link>
              </Button>
            ) : null}
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
