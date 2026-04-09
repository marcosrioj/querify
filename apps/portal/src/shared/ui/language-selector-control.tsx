import { Globe } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/components/ui/select";
import { getLanguageOption, portalLanguageOptions } from "@/shared/lib/language";

type LanguageSelectorControlProps = {
  language?: string | null;
  onLanguageChange: (nextLanguage: string) => void;
  disabled?: boolean;
  ariaLabel: string;
};

export function LanguageSelectorControl({
  language,
  onLanguageChange,
  disabled = false,
  ariaLabel,
}: LanguageSelectorControlProps) {
  const activeLanguage = getLanguageOption(language);

  return (
    <Select
      value={activeLanguage.code}
      onValueChange={onLanguageChange}
      disabled={disabled}
    >
      <SelectTrigger
        className="w-auto min-w-0 gap-2 px-3 [&>svg:last-child]:hidden"
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
