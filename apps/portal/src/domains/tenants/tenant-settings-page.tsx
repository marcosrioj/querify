import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useEffect } from 'react';
import { Bot, Building2, KeyRound, Sparkles, WandSparkles } from 'lucide-react';
import { TenantSettingsSkeleton } from '@/domains/tenants/tenant-settings-skeleton';
import { useCurrentWorkspace, useGenerateClientKey, useSetAiProviderCredentials, useTenantWorkspace, useUpdateTenantWorkspace } from '@/domains/tenants/hooks';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { usePermission } from '@/platform/permissions/use-permission';
import { useTenant } from '@/platform/tenant/use-tenant';
import { AiCommandType, TenantEdition, tenantUserRoleTypeLabels, tenantEditionLabels } from '@/shared/constants/backend-enums';
import { KeyValueList, PageHeader, SectionGrid, SettingsLayout } from '@/shared/layout/page-layouts';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, CardToolbar, Form } from '@/shared/ui';
import { SelectField, TextField } from '@/shared/ui/form-fields';
import { EmptyState } from '@/shared/ui/placeholder-state';
import { TenantEditionBadge } from '@/shared/ui/status-badges';
import { translateText } from '@/shared/lib/i18n-core';

const workspaceSchema = z.object({
  name: z.string().min(2, 'Workspace name is required.'),
  edition: z
    .coerce.number()
    .refine((value) => Object.values(TenantEdition).includes(value as TenantEdition)),
});

const credentialsSchema = z.object({
  aiProviderId: z.string().uuid('Choose a configured provider.'),
  aiProviderKey: z.string().min(8, 'Provider key is required.'),
});

type WorkspaceFormValues = z.infer<typeof workspaceSchema>;
type CredentialFormValues = z.infer<typeof credentialsSchema>;

export function TenantSettingsPage() {
  const { isLoading: isTenantLoading } = useTenant();
  const currentWorkspace = useCurrentWorkspace();
  const { clientKeyQuery, aiProvidersQuery } = useTenantWorkspace();
  const updateWorkspace = useUpdateTenantWorkspace();
  const regenerateClientKey = useGenerateClientKey();
  const storeCredentials = useSetAiProviderCredentials();
  const canManageTenant = usePermission('tenant.manage');
  const canManageWorkspaceProfile = !currentWorkspace || canManageTenant;

  const workspaceForm = useForm<WorkspaceFormValues>({
    resolver: zodResolver(workspaceSchema),
    defaultValues: {
      name: currentWorkspace?.name ?? '',
      edition: currentWorkspace?.edition ?? TenantEdition.Starter,
    },
  });

  const credentialsForm = useForm<CredentialFormValues>({
    resolver: zodResolver(credentialsSchema),
    defaultValues: {
      aiProviderId: '',
      aiProviderKey: '',
    },
  });

  useEffect(() => {
    workspaceForm.reset({
      name: currentWorkspace?.name ?? '',
      edition: currentWorkspace?.edition ?? TenantEdition.Starter,
    });
  }, [currentWorkspace, workspaceForm]);

  const configuredProviders = aiProvidersQuery.data ?? [];
  const credentialedProviderCount = configuredProviders.filter(
    (provider) => provider.isAiProviderKeyConfigured,
  ).length;
  const generationProviderCount = configuredProviders.filter(
    (provider) => provider.command === AiCommandType.Generation,
  ).length;
  const showLoadingState =
    isTenantLoading ||
    (Boolean(currentWorkspace) &&
      ((clientKeyQuery.isLoading && clientKeyQuery.data === undefined) ||
        (aiProvidersQuery.isLoading && aiProvidersQuery.data === undefined)));

  if (showLoadingState) {
    return <TenantSettingsSkeleton />;
  }

  return (
    <SettingsLayout
      currentKey="tenant"
      items={settingsNavItems}
      header={
        <PageHeader
          title="Workspace"
          description="Update workspace info, client key, and AI providers."
        />
      }
    >
      <SectionGrid
        items={[
          {
            title: 'Edition',
            value: currentWorkspace ? tenantEditionLabels[currentWorkspace.edition] : 'No workspace',
            description: currentWorkspace?.slug || 'Choose or create a workspace',
            icon: Building2,
          },
          {
            title: 'Public key',
            value: clientKeyQuery.data ? 'Live' : 'Missing',
            description: clientKeyQuery.data ? 'Ready for previews and embeds' : 'Generate one to expose public experiences',
            icon: KeyRound,
          },
          {
            title: 'AI providers',
            value: configuredProviders.length,
            description: configuredProviders.length
              ? translateText('{count} with credentials stored', {
                  count: credentialedProviderCount,
                })
              : 'No providers configured yet',
            icon: Bot,
          },
          {
            title: 'Generation models',
            value: generationProviderCount,
            description: 'Providers available for content generation',
            icon: WandSparkles,
          },
        ]}
      />

      {!currentWorkspace ? (
        <Card>
          <CardContent className="p-5">
            <EmptyState
              title="No active tenant workspace"
              description="Create or select a workspace before managing keys and provider credentials."
            />
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardHeader className="gap-4">
            <CardHeading>
              <CardTitle>{currentWorkspace.name}</CardTitle>
              <CardDescription>
                {translateText('Current workspace slug: {slug}', {
                  slug: currentWorkspace.slug,
                })}
              </CardDescription>
            </CardHeading>
            <CardToolbar>
              <TenantEditionBadge edition={currentWorkspace.edition} />
            </CardToolbar>
          </CardHeader>
          <CardContent>
            <KeyValueList
              items={[
                { label: 'Tenant ID', value: currentWorkspace.id },
                { label: 'App scope', value: 'FAQ Portal' },
                {
                  label: 'Public client key',
                  value: clientKeyQuery.data || 'No client key generated yet',
                },
                {
                  label: 'Your workspace role',
                  value: tenantUserRoleTypeLabels[currentWorkspace.currentUserRole],
                },
              ]}
            />
          </CardContent>
        </Card>
      )}

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{translateText('Workspace profile')}</CardTitle>
              <CardDescription>
                {translateText(
                  'Rename the workspace and choose the active plan tier.',
                )}
              </CardDescription>
            </CardHeading>
            <CardToolbar>
              <Badge variant={canManageTenant ? 'success' : 'outline'}>
                {!currentWorkspace
                  ? translateText('Workspace creation')
                  : canManageTenant
                    ? translateText('Owner access')
                    : translateText('Member access')}
              </Badge>
            </CardToolbar>
          </CardHeader>
          <CardContent>
            <Form {...workspaceForm}>
              <form
                className="space-y-4"
                onSubmit={workspaceForm.handleSubmit(async (values) => {
                  await updateWorkspace.mutateAsync(values);
                })}
              >
                <TextField
                  control={workspaceForm.control}
                  name="name"
                  label="Workspace name"
                  placeholder="BaseFAQ Labs"
                  disabled={!canManageWorkspaceProfile}
                />
                <SelectField
                  control={workspaceForm.control}
                  name="edition"
                  label="Edition"
                  disabled={!canManageWorkspaceProfile}
                  options={Object.entries(tenantEditionLabels).map(([value, label]) => ({
                    value,
                    label,
                  }))}
                />
                <Button
                  type="submit"
                  disabled={!canManageWorkspaceProfile || updateWorkspace.isPending}
                >
                  {translateText('Save workspace')}
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{translateText('Public preview key')}</CardTitle>
              <CardDescription>
                {translateText(
                  'Use this key for public previews and embedded experiences.',
                )}
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="rounded-2xl border border-border bg-muted/30 p-4">
              <div className="flex items-center gap-2 text-sm font-medium text-mono">
                <KeyRound className="size-4" />
                {translateText('Current client key')}
              </div>
              <p className="mt-3 break-all text-sm text-muted-foreground">
                {clientKeyQuery.data || translateText('No client key has been generated yet.')}
              </p>
            </div>
            <Button
              variant="outline"
              disabled={!canManageTenant || regenerateClientKey.isPending}
              onClick={() => {
                void regenerateClientKey.mutateAsync();
              }}
            >
              {translateText('Generate new client key')}
            </Button>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_360px] lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{translateText('Configured AI providers')}</CardTitle>
              <CardDescription>
                {translateText(
                  'Review which providers are ready for matching and generation.',
                )}
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            {(aiProvidersQuery.data ?? []).map((provider) => (
              <div key={provider.id} className="rounded-2xl border border-border bg-muted/15 p-4">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="font-medium text-mono">{provider.provider}</p>
                    <p className="text-sm text-muted-foreground">{provider.model}</p>
                  </div>
                  <Badge variant={provider.isAiProviderKeyConfigured ? 'success' : 'warning'}>
                    {translateText(
                      provider.command === AiCommandType.Generation
                        ? 'Generation'
                        : 'Matching',
                    )}
                  </Badge>
                </div>
                <p className="mt-3 text-sm text-muted-foreground">
                  {translateText(
                    provider.isAiProviderKeyConfigured
                      ? 'Credential stored for this workspace.'
                      : 'Provider available, but credentials are still missing.',
                  )}
                </p>
              </div>
            ))}

            {!aiProvidersQuery.isLoading && !aiProvidersQuery.data?.length ? (
              <EmptyState
                title="No AI providers configured"
                description="Add a provider before connecting credentials or launching generation."
              />
            ) : null}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{translateText('Store provider credentials')}</CardTitle>
              <CardDescription>
                {translateText(
                  'Attach or rotate provider secrets for this workspace.',
                )}
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <Form {...credentialsForm}>
              <form
                className="space-y-4"
                onSubmit={credentialsForm.handleSubmit(async (values) => {
                  await storeCredentials.mutateAsync(values);
                  credentialsForm.reset({
                    aiProviderId: values.aiProviderId,
                    aiProviderKey: '',
                  });
                })}
              >
                <SelectField
                  control={credentialsForm.control}
                  name="aiProviderId"
                  label="Provider"
                  disabled={!canManageTenant}
                  options={(aiProvidersQuery.data ?? []).map((provider) => ({
                    value: provider.aiProviderId,
                    label: `${provider.provider} · ${provider.model}`,
                  }))}
                />
                <TextField
                  control={credentialsForm.control}
                  name="aiProviderKey"
                  label="Provider secret"
                  placeholder="Paste provider API key"
                  description="Stored against the selected provider."
                  disabled={!canManageTenant}
                />
                <Button type="submit" disabled={!canManageTenant || storeCredentials.isPending}>
                  <Sparkles className="size-4" />
                  {translateText('Save provider credentials')}
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
      </div>
    </SettingsLayout>
  );
}
