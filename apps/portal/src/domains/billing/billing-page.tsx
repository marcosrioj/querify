import {
  ArrowUpRight,
  CreditCard,
  Download,
  Receipt,
  ShieldCheck,
  Wallet,
} from 'lucide-react';
import { BillingPageSkeleton } from '@/domains/billing/billing-page-skeleton';
import { useBillingWorkspace } from '@/domains/billing/hooks';
import type {
  BillingInvoiceDto,
  BillingPaymentDto,
} from '@/domains/billing/types';
import { usePermission } from '@/platform/permissions/use-permission';
import { useTenant } from '@/platform/tenant/use-tenant';
import {
  KeyValueList,
  PageHeader,
  PageSurface,
  SectionGrid,
} from '@/shared/layout/page-layouts';
import { translateText } from '@/shared/lib/i18n-core';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/ui';
import { ErrorState, EmptyState } from '@/shared/ui/placeholder-state';
import {
  billingIntervalLabels,
  billingInvoiceStatusLabels,
  billingPaymentStatusLabels,
  billingProviderLabels,
  tenantEditionLabels,
  tenantSubscriptionStatusLabels,
  type BillingInvoiceStatus,
  type BillingPaymentStatus,
  type TenantSubscriptionStatus,
} from '@/shared/constants/backend-enums';

function formatDateTime(value: string | null | undefined, language: string) {
  if (!value) {
    return 'Not available';
  }

  return new Intl.DateTimeFormat(language, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

function formatPeriod(
  startValue: string | null | undefined,
  endValue: string | null | undefined,
  language: string,
) {
  if (!startValue && !endValue) {
    return 'Not scheduled';
  }

  if (!startValue) {
    return `Until ${formatDateTime(endValue, language)}`;
  }

  if (!endValue) {
    return `From ${formatDateTime(startValue, language)}`;
  }

  return `${formatDateTime(startValue, language)} - ${formatDateTime(endValue, language)}`;
}

function formatMoney(
  amountMinor: number | null | undefined,
  currency: string | null | undefined,
  language: string,
) {
  const resolvedAmountMinor = amountMinor ?? 0;
  const resolvedCurrency = (currency || 'USD').toUpperCase();

  return new Intl.NumberFormat(language, {
    style: 'currency',
    currency: resolvedCurrency,
  }).format(resolvedAmountMinor / 100);
}

function getSubscriptionBadgeVariant(status: TenantSubscriptionStatus) {
  switch (status) {
    case 1:
    case 2:
      return 'success';
    case 3:
    case 6:
    case 7:
    case 8:
      return 'warning';
    case 4:
    case 5:
      return 'destructive';
    default:
      return 'outline';
  }
}

function getInvoiceBadgeVariant(status: BillingInvoiceStatus) {
  switch (status) {
    case 3:
      return 'success';
    case 1:
    case 2:
      return 'warning';
    case 4:
    case 5:
    case 6:
      return 'destructive';
    default:
      return 'outline';
  }
}

function getPaymentBadgeVariant(status: BillingPaymentStatus) {
  switch (status) {
    case 2:
      return 'success';
    case 1:
      return 'warning';
    case 3:
    case 4:
      return 'destructive';
    default:
      return 'outline';
  }
}

function InvoiceActions({ invoice }: { invoice: BillingInvoiceDto }) {
  if (!invoice.hostedUrl && !invoice.pdfUrl) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {invoice.hostedUrl ? (
        <Button asChild variant="outline" size="sm">
          <a href={invoice.hostedUrl} target="_blank" rel="noreferrer">
            {translateText('Open')}
            <ArrowUpRight className="size-4" />
          </a>
        </Button>
      ) : null}
      {invoice.pdfUrl ? (
        <Button asChild variant="outline" size="sm">
          <a href={invoice.pdfUrl} target="_blank" rel="noreferrer">
            PDF
            <Download className="size-4" />
          </a>
        </Button>
      ) : null}
    </div>
  );
}

function InvoiceHistoryCard({
  invoices,
  isError,
  error,
  onRetry,
  language,
}: {
  invoices: BillingInvoiceDto[];
  isError: boolean;
  error: unknown;
  onRetry: () => void;
  language: string;
}) {
  return (
    <Card>
      <CardHeader>
        <CardHeading>
          <CardTitle>{translateText('Invoice history')}</CardTitle>
          <CardDescription>
            Latest invoices recorded for this workspace.
          </CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent className="space-y-4">
        {isError && !invoices.length ? (
          <ErrorState
            title="Billing request failed"
            error={error}
            retry={onRetry}
          />
        ) : !invoices.length ? (
          <EmptyState
            title="No invoices yet"
            description="Invoices will appear here after the first billing cycle is recorded."
          />
        ) : (
          <>
            <div className="space-y-3 lg:hidden">
              {invoices.map((invoice) => (
                <div
                  key={invoice.id}
                  className="rounded-xl border border-border/80 bg-card p-4"
                >
                  <div className="space-y-3">
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Invoice
                      </p>
                      <p className="mt-1.5 font-medium text-mono">
                        {invoice.externalInvoiceId}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Date
                      </p>
                      <p className="mt-1.5 text-sm">
                        {formatDateTime(
                          invoice.paidAtUtc ||
                            invoice.dueDateUtc ||
                            invoice.createdDateUtc,
                          language,
                        )}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Status
                      </p>
                      <div className="mt-1.5">
                        <Badge
                          variant={getInvoiceBadgeVariant(invoice.status)}
                          appearance="outline"
                        >
                          {billingInvoiceStatusLabels[invoice.status]}
                        </Badge>
                      </div>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Amount
                      </p>
                      <p className="mt-1.5 text-sm font-medium">
                        {formatMoney(invoice.amountMinor, invoice.currency, language)}
                      </p>
                    </div>
                    <InvoiceActions invoice={invoice} />
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
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {invoices.map((invoice) => (
                    <TableRow key={invoice.id}>
                      <TableCell className="font-medium text-mono">
                        {invoice.externalInvoiceId}
                      </TableCell>
                      <TableCell>
                        {formatDateTime(
                          invoice.paidAtUtc ||
                            invoice.dueDateUtc ||
                            invoice.createdDateUtc,
                          language,
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={getInvoiceBadgeVariant(invoice.status)}
                          appearance="outline"
                        >
                          {billingInvoiceStatusLabels[invoice.status]}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        {formatMoney(invoice.amountMinor, invoice.currency, language)}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end">
                          <InvoiceActions invoice={invoice} />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

function PaymentHistoryCard({
  payments,
  isError,
  error,
  onRetry,
  language,
}: {
  payments: BillingPaymentDto[];
  isError: boolean;
  error: unknown;
  onRetry: () => void;
  language: string;
}) {
  return (
    <Card>
      <CardHeader>
        <CardHeading>
          <CardTitle>Payment history</CardTitle>
          <CardDescription>
            Latest payment attempts and their recorded outcome.
          </CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent className="space-y-4">
        {isError && !payments.length ? (
          <ErrorState
            title="Billing request failed"
            error={error}
            retry={onRetry}
          />
        ) : !payments.length ? (
          <EmptyState
            title="No payments yet"
            description="Successful and failed payment attempts will appear here."
          />
        ) : (
          <>
            <div className="space-y-3 lg:hidden">
              {payments.map((payment) => (
                <div
                  key={payment.id}
                  className="rounded-xl border border-border/80 bg-card p-4"
                >
                  <div className="space-y-3">
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Payment
                      </p>
                      <p className="mt-1.5 font-medium text-mono">
                        {payment.externalPaymentId || payment.id}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Date
                      </p>
                      <p className="mt-1.5 text-sm">
                        {formatDateTime(
                          payment.paidAtUtc || payment.createdDateUtc,
                          language,
                        )}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Status
                      </p>
                      <div className="mt-1.5">
                        <Badge
                          variant={getPaymentBadgeVariant(payment.status)}
                          appearance="outline"
                        >
                          {billingPaymentStatusLabels[payment.status]}
                        </Badge>
                      </div>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Amount
                      </p>
                      <p className="mt-1.5 text-sm font-medium">
                        {formatMoney(payment.amountMinor, payment.currency, language)}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        Method
                      </p>
                      <p className="mt-1.5 text-sm">
                        {payment.method || 'Not available'}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <div className="hidden overflow-hidden rounded-xl border border-border lg:block">
              <Table>
                <TableHeader className="bg-muted/50">
                  <TableRow>
                    <TableHead>Payment</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Method</TableHead>
                    <TableHead className="text-right">Amount</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {payments.map((payment) => (
                    <TableRow key={payment.id}>
                      <TableCell className="font-medium text-mono">
                        {payment.externalPaymentId || payment.id}
                      </TableCell>
                      <TableCell>
                        {formatDateTime(
                          payment.paidAtUtc || payment.createdDateUtc,
                          language,
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={getPaymentBadgeVariant(payment.status)}
                          appearance="outline"
                        >
                          {billingPaymentStatusLabels[payment.status]}
                        </Badge>
                      </TableCell>
                      <TableCell>{payment.method || 'Not available'}</TableCell>
                      <TableCell className="text-right">
                        {formatMoney(payment.amountMinor, payment.currency, language)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

export function BillingPage() {
  const { language } = usePortalI18n();
  const { currentTenant, isLoading: isTenantLoading } = useTenant();
  const canManageBilling = usePermission('billing.manage');
  const {
    summaryQuery,
    subscriptionQuery,
    invoicesQuery,
    paymentsQuery,
  } = useBillingWorkspace();

  const showLoadingState =
    isTenantLoading ||
    (!currentTenant
      ? false
      : ((summaryQuery.isLoading && summaryQuery.data === undefined) ||
          (subscriptionQuery.isLoading && subscriptionQuery.data === undefined) ||
          (invoicesQuery.isLoading && invoicesQuery.data === undefined) ||
          (paymentsQuery.isLoading && paymentsQuery.data === undefined)));

  if (showLoadingState) {
    return <BillingPageSkeleton />;
  }

  if (!currentTenant) {
    return (
      <PageSurface className="space-y-5 lg:space-y-7.5">
        <PageHeader
          title="Billing"
          description="See plan details, invoices, and recent payments for this workspace."
        />
        <EmptyState
          title="No workspace selected"
          description="Select a workspace to load its billing details."
        />
      </PageSurface>
    );
  }

  if (
    (summaryQuery.isError && summaryQuery.data === undefined) ||
    (subscriptionQuery.isError && subscriptionQuery.data === undefined)
  ) {
    return (
      <PageSurface className="space-y-5 lg:space-y-7.5">
        <PageHeader
          title="Billing"
          description="See plan details, invoices, and recent payments for this workspace."
        />
        <ErrorState
          title="Billing request failed"
          error={summaryQuery.error || subscriptionQuery.error}
          retry={() => {
            void Promise.all([
              summaryQuery.refetch(),
              subscriptionQuery.refetch(),
              invoicesQuery.refetch(),
              paymentsQuery.refetch(),
            ]);
          }}
        />
      </PageSurface>
    );
  }

  const summary = summaryQuery.data;
  const subscription = subscriptionQuery.data;
  const invoices = invoicesQuery.data?.items ?? [];
  const payments = paymentsQuery.data?.items ?? [];
  const latestInvoice = summary?.lastInvoice;

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Billing"
        description="See plan details, invoices, and recent payments for this workspace."
      />

      <SectionGrid
        items={[
          {
            title: 'Current plan',
            value:
              summary?.currentPlanCode ||
              tenantEditionLabels[currentTenant.edition],
            description: currentTenant.slug,
            icon: CreditCard,
          },
          {
            title: 'Subscription status',
            value: tenantSubscriptionStatusLabels[
              subscription?.status ?? summary?.subscriptionStatus ?? 0
            ],
            description: billingProviderLabels[
              subscription?.defaultProvider ?? summary?.defaultProvider ?? 0
            ],
            icon: ShieldCheck,
          },
          {
            title: 'Invoices shown',
            value: invoicesQuery.data?.totalCount ?? 0,
            description: 'Latest records sorted by billing activity',
            icon: Receipt,
          },
          {
            title: 'Payments shown',
            value: paymentsQuery.data?.totalCount ?? 0,
            description: canManageBilling
              ? 'Visible for workspace owners'
              : 'Visible in read-only mode',
            icon: Wallet,
          },
        ]}
      />

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <Card>
          <CardHeader className="gap-4 md:flex-row md:items-start md:justify-between">
            <CardHeading>
              <CardTitle>Subscription overview</CardTitle>
              <CardDescription>
                Live subscription state for the current workspace.
              </CardDescription>
            </CardHeading>
            <Badge
              variant={getSubscriptionBadgeVariant(
                subscription?.status ?? summary?.subscriptionStatus ?? 0,
              )}
              appearance="outline"
            >
              {
                tenantSubscriptionStatusLabels[
                  subscription?.status ?? summary?.subscriptionStatus ?? 0
                ]
              }
            </Badge>
          </CardHeader>
          <CardContent className="space-y-4">
            <KeyValueList
              items={[
                {
                  label: 'Workspace',
                  value: currentTenant.name,
                },
                {
                  label: 'Plan',
                  value:
                    subscription?.planCode ||
                    summary?.currentPlanCode ||
                    'Not available',
                },
                {
                  label: 'Billing interval',
                  value: billingIntervalLabels[subscription?.billingInterval ?? 0],
                },
                {
                  label: 'Current period',
                  value: formatPeriod(
                    subscription?.currentPeriodStartUtc ||
                      summary?.currentPeriodStartUtc,
                    subscription?.currentPeriodEndUtc ||
                      summary?.currentPeriodEndUtc,
                    language,
                  ),
                },
                {
                  label: 'Grace period',
                  value: summary?.graceUntilUtc
                    ? formatDateTime(summary.graceUntilUtc, language)
                    : 'Not in grace period',
                },
                {
                  label: 'Provider',
                  value:
                    billingProviderLabels[
                      subscription?.defaultProvider ??
                        summary?.defaultProvider ??
                        0
                    ],
                },
                {
                  label: 'Provider subscriptions',
                  value: subscription?.providerSubscriptions.length ?? 0,
                },
              ]}
            />

            <div className="flex flex-wrap gap-3">
              <Button
                asChild
                variant="outline"
                disabled={!canManageBilling || !latestInvoice?.hostedUrl}
              >
                <a
                  href={latestInvoice?.hostedUrl || '#'}
                  target="_blank"
                  rel="noreferrer"
                >
                  {translateText('Open latest invoice')}
                  <ArrowUpRight className="size-4" />
                </a>
              </Button>
              <Button
                asChild
                variant="outline"
                disabled={!canManageBilling || !latestInvoice?.pdfUrl}
              >
                <a
                  href={latestInvoice?.pdfUrl || '#'}
                  target="_blank"
                  rel="noreferrer"
                >
                  Download invoice PDF
                  <Download className="size-4" />
                </a>
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>Entitlement snapshot</CardTitle>
              <CardDescription>
                Current access derived from billing events.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            {summary?.entitlement ? (
              <KeyValueList
                items={[
                  {
                    label: 'Plan',
                    value: summary.entitlement.planCode || 'Not available',
                  },
                  {
                    label: 'Status',
                    value:
                      tenantSubscriptionStatusLabels[
                        summary.entitlement.subscriptionStatus
                      ],
                  },
                  {
                    label: 'Active',
                    value: summary.entitlement.isActive ? 'Yes' : 'No',
                  },
                  {
                    label: 'In grace period',
                    value: summary.entitlement.isInGracePeriod ? 'Yes' : 'No',
                  },
                  {
                    label: 'Effective until',
                    value: summary.entitlement.effectiveUntilUtc
                      ? formatDateTime(
                          summary.entitlement.effectiveUntilUtc,
                          language,
                        )
                      : 'Not available',
                  },
                  {
                    label: 'Updated',
                    value: summary.entitlement.updatedAtUtc
                      ? formatDateTime(summary.entitlement.updatedAtUtc, language)
                      : 'Not available',
                  },
                ]}
              />
            ) : (
              <EmptyState
                title="No entitlement snapshot yet"
                description="The billing worker has not produced an entitlement snapshot for this workspace yet."
              />
            )}
          </CardContent>
        </Card>
      </div>

      <InvoiceHistoryCard
        invoices={invoices}
        isError={invoicesQuery.isError}
        error={invoicesQuery.error}
        onRetry={() => {
          void invoicesQuery.refetch();
        }}
        language={language}
      />

      <PaymentHistoryCard
        payments={payments}
        isError={paymentsQuery.isError}
        error={paymentsQuery.error}
        onRetry={() => {
          void paymentsQuery.refetch();
        }}
        language={language}
      />
    </PageSurface>
  );
}
