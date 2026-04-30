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
  children,
}: PropsWithChildren<{
  title?: string;
  description?: string;
}>) {
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

export function ActionButton({
  tone,
  span,
  className,
  autoHeight = true,
  size = "sm",
  variant = "foreground",
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
      className={cn(actionButtonClassName({ tone, span }), className)}
      {...props}
    />
  );
}
