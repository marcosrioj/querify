import { RouteObject } from 'react-router-dom';
import { useCurrentWorkspace } from '@/domains/tenants/hooks';
import { usePermission } from '@/platform/permissions/permissions';
import { PageHeader, PageSurface, SectionGrid } from '@/shared/layout/page-layouts';
import { EmptyState } from '@/shared/ui/placeholder-state';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui';
import { tenantEditionLabels } from '@/shared/constants/backend-enums';

function BillingPage() {
  const currentWorkspace = useCurrentWorkspace();
  const canManageBilling = usePermission('billing.manage');
  const placeholderInvoices = [
    { id: 'INV-2026-001', amount: '$0.00', status: 'Draft', date: 'April 1, 2026' },
    { id: 'INV-2026-000', amount: '$0.00', status: 'Preview', date: 'March 1, 2026' },
  ];

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        eyebrow="Billing"
        title="Plan and usage"
        description="Track plan visibility, billing ownership, and invoice placeholders from one workspace view."
      />

      <SectionGrid
        items={[
          {
            title: 'Current plan',
            value: currentWorkspace
              ? tenantEditionLabels[currentWorkspace.edition]
              : 'Unknown',
            description: currentWorkspace?.slug || 'No active workspace',
          },
          {
            title: 'Billing access',
            value: canManageBilling ? 'Enabled' : 'Hidden',
            description: canManageBilling ? 'Workspace owner controls visible' : 'Restricted by role',
          },
          {
            title: 'Invoices shown',
            value: placeholderInvoices.length,
            description: 'Preview rows until the live billing surface lands',
          },
          {
            title: 'Billing contact',
            value: currentWorkspace?.slug ? 'Configured' : 'Missing',
            description: currentWorkspace?.slug
              ? `${currentWorkspace.slug}@billing.basefaq.com`
              : 'No workspace contact yet',
          },
        ]}
      />

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Subscription management</CardTitle>
            <CardDescription>
              Review the workspace billing contact and self-serve controls.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-2xl border border-border bg-muted/30 p-4 text-sm text-muted-foreground">
            Billing contact: {currentWorkspace?.slug ? `${currentWorkspace.slug}@billing.basefaq.com` : 'Not configured'}
          </div>
          <Button disabled={!canManageBilling}>Manage subscription</Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Invoice history</CardTitle>
            <CardDescription>
              Recent invoice placeholders for the current workspace.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent>
          <div className="space-y-3 lg:hidden">
            {placeholderInvoices.map((invoice) => (
              <div key={invoice.id} className="rounded-xl border border-border/80 bg-card p-4">
                <div className="space-y-3">
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      Invoice
                    </p>
                    <p className="mt-1.5 font-medium text-mono">{invoice.id}</p>
                  </div>
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      Date
                    </p>
                    <p className="mt-1.5 text-sm">{invoice.date}</p>
                  </div>
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      Status
                    </p>
                    <p className="mt-1.5 text-sm">{invoice.status}</p>
                  </div>
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      Amount
                    </p>
                    <p className="mt-1.5 text-sm font-medium">{invoice.amount}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="hidden overflow-hidden rounded-xl border border-border lg:block">
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
              title="Live billing surface pending"
              description="Invoice history and payment methods will move here when the customer billing APIs land."
            />
          </div>
        </CardContent>
      </Card>
    </PageSurface>
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
