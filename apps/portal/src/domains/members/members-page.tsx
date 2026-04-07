import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Building2, MailPlus, ShieldCheck, Trash2, Users } from 'lucide-react';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { useTenant } from '@/platform/tenant/tenant-context';
import { usePermission } from '@/platform/permissions/permissions';
import {
  useCreateTenantMember,
  useDeleteTenantMember,
  useTenantMembers,
  useUpdateTenantMember,
} from '@/domains/members/hooks';
import type { TenantUserDto } from '@/domains/members/types';
import {
  TenantUserRoleType,
  tenantUserRoleTypeLabels,
} from '@/shared/constants/backend-enums';
import { TenantUserRoleBadge } from '@/shared/ui/status-badges';
import {
  Badge,
  Button,
  Card,
  CardContent,
  ConfirmAction,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  Form,
} from '@/shared/ui';
import { SelectField, TextField } from '@/shared/ui/form-fields';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { EmptyState } from '@/shared/ui/placeholder-state';

const inviteSchema = z.object({
  email: z.string().email('Enter a valid email address.'),
  role: z
    .coerce.number()
    .refine((value) => Object.values(TenantUserRoleType).includes(value as TenantUserRoleType)),
});

type InviteFormValues = z.infer<typeof inviteSchema>;

function getMemberName(member: TenantUserDto) {
  const fullName = [member.givenName, member.surName].filter(Boolean).join(' ').trim();
  return fullName || member.email;
}

export function MembersPage() {
  const { currentTenant } = useTenant();
  const canManageMembers = usePermission('members.manage');
  const [open, setOpen] = useState(false);
  const membersQuery = useTenantMembers();
  const createMember = useCreateTenantMember();
  const updateMember = useUpdateTenantMember();
  const deleteMember = useDeleteTenantMember();
  const members = membersQuery.data ?? [];

  const form = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: {
      email: '',
      role: TenantUserRoleType.Member,
    },
  });
  const activeCount = members.length;
  const pendingCount = 0;
  const ownerCount = members.filter((member) => member.role === TenantUserRoleType.Owner).length;
  const currentUserCount = members.filter((member) => member.isCurrentUser).length;

  const columns: DataTableColumn<TenantUserDto>[] = [
    {
      key: 'member',
      header: 'Member',
      cell: (member) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">
            {getMemberName(member)}
            {member.isCurrentUser ? (
              <span className="ml-2 text-xs text-muted-foreground">(You)</span>
            ) : null}
          </div>
          <div className="text-sm text-muted-foreground">{member.email}</div>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      cell: (member) => <TenantUserRoleBadge role={member.role} />,
    },
    {
      key: 'actions',
      header: 'Access',
      className: 'w-[240px]',
      cell: (member) => {
        if (member.role === TenantUserRoleType.Owner) {
          return (
            <span className="text-sm text-muted-foreground">
              {member.isCurrentUser ? 'Current owner' : 'Owner'}
            </span>
          );
        }

        if (!canManageMembers) {
          return <span className="text-sm text-muted-foreground">No actions</span>;
        }

        return (
          <div
            className="flex items-center justify-end gap-2"
            onClick={(event) => event.stopPropagation()}
          >
            <ConfirmAction
              title={`Transfer workspace ownership to ${member.email}?`}
              description="This member will become the owner and your access will switch to member."
              confirmLabel="Transfer ownership"
              onConfirm={() =>
                updateMember.mutateAsync({
                  id: member.id,
                  body: { role: TenantUserRoleType.Owner },
                })
              }
              trigger={
                <Button variant="outline" size="sm" disabled={updateMember.isPending}>
                  Make owner
                </Button>
              }
            />
            <ConfirmAction
              title={`Remove ${member.email} from this workspace?`}
              description="This removes their access to the current workspace but does not delete their account."
              confirmLabel="Remove member"
              onConfirm={() => deleteMember.mutateAsync(member.id)}
              trigger={
                <Button variant="ghost" mode="icon" disabled={deleteMember.isPending}>
                  <Trash2 className="size-4 text-destructive" />
                </Button>
              }
            />
          </div>
        );
      },
    },
  ];

  return (
    <>
      <ListLayout
        header={
          <PageHeader
            eyebrow="Members"
            title="Members"
            description="Manage workspace access and transfer ownership when needed."
            actions={
              <Button
                disabled={!canManageMembers || !currentTenant}
                onClick={() => setOpen(true)}
              >
                <MailPlus className="size-4" />
                Add member
              </Button>
            }
          />
        }
      >
        {currentTenant ? (
          <SectionGrid
            items={[
              {
                title: 'Workspace',
                value: currentTenant.name,
                description: currentTenant.slug,
                icon: Building2,
              },
              {
                title: 'Active members',
                value: activeCount,
                description: activeCount ? 'Already inside the workspace' : 'No active members yet',
                icon: Users,
              },
              {
                title: 'Pending invites',
                value: pendingCount,
                description: 'Existing-user memberships are active immediately',
                icon: MailPlus,
              },
              {
                title: 'Owners',
                value: ownerCount,
                description: currentUserCount ? 'Includes your current access' : 'No current user context',
                icon: ShieldCheck,
              },
            ]}
          />
        ) : null}
        {currentTenant ? (
          <DataTable
            title="Members"
            description="See who has access to the current workspace and which role each person has."
            columns={columns}
            rows={members}
            getRowId={(row) => row.id}
            loading={membersQuery.isLoading}
            toolbar={
              <>
                <Badge variant="outline">{members.length} people</Badge>
                <Badge variant="outline" appearance="outline">
                  Existing users only
                </Badge>
              </>
            }
            emptyState={
              <EmptyState
                title="No members yet"
                description="Add the first teammate by email to share this workspace."
              />
            }
          />
        ) : (
          <Card>
            <CardContent className="p-5">
              <EmptyState
                title="No active tenant"
                description="Select a workspace before managing access."
              />
            </CardContent>
          </Card>
        )}
      </ListLayout>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add member</DialogTitle>
            <DialogDescription>
              Add an existing BaseFAQ user to this workspace and choose their role.
            </DialogDescription>
          </DialogHeader>
          <Form {...form}>
            <form
              className="space-y-4"
              onSubmit={form.handleSubmit(async (values) => {
                if (!currentTenant) {
                  return;
                }

                await createMember.mutateAsync({
                  email: values.email,
                  role: values.role as TenantUserRoleType,
                });
                setOpen(false);
                form.reset();
              })}
            >
              <TextField
                control={form.control}
                name="email"
                label="Email"
                description="The user must already exist in BaseFAQ."
              />
              <SelectField
                control={form.control}
                name="role"
                label="Role"
                options={[
                  {
                    value: String(TenantUserRoleType.Member),
                    label: tenantUserRoleTypeLabels[TenantUserRoleType.Member],
                  },
                  {
                    value: String(TenantUserRoleType.Owner),
                    label: tenantUserRoleTypeLabels[TenantUserRoleType.Owner],
                  },
                ]}
              />
              <Button type="submit" disabled={createMember.isPending}>
                Add member
              </Button>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </>
  );
}
