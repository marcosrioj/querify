import { MoonStar, Sparkles } from 'lucide-react';
import { useTheme } from 'next-themes';
import { PageHeader, SettingsLayout } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';
import { settingsNavItems } from '@/domains/settings/settings-nav';

export function GeneralSettingsPage() {
  const { setTheme } = useTheme();

  return (
    <SettingsLayout
      currentKey="general"
      items={settingsNavItems}
      header={
        <PageHeader
          eyebrow="Settings"
          title="General"
          description="Portal-level preferences stay local to the frontend foundation until dedicated APIs exist."
        />
      }
    >
      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Workspace preferences</CardTitle>
            <CardDescription>
              These controls are frontend-only and intentionally isolated from the
              .NET services.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-3">
          <Button variant="outline" onClick={() => setTheme('light')}>
            <Sparkles className="size-4" />
            Use light theme
          </Button>
          <Button variant="outline" onClick={() => setTheme('dark')}>
            <MoonStar className="size-4" />
            Use dark theme
          </Button>
          <Button variant="outline" onClick={() => setTheme('system')}>
            Follow system
          </Button>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
