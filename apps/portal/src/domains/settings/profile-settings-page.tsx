import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import { ProfileSettingsSkeleton } from '@/domains/settings/profile-settings-skeleton';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { useUpdateUserProfile, useUserProfile } from '@/domains/settings/settings-hooks';
import { usePortalI18n } from '@/shared/lib/i18n';
import { translateText } from '@/shared/lib/i18n-core';
import { portalLanguageOptions } from '@/shared/lib/language';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Form } from '@/shared/ui';
import { DEFAULT_PORTAL_TIME_ZONE, getTimeZoneOptions } from '@/shared/lib/time-zone';
import { SearchSelectField, TextField } from '@/shared/ui/form-fields';

const profileSchema = z.object({
  givenName: z.string().min(1, 'First name is required.'),
  surName: z.string().optional(),
  phoneNumber: z.string().min(1, 'Phone number is required.'),
  language: z.string().optional(),
  timeZone: z.string().optional(),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export function ProfileSettingsPage() {
  const { t } = usePortalI18n();
  const profileQuery = useUserProfile();
  const updateProfile = useUpdateUserProfile();
  const languageOptions = useMemo(() => portalLanguageOptions, []);
  const timeZoneOptions = useMemo(() => getTimeZoneOptions(), []);
  const showLoadingState =
    profileQuery.isLoading && profileQuery.data === undefined;

  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      givenName: '',
      surName: '',
      phoneNumber: '',
      language: '',
      timeZone: '',
    },
  });
  const selectedLanguageValue = form.watch('language');
  const selectedLanguageOption =
    languageOptions.find((option) => option.code === selectedLanguageValue)
      ? {
          value: selectedLanguageValue,
          label:
            languageOptions.find((option) => option.code === selectedLanguageValue)
              ?.label ?? selectedLanguageValue,
          description: translateText('{code} • {direction}', {
            code: selectedLanguageValue,
            direction:
              languageOptions.find((option) => option.code === selectedLanguageValue)
                ?.direction.toUpperCase() ?? 'LTR',
          }),
        }
      : null;
  const selectedTimeZoneValue = form.watch('timeZone');
  const selectedTimeZoneOption =
    timeZoneOptions.find((option) => option.value === selectedTimeZoneValue) ?? null;

  useEffect(() => {
    if (!profileQuery.data) {
      return;
    }

    form.reset({
      givenName: profileQuery.data.givenName,
      surName: profileQuery.data.surName ?? '',
      phoneNumber: profileQuery.data.phoneNumber ?? '',
      language: profileQuery.data.language ?? '',
      timeZone: profileQuery.data.timeZone ?? '',
    });
  }, [form, profileQuery.data]);

  if (showLoadingState) {
    return <ProfileSettingsSkeleton />;
  }

  return (
    <SettingsLayout
      currentKey="profile"
      items={settingsNavItems}
      header={
        <PageHeader
          title="Profile"
          description={t('Update your name and contact info.')}
        />
      }
    >
      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>{t('User profile')}</CardTitle>
            <CardDescription>
              {t('Update the details your team sees across the portal.')}
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form
              className="space-y-4"
              onSubmit={form.handleSubmit(async (values) => {
                const currentValues = form.getValues();
                await updateProfile.mutateAsync({
                  ...values,
                  language: currentValues.language?.trim()
                    ? currentValues.language
                    : null,
                  timeZone: currentValues.timeZone?.trim()
                    ? currentValues.timeZone
                    : null,
                });
              })}
            >
              <div className="grid gap-4 md:grid-cols-2">
                <TextField control={form.control} name="givenName" label="Given name" />
                <TextField control={form.control} name="surName" label="Surname" />
              </div>
              <TextField control={form.control} name="phoneNumber" label="Phone number" />
              <SearchSelectField
                control={form.control}
                name="language"
                label="Language"
                description="Choose the portal language saved in your profile."
                hint="The portal loads your profile language first, then the browser language, then English."
                options={languageOptions.map((option) => ({
                  value: option.code,
                  label: option.label,
                  description: translateText('{code} • {direction}', {
                    code: option.code,
                    direction: option.direction.toUpperCase(),
                  }),
                  keywords: [option.code, option.direction, option.label],
                }))}
                selectedOption={selectedLanguageOption}
                placeholder="Use browser language"
                searchPlaceholder="Search languages"
                emptyMessage="No languages found."
                allowClear
                clearLabel="Use browser language"
                resultCountHint={translateText('{count} languages available', {
                  count: languageOptions.length,
                })}
              />
              <SearchSelectField
                control={form.control}
                name="timeZone"
                label="Time zone"
                description="Choose a preferred time zone for dates across the portal."
                hint={translateText(
                  'If left empty, the portal keeps using {timeZone}. All timestamps stored in the database should be UTC.',
                  { timeZone: DEFAULT_PORTAL_TIME_ZONE },
                )}
                options={timeZoneOptions}
                selectedOption={selectedTimeZoneOption}
                placeholder={translateText('Use {timeZone}', {
                  timeZone: DEFAULT_PORTAL_TIME_ZONE,
                })}
                searchPlaceholder="Search time zones"
                emptyMessage="No time zones found."
                allowClear
                clearLabel={translateText('Use {timeZone}', {
                  timeZone: DEFAULT_PORTAL_TIME_ZONE,
                })}
                resultCountHint={translateText('{count} time zones available', {
                  count: timeZoneOptions.length,
                })}
              />
              <Button type="submit" disabled={updateProfile.isPending}>
                {t('Save profile')}
              </Button>
            </form>
          </Form>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
