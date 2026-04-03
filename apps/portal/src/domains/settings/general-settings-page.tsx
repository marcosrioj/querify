import { MoonStar, Sparkles } from 'lucide-react';
import { useTheme } from 'next-themes';
import { PageHeader, SectionGrid, SettingsLayout } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';
import { settingsNavItems } from '@/domains/settings/settings-nav';

export function GeneralSettingsPage() {
  const { resolvedTheme, setTheme, theme } = useTheme();

  return (
    <SettingsLayout
      currentKey="general"
      items={settingsNavItems}
      header={
        <PageHeader
          eyebrow="Settings"
          title="General"
          description="Adjust how the portal looks and feels for your current device."
        />
      }
    >
      <SectionGrid
        items={[
          {
            title: 'Current theme',
            value: resolvedTheme === 'dark' ? 'Dark' : 'Light',
            description: 'Applied theme right now',
          },
          {
            title: 'Preference',
            value:
              theme === 'system' || !theme
                ? 'System'
                : theme === 'dark'
                  ? 'Dark'
                  : 'Light',
            description: 'Saved user choice',
          },
          {
            title: 'Sync scope',
            value: 'This browser',
            description: 'Stored locally for the portal app',
          },
        ]}
      />
      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Appearance</CardTitle>
            <CardDescription>
              Choose how the workspace should render on this device.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="grid gap-3 md:grid-cols-3">
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
