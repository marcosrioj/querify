import { RouteObject } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { useFaqList, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { useTenantWorkspace } from '@/domains/tenants/hooks';
import { AiCommandType } from '@/shared/constants/backend-enums';
import { PageHeader, PageSurface } from '@/shared/layout/page-layouts';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';
import { EmptyState } from '@/shared/ui/placeholder-state';

function AiWorkspacePage() {
  const { aiProvidersQuery } = useTenantWorkspace();
  const faqQuery = useFaqList({ page: 1, pageSize: 6, sorting: 'UpdatedDate DESC' });
  const requestGeneration = useRequestFaqGeneration();

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        eyebrow="AI"
        title="AI workspace"
        description="This area stays Portal-facing. It exposes real tenant AI provider configuration data and leaves job tracking as a placeholder until a dedicated API exists."
      />

      <Card>
        <CardHeader>
          <CardTitle>Configured providers</CardTitle>
          <CardDescription>
            Backed by the real Tenant Portal AI provider endpoints.
          </CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {(aiProvidersQuery.data ?? []).map((provider) => (
            <div key={provider.id} className="rounded-2xl border border-border p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-medium text-mono">{provider.provider}</p>
                  <p className="text-sm text-muted-foreground">{provider.model}</p>
                </div>
                <Badge variant="outline">
                  {provider.command === AiCommandType.Generation ? 'Generation' : 'Matching'}
                </Badge>
              </div>
              <p className="mt-3 text-sm text-muted-foreground">
                {provider.isAiProviderKeyConfigured
                  ? 'Credential present'
                  : 'Credential missing'}
              </p>
            </div>
          ))}

          {!aiProvidersQuery.isLoading && !aiProvidersQuery.data?.length ? (
            <EmptyState
              title="No AI providers"
              description="Configure tenant AI providers in Settings before exposing generation controls to users."
            />
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Generation launchpad</CardTitle>
          <CardDescription>
            The backend supports generation requests per FAQ, but not job tracking.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {(faqQuery.data?.items ?? []).map((faq) => (
            <div key={faq.id} className="flex flex-col gap-3 rounded-2xl border border-border p-4 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="font-medium text-mono">{faq.name}</p>
                <p className="text-sm text-muted-foreground">{faq.language}</p>
              </div>
              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => {
                    void requestGeneration.mutateAsync(faq.id);
                  }}
                >
                  Request generation
                </Button>
                <Button asChild variant="ghost">
                  <Link to={`/app/faq/${faq.id}`}>Open FAQ</Link>
                </Button>
              </div>
            </div>
          ))}

          {!faqQuery.isLoading && !faqQuery.data?.items.length ? (
            <EmptyState
              title="No FAQs available for generation"
              description="Create FAQs and associate processable content references before requesting generation."
              action={{ label: 'Create FAQ', to: '/app/faq/new' }}
            />
          ) : null}
        </CardContent>
      </Card>
    </PageSurface>
  );
}

export const AiRoutes: RouteObject[] = [
  {
    path: 'ai',
    element: <AiWorkspacePage />,
    handle: {
      title: 'AI workspace',
      breadcrumb: 'AI',
      navKey: 'ai',
    },
  },
];
