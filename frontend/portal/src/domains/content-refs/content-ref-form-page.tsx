import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useContentRef, useCreateContentRef, useUpdateContentRef } from '@/domains/content-refs/hooks';
import { contentRefFormSchema, type ContentRefFormValues } from '@/domains/content-refs/schemas';
import {
  ContentRefKind,
  contentRefKindLabels,
} from '@/shared/constants/backend-enums';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle, Form } from '@/shared/ui';
import { ErrorState } from '@/shared/ui/placeholder-state';
import { SelectField, TextField } from '@/shared/ui/form-fields';

export function ContentRefFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const contentRefQuery = useContentRef(mode === 'edit' ? id : undefined);
  const createContentRef = useCreateContentRef();
  const updateContentRef = useUpdateContentRef(id ?? '');

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
          description="These fields map directly to the real Content Ref DTOs."
          backTo={mode === 'edit' && id ? `/app/content-refs/${id}` : '/app/content-refs'}
        />
      }
      sidebar={
        <Card>
          <CardHeader>
            <CardTitle>Contract notes</CardTitle>
            <CardDescription>
              Content Refs are reusable tenant assets for FAQ workflows.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <KeyValueList
              items={[
                { label: 'Create route', value: 'POST /api/faqs/content-ref' },
                { label: 'Update route', value: 'PUT /api/faqs/content-ref/{id}' },
                { label: 'Kinds', value: 'Web, Pdf, Document, Video, Repository, Manual' },
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
            <CardTitle>{mode === 'create' ? 'New content ref' : 'Content ref settings'}</CardTitle>
            <CardDescription>
              Keep locators durable. FAQ generation depends on processable content ref kinds for ingestion.
            </CardDescription>
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
                    navigate(`/app/content-refs/${createdId}`);
                    return;
                  }

                  await updateContentRef.mutateAsync(body);
                  navigate(`/app/content-refs/${id}`);
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
                    <Link
                      to={
                        mode === 'edit' && id ? `/app/content-refs/${id}` : '/app/content-refs'
                      }
                    >
                      Cancel
                    </Link>
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
