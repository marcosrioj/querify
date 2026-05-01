import { Globe } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { getLanguageOption, portalLanguageOptions } from "@/shared/lib/language";

type LanguageSelectorControlProps = {
  language?: string | null;
  onLanguageChange: (nextLanguage: string) => void;
  disabled?: boolean;
  ariaLabel: string;
  variant?: "default" | "compact";
};

export function LanguageSelectorControl({
  language,
  onLanguageChange,
  disabled = false,
  ariaLabel,
  variant = "default",
}: LanguageSelectorControlProps) {
  const activeLanguage = getLanguageOption(language);
  const isCompact = variant === "compact";

  return (
    <Select
      value={activeLanguage.code}
      onValueChange={onLanguageChange}
      disabled={disabled}
    >
      <SelectTrigger
        className={cn(
          "w-auto min-w-0 gap-2 [&>svg:last-child]:hidden",
          isCompact
            ? "h-8.5 justify-center border-transparent bg-transparent px-2.5 shadow-none hover:bg-accent data-[state=open]:bg-accent"
            : "px-3",
        )}
        aria-label={ariaLabel}
      >
        <div className="flex items-center gap-2">
          <Globe className="size-4 text-muted-foreground" aria-hidden="true" />
          <span className="inline-flex items-center pt-px leading-none font-mono text-xs uppercase tracking-[0.18em] text-foreground">
            {activeLanguage.code}
          </span>
        </div>
      </SelectTrigger>
      <SelectContent className="min-w-[185px]">
        {portalLanguageOptions.map((option) => (
          <SelectItem key={option.code} value={option.code}>
            {option.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
