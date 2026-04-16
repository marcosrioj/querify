import { zodResolver } from '@hookform/resolvers/zod';
import { startTransition, useDeferredValue, useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  AnswerKind,
  AnswerStatus,
  VisibilityScope,
  answerKindLabels,
  answerStatusLabels,
  visibilityScopeLabels,
} from '@/shared/constants/backend-enums';
import { useAnswer, useCreateAnswer, useUpdateAnswer } from '@/domains/answers/hooks';
import { answerFormSchema, type AnswerFormValues } from '@/domains/answers/schemas';
import { useQuestion, useQuestionList } from '@/domains/questions/hooks';
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
import {
  DEFAULT_PORTAL_LANGUAGE,
  getStoredPortalLanguage,
  portalLanguageOptions,
} from '@/shared/lib/language';

function buildQuestionOption(question: { id: string; title: string; spaceKey: string }) {
  return {
    value: question.id,
    label: question.title,
    description: question.spaceKey,
    keywords: [question.title, question.spaceKey],
  };
}

export function AnswerFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const preselectedQuestionId = searchParams.get('questionId') ?? '';
  const [questionSearch, setQuestionSearch] = useState('');
  const deferredQuestionSearch = useDeferredValue(questionSearch.trim());
  const defaultLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;
  const answerQuery = useAnswer(mode === 'edit' ? id : undefined);
  const createAnswer = useCreateAnswer();
  const updateAnswer = useUpdateAnswer(id ?? '');

  const form = useForm<AnswerFormValues>({
    resolver: zodResolver(answerFormSchema),
    defaultValues: {
      questionId: preselectedQuestionId,
      headline: '',
      body: '',
      kind: AnswerKind.Official,
      status: AnswerStatus.Draft,
      visibility: VisibilityScope.Internal,
      language: defaultLanguage,
      contextKey: '',
      applicabilityRulesJson: '',
      trustNote: '',
      evidenceSummary: '',
      authorLabel: '',
      confidenceScore: 50,
      rank: 1,
    },
  });

  useEffect(() => {
    if (!answerQuery.data) {
      return;
    }

    form.reset({
      questionId: answerQuery.data.questionId,
      headline: answerQuery.data.headline,
      body: answerQuery.data.body ?? '',
      kind: answerQuery.data.kind,
      status: answerQuery.data.status,
      visibility: answerQuery.data.visibility,
      language: answerQuery.data.language ?? defaultLanguage,
      contextKey: answerQuery.data.contextKey ?? '',
      applicabilityRulesJson: answerQuery.data.applicabilityRulesJson ?? '',
      trustNote: answerQuery.data.trustNote ?? '',
      evidenceSummary: answerQuery.data.evidenceSummary ?? '',
      authorLabel: answerQuery.data.authorLabel ?? '',
      confidenceScore: answerQuery.data.confidenceScore,
      rank: answerQuery.data.rank,
    });
  }, [answerQuery.data, defaultLanguage, form]);

  const selectedQuestionId =
    form.watch('questionId') || answerQuery.data?.questionId || preselectedQuestionId;
  const selectedQuestionQuery = useQuestion(selectedQuestionId || undefined);
  const questionOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 20,
    sorting: 'Title ASC',
    searchText: deferredQuestionSearch || undefined,
  });
  const selectedQuestion =
    questionOptionsQuery.data?.items.find(
      (question) => question.id === selectedQuestionId,
    ) ?? selectedQuestionQuery.data;
  const questionOptions = (questionOptionsQuery.data?.items ?? []).map(
    buildQuestionOption,
  );
  const selectedQuestionOption = selectedQuestion
    ? buildQuestionOption(selectedQuestion)
    : null;
  const languageOptions = portalLanguageOptions.map((option) => ({
    value: option.code,
    label: option.label,
    description: `${option.code} • ${option.direction.toUpperCase()}`,
    keywords: [option.code, option.label, option.direction],
  }));
  const selectedLanguageValue = form.watch('language');
  const selectedLanguageOption =
    languageOptions.find((option) => option.value === selectedLanguageValue) ?? null;
  const isSubmitting = createAnswer.isPending || updateAnswer.isPending;
  const backTo =
    mode === 'edit' && id
      ? `/app/answers/${id}`
      : selectedQuestionId
        ? `/app/questions/${selectedQuestionId}`
        : '/app/answers';

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === 'create' ? 'New answer' : 'Edit answer'}
          description="Author the answer candidate, then tune confidence, rank, visibility, and trust cues."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === 'edit' && answerQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText('Quick notes')}</span>
                  <ContextHint
                    content={translateText(
                      'Answers are ranked, validated, retired, and often grounded with evidence links after they are saved.',
                    )}
                    label={translateText('Quick notes details')}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: 'Question',
                    value: selectedQuestion?.title || 'Choose in form',
                  },
                  { label: 'Ranking', value: 'Rank and vote score shape answer order' },
                  { label: 'Trust', value: 'Trust note and evidence summary are optional' },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {answerQuery.isError ? (
        <ErrorState
          title="Unable to load answer"
          error={answerQuery.error}
          retry={() => void answerQuery.refetch()}
        />
      ) : mode === 'edit' && answerQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText('Answer details')}</span>
                <ContextHint
                  content={translateText(
                    'Write the answer first, then decide how confident, visible, and official it should be.',
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
                    body: values.body || undefined,
                    language: values.language || undefined,
                    contextKey: values.contextKey || undefined,
                    applicabilityRulesJson: values.applicabilityRulesJson || undefined,
                    trustNote: values.trustNote || undefined,
                    evidenceSummary: values.evidenceSummary || undefined,
                    authorLabel: values.authorLabel || undefined,
                    kind: Number(values.kind) as AnswerKind,
                    status: Number(values.status) as AnswerStatus,
                    visibility: Number(values.visibility) as VisibilityScope,
                  };

                  if (mode === 'create') {
                    const createdId = await createAnswer.mutateAsync(body);
                    navigate(`/app/answers/${createdId}`);
                    return;
                  }

                  await updateAnswer.mutateAsync(body);
                  navigate(`/app/answers/${id}`);
                })}
              >
                <FormSectionHeading
                  title="Placement"
                  description="Attach the answer to the right thread before tuning visibility or trust."
                />
                <SearchSelectField
                  control={form.control}
                  name="questionId"
                  label="Question"
                  description="The question thread this answer candidate belongs to."
                  placeholder="Search and choose a question"
                  searchPlaceholder="Search questions"
                  emptyMessage={
                    deferredQuestionSearch
                      ? 'No questions match this search.'
                      : 'No questions available.'
                  }
                  options={questionOptions}
                  selectedOption={selectedQuestionOption}
                  loading={questionOptionsQuery.isFetching}
                  searchValue={questionSearch}
                  onSearchChange={(value) =>
                    startTransition(() => setQuestionSearch(value))
                  }
                />
                <FormSectionHeading
                  title="Content"
                  description="Write the headline customers should trust first, then add the deeper answer body."
                />
                <TextField
                  control={form.control}
                  name="headline"
                  label="Headline"
                  description="The short answer that should appear as the candidate title."
                />
                <TextareaField
                  control={form.control}
                  name="body"
                  label="Body"
                  rows={8}
                  description="The full guidance, including steps, caveats, and nuance."
                />
                <FormSectionHeading
                  title="Lifecycle and trust"
                  description="Control visibility, official status, confidence, and operational rank."
                />
                <div className="grid gap-4 md:grid-cols-4">
                  <SelectField
                    control={form.control}
                    name="kind"
                    label="Answer kind"
                    options={Object.entries(answerKindLabels).map(([value, label]) => ({
                      value,
                      label,
                    }))}
                  />
                  <SelectField
                    control={form.control}
                    name="status"
                    label="Status"
                    options={Object.entries(answerStatusLabels).map(([value, label]) => ({
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
                  <TextField
                    control={form.control}
                    name="rank"
                    label="Rank"
                    description="Lower numbers surface first when vote or acceptance does not override ordering."
                  />
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <SearchSelectField
                    control={form.control}
                    name="language"
                    label="Language"
                    description="Optional locale for multilingual answer operations."
                    options={languageOptions}
                    selectedOption={selectedLanguageOption}
                    searchPlaceholder="Search languages"
                    emptyMessage="No languages found."
                    resultCountHint={translateText('{count} languages available', {
                      count: portalLanguageOptions.length,
                    })}
                  />
                  <TextField
                    control={form.control}
                    name="confidenceScore"
                    label="Confidence score"
                  />
                  <TextField control={form.control} name="authorLabel" label="Author label" />
                  <TextField control={form.control} name="contextKey" label="Context key" />
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <TextareaField
                    control={form.control}
                    name="trustNote"
                    label="Trust note"
                    rows={3}
                    description="Optional note explaining why this answer should be trusted."
                  />
                  <TextareaField
                    control={form.control}
                    name="evidenceSummary"
                    label="Evidence summary"
                    rows={3}
                    description="Optional summary of the strongest supporting evidence."
                  />
                </div>
                <TextareaField
                  control={form.control}
                  name="applicabilityRulesJson"
                  label="Applicability rules JSON"
                  rows={4}
                  description="Optional machine-readable rules that constrain when the answer applies."
                />
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {translateText(mode === 'create' ? 'Create answer' : 'Save changes')}
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
