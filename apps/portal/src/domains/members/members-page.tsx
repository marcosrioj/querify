import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Building2, MailPlus, ShieldCheck, Trash2, Users } from 'lucide-react';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { useTenant } from '@/platform/tenant/tenant-context';
import { usePermission } from '@/platform/permissions/permissions';
import {
  useAddTenantMember,
  useDeleteTenantMember,
  useTenantMembers,
} from '@/domains/members/hooks';
import { MembersPageSkeleton } from '@/domains/members/members-page-skeleton';
import type { TenantUserDto } from '@/domains/members/types';
import { TenantUserRoleType } from '@/shared/constants/backend-enums';
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
import { TextField } from '@/shared/ui/form-fields';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { EmptyState } from '@/shared/ui/placeholder-state';

const inviteSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Enter a name.')
    .max(100, 'Name must be 100 characters or fewer.'),
  email: z.string().email('Enter a valid email address.'),
});

type InviteFormValues = z.infer<typeof inviteSchema>;

function getMemberName(member: TenantUserDto) {
  const fullName = [member.givenName, member.surName].filter(Boolean).join(' ').trim();
  return fullName || member.email;
}

export function MembersPage() {
  const { currentTenant, isLoading: isTenantLoading } = useTenant();
  const canManageMembers = usePermission('members.manage');
  const [open, setOpen] = useState(false);
  const membersQuery = useTenantMembers();
  const addMember = useAddTenantMember();
  const deleteMember = useDeleteTenantMember();
  const members = membersQuery.data ?? [];

  const form = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: {
      name: '',
      email: '',
    },
  });
  const activeCount = members.length;
  const pendingCount = 0;
  const ownerCount = members.filter((member) => member.role === TenantUserRoleType.Owner).length;
  const currentUserCount = members.filter((member) => member.isCurrentUser).length;
  const showLoadingState =
    isTenantLoading ||
    (Boolean(currentTenant) &&
      membersQuery.isLoading &&
      membersQuery.data === undefined);

  if (showLoadingState) {
    return <MembersPageSkeleton />;
  }

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
            description="Manage workspace access for the current tenant."
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
                description: 'Member access is saved immediately',
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
                  Members only
                </Badge>
              </>
            }
            emptyState={
              <EmptyState
                title="No members yet"
                description="Add the first teammate by name and email to share this workspace."
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
              Add a workspace member by name and email. The email must not already belong to a member in this workspace.
            </DialogDescription>
          </DialogHeader>
          <Form {...form}>
            <form
              className="space-y-4"
              onSubmit={form.handleSubmit(async (values) => {
                if (!currentTenant) {
                  return;
                }

                const normalizedEmail = values.email.trim().toLowerCase();
                const existingMember = members.find(
                  (member) => member.email.trim().toLowerCase() === normalizedEmail,
                );

                if (existingMember) {
                  form.setError('email', {
                    type: 'validate',
                    message: 'This email is already a member of the workspace.',
                  });
                  return;
                }

                form.clearErrors('email');

                await addMember.mutateAsync({
                  name: values.name,
                  email: values.email,
                  role: TenantUserRoleType.Member,
                });
                setOpen(false);
                form.reset();
              })}
            >
              <TextField
                control={form.control}
                name="name"
                label="Name"
                description="Used as the display name for the member account."
              />
              <TextField
                control={form.control}
                name="email"
                label="Email"
                description="Use an email that is not already assigned to a member in this workspace."
              />
              <Button type="submit" disabled={addMember.isPending}>
                Add member
              </Button>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </>
  );
}
