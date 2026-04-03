import { Pencil, Trash2, WandSparkles } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useDeleteFaq, useFaq, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';
import { ErrorState, EmptyState } from '@/shared/ui/placeholder-state';
import { FaqStatusBadge, SortStrategyBadge } from '@/shared/ui/status-badges';

export function FaqDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const faqQuery = useFaq(id);
  const faqItemQuery = useFaqItemList({
    page: 1,
    pageSize: 100,
    sorting: 'Question ASC',
  });
  const deleteFaq = useDeleteFaq();
  const requestGeneration = useRequestFaqGeneration();

  const relatedItems = (faqItemQuery.data?.items ?? []).filter(
    (item) => item.faqId === id,
  );

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
          description="The detail view uses the live FAQ endpoint and cross-references FAQ items client-side on the loaded dataset."
          backTo="/app/faq"
          actions={
            <>
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
          <Card>
            <CardHeader>
              <CardTitle>Generation readiness</CardTitle>
              <CardDescription>
                The backend accepts generation requests at `POST /api/faqs/faq/{'{id}'}/generation-request`.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3 text-sm leading-6 text-muted-foreground">
              <p>The backend validates that the FAQ exists inside the selected tenant context.</p>
              <p>It also requires at least one processable Content Ref association before generation can succeed.</p>
              <p>There is no jobs/status listing endpoint yet, so the mutation returns only a correlation id.</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Related FAQ items</CardTitle>
              <CardDescription>
                Loaded from the real FAQ item list endpoint and filtered client-side by FAQ id.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedItems.length ? (
                relatedItems.map((item) => (
                  <div
                    key={item.id}
                    className="rounded-2xl border border-border p-4"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-medium text-mono">{item.question}</p>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {item.shortAnswer}
                        </p>
                      </div>
                      <Button asChild variant="outline" size="sm">
                        <Link to={`/app/faq-items/${item.id}`}>Open</Link>
                      </Button>
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No FAQ items linked"
                  description="Create FAQ items and associate them with this FAQ to populate answer content."
                  action={{ label: 'Create FAQ item', to: '/app/faq-items/new' }}
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
