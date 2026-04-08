import { Globe } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui";
import { useUserProfile, useUpdateUserProfile } from "@/domains/settings/settings-hooks";
import { usePortalI18n } from "@/shared/lib/i18n";
import { portalLanguageOptions } from "@/shared/lib/language";

export function LanguageSelector() {
  const profileQuery = useUserProfile();
  const updateProfile = useUpdateUserProfile();
  const { language, t } = usePortalI18n();
  const selectedLanguage = profileQuery.data?.language?.trim() || language;

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
    <div className="flex items-center gap-2">
      <Globe className="size-4 text-muted-foreground" aria-hidden="true" />
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
          className="h-11 w-[185px]"
          aria-label={t("Language")}
        >
          <SelectValue placeholder={t("Language")} />
        </SelectTrigger>
        <SelectContent>
          {portalLanguageOptions.map((option) => (
            <SelectItem key={option.code} value={option.code}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
