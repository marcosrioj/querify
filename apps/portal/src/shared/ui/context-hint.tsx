import { ReactNode } from "react";
import { KeenIcon } from "@/components/keenicons";
import { cn } from "@/lib/utils";
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
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <button
          type="button"
          aria-label={label}
          className={cn(
            "inline-flex size-5 shrink-0 items-center justify-center rounded-full text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-hidden focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
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
        {content}
      </TooltipContent>
    </Tooltip>
  );
}
