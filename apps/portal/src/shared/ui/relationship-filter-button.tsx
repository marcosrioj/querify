import { LoaderCircle, SlidersHorizontal } from "lucide-react";
import { Badge, Button } from "@/shared/ui";
import { translateText } from "@/shared/lib/i18n-core";
import { cn } from "@/lib/utils";

export function RelationshipFilterButton({
  activeFilterCount,
  className,
  isLoading,
  onClick,
  open,
}: {
  activeFilterCount: number;
  className?: string;
  isLoading?: boolean;
  onClick: () => void;
  open: boolean;
}) {
  const label = translateText("Refine view");

  return (
    <Button
      type="button"
      variant={open || activeFilterCount > 0 ? "primary" : "outline"}
      size="sm"
      className={cn(
        "h-8 gap-1.5 border-primary/35 px-2 text-xs text-primary shadow-[0_10px_24px_-18px_rgba(16,185,129,0.9)] hover:border-primary/60 hover:bg-primary/10",
        (open || activeFilterCount > 0) && "text-primary-foreground",
        className,
      )}
      aria-label={label}
      title={label}
      aria-expanded={open}
      onClick={onClick}
    >
      {isLoading ? (
        <LoaderCircle className="size-4 animate-spin" />
      ) : (
        <SlidersHorizontal className="size-4" />
      )}
      {activeFilterCount > 0 ? (
        <Badge variant="primary" appearance="outline" size="sm">
          {activeFilterCount}
        </Badge>
      ) : null}
    </Button>
  );
}
