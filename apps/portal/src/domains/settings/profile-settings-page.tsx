import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import { ProfileSettingsSkeleton } from '@/domains/settings/profile-settings-skeleton';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { useUpdateUserProfile, useUserProfile } from '@/domains/settings/settings-hooks';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Form } from '@/shared/ui';
import { DEFAULT_PORTAL_TIME_ZONE, getTimeZoneOptions } from '@/shared/lib/time-zone';
import { SearchSelectField, TextField } from '@/shared/ui/form-fields';

const profileSchema = z.object({
  givenName: z.string().min(1, 'First name is required.'),
  surName: z.string().optional(),
  phoneNumber: z.string().min(1, 'Phone number is required.'),
  timeZone: z.string().optional(),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export function ProfileSettingsPage() {
  const profileQuery = useUserProfile();
  const updateProfile = useUpdateUserProfile();
  const timeZoneOptions = useMemo(() => getTimeZoneOptions(), []);
  const showLoadingState =
    profileQuery.isLoading && profileQuery.data === undefined;

  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      givenName: '',
      surName: '',
      phoneNumber: '',
      timeZone: '',
    },
  });
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
          description="Update your name and contact info."
        />
      }
    >
      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>User profile</CardTitle>
            <CardDescription>
              Update the details your team sees across the portal.
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
                name="timeZone"
                label="Time zone"
                description="Choose a preferred time zone for dates across the portal."
                hint={`If left empty, the portal keeps using ${DEFAULT_PORTAL_TIME_ZONE}. All timestamps stored in the database should be UTC.`}
                options={timeZoneOptions}
                selectedOption={selectedTimeZoneOption}
                placeholder={`Use ${DEFAULT_PORTAL_TIME_ZONE}`}
                searchPlaceholder="Search time zones"
                emptyMessage="No time zones found."
                allowClear
                clearLabel={`Use ${DEFAULT_PORTAL_TIME_ZONE}`}
                resultCountHint={`${timeZoneOptions.length} time zones available`}
              />
              <Button type="submit" disabled={updateProfile.isPending}>
                Save profile
              </Button>
            </form>
          </Form>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
