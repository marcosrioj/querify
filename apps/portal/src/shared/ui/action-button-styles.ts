import { cn } from "@/lib/utils";

export function actionButtonClassName({
  tone = "secondary",
  span = "single",
}: {
  tone?: "primary" | "secondary" | "danger";
  span?: "single" | "full";
} = {}) {
  return cn(
    "h-auto min-h-9 w-full justify-start rounded-md px-2.5 py-1.5 text-left text-xs font-medium leading-5 shadow-none",
    "border transition-[background-color,border-color,color,box-shadow,transform]",
    "[&_svg]:size-3.5 [&_svg]:text-current [&_svg]:opacity-75",
    "focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
    span === "full" && "col-span-full",
    tone === "primary" &&
      "border-primary/35 bg-primary text-primary-foreground hover:bg-primary/90 hover:shadow-[0_14px_28px_-18px_rgba(16,185,129,0.95)]",
    tone === "secondary" &&
      "border-border/80 bg-background text-foreground hover:border-primary/25 hover:bg-primary/[0.045]",
    tone === "danger" &&
      "border-destructive/25 bg-destructive/[0.045] text-destructive hover:border-destructive/35 hover:bg-destructive/[0.075]",
  );
}
