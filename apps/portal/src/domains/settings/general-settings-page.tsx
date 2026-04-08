import { MoonStar, Sparkles } from 'lucide-react';
import { useTheme } from 'next-themes';
import { PageHeader, SectionGrid, SettingsLayout } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { usePortalI18n } from '@/shared/lib/i18n';

export function GeneralSettingsPage() {
  const { t } = usePortalI18n();
  const { resolvedTheme, setTheme, theme } = useTheme();

  return (
    <SettingsLayout
      currentKey="general"
      items={settingsNavItems}
      header={
        <PageHeader
          title="Appearance"
          description="Choose how the portal looks on this device."
        />
      }
    >
      <SectionGrid
        items={[
          {
            title: 'Current theme',
            value: resolvedTheme === 'dark' ? t('Dark') : t('Light'),
            description: 'Applied theme right now',
          },
          {
            title: 'Preference',
            value:
              theme === 'system' || !theme
                ? t('System')
                : theme === 'dark'
                  ? t('Dark')
                  : t('Light'),
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
            {t('Use light theme')}
          </Button>
          <Button variant="outline" onClick={() => setTheme('dark')}>
            <MoonStar className="size-4" />
            {t('Use dark theme')}
          </Button>
          <Button variant="outline" onClick={() => setTheme('system')}>
            {t('Follow system')}
          </Button>
        </CardContent>
      </Card>
    </SettingsLayout>
  );
}
