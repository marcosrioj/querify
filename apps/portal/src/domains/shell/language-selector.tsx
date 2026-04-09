import { Globe } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/shared/ui";
import { useUserProfile, useUpdateUserProfile } from "@/domains/settings/settings-hooks";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { getLanguageOption, portalLanguageOptions } from "@/shared/lib/language";

export function LanguageSelector() {
  const profileQuery = useUserProfile();
  const updateProfile = useUpdateUserProfile();
  const { language, t } = usePortalI18n();
  const activeLanguage = getLanguageOption(profileQuery.data?.language?.trim() || language);
  const selectedLanguage = activeLanguage.code;

  async function handleValueChange(nextLanguage: string) {
    if (!profileQuery.data || nextLanguage === selectedLanguage) {
      return;
    }

    await updateProfile.mutateAsync({
      givenName: profileQuery.data.givenName,
      surName: profileQuery.data.surName ?? null,
      phoneNumber: profileQuery.data.phoneNumber ?? null,
      language: nextLanguage,
      timeZone: profileQuery.data.timeZone ?? null,
    });
  }

  return (
    <Select
      value={selectedLanguage}
      onValueChange={(value) => {
        void handleValueChange(value);
      }}
      disabled={
        updateProfile.isPending ||
        profileQuery.isLoading ||
        profileQuery.isError ||
        !profileQuery.data
      }
    >
      <SelectTrigger
        className="w-auto min-w-0 gap-2 px-3 [&>svg:last-child]:hidden"
        aria-label={`${t("Language")}: ${activeLanguage.code}`}
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
