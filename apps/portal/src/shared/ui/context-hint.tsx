import { type ReactNode, useRef, useState } from "react";
import { KeenIcon } from "@/components/keenicons";
import { cn } from "@/lib/utils";
import { translateMaybeString } from "@/shared/lib/i18n-render";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

export function ContextHint({
  content,
  label = "More information",
  className,
  contentClassName,
}: {
  content: ReactNode;
  label?: string;
  className?: string;
  contentClassName?: string;
}) {
  const { t } = usePortalI18n();
  const [open, setOpen] = useState(false);
  const lastPointerType = useRef<string | null>(null);

  return (
    <Tooltip open={open} onOpenChange={setOpen}>
      <TooltipTrigger asChild>
        <button
          type="button"
          aria-label={t(label)}
          onBlur={() => setOpen(false)}
          onClick={() => {
            if (
              lastPointerType.current === "touch" ||
              lastPointerType.current === "pen"
            ) {
              setOpen(true);
            }
          }}
          onPointerDown={(event) => {
            lastPointerType.current = event.pointerType;
          }}
          className={cn(
            "inline-flex size-6 shrink-0 items-center justify-center rounded-full text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-hidden focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 md:size-5",
            className,
          )}
        >
          <KeenIcon
            icon="information-2"
            style="outline"
            className="text-[0.875rem]"
            aria-hidden="true"
          />
        </button>
      </TooltipTrigger>
      <TooltipContent
        variant="light"
        className={cn(
          "max-w-80 text-xs leading-5 text-pretty",
          contentClassName,
        )}
      >
        {translateMaybeString(content, t)}
      </TooltipContent>
    </Tooltip>
  );
}
