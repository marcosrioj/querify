import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { useUpdateUserProfile, useUserProfile } from '@/domains/settings/settings-hooks';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Form } from '@/shared/ui';
import { TextField } from '@/shared/ui/form-fields';

const profileSchema = z.object({
  givenName: z.string().min(1, 'First name is required.'),
  surName: z.string().optional(),
  phoneNumber: z.string().min(1, 'Phone number is required.'),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export function ProfileSettingsPage() {
  const profileQuery = useUserProfile();
  const updateProfile = useUpdateUserProfile();

  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      givenName: '',
      surName: '',
      phoneNumber: '',
    },
  });

  useEffect(() => {
    if (!profileQuery.data) {
      return;
    }

    form.reset({
      givenName: profileQuery.data.givenName,
      surName: profileQuery.data.surName ?? '',
      phoneNumber: profileQuery.data.phoneNumber ?? '',
    });
  }, [form, profileQuery.data]);

  return (
    <SettingsLayout
      currentKey="profile"
      items={settingsNavItems}
      header={
        <PageHeader
          eyebrow="Settings"
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
                await updateProfile.mutateAsync(values);
              })}
            >
              <div className="grid gap-4 md:grid-cols-2">
                <TextField control={form.control} name="givenName" label="Given name" />
                <TextField control={form.control} name="surName" label="Surname" />
              </div>
              <TextField control={form.control} name="phoneNumber" label="Phone number" />
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
