import { zodResolver } from '@hookform/resolvers/zod';
import { startTransition, useDeferredValue, useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  ChannelKind,
  QuestionStatus,
  VisibilityScope,
  channelKindLabels,
  questionStatusLabels,
  visibilityScopeLabels,
} from '@/shared/constants/backend-enums';
import { useQuestion, useCreateQuestion, useUpdateQuestion } from '@/domains/questions/hooks';
import { questionFormSchema, type QuestionFormValues } from '@/domains/questions/schemas';
import { useSpace, useSpaceList } from '@/domains/spaces/hooks';
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
  TextField,
  TextareaField,
} from '@/shared/ui/form-fields';
import { translateText } from '@/shared/lib/i18n-core';

function buildSpaceOption(space: { id: string; name: string; key: string }) {
  return {
    value: space.id,
    label: space.name,
    description: space.key,
    keywords: [space.name, space.key],
  };
}

export function QuestionFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const preselectedSpaceId = searchParams.get('spaceId') ?? '';
  const [spaceSearch, setSpaceSearch] = useState('');
  const deferredSpaceSearch = useDeferredValue(spaceSearch.trim());
  const questionQuery = useQuestion(mode === 'edit' ? id : undefined);
  const createQuestion = useCreateQuestion();
  const updateQuestion = useUpdateQuestion(id ?? '');

  const form = useForm<QuestionFormValues>({
    resolver: zodResolver(questionFormSchema),
    defaultValues: {
      spaceId: preselectedSpaceId,
      title: '',
      summary: '',
      contextNote: '',
      status: QuestionStatus.Draft,
      visibility: VisibilityScope.Internal,
      originChannel: ChannelKind.Manual,
      aiConfidenceScore: 50,
      feedbackScore: 0,
      sort: 0,
    },
  });

  useEffect(() => {
    if (!questionQuery.data) {
      return;
    }

    form.reset({
      spaceId: questionQuery.data.spaceId,
      title: questionQuery.data.title,
      summary: questionQuery.data.summary ?? '',
      contextNote: questionQuery.data.contextNote ?? '',
      status: questionQuery.data.status,
      visibility: questionQuery.data.visibility,
      originChannel: questionQuery.data.originChannel,
      aiConfidenceScore: questionQuery.data.aiConfidenceScore,
      feedbackScore: questionQuery.data.feedbackScore,
      sort: questionQuery.data.sort,
    });
  }, [form, questionQuery.data]);

  const selectedSpaceId = form.watch('spaceId') || questionQuery.data?.spaceId || preselectedSpaceId;
  const selectedSpaceQuery = useSpace(selectedSpaceId || undefined);
  const spaceOptionsQuery = useSpaceList({
    page: 1,
    pageSize: 20,
    sorting: 'Name ASC',
    searchText: deferredSpaceSearch || undefined,
  });
  const selectedSpace =
    spaceOptionsQuery.data?.items.find((space) => space.id === selectedSpaceId) ??
    selectedSpaceQuery.data;
  const selectedVisibility = Number(form.watch('visibility')) as VisibilityScope;
  const selectedStatus = Number(form.watch('status')) as QuestionStatus;
  const publicVisibilitySelected =
    selectedVisibility === VisibilityScope.Public ||
    selectedVisibility === VisibilityScope.PublicIndexed;
  const invalidPublicStatus =
    publicVisibilitySelected &&
    selectedStatus !== QuestionStatus.Open &&
    selectedStatus !== QuestionStatus.Answered &&
    selectedStatus !== QuestionStatus.Validated;
  const spaceBlocksQuestions = selectedSpace?.acceptsQuestions === false;
  const spaceOptions = (spaceOptionsQuery.data?.items ?? []).map(buildSpaceOption);
  const selectedSpaceOption = selectedSpace ? buildSpaceOption(selectedSpace) : null;
  const isSubmitting = createQuestion.isPending || updateQuestion.isPending;
  const backTo =
    mode === 'edit' && id ? `/app/questions/${id}` : selectedSpaceId ? `/app/spaces/${selectedSpaceId}` : '/app/questions';

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === 'create' ? 'New question' : 'Edit question'}
          description="Capture the thread, its operational status, and the context needed for accurate answers."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === 'edit' && questionQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText('Quick notes')}</span>
                  <ContextHint
                    content={translateText(
                      'Questions own workflow, accepted answers, duplicate routing, and public feedback.',
                    )}
                    label={translateText('Quick notes details')}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  { label: 'Space', value: selectedSpace?.name || 'Choose in form' },
                  { label: 'Origin', value: 'Manual, widget, API, help center, and more' },
                  { label: 'Workflow', value: 'Draft to validated, escalated, or duplicate' },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {questionQuery.isError ? (
        <ErrorState
          title="Unable to load question"
          error={questionQuery.error}
          retry={() => void questionQuery.refetch()}
        />
      ) : mode === 'edit' && questionQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText('Thread details')}</span>
                <ContextHint
                  content={translateText(
                    'Start with the space, then set lifecycle, visibility, and thread context so downstream answers inherit the right guardrails.',
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
                    contextNote: values.contextNote || undefined,
                    status: Number(values.status) as QuestionStatus,
                    visibility: Number(values.visibility) as VisibilityScope,
                    originChannel: Number(values.originChannel) as ChannelKind,
                  };

                  if (mode === 'create') {
                    const createdId = await createQuestion.mutateAsync(body);
                    navigate(`/app/questions/${createdId}`);
                    return;
                  }

                  await updateQuestion.mutateAsync({
                    ...body,
                    acceptedAnswerId: questionQuery.data?.acceptedAnswerId || undefined,
                    duplicateOfQuestionId:
                      questionQuery.data?.duplicateOfQuestionId || undefined,
                  });
                  navigate(`/app/questions/${id}`);
                })}
              >
                <FormSectionHeading
                  title="Placement"
                  description="Pick the owning space first so the thread inherits the right exposure and operating mode."
                />
                <SearchSelectField
                  control={form.control}
                  name="spaceId"
                  label="Space"
                  description="The space controls exposure, operating mode, and how the question should be operated."
                  placeholder="Search and choose the owning space"
                  searchPlaceholder="Search spaces"
                  emptyMessage={
                    deferredSpaceSearch ? 'No spaces match this search.' : 'No spaces available.'
                  }
                  options={spaceOptions}
                  selectedOption={selectedSpaceOption}
                  loading={spaceOptionsQuery.isFetching}
                  searchValue={spaceSearch}
                  onSearchChange={(value) => startTransition(() => setSpaceSearch(value))}
                />
                <FormSectionHeading
                  title="Identity"
                  description="Use the wording customers or operators will actually search for."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField
                    control={form.control}
                    name="title"
                    label="Title"
                    placeholder="How do I activate the workspace for a new tenant?"
                    description="Use the canonical question wording."
                  />
                  <TextField
                    control={form.control}
                    name="sort"
                    label="Sort"
                    description="Lower values appear earlier in curated ordering."
                  />
                </div>
                <TextareaField
                  control={form.control}
                  name="summary"
                  label="Summary"
                  rows={3}
                  description="A compact explanation of the thread before the full context."
                />
                <TextareaField
                  control={form.control}
                  name="contextNote"
                  label="Context note"
                  rows={4}
                  description="Operational nuance that answer authors should understand."
                />
                <FormSectionHeading
                  title="Workflow"
                  description="Set the starting lifecycle, visibility, and intake channel."
                />
                <div className="grid gap-4 md:grid-cols-3">
                  <SelectField
                    control={form.control}
                    name="status"
                    label="Status"
                    options={Object.entries(questionStatusLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                  <SelectField
                    control={form.control}
                    name="visibility"
                    label="Visibility"
                    options={Object.entries(visibilityScopeLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                  <SelectField
                    control={form.control}
                    name="originChannel"
                    label="Origin channel"
                    options={Object.entries(channelKindLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField
                    control={form.control}
                    name="aiConfidenceScore"
                    label="AI confidence score"
                    description="Use a 0-100 score to indicate how strong the current thread framing is."
                  />
                  <TextField
                    control={form.control}
                    name="feedbackScore"
                    label="Feedback score"
                    description="Current aggregate feedback score for this question."
                  />
                </div>
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting || spaceBlocksQuestions}>
                    {translateText(mode === 'create' ? 'Create question' : 'Save changes')}
                  </Button>
                  <Button asChild variant="outline">
                    <Link to={backTo}>
                      <X className="size-4" />
                      {translateText('Cancel')}
                    </Link>
                  </Button>
                </div>
                {spaceBlocksQuestions ? (
                  <p className="text-sm text-muted-foreground">
                    {translateText('This space does not accept new questions.')}
                  </p>
                ) : null}
                {invalidPublicStatus ? (
                  <p className="text-sm text-muted-foreground">
                    {translateText(
                      'Public visibility requires status Open, Answered, or Validated.',
                    )}
                  </p>
                ) : null}
              </form>
            </Form>
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
