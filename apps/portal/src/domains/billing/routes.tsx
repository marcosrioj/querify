import { CreditCard, Mail, Receipt, ShieldCheck } from 'lucide-react';
import { RouteObject } from 'react-router-dom';
import { useCurrentWorkspace } from '@/domains/tenants/hooks';
import { usePermission } from '@/platform/permissions/permissions';
import { PageHeader, PageSurface, SectionGrid } from '@/shared/layout/page-layouts';
import { usePortalI18n } from '@/shared/lib/i18n';
import { translateText } from '@/shared/lib/i18n-core';
import { EmptyState } from '@/shared/ui/placeholder-state';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui';
import { tenantEditionLabels } from '@/shared/constants/backend-enums';

function formatInvoiceDate(date: string, language: string) {
  return new Intl.DateTimeFormat(language, { dateStyle: 'long' }).format(
    new Date(`${date}T00:00:00`),
  );
}

function BillingPage() {
  const { language } = usePortalI18n();
  const currentWorkspace = useCurrentWorkspace();
  const canManageBilling = usePermission('billing.manage');
  const placeholderInvoices = [
    { id: 'INV-2026-001', amount: '$0.00', status: 'Draft', date: '2026-04-01' },
    { id: 'INV-2026-000', amount: '$0.00', status: 'Preview', date: '2026-03-01' },
  ];

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Billing"
        description="See plan details and billing placeholders for this workspace."
      />

      <SectionGrid
        items={[
          {
            title: 'Current plan',
            value: currentWorkspace
              ? tenantEditionLabels[currentWorkspace.edition]
              : 'Unknown',
            description: currentWorkspace?.slug || 'No active workspace',
            icon: CreditCard,
          },
          {
            title: 'Billing access',
            value: canManageBilling ? 'Enabled' : 'Hidden',
            description: canManageBilling ? 'Workspace owner controls visible' : 'Restricted by role',
            icon: ShieldCheck,
          },
          {
            title: 'Invoices shown',
            value: placeholderInvoices.length,
            description: 'Preview rows until the live billing surface lands',
            icon: Receipt,
          },
          {
            title: 'Billing contact',
            value: currentWorkspace?.slug ? 'Configured' : 'Missing',
            description: currentWorkspace?.slug
              ? `${currentWorkspace.slug}@billing.basefaq.com`
              : 'No workspace contact yet',
            icon: Mail,
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
            {translateText('Billing contact: {value}', {
              value: currentWorkspace?.slug
                ? `${currentWorkspace.slug}@billing.basefaq.com`
                : translateText('Not configured'),
            })}
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
                      {translateText('Invoice')}
                    </p>
                    <p className="mt-1.5 font-medium text-mono">{invoice.id}</p>
                  </div>
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      {translateText('Date')}
                    </p>
                    <p className="mt-1.5 text-sm">
                      {formatInvoiceDate(invoice.date, language)}
                    </p>
                  </div>
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      {translateText('Status')}
                    </p>
                    <p className="mt-1.5 text-sm">
                      {translateText(invoice.status)}
                    </p>
                  </div>
                  <div>
                    <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                      {translateText('Amount')}
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
                    <TableCell>{formatInvoiceDate(invoice.date, language)}</TableCell>
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
