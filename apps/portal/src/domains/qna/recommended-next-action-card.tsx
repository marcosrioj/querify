import type { ReactNode } from "react";
import { Card, CardContent } from "@/shared/ui";
import { translateText } from "@/shared/lib/i18n-core";

type RecommendedNextActionCardProps = {
  label: string;
  text: string;
  action: ReactNode;
};

export function RecommendedNextActionCard({
  label,
  text,
  action,
}: RecommendedNextActionCardProps) {
  return (
    <Card className="border-emerald-500/20 bg-linear-to-br from-background via-background to-emerald-500/[0.06]">
      <CardContent className="flex flex-col gap-4 p-5 lg:flex-row lg:items-center lg:justify-between">
        <div className="space-y-1">
          <p className="text-xs font-medium uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-300">
            {translateText("Recommended next action")}
          </p>
          <p className="text-lg font-semibold text-mono">
            {translateText(label)}
          </p>
          <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
            {translateText(text)}
          </p>
        </div>
        <div className="flex shrink-0 flex-wrap gap-2">{action}</div>
      </CardContent>
    </Card>
  );
}
