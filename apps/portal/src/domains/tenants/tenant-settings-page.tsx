import { zodResolver } from '@hookform/resolvers/zod';
import { Building2, KeyRound } from 'lucide-react';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { TenantSettingsSkeleton } from '@/domains/tenants/tenant-settings-skeleton';
import {
  useCurrentWorkspace,
  useGenerateClientKey,
  useTenantWorkspace,
  useUpdateTenantWorkspace,
} from '@/domains/tenants/hooks';
import { settingsNavItems } from '@/domains/settings/settings-nav';
import { usePermission } from '@/platform/permissions/use-permission';
import { useTenant } from '@/platform/tenant/use-tenant';
import {
  TenantEdition,
  tenantEditionLabels,
  tenantUserRoleTypeLabels,
} from '@/shared/constants/backend-enums';
import {
  KeyValueList,
  PageHeader,
  SectionGrid,
  SettingsLayout,
} from '@/shared/layout/page-layouts';
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  CardToolbar,
  Form,
} from '@/shared/ui';
import { SelectField, TextField } from '@/shared/ui/form-fields';
import { translateText } from '@/shared/lib/i18n-core';
import { EmptyState } from '@/shared/ui/placeholder-state';
import { TenantEditionBadge } from '@/shared/ui/status-badges';

const workspaceSchema = z.object({
  name: z.string().min(2, 'Workspace name is required.'),
  edition: z
    .coerce.number()
    .refine((value) => Object.values(TenantEdition).includes(value as TenantEdition)),
});

type WorkspaceFormValues = z.infer<typeof workspaceSchema>;

export function TenantSettingsPage() {
  const { isLoading: isTenantLoading } = useTenant();
  const currentWorkspace = useCurrentWorkspace();
  const { clientKeyQuery } = useTenantWorkspace();
  const updateWorkspace = useUpdateTenantWorkspace();
  const regenerateClientKey = useGenerateClientKey();
  const canManageTenant = usePermission('tenant.manage');
  const canManageWorkspaceProfile = !currentWorkspace || canManageTenant;

  const workspaceForm = useForm<WorkspaceFormValues>({
    resolver: zodResolver(workspaceSchema),
    defaultValues: {
      name: currentWorkspace?.name ?? '',
      edition: currentWorkspace?.edition ?? TenantEdition.Starter,
    },
  });

  useEffect(() => {
    workspaceForm.reset({
      name: currentWorkspace?.name ?? '',
      edition: currentWorkspace?.edition ?? TenantEdition.Starter,
    });
  }, [currentWorkspace, workspaceForm]);

  const showLoadingState =
    isTenantLoading ||
    (Boolean(currentWorkspace) &&
      clientKeyQuery.isLoading &&
      clientKeyQuery.data === undefined);

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
          description="Update workspace info and the public client key."
        />
      }
    >
      <SectionGrid
        items={[
          {
            title: 'Edition',
            value: currentWorkspace
              ? tenantEditionLabels[currentWorkspace.edition]
              : 'No workspace',
            description: currentWorkspace?.slug || 'Choose or create a workspace',
            icon: Building2,
          },
          {
            title: 'Public key',
            value: clientKeyQuery.data ? 'Live' : 'Missing',
            description: clientKeyQuery.data
              ? 'Ready for previews and embeds'
              : 'Generate one to expose public experiences',
            icon: KeyRound,
          },
          {
            title: 'Workspace role',
            value: currentWorkspace
              ? tenantUserRoleTypeLabels[currentWorkspace.currentUserRole]
              : 'No workspace',
            description: currentWorkspace
              ? 'Current access level in this workspace'
              : 'Choose or create a workspace',
            icon: Building2,
          },
        ]}
      />

      {!currentWorkspace ? (
        <Card>
          <CardContent className="p-5">
            <EmptyState
              title="No active tenant workspace"
              description="Create or select a workspace before managing public access keys."
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
                { label: 'App scope', value: 'QnA Portal' },
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
                {clientKeyQuery.data ||
                  translateText('No client key has been generated yet.')}
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
    </SettingsLayout>
  );
}
