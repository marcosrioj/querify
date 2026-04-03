import { Pencil, Trash2 } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useFaqList } from '@/domains/faq/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { useDeleteFaqItem, useFaqItem } from '@/domains/faq-items/hooks';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Badge, Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';

export function FaqItemDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const itemQuery = useFaqItem(id);
  const faqOptionsQuery = useFaqList({ page: 1, pageSize: 100, sorting: 'Name ASC' });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: 'Label ASC',
  });
  const deleteFaqItem = useDeleteFaqItem();

  const faqName = faqOptionsQuery.data?.items.find((faq) => faq.id === itemQuery.data?.faqId)?.name;
  const contentRefLabel = contentRefQuery.data?.items.find(
    (contentRef) => contentRef.id === itemQuery.data?.contentRefId,
  );

  if (!id) {
    return (
      <ErrorState
        title="Invalid FAQ item route"
        description="FAQ item detail routes require an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="FAQ Items"
          title={itemQuery.data?.question ?? 'FAQ item detail'}
          description="This detail screen is backed by the live FAQ Item endpoint."
          backTo="/app/faq-items"
          actions={
            <>
              <Button asChild variant="outline">
                <Link to={`/app/faq-items/${id}/edit`}>
                  <Pencil className="size-4" />
                  Edit
                </Link>
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  if (itemQuery.data && window.confirm(`Delete FAQ item "${itemQuery.data.question}"?`)) {
                    void deleteFaqItem.mutateAsync(id).then(() => navigate('/app/faq-items'));
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
            <CardDescription>Direct fields from the backend DTO.</CardDescription>
          </CardHeader>
          <CardContent>
            {itemQuery.data ? (
              <KeyValueList
                items={[
                  {
                    label: 'Status',
                    value: (
                      <Badge variant={itemQuery.data.isActive ? 'success' : 'mono'}>
                        {itemQuery.data.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    ),
                  },
                  { label: 'FAQ', value: faqName ?? itemQuery.data.faqId },
                  {
                    label: 'Content ref',
                    value:
                      contentRefLabel?.label || contentRefLabel?.locator || 'None linked',
                  },
                  { label: 'Sort', value: String(itemQuery.data.sort) },
                  { label: 'Vote score', value: String(itemQuery.data.voteScore) },
                  {
                    label: 'AI confidence',
                    value: String(itemQuery.data.aiConfidenceScore),
                  },
                ]}
              />
            ) : null}
          </CardContent>
        </Card>
      }
    >
      {itemQuery.isError ? (
        <ErrorState
          title="Unable to load FAQ item"
          description="The FAQ item detail request failed."
          retry={() => void itemQuery.refetch()}
        />
      ) : itemQuery.data ? (
        <>
          <Card>
            <CardHeader>
              <CardTitle>Answer content</CardTitle>
              <CardDescription>
                `answer`, `additionalInfo`, and CTA fields are stored directly on the FAQ Item entity.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  Short answer
                </p>
                <p className="mt-2 text-sm leading-6">{itemQuery.data.shortAnswer}</p>
              </div>
              {itemQuery.data.answer ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    Full answer
                  </p>
                  <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                    {itemQuery.data.answer}
                  </p>
                </div>
              ) : null}
              {itemQuery.data.additionalInfo ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    Additional info
                  </p>
                  <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                    {itemQuery.data.additionalInfo}
                  </p>
                </div>
              ) : null}
              {itemQuery.data.ctaTitle || itemQuery.data.ctaUrl ? (
                <div className="rounded-2xl border border-border p-4">
                  <p className="font-medium text-mono">
                    {itemQuery.data.ctaTitle || 'CTA'}
                  </p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {itemQuery.data.ctaUrl || 'No URL configured'}
                  </p>
                </div>
              ) : (
                <EmptyState
                  title="No CTA configured"
                  description="Add a CTA title and URL if this answer should drive an external action."
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : (
        <Card>
          <CardContent className="py-8 text-sm text-muted-foreground">
            Loading FAQ item…
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
