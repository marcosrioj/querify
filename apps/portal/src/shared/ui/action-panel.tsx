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
    <Card className="overflow-hidden rounded-lg border-border/80 bg-card/95 shadow-[0_12px_32px_-28px_rgba(15,23,42,0.45)]">
      <CardHeader className="px-3.5 py-3">
        <CardHeading>
          <CardTitle className="flex items-center gap-2 text-sm">
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
      <CardContent className="p-3">
        <div className="grid grid-cols-2 gap-2">{children}</div>
      </CardContent>
    </Card>
  );
}

export function ActionButton({
  tone,
  span,
  className,
  autoHeight = true,
  size = "lg",
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
