import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { MailPlus, Trash2 } from 'lucide-react';
import { PageHeader } from '@/shared/layout/page-layouts';
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
  CardDescription,
  CardHeader,
  CardTitle,
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
          <Button
            variant="ghost"
            mode="icon"
            disabled={!canManageMembers || member.isCurrentUser}
            onClick={() => {
              if (
                currentTenant &&
                window.confirm(`Remove ${member.email} from this workspace?`)
              ) {
                removeTemporaryMember(currentTenant.id, member.id);
                setRevision((value) => value + 1);
              }
            }}
          >
            <Trash2 className="size-4 text-destructive" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Members"
        title="Tenant access"
        description="No Portal members API exists in the repository yet. This page uses an isolated temporary adapter so the Portal boundary and permission UX can still be exercised without pulling in BackOffice endpoints."
        actions={
          <Button disabled={!canManageMembers || !currentTenant} onClick={() => setOpen(true)}>
            <MailPlus className="size-4" />
            Invite member
          </Button>
        }
      />

      <Card>
        <CardHeader>
          <CardTitle>Access roster</CardTitle>
          <CardDescription>
            Temporary local adapter keyed by tenant id. Replace it when a Portal
            members contract is added.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {currentTenant ? (
            <DataTable
              columns={columns}
              rows={members}
              getRowId={(row) => row.id}
              emptyState={
                <EmptyState
                  title="No members yet"
                  description="Invite the first tenant user to start collaborative FAQ management."
                />
              }
            />
          ) : (
            <EmptyState
              title="No active tenant"
              description="Select or create a tenant workspace before managing members."
            />
          )}
        </CardContent>
      </Card>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Invite member</DialogTitle>
            <DialogDescription>
              Temporary local invite flow until the Portal members API is available.
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
    </div>
  );
}
