import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import {
  qnaProductSurfaceLabels,
  searchMarkupModeLabels,
  spaceKindLabels,
  visibilityScopeLabels,
  QnAProductSurface,
  SearchMarkupMode,
  SpaceKind,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import { useCreateSpace, useSpace, useUpdateSpace } from '@/domains/spaces/hooks';
import { spaceFormSchema, type SpaceFormValues } from '@/domains/spaces/schemas';
import { DetailLayout, KeyValueList, PageHeader } from '@/shared/layout/page-layouts';
import {
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ContextHint,
  Form,
  FormCardSkeleton,
  FormSectionHeading,
  SidebarSummarySkeleton,
} from '@/shared/ui';
import { ErrorState } from '@/shared/ui/placeholder-state';
import {
  SearchSelectField,
  SelectField,
  SwitchField,
  TextField,
  TextareaField,
} from '@/shared/ui/form-fields';
import { translateText } from '@/shared/lib/i18n-core';
import {
  DEFAULT_PORTAL_LANGUAGE,
  getStoredPortalLanguage,
  portalLanguageOptions,
} from '@/shared/lib/language';

export function SpaceFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const spaceQuery = useSpace(mode === 'edit' ? id : undefined);
  const createSpace = useCreateSpace();
  const updateSpace = useUpdateSpace(id ?? '');
  const defaultLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;

  const form = useForm<SpaceFormValues>({
    resolver: zodResolver(spaceFormSchema),
    defaultValues: {
      name: '',
      key: '',
      defaultLanguage,
      summary: '',
      kind: SpaceKind.ControlledPublication,
      productSurface: QnAProductSurface.Publish,
      visibility: VisibilityScope.Internal,
      searchMarkupMode: SearchMarkupMode.CuratedList,
      productScope: '',
      journeyScope: '',
      acceptsQuestions: true,
      acceptsAnswers: true,
      markValidated: false,
    },
  });

  useEffect(() => {
    if (!spaceQuery.data) {
      return;
    }

    form.reset({
      name: spaceQuery.data.name,
      key: spaceQuery.data.key,
      defaultLanguage: spaceQuery.data.defaultLanguage,
      summary: spaceQuery.data.summary ?? '',
      kind: spaceQuery.data.kind,
      productSurface: spaceQuery.data.productSurface,
      visibility: spaceQuery.data.visibility,
      searchMarkupMode: spaceQuery.data.searchMarkupMode,
      productScope: spaceQuery.data.productScope ?? '',
      journeyScope: spaceQuery.data.journeyScope ?? '',
      acceptsQuestions: spaceQuery.data.acceptsQuestions,
      acceptsAnswers: spaceQuery.data.acceptsAnswers,
      markValidated: false,
    });
  }, [form, spaceQuery.data]);

  const languageOptions = portalLanguageOptions.map((option) => ({
    value: option.code,
    label: option.label,
    description: `${option.code} • ${option.direction.toUpperCase()}`,
    keywords: [option.code, option.label, option.direction],
  }));
  const selectedLanguageValue = form.watch('defaultLanguage');
  const selectedLanguageOption =
    languageOptions.find((option) => option.value === selectedLanguageValue) ?? null;
  const isSubmitting = createSpace.isPending || updateSpace.isPending;
  const backTo =
    mode === 'edit' && id ? `/app/spaces/${id}` : '/app/spaces';

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === 'create' ? 'New space' : 'Edit space'}
          description="Define the QnA surface, operating mode, and exposure before threads start accumulating."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === 'edit' && spaceQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText('Quick notes')}</span>
                  <ContextHint
                    content={translateText(
                      'Spaces define the product surface, operating mode, exposure, and how questions and answers behave operationally.',
                    )}
                    label={translateText('Quick notes details')}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  { label: 'Mode', value: 'Controlled, moderated, or public validation' },
                  { label: 'Surface', value: 'Publish, resolve, listen, collaborate, or govern' },
                  { label: 'Visibility', value: 'Internal to public indexed' },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {spaceQuery.isError ? (
        <ErrorState
          title="Unable to load space"
          error={spaceQuery.error}
          retry={() => void spaceQuery.refetch()}
        />
      ) : mode === 'edit' && spaceQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText('Configuration')}</span>
                <ContextHint
                  content={translateText(
                    'Start with the operating model, then decide who can see the space and how submissions move through review.',
                  )}
                  label={translateText('Form details')}
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form
                className="space-y-5"
                onSubmit={form.handleSubmit(async (values) => {
                  const body = {
                    ...values,
                    summary: values.summary || undefined,
                    productScope: values.productScope || undefined,
                    journeyScope: values.journeyScope || undefined,
                    kind: Number(values.kind) as SpaceKind,
                    productSurface: Number(values.productSurface) as QnAProductSurface,
                    visibility: Number(values.visibility) as VisibilityScope,
                    searchMarkupMode:
                      Number(values.searchMarkupMode) as SearchMarkupMode,
                  };

                  if (mode === 'create') {
                    const createdId = await createSpace.mutateAsync(body);
                    navigate(`/app/spaces/${createdId}`);
                    return;
                  }

                  await updateSpace.mutateAsync(body);
                  navigate(`/app/spaces/${id}`);
                })}
              >
                <FormSectionHeading
                  title="Identity"
                  description="Name the space clearly so operators know exactly what knowledge surface they are configuring."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField
                    control={form.control}
                    name="name"
                    label="Name"
                    placeholder="Product support space"
                    description="Use the operational name teammates will recognize."
                  />
                  <TextField
                    control={form.control}
                    name="key"
                    label="Key"
                    placeholder="product-support"
                    description="Use a stable slug-style key for routing and integrations."
                  />
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <SearchSelectField
                    control={form.control}
                    name="defaultLanguage"
                    label="Default language"
                    description="Use the main locale for the questions and answers in this space."
                    options={languageOptions}
                    selectedOption={selectedLanguageOption}
                    searchPlaceholder="Search languages"
                    emptyMessage="No languages found."
                    resultCountHint={translateText('{count} languages available', {
                      count: portalLanguageOptions.length,
                    })}
                  />
                  <SelectField
                    control={form.control}
                    name="kind"
                    label="Operating mode"
                    description="Pick the operating model that best matches how this space should gather and govern answers."
                    options={Object.entries(spaceKindLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                </div>
                <SelectField
                  control={form.control}
                  name="productSurface"
                  label="Product surface"
                  description="Choose the product module that primarily owns this QnA space."
                  options={Object.entries(qnaProductSurfaceLabels).map(([value, label]) => ({
                    value,
                    label,
                  }))}
                />
                <TextareaField
                  control={form.control}
                  name="summary"
                  label="Summary"
                  rows={3}
                  description="Explain what the space covers and when teams should route content here."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField
                    control={form.control}
                    name="productScope"
                    label="Product scope"
                    description="Optional product boundary such as Billing, Core API, or Onboarding."
                  />
                  <TextField
                    control={form.control}
                    name="journeyScope"
                    label="Journey scope"
                    description="Optional lifecycle segment such as Activation or Renewal."
                  />
                </div>
                <FormSectionHeading
                  title="Exposure"
                  description="Decide who can see the space and how it should behave from a search-surface perspective."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <SelectField
                    control={form.control}
                    name="visibility"
                    label="Visibility"
                    description="Choose the strongest audience exposure the space should allow."
                    options={Object.entries(visibilityScopeLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                  <SelectField
                    control={form.control}
                    name="searchMarkupMode"
                    label="Search markup"
                    description="Align the space with list, canonical question, hybrid, or off behavior."
                    options={Object.entries(searchMarkupModeLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                </div>
                <FormSectionHeading
                  title="Workflow rules"
                  description="Tune whether the space accepts new threads. Review behavior comes from the operating mode."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <SwitchField
                    control={form.control}
                    name="acceptsQuestions"
                    label="Accept questions"
                    description="Disable this for frozen or read-only knowledge spaces."
                  />
                  <SwitchField
                    control={form.control}
                    name="acceptsAnswers"
                    label="Accept answers"
                    description="Disable this if questions should route elsewhere instead of collecting answers."
                  />
                  <SwitchField
                    control={form.control}
                    name="markValidated"
                    label="Mark validated now"
                    description="Use this when the operational setup is already trusted enough to count as validated."
                  />
                </div>
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {translateText(mode === 'create' ? 'Create space' : 'Save changes')}
                  </Button>
                  <Button asChild variant="outline">
                    <Link to={backTo}>
                      <X className="size-4" />
                      {translateText('Cancel')}
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
