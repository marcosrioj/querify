import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useEffect } from 'react';
import { KeyRound, Sparkles } from 'lucide-react';
import { useCurrentWorkspace, useGenerateClientKey, useSetAiProviderCredentials, useTenantWorkspace, useUpdateTenantWorkspace } from '@/domains/tenants/hooks';
import { AiCommandType, TenantEdition, tenantEditionLabels } from '@/shared/constants/backend-enums';
import { KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardTitle, Form } from '@/shared/ui';
import { SelectField, TextField } from '@/shared/ui/form-fields';
import { EmptyState } from '@/shared/ui/placeholder-state';
import { TenantEditionBadge } from '@/shared/ui/status-badges';

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
  const currentWorkspace = useCurrentWorkspace();
  const { clientKeyQuery, aiProvidersQuery } = useTenantWorkspace();
  const updateWorkspace = useUpdateTenantWorkspace();
  const regenerateClientKey = useGenerateClientKey();
  const storeCredentials = useSetAiProviderCredentials();

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

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Tenant"
        title="Workspace controls"
        description="These forms are wired to the real Tenant Portal API surface: tenant rename/update, public client key generation, and AI provider credential assignment."
      />

      {!currentWorkspace ? (
        <EmptyState
          title="No active tenant workspace"
          description="`GET /api/tenant/tenants/GetAll` returned no active FAQ workspace for this user. Create a tenant name and edition below to provision the Portal-side tenant records."
        />
      ) : (
        <Card>
          <CardHeader className="flex-row items-start justify-between gap-4">
            <div className="space-y-1.5">
              <CardTitle>{currentWorkspace.name}</CardTitle>
              <CardDescription>
                Current workspace slug: {currentWorkspace.slug}
              </CardDescription>
            </div>
            <TenantEditionBadge edition={currentWorkspace.edition} />
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
              ]}
            />
          </CardContent>
        </Card>
      )}

      <div className="grid gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Branding and plan</CardTitle>
            <CardDescription>
              Uses `POST /api/tenant/tenants/CreateOrUpdate`.
            </CardDescription>
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
                />
                <SelectField
                  control={workspaceForm.control}
                  name="edition"
                  label="Edition"
                  options={Object.entries(tenantEditionLabels).map(([value, label]) => ({
                    value,
                    label,
                  }))}
                />
                <Button
                  type="submit"
                  disabled={updateWorkspace.isPending}
                >
                  Save workspace
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Public preview key</CardTitle>
            <CardDescription>
              Uses `GET /api/tenant/tenants/GetClientKey` and `POST /api/tenant/tenants/GenerateNewClientKey`.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="rounded-2xl border border-border bg-muted/40 p-4">
              <div className="flex items-center gap-2 text-sm font-medium text-mono">
                <KeyRound className="size-4" />
                Current client key
              </div>
              <p className="mt-3 break-all text-sm text-muted-foreground">
                {clientKeyQuery.data || 'No public client key has been generated yet.'}
              </p>
            </div>
            <Button
              variant="outline"
              disabled={regenerateClientKey.isPending}
              onClick={() => {
                void regenerateClientKey.mutateAsync();
              }}
            >
              Generate new client key
            </Button>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <Card>
          <CardHeader>
            <CardTitle>Configured AI providers</CardTitle>
            <CardDescription>
              Uses `GET /api/tenant/tenants/GetConfiguredAiProviders`.
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            {(aiProvidersQuery.data ?? []).map((provider) => (
              <div key={provider.id} className="rounded-2xl border border-border p-4">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="font-medium text-mono">{provider.provider}</p>
                    <p className="text-sm text-muted-foreground">{provider.model}</p>
                  </div>
                  <Badge variant={provider.isAiProviderKeyConfigured ? 'success' : 'warning'}>
                    {provider.command === AiCommandType.Generation ? 'Generation' : 'Matching'}
                  </Badge>
                </div>
                <p className="mt-3 text-sm text-muted-foreground">
                  {provider.isAiProviderKeyConfigured
                    ? 'Credential present for the current tenant.'
                    : 'Provider registered but no credential stored yet.'}
                </p>
              </div>
            ))}

            {!aiProvidersQuery.isLoading && !aiProvidersQuery.data?.length ? (
              <EmptyState
                title="No AI providers configured"
                description="The Tenant Portal API returned no configured providers for this user’s active workspace."
              />
            ) : null}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Store provider credentials</CardTitle>
            <CardDescription>
              Uses `POST /api/tenant/tenants/SetAiProviderCredentials`.
            </CardDescription>
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
                  description="The backend contract expects raw provider credentials."
                />
                <Button type="submit" disabled={storeCredentials.isPending}>
                  <Sparkles className="size-4" />
                  Save provider credentials
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
