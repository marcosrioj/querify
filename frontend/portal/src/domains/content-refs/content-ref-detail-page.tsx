import { Pencil, Trash2 } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { useContentRef, useDeleteContentRef } from '@/domains/content-refs/hooks';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';
import { ContentRefKindBadge } from '@/shared/ui/status-badges';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';

export function ContentRefDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const contentRefQuery = useContentRef(id);
  const faqItemQuery = useFaqItemList({ page: 1, pageSize: 100, sorting: 'Question ASC' });
  const deleteContentRef = useDeleteContentRef();

  const relatedItems = (faqItemQuery.data?.items ?? []).filter(
    (item) => item.contentRefId === id,
  );

  if (!id) {
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
          backTo="/app/content-refs"
          actions={
            <>
              <Button asChild variant="outline">
                <Link to={`/app/content-refs/${id}/edit`}>
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
                    void deleteContentRef.mutateAsync(id).then(() => navigate('/app/content-refs'));
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
              <CardTitle>FAQ items using this content ref</CardTitle>
              <CardDescription>
                The FAQ Item API exposes direct `contentRefId` associations.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedItems.length ? (
                relatedItems.map((item) => (
                  <div key={item.id} className="rounded-2xl border border-border p-4">
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
                  description="Associate this content ref to FAQ items to reuse the source material across answers."
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
