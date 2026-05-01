import { useUserProfile, useUpdateUserProfile } from "@/domains/settings/settings-hooks";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { getLanguageOption } from "@/shared/lib/language";
import { LanguageSelectorControl } from "@/shared/ui/language-selector-control";

export function LanguageSelector({
  variant = "default",
}: {
  variant?: "default" | "compact";
}) {
  const profileQuery = useUserProfile();
  const updateProfile = useUpdateUserProfile();
  const { language, setLanguage, t } = usePortalI18n();
  const activeLanguage = getLanguageOption(profileQuery.data?.language?.trim() || language);
  const selectedLanguage = activeLanguage.code;

  async function handleValueChange(nextLanguage: string) {
    if (!profileQuery.data || nextLanguage === selectedLanguage) {
      return;
    }

    setLanguage(nextLanguage);
    await updateProfile.mutateAsync({
      givenName: profileQuery.data.givenName,
      surName: profileQuery.data.surName ?? null,
      phoneNumber: profileQuery.data.phoneNumber ?? null,
      language: nextLanguage,
      timeZone: profileQuery.data.timeZone ?? null,
    });
  }

  return (
    <LanguageSelectorControl
      language={selectedLanguage}
      onLanguageChange={(value) => {
        void handleValueChange(value);
      }}
      ariaLabel={`${t("Language")}: ${activeLanguage.code}`}
      variant={variant}
      disabled={
        updateProfile.isPending ||
        profileQuery.isLoading ||
        profileQuery.isError ||
        !profileQuery.data
      }
    />
  );
}
