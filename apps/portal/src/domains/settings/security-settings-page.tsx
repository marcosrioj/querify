import { ShieldCheck } from 'lucide-react';
import { Link } from 'react-router-dom';
import { KeyValueList, PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { useAuth } from '@/platform/auth/use-auth';
import { RuntimeEnv } from '@/platform/runtime/env';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';

export function SecuritySettingsPage() {
  const { t } = usePortalI18n();
  const { user } = useAuth();

  return (
    <SettingsLayout
      currentKey="security"
      items={settingsNavItems}
      header={
        <PageHeader
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
                  { label: 'User', value: user?.email ?? t('Email claim unavailable') },
                  { label: 'Authority', value: RuntimeEnv.auth0Domain },
                  { label: 'Audience', value: RuntimeEnv.auth0Audience },
                ]}
              />
            </div>
          </div>
          <Button asChild variant="outline">
            <Link to="/logout">Sign out</Link>
          </Button>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
