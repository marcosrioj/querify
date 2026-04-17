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
import {
  FormCardSkeleton,
  KeyValueListSkeleton,
  SectionGridSkeleton,
} from '@/shared/ui/loading-states';

export function TenantSettingsSkeleton() {
  return (
    <SettingsLayout
      currentKey="tenant"
      items={settingsNavItems}
      header={
        <PageHeader
          title="Workspace"
          description="Update workspace info and the public client key."
        />
      }
    >
      <SectionGridSkeleton />

      <Card>
        <CardHeader className="gap-4">
          <CardHeading>
            <Skeleton className="h-7 w-48" />
            <Skeleton className="h-4 w-56" />
          </CardHeading>
          <Skeleton className="h-6 w-24 rounded-full" />
        </CardHeader>
        <CardContent>
          <KeyValueListSkeleton items={4} />
        </CardContent>
      </Card>

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <FormCardSkeleton fields={2} />

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>Public preview key</CardTitle>
              <CardDescription>
                Use this key for public previews and embedded experiences.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="rounded-2xl border border-border bg-muted/30 p-4">
              <div className="space-y-2">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-[85%]" />
              </div>
            </div>
            <Skeleton className="h-10 w-48" />
          </CardContent>
        </Card>
      </div>
    </SettingsLayout>
  );
}
