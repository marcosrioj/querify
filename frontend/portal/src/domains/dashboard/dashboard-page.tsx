import { Link } from 'react-router-dom';
import { useCurrentWorkspace, useTenantWorkspace } from '@/domains/tenants/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { useFaqList } from '@/domains/faq/hooks';
import { useAuth } from '@/platform/auth/auth-context';
import { PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';
import { tenantEditionLabels } from '@/shared/constants/backend-enums';

export function DashboardPage() {
  const { user } = useAuth();
  const currentWorkspace = useCurrentWorkspace();
  const { aiProvidersQuery, clientKeyQuery } = useTenantWorkspace();
  const faqQuery = useFaqList({ page: 1, pageSize: 1, sorting: 'UpdatedDate DESC' });
  const faqItemQuery = useFaqItemList({
    page: 1,
    pageSize: 1,
    sorting: 'UpdatedDate DESC',
  });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 1,
    sorting: 'UpdatedDate DESC',
  });

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Dashboard"
        title={`Welcome back${user?.name ? `, ${user.name}` : ''}`}
        description="This dashboard uses only the confirmed Portal-side contracts that exist in the repository today. It shows workspace context, plan posture, and AI readiness without pulling in BackOffice concerns."
        actions={
          <Button asChild>
            <Link to="/app/faq/new">Create FAQ</Link>
          </Button>
        }
      />

      <SectionGrid
        items={[
          {
            title: 'Current workspace',
            value: currentWorkspace?.name ?? 'Not configured',
            description: currentWorkspace?.slug ?? 'Create a tenant workspace in Settings',
          },
          {
            title: 'Edition',
            value: currentWorkspace
              ? tenantEditionLabels[currentWorkspace.edition]
              : 'Unknown',
            description: 'Pulled from the Tenant Portal API tenant summary',
          },
          {
            title: 'FAQ products',
            value: faqQuery.data?.totalCount ?? 0,
            description: 'Live count from the FAQ Portal API',
          },
          {
            title: 'FAQ items',
            value: faqItemQuery.data?.totalCount ?? 0,
            description: 'Live count from the FAQ Item Portal API',
          },
          {
            title: 'Content refs',
            value: contentRefQuery.data?.totalCount ?? 0,
            description: 'Live count from the Content Ref Portal API',
          },
          {
            title: 'AI providers',
            value: aiProvidersQuery.data?.length ?? 0,
            description: clientKeyQuery.data
              ? 'Providers configured and client key ready'
              : 'Providers configured but no public client key yet',
          },
        ]}
      />

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <Card>
          <CardHeader>
            <CardTitle>Recommended next actions</CardTitle>
            <CardDescription>
              These links reflect the Portal product surface, not internal
              BackOffice responsibilities.
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3 md:grid-cols-2">
            <Button asChild variant="outline" className="justify-start">
              <Link to="/app/settings/tenant">Review tenant setup</Link>
            </Button>
            <Button asChild variant="outline" className="justify-start">
              <Link to="/app/faq">Open FAQ catalog</Link>
            </Button>
            <Button asChild variant="outline" className="justify-start">
              <Link to="/app/faq-items">Manage FAQ items</Link>
            </Button>
            <Button asChild variant="outline" className="justify-start">
              <Link to="/app/content-refs">Open content refs</Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Operational notes</CardTitle>
            <CardDescription>
              Portal gaps still visible in the current backend.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3 text-sm leading-6 text-muted-foreground">
            <p>No Portal members API exists yet, so member management stays behind a temporary adapter.</p>
            <p>No Portal billing or invoice API exists yet, so billing is limited to plan visibility scaffolding.</p>
            <p>AI generation can be requested through the FAQ API, but there is no jobs/status listing API yet.</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
