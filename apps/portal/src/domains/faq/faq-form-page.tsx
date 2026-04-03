import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { faqFormSchema, type FaqFormValues } from '@/domains/faq/schemas';
import { useCreateFaq, useFaq, useUpdateFaq } from '@/domains/faq/hooks';
import {
  ctaTargetLabels,
  faqSortStrategyLabels,
  faqStatusLabels,
  CtaTarget,
  FaqSortStrategy,
  FaqStatus,
} from '@/shared/constants/backend-enums';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle, Form } from '@/shared/ui';
import { SelectField, SwitchField, TextField } from '@/shared/ui/form-fields';
import { ErrorState } from '@/shared/ui/placeholder-state';

export function FaqFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const faqQuery = useFaq(mode === 'edit' ? id : undefined);
  const createFaq = useCreateFaq();
  const updateFaq = useUpdateFaq(id ?? '');

  const form = useForm<FaqFormValues>({
    resolver: zodResolver(faqFormSchema),
    defaultValues: {
      name: '',
      language: 'en-US',
      status: FaqStatus.Draft,
      sortStrategy: FaqSortStrategy.Sort,
      ctaEnabled: false,
      ctaTarget: CtaTarget.Self,
    },
  });

  useEffect(() => {
    if (!faqQuery.data) {
      return;
    }

    form.reset({
      name: faqQuery.data.name,
      language: faqQuery.data.language,
      status: faqQuery.data.status,
      sortStrategy: faqQuery.data.sortStrategy,
      ctaEnabled: faqQuery.data.ctaEnabled,
      ctaTarget: faqQuery.data.ctaTarget,
    });
  }, [faqQuery.data, form]);

  const isSubmitting = createFaq.isPending || updateFaq.isPending;

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="FAQ"
          title={mode === 'create' ? 'Create FAQ' : 'Edit FAQ'}
          description="Set the identity, status, and CTA behavior of this knowledge space."
          backTo={mode === 'edit' && id ? `/app/faq/${id}` : '/app/faq'}
        />
      }
      sidebar={
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>Quick notes</CardTitle>
              <CardDescription>
                Keep the FAQ simple, searchable, and ready for publication.
              </CardDescription>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <KeyValueList
              items={[
                { label: 'Language', value: 'Use the locale customers will search in' },
                { label: 'Status', value: 'Draft, published, or archived' },
                { label: 'CTA', value: 'Optional next step for answer cards' },
              ]}
            />
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
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle>{mode === 'create' ? 'New FAQ' : 'FAQ settings'}</CardTitle>
              <CardDescription>
                Define how this knowledge space should behave across the portal.
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
                    status: Number(values.status) as FaqStatus,
                    sortStrategy: Number(values.sortStrategy) as FaqSortStrategy,
                    ctaTarget: Number(values.ctaTarget) as CtaTarget,
                  };

                  if (mode === 'create') {
                    const createdId = await createFaq.mutateAsync(body);
                    navigate(`/app/faq/${createdId}`);
                    return;
                  }

                  await updateFaq.mutateAsync(body);
                  navigate(`/app/faq/${id}`);
                })}
              >
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField control={form.control} name="name" label="FAQ name" />
                  <TextField
                    control={form.control}
                    name="language"
                    label="Language"
                    description="Example: en-US"
                  />
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <SelectField
                    control={form.control}
                    name="status"
                    label="Status"
                    options={Object.entries(faqStatusLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                  <SelectField
                    control={form.control}
                    name="sortStrategy"
                    label="Sort strategy"
                    options={Object.entries(faqSortStrategyLabels).map(
                      ([value, label]) => ({
                        value,
                        label,
                      }),
                    )}
                  />
                </div>
                <SwitchField
                  control={form.control}
                  name="ctaEnabled"
                  label="Enable CTA"
                  description="Controls whether FAQ items can surface CTA links."
                />
                <SelectField
                  control={form.control}
                  name="ctaTarget"
                  label="CTA target"
                  options={Object.entries(ctaTargetLabels).map(([value, label]) => ({
                    value,
                    label,
                  }))}
                />
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {mode === 'create' ? 'Create FAQ' : 'Save changes'}
                  </Button>
                  <Button asChild variant="outline">
                    <Link to={mode === 'edit' && id ? `/app/faq/${id}` : '/app/faq'}>
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
