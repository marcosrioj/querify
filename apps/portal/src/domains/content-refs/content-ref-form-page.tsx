import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useContentRef, useCreateContentRef, useUpdateContentRef } from '@/domains/content-refs/hooks';
import { contentRefFormSchema, type ContentRefFormValues } from '@/domains/content-refs/schemas';
import {
  ContentRefKind,
  contentRefKindLabels,
} from '@/shared/constants/backend-enums';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Form } from '@/shared/ui';
import { ErrorState } from '@/shared/ui/placeholder-state';
import { SelectField, TextField } from '@/shared/ui/form-fields';

export function ContentRefFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id: faqId, contentRefId } = useParams();
  const [searchParams] = useSearchParams();
  const originatingFaqItemId = searchParams.get('faqItemId') ?? '';
  const resolvedContentRefId = contentRefId;
  const contentRefQuery = useContentRef(mode === 'edit' ? resolvedContentRefId : undefined);
  const createContentRef = useCreateContentRef();
  const updateContentRef = useUpdateContentRef(resolvedContentRefId ?? '');
  const backTo = faqId ? `/app/faq/${faqId}` : '/app/faq';
  const detailPath =
    mode === 'edit' && faqId && resolvedContentRefId
      ? `/app/faq/${faqId}/content-refs/${resolvedContentRefId}`
      : backTo;
  const buildDetailPath = (nextContentRefId: string) =>
    faqId
      ? `/app/faq/${faqId}/content-refs/${nextContentRefId}${originatingFaqItemId ? `?faqItemId=${originatingFaqItemId}` : ''}`
      : '/app/faq';

  const form = useForm<ContentRefFormValues>({
    resolver: zodResolver(contentRefFormSchema),
    defaultValues: {
      kind: ContentRefKind.Web,
      locator: '',
      label: '',
      scope: '',
    },
  });

  useEffect(() => {
    if (!contentRefQuery.data) {
      return;
    }

    form.reset({
      kind: contentRefQuery.data.kind,
      locator: contentRefQuery.data.locator,
      label: contentRefQuery.data.label ?? '',
      scope: contentRefQuery.data.scope ?? '',
    });
  }, [contentRefQuery.data, form]);

  const isSubmitting = createContentRef.isPending || updateContentRef.isPending;

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="Content Refs"
          title={mode === 'create' ? 'Create content ref' : 'Edit content ref'}
          description="Capture reusable source material for answers and generation."
          backTo={detailPath}
        />
      }
      sidebar={
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>Quick notes</CardTitle>
              <CardDescription>
                Good source records stay durable, labeled, and easy to reuse.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <KeyValueList
              items={[
                { label: 'Kinds', value: 'Web, PDF, document, video, repository, manual' },
                { label: 'Locator', value: 'Use a stable URI or file path reference' },
                { label: 'Scope', value: 'Optional grouping label for the workspace' },
              ]}
            />
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
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{mode === 'create' ? 'New content ref' : 'Content ref settings'}</CardTitle>
              <CardDescription>
                Make this source easy to identify, reuse, and ingest later.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form
                className="space-y-4"
                onSubmit={form.handleSubmit(async (values) => {
                  const body = {
                    ...values,
                    kind: Number(values.kind) as ContentRefKind,
                    label: values.label || undefined,
                    scope: values.scope || undefined,
                  };

                  if (mode === 'create') {
                    const createdId = await createContentRef.mutateAsync(body);
                    navigate(buildDetailPath(createdId));
                    return;
                  }

                  await updateContentRef.mutateAsync(body);
                  navigate(detailPath);
                })}
              >
                <SelectField
                  control={form.control}
                  name="kind"
                  label="Kind"
                  options={Object.entries(contentRefKindLabels).map(([value, label]) => ({
                    value,
                    label,
                  }))}
                />
                <TextField
                  control={form.control}
                  name="locator"
                  label="Locator"
                  description="Examples: URL, PDF path, repository URI, or document locator"
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField control={form.control} name="label" label="Label" />
                  <TextField control={form.control} name="scope" label="Scope" />
                </div>
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {mode === 'create' ? 'Create content ref' : 'Save changes'}
                  </Button>
                  <Button asChild variant="outline">
                    <Link to={detailPath}>Cancel</Link>
                  </Button>
                </div>
              </form>
            </Form>
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
