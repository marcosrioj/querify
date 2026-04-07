import { settingsNavItems } from '@/domains/settings/settings-nav';
import { PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  Skeleton,
} from '@/shared/ui';

export function ProfileSettingsSkeleton() {
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
          <div className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              {Array.from({ length: 2 }, (_, index) => (
                <div key={`profile-field-skeleton-${index}`} className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ))}
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-28" />
              <Skeleton className="h-10 w-full" />
            </div>
            <Skeleton className="h-10 w-32" />
          </div>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
