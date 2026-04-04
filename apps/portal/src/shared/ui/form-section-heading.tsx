import { ReactNode } from "react";
import { cn } from "@/lib/utils";
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
  return (
    <div className={cn("space-y-1", className)}>
      {eyebrow ? (
        <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">
          {eyebrow}
        </p>
      ) : null}
      <div className="flex flex-wrap items-center gap-2">
        <h3 className="text-lg font-semibold tracking-tight text-primary">
          {title}
        </h3>
        {description ? (
          <ContextHint
            content={description}
            label={tooltipLabel ?? `${title} details`}
          />
        ) : null}
      </div>
    </div>
  );
}
