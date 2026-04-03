import { Pencil, Plus, Trash2, WandSparkles } from 'lucide-react';
import { useMemo } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useDeleteFaq, useFaq, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { DetailLayout, KeyValueList, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle, Badge } from '@/shared/ui';
import { ErrorState, EmptyState } from '@/shared/ui/placeholder-state';
import { ContentRefKindBadge, FaqStatusBadge, SortStrategyBadge } from '@/shared/ui/status-badges';

export function FaqDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const faqQuery = useFaq(id);
  const faqItemQuery = useFaqItemList({
    page: 1,
    pageSize: 100,
    sorting: 'Question ASC',
    faqId: id,
  });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: 'Label ASC',
    faqId: id,
  });
  const deleteFaq = useDeleteFaq();
  const requestGeneration = useRequestFaqGeneration();

  const relatedItems = faqItemQuery.data?.items ?? [];
  const relatedContentRefs = useMemo(() => {
    const usageByContentRef = new Map<string, number>();

    relatedItems.forEach((item) => {
      if (!item.contentRefId) {
        return;
      }

      usageByContentRef.set(
        item.contentRefId,
        (usageByContentRef.get(item.contentRefId) ?? 0) + 1,
      );
    });

    return (contentRefQuery.data?.items ?? [])
      .map((contentRef) => ({
        ...contentRef,
        usageCount: usageByContentRef.get(contentRef.id) ?? 0,
      }));
  }, [contentRefQuery.data?.items, relatedItems]);

  const activeItemCount = relatedItems.filter((item) => item.isActive).length;
  const generationReady =
    relatedItems.length > 0 && relatedContentRefs.length > 0;
  const createFaqItemPath = `/app/faq/${id}/items/new`;
  const createContentRefPath = `/app/faq/${id}/content-refs/new`;

  if (!id) {
    return (
      <ErrorState
        title="Invalid FAQ route"
        description="FAQ detail routes require an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="FAQ"
          title={faqQuery.data?.name ?? 'FAQ detail'}
          description="The detail view uses the live FAQ endpoint with API-filtered answer and source lists scoped to this FAQ."
          backTo="/app/faq"
          actions={
            <>
              <Button asChild>
                <Link to={createFaqItemPath}>
                  <Plus className="size-4" />
                  Add FAQ item
                </Link>
              </Button>
              <Button asChild variant="outline">
                <Link to={createContentRefPath}>
                  <Plus className="size-4" />
                  Add content ref
                </Link>
              </Button>
              <Button
                variant="outline"
                onClick={() => {
                  void requestGeneration.mutateAsync(id);
                }}
              >
                <WandSparkles className="size-4" />
                Request generation
              </Button>
              <Button asChild variant="outline">
                <Link to={`/app/faq/${id}/edit`}>
                  <Pencil className="size-4" />
                  Edit
                </Link>
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  if (faqQuery.data && window.confirm(`Delete FAQ "${faqQuery.data.name}"?`)) {
                    void deleteFaq.mutateAsync(id).then(() => navigate('/app/faq'));
                  }
                }}
              >
                <Trash2 className="size-4" />
                Delete
              </Button>
            </>
          }
        />
      }
      sidebar={
        <Card>
          <CardHeader>
            <CardTitle>FAQ settings</CardTitle>
            <CardDescription>Direct mapping from the real DTO.</CardDescription>
          </CardHeader>
          <CardContent>
            {faqQuery.data ? (
              <KeyValueList
                items={[
                  { label: 'Status', value: <FaqStatusBadge status={faqQuery.data.status} /> },
                  {
                    label: 'Sort strategy',
                    value: <SortStrategyBadge value={faqQuery.data.sortStrategy} />,
                  },
                  { label: 'Language', value: faqQuery.data.language },
                  { label: 'CTA', value: faqQuery.data.ctaEnabled ? 'Enabled' : 'Disabled' },
                  {
                    label: 'Related FAQ items',
                    value: String(relatedItems.length),
                  },
                  {
                    label: 'Linked content refs',
                    value: String(relatedContentRefs.length),
                  },
                ]}
              />
            ) : null}
          </CardContent>
        </Card>
      }
    >
      {faqQuery.isError ? (
        <ErrorState
          title="Unable to load FAQ"
          description="The FAQ detail request failed."
          retry={() => void faqQuery.refetch()}
        />
      ) : faqQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: 'FAQ items',
                value: relatedItems.length,
                description:
                  relatedItems.length === 1 ? '1 answer record linked' : `${relatedItems.length} answer records linked`,
              },
              {
                title: 'Active answers',
                value: activeItemCount,
                description:
                  activeItemCount === relatedItems.length
                    ? 'All linked items are active'
                    : 'Some answers are inactive or still drafts',
              },
              {
                title: 'Source coverage',
                value: relatedContentRefs.length,
                description:
                  relatedContentRefs.length
                    ? 'Unique content refs linked through FAQ items'
                    : 'No content refs linked yet',
              },
              {
                title: 'Generation readiness',
                value: generationReady ? 'Ready' : 'Needs setup',
                description:
                  generationReady
                    ? 'This FAQ has answer records and source material'
                    : 'Add answer records and source material before generation',
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardTitle>Knowledge workflow</CardTitle>
              <CardDescription>
                Manage the FAQ itself, the answer records inside it, and the source
                material those answers depend on from one place.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3 text-sm leading-6 text-muted-foreground">
              <div className="flex flex-wrap gap-2">
                <Badge variant={generationReady ? 'success' : 'warning'}>
                  {generationReady ? 'Ready for generation request' : 'Needs content and answer coverage'}
                </Badge>
                <Badge variant="outline">
                  {faqQuery.data.ctaEnabled ? 'CTA enabled' : 'CTA disabled'}
                </Badge>
              </div>
              <p>The backend validates that the FAQ exists inside the selected tenant context.</p>
              <p>Generation requests succeed only when at least one processable content source is linked through FAQ items.</p>
              <p>There is no jobs/status listing endpoint yet, so the mutation returns only a correlation id.</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Related FAQ items</CardTitle>
              <CardDescription>
                Loaded from the FAQ item endpoint with the current FAQ id applied at the API layer.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedItems.length ? (
                relatedItems.map((item) => (
                  <div
                    key={item.id}
                    className="rounded-2xl border border-border p-4"
                  >
                    <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                      <div>
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="font-medium text-mono">{item.question}</p>
                          <Badge variant={item.isActive ? 'success' : 'mono'}>
                            {item.isActive ? 'Active' : 'Inactive'}
                          </Badge>
                        </div>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {item.shortAnswer}
                        </p>
                        <div className="mt-3 flex flex-wrap gap-2 text-xs text-muted-foreground">
                          <span>Sort {item.sort}</span>
                          <span>Vote {item.voteScore}</span>
                          <span>AI {item.aiConfidenceScore}</span>
                          {item.contentRefId ? (
                            <span>
                              Linked to{' '}
                              <Link
                                className="font-medium text-primary hover:underline"
                                to={`/app/faq/${id}/content-refs/${item.contentRefId}`}
                              >
                                source material
                              </Link>
                            </span>
                          ) : (
                            <span>No content ref linked yet</span>
                          )}
                        </div>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <Button asChild variant="ghost" size="sm">
                          <Link to={`/app/faq/${id}/items/${item.id}/edit`}>Edit</Link>
                        </Button>
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/faq/${id}/items/${item.id}`}>Open</Link>
                        </Button>
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No FAQ items linked"
                  description="Create FAQ items and associate them with this FAQ to populate answer content."
                  action={{ label: 'Create FAQ item', to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Connected content refs</CardTitle>
              <CardDescription>
                These are the source materials currently used by the FAQ items inside this FAQ.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedContentRefs.length ? (
                relatedContentRefs.map((contentRef) => (
                  <div
                    key={contentRef.id}
                    className="rounded-2xl border border-border p-4"
                  >
                    <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                      <div>
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="font-medium text-mono">
                            {contentRef.label || 'Untitled content ref'}
                          </p>
                          <ContentRefKindBadge kind={contentRef.kind} />
                        </div>
                        <p className="mt-1 break-all text-sm text-muted-foreground">
                          {contentRef.locator}
                        </p>
                        <p className="mt-3 text-xs text-muted-foreground">
                          Used by {contentRef.usageCount}{' '}
                          {contentRef.usageCount === 1 ? 'FAQ item' : 'FAQ items'} in this FAQ
                        </p>
                      </div>
                      <Button asChild variant="outline" size="sm">
                        <Link to={`/app/faq/${id}/content-refs/${contentRef.id}`}>Open</Link>
                      </Button>
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No content refs connected"
                  description="Link content refs to FAQ items so generation and curation have real source material."
                  action={{ label: 'Add content ref', to: createContentRefPath }}
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : (
        <Card>
          <CardContent className="py-8 text-sm text-muted-foreground">
            Loading FAQ…
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
