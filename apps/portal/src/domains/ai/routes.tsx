import { ArrowUpRight, BookOpen, Bot, ShieldCheck, WandSparkles } from 'lucide-react';
import { RouteObject } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { AiWorkspaceSkeleton } from '@/domains/ai/ai-workspace-skeleton';
import { useFaqList, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { useTenantWorkspace } from '@/domains/tenants/hooks';
import { AiCommandType } from '@/shared/constants/backend-enums';
import { PageHeader, PageSurface, SectionGrid } from '@/shared/layout/page-layouts';
import { translateText } from '@/shared/lib/i18n-core';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, ConfirmAction } from '@/shared/ui';
import { EmptyState } from '@/shared/ui/placeholder-state';

function AiWorkspacePage() {
  const { aiProvidersQuery } = useTenantWorkspace();
  const faqQuery = useFaqList({ page: 1, pageSize: 6, sorting: 'UpdatedDate DESC' });
  const requestGeneration = useRequestFaqGeneration();
  const providers = aiProvidersQuery.data ?? [];
  const generationProviders = providers.filter(
    (provider) => provider.command === AiCommandType.Generation,
  ).length;
  const readyProviders = providers.filter(
    (provider) => provider.isAiProviderKeyConfigured,
  ).length;
  const showLoadingState =
    (aiProvidersQuery.isLoading && aiProvidersQuery.data === undefined) ||
    (faqQuery.isLoading && faqQuery.data === undefined);

  if (showLoadingState) {
    return <AiWorkspaceSkeleton />;
  }

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="AI"
        description="Check providers and run FAQ generation."
      />

      <SectionGrid
        items={[
          {
            title: 'Providers',
            value: providers.length,
            description: providers.length
              ? translateText('{count} ready to use', { count: readyProviders })
              : 'No providers configured',
            icon: Bot,
          },
          {
            title: 'Generation models',
            value: generationProviders,
            description: 'Available for answer generation',
            icon: WandSparkles,
          },
          {
            title: 'Credential coverage',
            value: readyProviders,
            description: providers.length ? 'Providers with stored secrets' : 'Waiting for provider setup',
            icon: ShieldCheck,
          },
          {
            title: 'Launchable FAQs',
            value: faqQuery.data?.items.length ?? 0,
            description: 'Recent knowledge spaces ready for action',
            icon: BookOpen,
          },
        ]}
      />

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Configured providers</CardTitle>
            <CardDescription>
              Review what is ready for matching and generation.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {providers.map((provider) => (
            <div key={provider.id} className="rounded-2xl border border-border bg-muted/15 p-4">
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

          {!aiProvidersQuery.isLoading && !providers.length ? (
            <EmptyState
              title="No AI providers"
              description="Configure providers in settings before launching generation."
            />
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Generation launchpad</CardTitle>
            <CardDescription>
              Trigger generation directly from the latest FAQs.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="space-y-3">
          {(faqQuery.data?.items ?? []).map((faq) => (
            <div key={faq.id} className="flex flex-col gap-3 rounded-2xl border border-border p-4 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="font-medium text-mono">{faq.name}</p>
                <p className="text-sm text-muted-foreground">{faq.language}</p>
              </div>
              <div className="flex items-center gap-2">
                <ConfirmAction
                  title={`Run AI generation for "${faq.name}"?`}
                  description="This queues generation for the FAQ and uses the configured AI provider setup for the current workspace."
                  confirmLabel="Run generation"
                  variant="primary"
                  isPending={requestGeneration.isPending}
                  onConfirm={() => requestGeneration.mutateAsync(faq.id)}
                  trigger={
                    <Button variant="outline">
                      Request generation
                    </Button>
                  }
                />
                <Button asChild variant="ghost">
                  <Link to={`/app/faq/${faq.id}`}>
                    <ArrowUpRight className="size-4" />
                    Open FAQ
                  </Link>
                </Button>
              </div>
            </div>
          ))}

          {!faqQuery.isLoading && !faqQuery.data?.items.length ? (
            <EmptyState
              title="No FAQs available for generation"
              description="Create a FAQ and connect source material before launching generation."
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
      title: 'AI',
      breadcrumb: 'AI',
      navKey: 'ai',
    },
  },
];
