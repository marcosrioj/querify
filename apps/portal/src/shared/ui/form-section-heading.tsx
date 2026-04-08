import { ReactNode } from "react";
import { cn } from "@/lib/utils";
import { translateMaybeString, usePortalI18n } from "@/shared/lib/i18n";
import { ContextHint } from "@/shared/ui/context-hint";

export function FormSectionHeading({
  title,
  description,
  eyebrow,
  tooltipLabel,
  className,
}: {
  title: string;
  description?: ReactNode;
  eyebrow?: string;
  tooltipLabel?: string;
  className?: string;
}) {
  const { t } = usePortalI18n();

  return (
    <div className={cn("space-y-1", className)}>
      {eyebrow ? (
        <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">
          {t(eyebrow)}
        </p>
      ) : null}
      <div className="flex flex-wrap items-center gap-2">
        <h3 className="text-lg font-semibold tracking-tight text-primary">
          {t(title)}
        </h3>
        {description ? (
          <ContextHint
            content={translateMaybeString(description, t)}
            label={
              tooltipLabel ??
              t("{title} details", {
                title: t(title),
              })
            }
          />
        ) : null}
      </div>
    </div>
  );
}
