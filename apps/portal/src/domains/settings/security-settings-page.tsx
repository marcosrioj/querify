import { ShieldCheck } from 'lucide-react';
import { KeyValueList, PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { useAuth } from '@/platform/auth/auth-context';
import { RuntimeEnv } from '@/platform/runtime/env';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';

export function SecuritySettingsPage() {
  const { user, logout } = useAuth();

  return (
    <SettingsLayout
      currentKey="security"
      items={settingsNavItems}
      header={
        <PageHeader
          eyebrow="Settings"
          title="Security"
          description="See how this session is signed in."
        />
      }
    >
      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Identity</CardTitle>
            <CardDescription>
              Authentication is managed by Auth0 for the portal.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-2xl border border-border bg-muted/30 p-4">
            <div className="flex items-center gap-2 text-sm font-medium text-mono">
              <ShieldCheck className="size-4" />
              Active identity context
            </div>
            <div className="mt-3">
              <KeyValueList
                items={[
                  { label: 'User', value: user?.email ?? 'Email claim unavailable' },
                  { label: 'Authority', value: RuntimeEnv.auth0Domain },
                  { label: 'Audience', value: RuntimeEnv.auth0Audience },
                ]}
              />
            </div>
          </div>
          <Button
            variant="outline"
            onClick={() => {
              void logout();
            }}
          >
            Sign out
          </Button>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
