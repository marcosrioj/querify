import { ShieldCheck } from 'lucide-react';
import { PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
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
          description="Portal authentication is delegated to Auth0. There is no custom password, MFA, or session API in the current Portal backend."
        />
      }
    >
      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Identity provider</CardTitle>
            <CardDescription>
              Auth0 configuration aligned with backend JWT validation.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="space-y-4 text-sm text-muted-foreground">
          <div className="rounded-2xl border border-border bg-muted/40 p-4">
            <div className="flex items-center gap-2 font-medium text-mono">
              <ShieldCheck className="size-4" />
              Active identity context
            </div>
            <dl className="mt-3 space-y-2">
              <div className="flex items-center justify-between gap-3">
                <dt>User</dt>
                <dd>{user?.email ?? 'Email claim unavailable'}</dd>
              </div>
              <div className="flex items-center justify-between gap-3">
                <dt>Authority</dt>
                <dd>{RuntimeEnv.auth0Domain}</dd>
              </div>
              <div className="flex items-center justify-between gap-3">
                <dt>Audience</dt>
                <dd>{RuntimeEnv.auth0Audience}</dd>
              </div>
            </dl>
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
