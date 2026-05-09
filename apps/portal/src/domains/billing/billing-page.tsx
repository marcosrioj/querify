import { useState } from 'react';
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
import { usePortalTimeZone } from '@/domains/settings/settings-hooks';
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
import { PaginationControls } from '@/shared/ui/pagination-controls';
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

function formatDateTime(
  value: string | null | undefined,
  language: string,
  timeZone: string,
) {
  if (!value) {
    return translateText('Not available', undefined, language);
  }

  return new Intl.DateTimeFormat(language, {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone,
  }).format(new Date(value));
}

function formatPeriod(
  startValue: string | null | undefined,
  endValue: string | null | undefined,
  language: string,
  timeZone: string,
) {
  if (!startValue && !endValue) {
    return translateText('Not scheduled', undefined, language);
  }

  if (!startValue) {
    return translateText(
      'Until {value}',
      { value: formatDateTime(endValue, language, timeZone) },
      language,
    );
  }

  if (!endValue) {
    return translateText(
      'From {value}',
      { value: formatDateTime(startValue, language, timeZone) },
      language,
    );
  }

  return `${formatDateTime(startValue, language, timeZone)} - ${formatDateTime(endValue, language, timeZone)}`;
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

function InvoiceActions({
  invoice,
  language,
}: {
  invoice: BillingInvoiceDto;
  language: string;
}) {
  if (!invoice.hostedUrl && !invoice.pdfUrl) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {invoice.hostedUrl ? (
        <Button asChild variant="outline" size="sm">
          <a href={invoice.hostedUrl} target="_blank" rel="noreferrer">
            {translateText('Open', undefined, language)}
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

const BILLING_PAGE_SIZE_OPTIONS = [5, 10, 20];

function InvoiceHistoryCard({
  invoices,
  isError,
  error,
  onRetry,
  language,
  timeZone,
}: {
  invoices: BillingInvoiceDto[];
  isError: boolean;
  error: unknown;
  onRetry: () => void;
  language: string;
  timeZone: string;
}) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const showPagination = invoices.length > 5;
  const start = (page - 1) * pageSize;
  const visibleInvoices = showPagination ? invoices.slice(start, start + pageSize) : invoices;

  return (
    <Card>
      <CardHeader>
        <CardHeading>
          <CardTitle>{translateText('Invoice history', undefined, language)}</CardTitle>
          <CardDescription>
            {translateText(
              'Latest invoices recorded for this workspace.',
              undefined,
              language,
            )}
          </CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent className="space-y-4">
        {isError && !invoices.length ? (
          <ErrorState
            title={translateText('Billing request failed', undefined, language)}
            error={error}
            retry={onRetry}
          />
        ) : !invoices.length ? (
          <EmptyState
            title={translateText('No invoices yet', undefined, language)}
            description={translateText(
              'Invoices will appear here after the first billing cycle is recorded.',
              undefined,
              language,
            )}
          />
        ) : (
          <>
            <div className="space-y-3 lg:hidden">
              {visibleInvoices.map((invoice) => (
                <div
                  key={invoice.id}
                  className="rounded-xl border border-border/80 bg-card p-4"
                >
                  <div className="space-y-3">
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Invoice', undefined, language)}
                      </p>
                      <p className="mt-1.5 font-medium text-mono">
                        {invoice.externalInvoiceId}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Date', undefined, language)}
                      </p>
                      <p className="mt-1.5 text-sm">
                        {formatDateTime(
                          invoice.paidAtUtc ||
                            invoice.dueDateUtc ||
                            invoice.createdDateUtc,
                          language,
                          timeZone,
                        )}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Status', undefined, language)}
                      </p>
                      <div className="mt-1.5">
                        <Badge
                          variant={getInvoiceBadgeVariant(invoice.status)}
                          appearance="outline"
                        >
                          {translateText(
                            billingInvoiceStatusLabels[invoice.status],
                            undefined,
                            language,
                          )}
                        </Badge>
                      </div>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Amount', undefined, language)}
                      </p>
                      <p className="mt-1.5 text-sm font-medium">
                        {formatMoney(invoice.amountMinor, invoice.currency, language)}
                      </p>
                    </div>
                    <InvoiceActions invoice={invoice} language={language} />
                  </div>
                </div>
              ))}
            </div>

            <div className="hidden overflow-hidden rounded-xl border border-border lg:block">
              <Table>
                <TableHeader className="bg-muted/50">
                  <TableRow>
                    <TableHead>{translateText('Invoice', undefined, language)}</TableHead>
                    <TableHead>{translateText('Date', undefined, language)}</TableHead>
                    <TableHead>{translateText('Status', undefined, language)}</TableHead>
                    <TableHead className="text-right">
                      {translateText('Amount', undefined, language)}
                    </TableHead>
                    <TableHead className="text-right">
                      {translateText('Actions', undefined, language)}
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {visibleInvoices.map((invoice) => (
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
                          timeZone,
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={getInvoiceBadgeVariant(invoice.status)}
                          appearance="outline"
                        >
                          {translateText(
                            billingInvoiceStatusLabels[invoice.status],
                            undefined,
                            language,
                          )}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        {formatMoney(invoice.amountMinor, invoice.currency, language)}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end">
                          <InvoiceActions invoice={invoice} language={language} />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>

            {showPagination && (
              <PaginationControls
                page={page}
                pageSize={pageSize}
                totalCount={invoices.length}
                onPageChange={(nextPage) => setPage(nextPage)}
                onPageSizeChange={(nextPageSize) => {
                  setPageSize(nextPageSize);
                  setPage(1);
                }}
                pageSizeOptions={BILLING_PAGE_SIZE_OPTIONS}
              />
            )}
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
  timeZone,
}: {
  payments: BillingPaymentDto[];
  isError: boolean;
  error: unknown;
  onRetry: () => void;
  language: string;
  timeZone: string;
}) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const showPagination = payments.length > 5;
  const start = (page - 1) * pageSize;
  const visiblePayments = showPagination ? payments.slice(start, start + pageSize) : payments;

  return (
    <Card>
      <CardHeader>
        <CardHeading>
          <CardTitle>{translateText('Payment history', undefined, language)}</CardTitle>
          <CardDescription>
            {translateText(
              'Latest payment attempts and results.',
              undefined,
              language,
            )}
          </CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent className="space-y-4">
        {isError && !payments.length ? (
          <ErrorState
            title={translateText('Billing request failed', undefined, language)}
            error={error}
            retry={onRetry}
          />
        ) : !payments.length ? (
          <EmptyState
            title={translateText('No payments yet', undefined, language)}
            description={translateText(
              'Successful and failed payment attempts will appear here.',
              undefined,
              language,
            )}
          />
        ) : (
          <>
            <div className="space-y-3 lg:hidden">
              {visiblePayments.map((payment) => (
                <div
                  key={payment.id}
                  className="rounded-xl border border-border/80 bg-card p-4"
                >
                  <div className="space-y-3">
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Payment', undefined, language)}
                      </p>
                      <p className="mt-1.5 font-medium text-mono">
                        {payment.externalPaymentId || payment.id}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Date', undefined, language)}
                      </p>
                      <p className="mt-1.5 text-sm">
                        {formatDateTime(
                          payment.paidAtUtc || payment.createdDateUtc,
                          language,
                          timeZone,
                        )}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Status', undefined, language)}
                      </p>
                      <div className="mt-1.5">
                        <Badge
                          variant={getPaymentBadgeVariant(payment.status)}
                          appearance="outline"
                        >
                          {translateText(
                            billingPaymentStatusLabels[payment.status],
                            undefined,
                            language,
                          )}
                        </Badge>
                      </div>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Amount', undefined, language)}
                      </p>
                      <p className="mt-1.5 text-sm font-medium">
                        {formatMoney(payment.amountMinor, payment.currency, language)}
                      </p>
                    </div>
                    <div>
                      <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                        {translateText('Method', undefined, language)}
                      </p>
                      <p className="mt-1.5 text-sm">
                        {payment.method || translateText('Not available', undefined, language)}
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
                    <TableHead>{translateText('Payment', undefined, language)}</TableHead>
                    <TableHead>{translateText('Date', undefined, language)}</TableHead>
                    <TableHead>{translateText('Status', undefined, language)}</TableHead>
                    <TableHead>{translateText('Method', undefined, language)}</TableHead>
                    <TableHead className="text-right">
                      {translateText('Amount', undefined, language)}
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {visiblePayments.map((payment) => (
                    <TableRow key={payment.id}>
                      <TableCell className="font-medium text-mono">
                        {payment.externalPaymentId || payment.id}
                      </TableCell>
                      <TableCell>
                        {formatDateTime(
                          payment.paidAtUtc || payment.createdDateUtc,
                          language,
                          timeZone,
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={getPaymentBadgeVariant(payment.status)}
                          appearance="outline"
                        >
                          {translateText(
                            billingPaymentStatusLabels[payment.status],
                            undefined,
                            language,
                          )}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {payment.method || translateText('Not available', undefined, language)}
                      </TableCell>
                      <TableCell className="text-right">
                        {formatMoney(payment.amountMinor, payment.currency, language)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>

            {showPagination && (
              <PaginationControls
                page={page}
                pageSize={pageSize}
                totalCount={payments.length}
                onPageChange={(nextPage) => setPage(nextPage)}
                onPageSizeChange={(nextPageSize) => {
                  setPageSize(nextPageSize);
                  setPage(1);
                }}
                pageSizeOptions={BILLING_PAGE_SIZE_OPTIONS}
              />
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}

export function BillingPage() {
  const { language } = usePortalI18n();
  const timeZone = usePortalTimeZone();
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
          title={translateText('Billing', undefined, language)}
          description={translateText(
            'See plan details, invoices, and recent payments for this workspace.',
            undefined,
            language,
          )}
        />
        <EmptyState
          title={translateText('No workspace selected', undefined, language)}
          description={translateText(
            'Select a workspace to load its billing details.',
            undefined,
            language,
          )}
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
          title={translateText('Billing', undefined, language)}
          description={translateText(
            'See plan details, invoices, and recent payments for this workspace.',
            undefined,
            language,
          )}
        />
        <ErrorState
          title={translateText('Billing request failed', undefined, language)}
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
        title={translateText('Billing', undefined, language)}
        description={translateText(
          'See plan details, invoices, and recent payments for this workspace.',
          undefined,
          language,
        )}
      />

      <SectionGrid
        items={[
          {
            title: translateText('Current plan', undefined, language),
            value:
              summary?.currentPlanCode ||
              translateText(tenantEditionLabels[currentTenant.edition], undefined, language),
            description: currentTenant.slug,
            icon: CreditCard,
          },
          {
            title: translateText('Subscription status', undefined, language),
            value: translateText(
              tenantSubscriptionStatusLabels[
                subscription?.status ?? summary?.subscriptionStatus ?? 0
              ],
              undefined,
              language,
            ),
            description: translateText(
              billingProviderLabels[
                subscription?.defaultProvider ?? summary?.defaultProvider ?? 0
              ],
              undefined,
              language,
            ),
            icon: ShieldCheck,
          },
          {
            title: translateText('Invoices shown', undefined, language),
            value: invoicesQuery.data?.totalCount ?? 0,
            description: translateText(
              'Latest records sorted by billing activity',
              undefined,
              language,
            ),
            icon: Receipt,
          },
          {
            title: translateText('Payments shown', undefined, language),
            value: paymentsQuery.data?.totalCount ?? 0,
            description: canManageBilling
              ? translateText('Visible for workspace owners', undefined, language)
              : translateText('Visible in read-only mode', undefined, language),
            icon: Wallet,
          },
        ]}
      />

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <Card>
          <CardHeader className="gap-4 md:flex-row md:items-start md:justify-between">
            <CardHeading>
              <CardTitle>{translateText('Subscription overview', undefined, language)}</CardTitle>
              <CardDescription>
                {translateText(
                  'Live subscription state for the current workspace.',
                  undefined,
                  language,
                )}
              </CardDescription>
            </CardHeading>
            <Badge
              variant={getSubscriptionBadgeVariant(
                subscription?.status ?? summary?.subscriptionStatus ?? 0,
              )}
              appearance="outline"
            >
              {
                translateText(
                  tenantSubscriptionStatusLabels[
                    subscription?.status ?? summary?.subscriptionStatus ?? 0
                  ],
                  undefined,
                  language,
                )
              }
            </Badge>
          </CardHeader>
          <CardContent className="space-y-4">
            <KeyValueList
              items={[
                {
                  label: translateText('Workspace', undefined, language),
                  value: currentTenant.name,
                },
                {
                  label: translateText('Plan', undefined, language),
                  value:
                    subscription?.planCode ||
                    summary?.currentPlanCode ||
                    translateText('Not available', undefined, language),
                },
                {
                  label: translateText('Billing interval', undefined, language),
                  value: translateText(
                    billingIntervalLabels[subscription?.billingInterval ?? 0],
                    undefined,
                    language,
                  ),
                },
                {
                  label: translateText('Current period', undefined, language),
                  value: formatPeriod(
                    subscription?.currentPeriodStartUtc ||
                      summary?.currentPeriodStartUtc,
                    subscription?.currentPeriodEndUtc ||
                      summary?.currentPeriodEndUtc,
                    language,
                    timeZone,
                  ),
                },
                {
                  label: translateText('Grace period', undefined, language),
                  value: summary?.graceUntilUtc
                    ? formatDateTime(summary.graceUntilUtc, language, timeZone)
                    : translateText('Not in grace period', undefined, language),
                },
                {
                  label: translateText('Provider', undefined, language),
                  value: translateText(
                    billingProviderLabels[
                      subscription?.defaultProvider ??
                        summary?.defaultProvider ??
                        0
                    ],
                    undefined,
                    language,
                  ),
                },
                {
                  label: translateText('Provider subscriptions', undefined, language),
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
                  {translateText('Open latest invoice', undefined, language)}
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
                  {translateText('Download invoice PDF', undefined, language)}
                  <Download className="size-4" />
                </a>
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{translateText('Entitlement snapshot', undefined, language)}</CardTitle>
              <CardDescription>
                {translateText(
                  'Current access derived from billing events.',
                  undefined,
                  language,
                )}
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            {summary?.entitlement ? (
              <KeyValueList
                items={[
                  {
                    label: translateText('Plan', undefined, language),
                    value:
                      summary.entitlement.planCode ||
                      translateText('Not available', undefined, language),
                  },
                  {
                    label: translateText('Status', undefined, language),
                    value: translateText(
                      tenantSubscriptionStatusLabels[
                        summary.entitlement.subscriptionStatus
                      ],
                      undefined,
                      language,
                    ),
                  },
                  {
                    label: translateText('Active', undefined, language),
                    value: summary.entitlement.isActive
                      ? translateText('Yes', undefined, language)
                      : translateText('No', undefined, language),
                  },
                  {
                    label: translateText('In grace period', undefined, language),
                    value: summary.entitlement.isInGracePeriod
                      ? translateText('Yes', undefined, language)
                      : translateText('No', undefined, language),
                  },
                  {
                    label: translateText('Effective until', undefined, language),
                    value: summary.entitlement.effectiveUntilUtc
                      ? formatDateTime(
                          summary.entitlement.effectiveUntilUtc,
                          language,
                          timeZone,
                        )
                      : translateText('Not available', undefined, language),
                  },
                  {
                    label: translateText('Updated', undefined, language),
                    value: summary.entitlement.updatedAtUtc
                      ? formatDateTime(summary.entitlement.updatedAtUtc, language, timeZone)
                      : translateText('Not available', undefined, language),
                  },
                ]}
              />
            ) : (
              <EmptyState
                title={translateText(
                  'No entitlement snapshot yet',
                  undefined,
                  language,
                )}
                description={translateText(
                  'The billing worker has not produced an entitlement snapshot for this workspace yet.',
                  undefined,
                  language,
                )}
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
        timeZone={timeZone}
      />

      <PaymentHistoryCard
        payments={payments}
        isError={paymentsQuery.isError}
        error={paymentsQuery.error}
        onRetry={() => {
          void paymentsQuery.refetch();
        }}
        language={language}
        timeZone={timeZone}
      />
    </PageSurface>
  );
}
