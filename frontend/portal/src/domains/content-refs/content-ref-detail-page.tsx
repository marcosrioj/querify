import { Pencil, Plus, Trash2 } from 'lucide-react';
import { useMemo } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useFaqList } from '@/domains/faq/hooks';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { useContentRef, useDeleteContentRef } from '@/domains/content-refs/hooks';
import { DetailLayout, KeyValueList, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';
import { ContentRefKindBadge } from '@/shared/ui/status-badges';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';

export function ContentRefDetailPage() {
  const navigate = useNavigate();
  const { id: faqId, contentRefId } = useParams();
  const [searchParams] = useSearchParams();
  const resolvedContentRefId = contentRefId;
  const originatingFaqItemId = searchParams.get('faqItemId') ?? '';
  const contentRefQuery = useContentRef(resolvedContentRefId);
  const faqItemQuery = useFaqItemList({
    page: 1,
    pageSize: 100,
    sorting: 'Question ASC',
    faqId,
    contentRefId: resolvedContentRefId,
  });
  const deleteContentRef = useDeleteContentRef();

  const relatedItems = faqItemQuery.data?.items ?? [];
  const relatedFaqIds = useMemo(
    () => Array.from(new Set([...(faqId ? [faqId] : []), ...relatedItems.map((item) => item.faqId)])),
    [faqId, relatedItems],
  );
  const faqQuery = useFaqList({
    page: 1,
    pageSize: Math.max(relatedFaqIds.length, 1),
    sorting: 'Name ASC',
    faqIds: relatedFaqIds.length ? relatedFaqIds : undefined,
  });
  const relatedFaqs = useMemo(() => {
    const usageByFaq = new Map<string, number>();

    relatedItems.forEach((item) => {
      usageByFaq.set(item.faqId, (usageByFaq.get(item.faqId) ?? 0) + 1);
    });

    return (faqQuery.data?.items ?? [])
      .filter((faq) => usageByFaq.has(faq.id))
      .map((faq) => ({
        ...faq,
        usageCount: usageByFaq.get(faq.id) ?? 0,
      }));
  }, [faqQuery.data?.items, relatedItems]);
  const resolvedFaqId = faqId ?? relatedFaqs[0]?.id ?? relatedItems[0]?.faqId;
  const backTo = resolvedFaqId ? `/app/faq/${resolvedFaqId}` : '/app/faq';
  const createFaqItemPath =
    resolvedFaqId && resolvedContentRefId
      ? `/app/faq/${resolvedFaqId}/items/new?contentRefId=${resolvedContentRefId}`
      : '/app/faq';
  const editPath =
    resolvedFaqId && resolvedContentRefId
      ? `/app/faq/${resolvedFaqId}/content-refs/${resolvedContentRefId}/edit`
      : backTo;
  const attachOriginItemPath =
    resolvedFaqId && originatingFaqItemId && resolvedContentRefId
      ? `/app/faq/${resolvedFaqId}/items/${originatingFaqItemId}/edit?contentRefId=${resolvedContentRefId}`
      : '';

  if (!resolvedContentRefId) {
    return (
      <ErrorState
        title="Invalid content ref route"
        description="Content ref detail routes require an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="Content Refs"
          title={contentRefQuery.data?.label || 'Content ref detail'}
          description={contentRefQuery.data?.locator || 'Loading content ref detail'}
          backTo={backTo}
          actions={
            <>
              <Button asChild>
                <Link to={createFaqItemPath}>
                  <Plus className="size-4" />
                  Create FAQ item
                </Link>
              </Button>
              {attachOriginItemPath ? (
                <Button asChild variant="outline">
                  <Link to={attachOriginItemPath}>Attach to FAQ item</Link>
                </Button>
              ) : null}
              <Button asChild variant="outline">
                <Link to={editPath}>
                  <Pencil className="size-4" />
                  Edit
                </Link>
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  if (
                    contentRefQuery.data &&
                    window.confirm(
                      `Delete content ref "${contentRefQuery.data.label || contentRefQuery.data.locator}"?`,
                    )
                  ) {
                    void deleteContentRef.mutateAsync(resolvedContentRefId).then(() => navigate(backTo));
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
            <CardTitle>Metadata</CardTitle>
            <CardDescription>Direct fields from the content ref DTO.</CardDescription>
          </CardHeader>
          <CardContent>
            {contentRefQuery.data ? (
              <KeyValueList
                items={[
                  {
                    label: 'Kind',
                    value: <ContentRefKindBadge kind={contentRefQuery.data.kind} />,
                  },
                  { label: 'Scope', value: contentRefQuery.data.scope || 'No scope' },
                  { label: 'Related FAQs', value: String(relatedFaqs.length) },
                  { label: 'Related FAQ items', value: String(relatedItems.length) },
                ]}
              />
            ) : null}
          </CardContent>
        </Card>
      }
    >
      {contentRefQuery.isError ? (
        <ErrorState
          title="Unable to load content ref"
          description="The content ref detail request failed."
          retry={() => void contentRefQuery.refetch()}
        />
      ) : contentRefQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: 'Type',
                value: <ContentRefKindBadge kind={contentRefQuery.data.kind} />,
                description: 'How this source material is classified',
              },
              {
                title: 'FAQ items using it',
                value: relatedItems.length,
                description:
                  relatedItems.length
                    ? 'Direct answer records linked to this content ref'
                    : 'No answer records linked yet',
              },
              {
                title: 'FAQs affected',
                value: relatedFaqs.length,
                description:
                  relatedFaqs.length
                    ? 'Unique FAQs currently relying on this source'
                    : 'No FAQs currently depend on this source',
              },
              {
                title: 'Scope',
                value: contentRefQuery.data.scope || 'Not set',
                description: 'Optional tenant-side grouping label',
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardTitle>Locator</CardTitle>
              <CardDescription>
                This is the source material pointer stored in the backend.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <p className="break-all text-sm leading-6">{contentRefQuery.data.locator}</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>FAQs using this content ref</CardTitle>
              <CardDescription>
                This shows where the source material is already influencing the knowledge base.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedFaqs.length ? (
                relatedFaqs.map((faq) => (
                  <div key={faq.id} className="rounded-2xl border border-border p-4">
                    <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                      <div>
                        <p className="font-medium text-mono">{faq.name}</p>
                        <p className="mt-1 text-sm text-muted-foreground">
                          Used by {faq.usageCount} linked{' '}
                          {faq.usageCount === 1 ? 'FAQ item' : 'FAQ items'}
                        </p>
                      </div>
                      <Button asChild variant="outline" size="sm">
                        <Link to={`/app/faq/${faq.id}`}>Open FAQ</Link>
                      </Button>
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No FAQs using this content ref"
                  description="Create FAQ items linked to this source material to make it part of a tenant FAQ workflow."
                  action={{ label: 'Create FAQ item', to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>FAQ items using this content ref</CardTitle>
              <CardDescription>
                The FAQ Item API is queried with the current content ref id instead of filtering loaded rows in the browser.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedItems.length ? (
                relatedItems.map((item) => (
                  <div key={item.id} className="rounded-2xl border border-border p-4">
                    <div className="flex items-start justify-between gap-3">
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
                        <p className="mt-3 text-xs text-muted-foreground">
                          FAQ:{' '}
                          <Link
                            className="font-medium text-primary hover:underline"
                            to={`/app/faq/${item.faqId}`}
                          >
                            {relatedFaqs.find((faq) => faq.id === item.faqId)?.name ?? item.faqId}
                          </Link>
                        </p>
                      </div>
                      <Button asChild variant="outline" size="sm">
                        <Link to={`/app/faq/${item.faqId}/items/${item.id}`}>Open</Link>
                      </Button>
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No FAQ items linked"
                  description="Associate this content ref to FAQ items to reuse the source material across answers."
                  action={{ label: 'Create FAQ item', to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : (
        <Card>
          <CardContent className="py-8 text-sm text-muted-foreground">
            Loading content ref…
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
