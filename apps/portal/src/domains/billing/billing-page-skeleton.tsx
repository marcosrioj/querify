import { PageHeader, PageSurface } from '@/shared/layout/page-layouts';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  KeyValueListSkeleton,
  SectionGridSkeleton,
  TableCardSkeleton,
} from '@/shared/ui';

export function BillingPageSkeleton() {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Billing"
        description="See plan details, invoices, and recent payments for this workspace."
      />

      <SectionGridSkeleton />

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>Subscription overview</CardTitle>
              <CardDescription>
                Live subscription state for the current workspace.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <KeyValueListSkeleton items={7} />
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
            <KeyValueListSkeleton items={5} />
          </CardContent>
        </Card>
      </div>

      <TableCardSkeleton
        title="Invoice history"
        description="Latest invoices recorded for this workspace."
        columns={5}
        rows={5}
      />

      <TableCardSkeleton
        title="Payment history"
        description="Latest payment attempts and results."
        columns={5}
        rows={5}
      />
    </PageSurface>
  );
}
