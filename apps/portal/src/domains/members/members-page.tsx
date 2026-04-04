import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Building2, MailPlus, ShieldCheck, Trash2, Users } from 'lucide-react';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { useTenant } from '@/platform/tenant/tenant-context';
import { useAuth } from '@/platform/auth/auth-context';
import { usePermission } from '@/platform/permissions/permissions';
import {
  inviteTemporaryMember,
  listTemporaryMembers,
  removeTemporaryMember,
  type MemberRecord,
} from '@/domains/members/temporary-members-adapter';
import { RoleBadge } from '@/shared/ui/status-badges';
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
  name: z.string().min(2, 'Name is required.'),
  email: z.string().email('Enter a valid email address.'),
  role: z.enum(['Admin', 'Member']),
});

type InviteFormValues = z.infer<typeof inviteSchema>;

export function MembersPage() {
  const { currentTenant } = useTenant();
  const { user } = useAuth();
  const canManageMembers = usePermission('members.manage');
  const [open, setOpen] = useState(false);
  const [revision, setRevision] = useState(0);

  const members = useMemo(() => {
    if (!currentTenant) {
      return [] as MemberRecord[];
    }

    return listTemporaryMembers(currentTenant.id, user);
  }, [currentTenant, revision, user]);

  const form = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: {
      name: '',
      email: '',
      role: 'Member',
    },
  });
  const activeCount = members.filter((member) => member.status === 'active').length;
  const pendingCount = members.filter((member) => member.status === 'pending').length;
  const adminCount = members.filter((member) => member.role === 'Admin').length;
  const currentUserCount = members.filter((member) => member.isCurrentUser).length;

  const columns: DataTableColumn<MemberRecord>[] = [
    {
      key: 'member',
      header: 'Member',
      cell: (member) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">
            {member.name}
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
      cell: (member) => <RoleBadge role={member.role} />,
    },
    {
      key: 'status',
      header: 'Status',
      cell: (member) => (
        <Badge variant={member.status === 'active' ? 'success' : 'warning'}>
          {member.status}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'w-[120px]',
      cell: (member) => (
        <div
          className="flex items-center justify-end"
          onClick={(event) => event.stopPropagation()}
        >
          <ConfirmAction
            title={`Remove ${member.email} from this workspace?`}
            description="This removes their access to the current workspace but does not delete their account."
            confirmLabel="Remove member"
            onConfirm={() => {
              if (!currentTenant) {
                return;
              }

              removeTemporaryMember(currentTenant.id, member.id);
              setRevision((value) => value + 1);
            }}
            trigger={
              <Button
                variant="ghost"
                mode="icon"
                disabled={!canManageMembers || member.isCurrentUser}
              >
                <Trash2 className="size-4 text-destructive" />
              </Button>
            }
          />
        </div>
      ),
    },
  ];

  return (
    <>
      <ListLayout
        header={
          <PageHeader
            eyebrow="Members"
            title="Members"
            description="Invite people, set roles, and track pending invites."
            actions={
              <Button
                disabled={!canManageMembers || !currentTenant}
                onClick={() => setOpen(true)}
              >
                <MailPlus className="size-4" />
                Invite member
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
                description: pendingCount ? 'Waiting for acceptance' : 'No invites outstanding',
                icon: MailPlus,
              },
              {
                title: 'Admins',
                value: adminCount,
                description: currentUserCount ? 'Includes your current access' : 'No current user context',
                icon: ShieldCheck,
              },
            ]}
          />
        ) : null}
        {currentTenant ? (
          <DataTable
            title="Members"
            description="See who has access and which invites are still pending."
            columns={columns}
            rows={members}
            getRowId={(row) => row.id}
            toolbar={
              <>
                <Badge variant="outline">{members.length} people</Badge>
                <Badge variant={pendingCount ? 'warning' : 'outline'} appearance="outline">
                  {pendingCount ? `${pendingCount} pending` : 'No pending invites'}
                </Badge>
              </>
            }
            emptyState={
              <EmptyState
                title="No members yet"
                description="Invite the first teammate to start collaborative FAQ operations."
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
            <DialogTitle>Invite member</DialogTitle>
            <DialogDescription>
              Add a teammate and assign their initial role.
            </DialogDescription>
          </DialogHeader>
          <Form {...form}>
            <form
              className="space-y-4"
              onSubmit={form.handleSubmit((values) => {
                if (!currentTenant) {
                  return;
                }

                inviteTemporaryMember(currentTenant.id, values);
                setRevision((value) => value + 1);
                setOpen(false);
                form.reset();
              })}
            >
              <TextField control={form.control} name="name" label="Name" />
              <TextField control={form.control} name="email" label="Email" />
              <SelectField
                control={form.control}
                name="role"
                label="Role"
                options={[
                  { value: 'Member', label: 'Member' },
                  { value: 'Admin', label: 'Admin' },
                ]}
              />
              <Button type="submit">Send invite</Button>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </>
  );
}
