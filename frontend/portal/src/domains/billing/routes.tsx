import { RouteObject } from 'react-router-dom';
import { useCurrentWorkspace } from '@/domains/tenants/hooks';
import { usePermission } from '@/platform/permissions/permissions';
import { PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { EmptyState } from '@/shared/ui/placeholder-state';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui';
import { tenantEditionLabels } from '@/shared/constants/backend-enums';

function BillingPage() {
  const currentWorkspace = useCurrentWorkspace();
  const canManageBilling = usePermission('billing.manage');
  const placeholderInvoices = [
    { id: 'INV-2026-001', amount: '$0.00', status: 'Draft', date: 'April 1, 2026' },
    { id: 'INV-2026-000', amount: '$0.00', status: 'Preview', date: 'March 1, 2026' },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Billing"
        title="Plan and usage"
        description="The repo does not expose a Portal billing API yet, so this page is limited to current tenant edition visibility and scaffolded customer-facing controls."
      />

      <SectionGrid
        items={[
          {
            title: 'Current plan',
            value: currentWorkspace
              ? tenantEditionLabels[currentWorkspace.edition]
              : 'Unknown',
            description: 'Derived from the active tenant summary',
          },
          {
            title: 'Billing owner actions',
            value: canManageBilling ? 'Enabled' : 'Hidden',
            description: 'Current frontend permission mapping',
          },
        ]}
      />

      <Card>
        <CardHeader>
          <CardTitle>Subscription management</CardTitle>
          <CardDescription>
            Self-service billing stays Portal-side, but the live endpoints are not in
            the current backend.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-2xl border border-border bg-muted/40 p-4 text-sm text-muted-foreground">
            Billing contact: {currentWorkspace?.slug ? `${currentWorkspace.slug}@billing.basefaq.com` : 'Not configured'}
          </div>
          <Button disabled={!canManageBilling}>Manage subscription</Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Invoice history</CardTitle>
          <CardDescription>
            Placeholder table until the Portal billing API exists.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-hidden rounded-xl border border-border">
            <Table>
              <TableHeader className="bg-muted/50">
                <TableRow>
                  <TableHead>Invoice</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {placeholderInvoices.map((invoice) => (
                  <TableRow key={invoice.id}>
                    <TableCell className="font-medium text-mono">{invoice.id}</TableCell>
                    <TableCell>{invoice.date}</TableCell>
                    <TableCell>{invoice.status}</TableCell>
                    <TableCell className="text-right">{invoice.amount}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <div className="mt-4">
            <EmptyState
              title="Live billing API pending"
              description="Replace this placeholder table when invoice history and payment method endpoints are added to the Portal surface."
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export const BillingRoutes: RouteObject[] = [
  {
    path: 'billing',
    element: <BillingPage />,
    handle: {
      title: 'Billing',
      breadcrumb: 'Billing',
      navKey: 'billing',
    },
  },
];
