import type { ComponentProps, PropsWithChildren } from "react";
import { cn } from "@/lib/utils";
import { actionButtonClassName } from "@/shared/ui/action-button-styles";
import { translateText } from "@/shared/lib/i18n-core";
import { Button } from "@/components/ui/button";
import { ContextHint } from "@/shared/ui/context-hint";
import {
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
} from "@/components/ui/card";

export function ActionPanel({
  title = "Actions",
  description,
  layout = "panel",
  children,
}: PropsWithChildren<{
  title?: string;
  description?: string;
  layout?: "panel" | "bar";
}>) {
  const isBar = layout === "bar";

  if (isBar) {
    return (
      <Card className="rounded-lg border-border/70 bg-card/90 shadow-[0_14px_34px_-28px_rgba(15,23,42,0.55)] ring-1 ring-black/[0.015] backdrop-blur supports-[backdrop-filter]:bg-card/80 dark:ring-white/[0.035]">
        <CardContent className="px-4 py-3 sm:px-5">
          <div className="flex min-w-0 flex-col gap-3 sm:flex-row sm:items-center">
            <div className="flex shrink-0 items-center gap-2 sm:border-r sm:border-border/70 sm:pr-4">
              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                {translateText(title)}
              </p>
              {description ? (
                <ContextHint
                  content={translateText(description)}
                  label={translateText("{title} details", { title })}
                  className="size-4 text-muted-foreground"
                />
              ) : null}
            </div>

            <div className="min-w-0 flex-1 overflow-x-auto">
              <div className="flex min-w-full flex-wrap items-center gap-1.5 sm:flex-nowrap [&>p]:shrink-0 [&>p]:px-2 [&_[data-action-align=end]]:ml-auto [&_[data-action-tone=danger]]:!basis-auto [&_[data-action-tone=danger]]:!grow-0 [&_[data-action-tone=danger]]:!shrink-0 [&_[data-action-tone=danger]]:!w-fit [&_[data-slot=button]]:!h-8 [&_[data-slot=button]]:!min-h-8 [&_[data-slot=button]]:!w-auto [&_[data-slot=button]]:!justify-center [&_[data-slot=button]]:!whitespace-nowrap [&_[data-slot=button]]:!px-3 [&_[data-slot=button]]:!py-1.5">
                {children}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="overflow-hidden rounded-lg border-border/70 bg-card/95 shadow-none">
      <CardHeader className="px-3 py-2.5">
        <CardHeading>
          <CardTitle className="flex items-center gap-2 text-[0.8125rem]">
            <span>{translateText(title)}</span>
            {description ? (
              <ContextHint
                content={translateText(description)}
                label={translateText("{title} details", { title })}
              />
            ) : null}
          </CardTitle>
        </CardHeading>
      </CardHeader>
      <CardContent className="px-2.5 pb-2.5 pt-2">
        <div className="grid grid-cols-1 gap-1.5 sm:grid-cols-2 [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal">
          {children}
        </div>
      </CardContent>
    </Card>
  );
}

export function ActionPanelEndGroup({
  children,
  className,
}: PropsWithChildren<{ className?: string }>) {
  return (
    <span
      data-action-group="end"
      className={cn(
        "ml-auto flex min-w-0 max-w-full flex-wrap items-center justify-end gap-1.5",
        className,
      )}
    >
      {children}
    </span>
  );
}

export function ActionButton({
  tone,
  span,
  className,
  autoHeight = true,
  size = "sm",
  variant = "foreground",
  "data-action-align": actionAlign,
  ...props
}: ComponentProps<typeof Button> & {
  tone?: "primary" | "secondary" | "danger";
  span?: "single" | "full";
}) {
  return (
    <Button
      autoHeight={autoHeight}
      size={size}
      variant={variant}
      data-action-tone={tone}
      data-action-align={actionAlign ?? (tone === "danger" ? "end" : undefined)}
      className={cn(
        actionButtonClassName({ tone, span }),
        tone === "danger" &&
          span !== "full" &&
          "w-fit max-w-max shrink-0 grow-0 basis-auto justify-center",
        className,
      )}
      {...props}
    />
  );
}
