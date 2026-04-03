import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useFaqList } from '@/domains/faq/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { useCreateFaqItem, useFaqItem, useUpdateFaqItem } from '@/domains/faq-items/hooks';
import { faqItemFormSchema, type FaqItemFormValues } from '@/domains/faq-items/schemas';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Form } from '@/shared/ui';
import { ErrorState } from '@/shared/ui/placeholder-state';
import { SelectField, SwitchField, TextField, TextareaField } from '@/shared/ui/form-fields';

export function FaqItemFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id: faqId, itemId } = useParams();
  const [searchParams] = useSearchParams();
  const resolvedItemId = itemId;
  const preselectedFaqId = faqId ?? searchParams.get('faqId') ?? '';
  const preselectedContentRefId = searchParams.get('contentRefId') ?? '';
  const itemQuery = useFaqItem(mode === 'edit' ? resolvedItemId : undefined);
  const faqOptionsQuery = useFaqList({ page: 1, pageSize: 100, sorting: 'Name ASC' });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: 'Label ASC',
  });
  const createFaqItem = useCreateFaqItem();
  const updateFaqItem = useUpdateFaqItem(resolvedItemId ?? '');

  const form = useForm<FaqItemFormValues>({
    resolver: zodResolver(faqItemFormSchema),
    defaultValues: {
      question: '',
      shortAnswer: '',
      answer: '',
      additionalInfo: '',
      ctaTitle: '',
      ctaUrl: '',
      sort: 10,
      voteScore: 0,
      aiConfidenceScore: 0,
      isActive: true,
      faqId: preselectedFaqId,
      contentRefId: preselectedContentRefId,
    },
  });

  useEffect(() => {
    if (!itemQuery.data) {
      return;
    }

    form.reset({
      question: itemQuery.data.question,
      shortAnswer: itemQuery.data.shortAnswer,
      answer: itemQuery.data.answer ?? '',
      additionalInfo: itemQuery.data.additionalInfo ?? '',
      ctaTitle: itemQuery.data.ctaTitle ?? '',
      ctaUrl: itemQuery.data.ctaUrl ?? '',
      sort: itemQuery.data.sort,
      voteScore: itemQuery.data.voteScore,
      aiConfidenceScore: itemQuery.data.aiConfidenceScore,
      isActive: itemQuery.data.isActive,
      faqId: itemQuery.data.faqId,
      contentRefId: preselectedContentRefId || itemQuery.data.contentRefId || '',
    });
  }, [form, itemQuery.data, preselectedContentRefId]);

  const selectedFaq = faqOptionsQuery.data?.items.find(
    (faq) => faq.id === form.watch('faqId'),
  );
  const selectedContentRef = contentRefQuery.data?.items.find(
    (contentRef) => contentRef.id === form.watch('contentRefId'),
  );
  const currentFaqId = form.watch('faqId') || itemQuery.data?.faqId || preselectedFaqId;
  const backTo =
    mode === 'edit' && currentFaqId && resolvedItemId
      ? `/app/faq/${currentFaqId}/items/${resolvedItemId}`
      : currentFaqId
        ? `/app/faq/${currentFaqId}`
        : '/app/faq';

  const isSubmitting = createFaqItem.isPending || updateFaqItem.isPending;

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="FAQ Items"
          title={mode === 'create' ? 'Create FAQ item' : 'Edit FAQ item'}
          description="The FAQ Item DTO is CRUD-heavy and preserved as-is from the backend contract."
          backTo={backTo}
        />
      }
      sidebar={
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>Contract notes</CardTitle>
              <CardDescription>
                This form uses the real FAQ Item request DTO fields without inventing client-only properties.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <KeyValueList
              items={[
                { label: 'Create route', value: 'POST /api/faqs/faq-item' },
                { label: 'Update route', value: 'PUT /api/faqs/faq-item/{id}' },
                { label: 'Associations', value: 'FAQ required, Content Ref optional' },
                {
                  label: 'Selected FAQ',
                  value: selectedFaq?.name || (mode === 'create' ? 'Choose in form' : itemQuery.data?.faqId || 'Loading'),
                },
                {
                  label: 'Selected content ref',
                  value:
                    selectedContentRef?.label ||
                    selectedContentRef?.locator ||
                    'Optional',
                },
              ]}
            />
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
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{mode === 'create' ? 'New FAQ item' : 'FAQ item settings'}</CardTitle>
              <CardDescription>
                Use sort, vote score, and AI confidence exactly as the backend expects them.
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
                    answer: values.answer || undefined,
                    additionalInfo: values.additionalInfo || undefined,
                    ctaTitle: values.ctaTitle || undefined,
                    ctaUrl: values.ctaUrl || undefined,
                    contentRefId: values.contentRefId || undefined,
                  };

                  if (mode === 'create') {
                    const createdId = await createFaqItem.mutateAsync(body);
                    navigate(
                      body.faqId ? `/app/faq/${body.faqId}/items/${createdId}` : '/app/faq',
                    );
                    return;
                  }

                  await updateFaqItem.mutateAsync(body);
                  navigate(
                    body.faqId && resolvedItemId
                      ? `/app/faq/${body.faqId}/items/${resolvedItemId}`
                      : '/app/faq',
                  );
                })}
              >
                <TextField control={form.control} name="question" label="Question" />
                <TextField
                  control={form.control}
                  name="shortAnswer"
                  label="Short answer"
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <SelectField
                    control={form.control}
                    name="faqId"
                    label="FAQ"
                    options={(faqOptionsQuery.data?.items ?? []).map((faq) => ({
                      value: faq.id,
                      label: faq.name,
                    }))}
                  />
                  <SelectField
                    control={form.control}
                    name="contentRefId"
                    label="Content ref"
                    options={[
                      { value: '', label: 'None' },
                      ...(contentRefQuery.data?.items ?? []).map((contentRef) => ({
                        value: contentRef.id,
                        label: contentRef.label || contentRef.locator,
                      })),
                    ]}
                  />
                </div>
                <div className="grid gap-4 md:grid-cols-3">
                  <TextField control={form.control} name="sort" label="Sort" type="number" />
                  <TextField
                    control={form.control}
                    name="voteScore"
                    label="Vote score"
                    type="number"
                  />
                  <TextField
                    control={form.control}
                    name="aiConfidenceScore"
                    label="AI confidence"
                    type="number"
                  />
                </div>
                <TextareaField control={form.control} name="answer" label="Answer" rows={7} />
                <TextareaField
                  control={form.control}
                  name="additionalInfo"
                  label="Additional info"
                  rows={4}
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField control={form.control} name="ctaTitle" label="CTA title" />
                  <TextField control={form.control} name="ctaUrl" label="CTA URL" />
                </div>
                <SwitchField
                  control={form.control}
                  name="isActive"
                  label="Active"
                  description="Inactive FAQ items stay in the dataset but should not be surfaced to end users."
                />
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {mode === 'create' ? 'Create FAQ item' : 'Save changes'}
                  </Button>
                  <Button asChild variant="outline">
                    <Link to={backTo}>Cancel</Link>
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
