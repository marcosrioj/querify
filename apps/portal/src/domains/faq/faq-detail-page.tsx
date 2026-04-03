import { Pencil, Plus, Trash2, WandSparkles } from 'lucide-react';
import { useMemo } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useDeleteFaq, useFaq, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { FaqStatus } from '@/shared/constants/backend-enums';
import { DetailLayout, KeyValueList, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { useLocalPagination } from '@/shared/lib/use-local-pagination';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { ErrorState, EmptyState } from '@/shared/ui/placeholder-state';
import { ContentRefKindBadge, FaqStatusBadge, SortStrategyBadge } from '@/shared/ui/status-badges';

const DETAIL_PAGE_SIZE_OPTIONS = [5, 10, 20];

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
  const relatedItemsPagination = useLocalPagination({
    items: relatedItems,
    defaultPageSize: DETAIL_PAGE_SIZE_OPTIONS[0],
  });
  const relatedContentRefsPagination = useLocalPagination({
    items: relatedContentRefs,
    defaultPageSize: DETAIL_PAGE_SIZE_OPTIONS[0],
  });
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
          description="Review the answers, sources, and readiness of this knowledge space."
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
            <CardHeading>
              <CardTitle>At a glance</CardTitle>
              <CardDescription>Key publishing and orchestration settings.</CardDescription>
            </CardHeading>
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
                  relatedItems.length === 1 ? '1 answer linked' : `${relatedItems.length} answers linked`,
              },
              {
                title: 'Active answers',
                value: activeItemCount,
                description:
                  activeItemCount === relatedItems.length
                    ? 'Everything in view is active'
                    : 'Some answers still need activation',
              },
              {
                title: 'Source coverage',
                value: relatedContentRefs.length,
                description:
                  relatedContentRefs.length
                    ? 'Sources already connected'
                    : 'No content refs linked yet',
              },
              {
                title: 'Generation readiness',
                value: generationReady ? 'Ready' : 'Needs setup',
                description:
                  generationReady
                    ? 'Answers and sources are in place'
                    : 'Add answers and sources before generating',
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>Operational state</CardTitle>
                <CardDescription>
                  Monitor readiness before you publish or request generation.
                </CardDescription>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex flex-wrap gap-2">
                <Badge variant={generationReady ? 'success' : 'warning'}>
                  {generationReady ? 'Ready for generation request' : 'Needs content and answer coverage'}
                </Badge>
                <Badge variant="outline">
                  {faqQuery.data.ctaEnabled ? 'CTA enabled' : 'CTA disabled'}
                </Badge>
              </div>
              <KeyValueList
                items={[
                  {
                    label: 'Visibility',
                    value:
                      faqQuery.data.status === FaqStatus.Published
                        ? 'Customer-facing'
                        : 'Internal or draft',
                  },
                  {
                    label: 'Generation',
                    value: generationReady ? 'Ready to request' : 'Waiting on setup',
                  },
                  {
                    label: 'Request tracking',
                    value: 'Correlation id only',
                  },
                ]}
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>Related FAQ items</CardTitle>
                <CardDescription>
                  Answers currently attached to this FAQ.
                </CardDescription>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedItems.length ? (
                <>
                  {relatedItemsPagination.pagedItems.map((item) => (
                    <div
                      key={item.id}
                      className="rounded-2xl border border-border bg-muted/15 p-4"
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
                  ))}
                  {relatedItemsPagination.totalCount > DETAIL_PAGE_SIZE_OPTIONS[0] ? (
                    <PaginationControls
                      page={relatedItemsPagination.page}
                      pageSize={relatedItemsPagination.pageSize}
                      totalCount={relatedItemsPagination.totalCount}
                      onPageChange={relatedItemsPagination.setPage}
                      onPageSizeChange={relatedItemsPagination.setPageSize}
                      pageSizeOptions={DETAIL_PAGE_SIZE_OPTIONS}
                    />
                  ) : null}
                </>
              ) : (
                <EmptyState
                  title="No FAQ items linked"
                  description="Create answers inside this FAQ to start filling its knowledge coverage."
                  action={{ label: 'Create FAQ item', to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>Connected content refs</CardTitle>
                <CardDescription>
                  Source material already supporting this FAQ.
                </CardDescription>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedContentRefs.length ? (
                <>
                  {relatedContentRefsPagination.pagedItems.map((contentRef) => (
                    <div
                      key={contentRef.id}
                      className="rounded-2xl border border-border bg-muted/15 p-4"
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
                  ))}
                  {relatedContentRefsPagination.totalCount > DETAIL_PAGE_SIZE_OPTIONS[0] ? (
                    <PaginationControls
                      page={relatedContentRefsPagination.page}
                      pageSize={relatedContentRefsPagination.pageSize}
                      totalCount={relatedContentRefsPagination.totalCount}
                      onPageChange={relatedContentRefsPagination.setPage}
                      onPageSizeChange={relatedContentRefsPagination.setPageSize}
                      pageSizeOptions={DETAIL_PAGE_SIZE_OPTIONS}
                    />
                  ) : null}
                </>
              ) : (
                <EmptyState
                  title="No content refs connected"
                  description="Attach source material to the answers in this FAQ so generation has something to work from."
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
