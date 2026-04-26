import { useEffect, useMemo } from 'react';
import {
  CheckCircle2,
  Medal,
  Pencil,
  Plus,
  ShieldCheck,
  Trash2,
  Vote,
} from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useAnswerList, useDeleteAnswer } from '@/domains/answers/hooks';
import type { AnswerDto } from '@/domains/answers/types';
import { useQuestionList } from '@/domains/questions/hooks';
import {
  AnswerStatus,
  answerStatusLabels,
  visibilityScopeLabels,
} from '@/shared/constants/backend-enums';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { clampPage } from '@/shared/lib/pagination';
import { useListQueryState } from '@/shared/lib/use-list-query-state';
import { translateText } from '@/shared/lib/i18n-core';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import {
  Badge,
  Button,
  ConfirmAction,
  SectionGridSkeleton,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/ui';
import { AnswerKindBadge, AnswerStatusBadge, VisibilityBadge } from '@/shared/ui/status-badges';

const sortingOptions = [
  { value: 'Sort ASC', label: 'Sort' },
  { value: 'Score DESC', label: 'Score' },
  { value: 'PublishedAtUtc DESC', label: 'Recently published' },
  { value: 'VoteScore DESC', label: 'Vote score' },
  { value: 'AiConfidenceScore DESC', label: 'AI confidence' },
];

const ANSWER_FILTER_DEFAULTS = {
  status: 'all',
  questionId: 'all',
  accepted: 'all',
  visibility: 'all',
} as const;

export function AnswerListPage() {
  const navigate = useNavigate();
  const {
    filters,
    page,
    pageSize,
    setFilter,
    setPage,
    setPageSize,
    setSorting,
    sorting,
  } = useListQueryState({
    defaultSorting: 'Sort ASC',
    filterDefaults: ANSWER_FILTER_DEFAULTS,
  });
  const statusFilter = filters.status;
  const questionFilter = filters.questionId;
  const acceptedFilter = filters.accepted;
  const visibilityFilter = filters.visibility;
  const apiStatus = statusFilter === 'all' ? undefined : Number(statusFilter);
  const apiQuestionId = questionFilter === 'all' ? undefined : questionFilter;
  const apiAccepted =
    acceptedFilter === 'all' ? undefined : acceptedFilter === 'true';
  const apiVisibility =
    visibilityFilter === 'all' ? undefined : Number(visibilityFilter);

  const answerQuery = useAnswerList({
    page,
    pageSize,
    sorting,
    status: apiStatus,
    questionId: apiQuestionId,
    isAccepted: apiAccepted,
    visibility: apiVisibility,
  });
  const questionOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 100,
    sorting: 'Title ASC',
  });

  useEffect(() => {
    const totalCount = answerQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [answerQuery.data?.totalCount, page, pageSize, setPage]);

  const deleteAnswer = useDeleteAnswer();
  const answerRows = answerQuery.data?.items ?? [];
  const publishedCount = answerRows.filter(
    (answer) =>
      answer.status === AnswerStatus.Published ||
      answer.status === AnswerStatus.Validated,
  ).length;
  const acceptedCount = answerRows.filter((answer) => answer.isAccepted).length;
  const officialCount = answerRows.filter((answer) => answer.isOfficial).length;
  const questionLookup = useMemo(
    () =>
      Object.fromEntries(
        (questionOptionsQuery.data?.items ?? []).map((question) => [
          question.id,
          question.title,
        ]),
      ),
    [questionOptionsQuery.data?.items],
  );
  const showMetricsLoadingState =
    answerQuery.isLoading && answerQuery.data === undefined;

  const columns: DataTableColumn<AnswerDto>[] = [
    {
      key: 'headline',
      header: 'Answer',
      cell: (answer) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{answer.headline}</div>
          <div className="text-sm text-muted-foreground">
            {questionLookup[answer.questionId] ?? answer.questionId}
          </div>
          {answer.body ? (
            <div className="line-clamp-2 text-sm text-muted-foreground">{answer.body}</div>
          ) : null}
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      className: 'lg:w-[160px]',
      cell: (answer) => (
        <div className="space-y-2">
          <AnswerStatusBadge status={answer.status} />
          <VisibilityBadge visibility={answer.visibility} />
        </div>
      ),
    },
    {
      key: 'kind',
      header: 'Type',
      className: 'lg:w-[160px]',
      cell: (answer) => (
        <div className="space-y-2">
          <AnswerKindBadge kind={answer.kind} />
          {answer.isAccepted ? (
            <Badge variant="success">{translateText('Accepted')}</Badge>
          ) : null}
        </div>
      ),
    },
    {
      key: 'signals',
      header: 'Signals',
      className: 'lg:w-[160px]',
      cell: (answer) => (
        <div className="space-y-1 text-sm text-muted-foreground">
          <div>{translateText('Score {value}', { value: answer.score })}</div>
          <div>{translateText('Sort {value}', { value: answer.sort })}</div>
          <div>{translateText('Votes {value}', { value: answer.voteScore })}</div>
          <div>
            {translateText('Confidence {value}', {
              value: answer.aiConfidenceScore,
            })}
          </div>
        </div>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'lg:w-[120px]',
      cell: (answer) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/answers/${answer.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <ConfirmAction
            title={translateText('Delete answer "{name}"?', {
              name: answer.headline,
            })}
            description={translateText(
              'This removes the answer candidate and any vote-based ranking attached to it.',
            )}
            confirmLabel={translateText('Delete answer')}
            isPending={deleteAnswer.isPending}
            onConfirm={() => deleteAnswer.mutateAsync(answer.id)}
            trigger={
              <Button variant="ghost" mode="icon">
                <Trash2 className="size-4 text-destructive" />
              </Button>
            }
          />
        </div>
      ),
    },
  ];

  return (
    <ListLayout
      header={
        <PageHeader
          title="Answers"
          description="Manage answer candidates, official guidance, ranking, validation, and retirement across all threads."
          descriptionMode="inline"
          actions={
            <Button asChild>
              <Link to="/app/answers/new">
                <Plus className="size-4" />
                {translateText('New answer')}
              </Link>
            </Button>
          }
        />
      }
    >
      {showMetricsLoadingState ? (
        <SectionGridSkeleton />
      ) : (
        <SectionGrid
          items={[
            {
              title: 'Total',
              value: answerQuery.data?.totalCount ?? 0,
              description: translateText('Answer candidates in this workspace'),
              icon: Medal,
            },
            {
              title: 'Published',
              value: publishedCount,
              description: translateText('Visible or validated answers'),
              icon: ShieldCheck,
            },
            {
              title: 'Accepted',
              value: acceptedCount,
              description: translateText('Currently chosen thread resolutions'),
              icon: CheckCircle2,
            },
            {
              title: 'Official',
              value: officialCount,
              description: translateText('Brand-owned or operationally official answers'),
              icon: Vote,
            },
          ]}
        />
      )}
      <DataTable
        title="Answers"
        description="Open an answer to manage source links, validation, retirement, and question context."
        descriptionMode="hint"
        columns={columns}
        rows={answerRows}
        getRowId={(row) => row.id}
        loading={answerQuery.isLoading}
        onRowClick={(answer) => navigate(`/app/answers/${answer.id}`)}
        toolbar={
          <div className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-[220px_220px_220px_220px]">
            <Select value={questionFilter} onValueChange={(value) => setFilter('questionId', value)}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Question')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All questions</SelectItem>
                {(questionOptionsQuery.data?.items ?? []).map((question) => (
                  <SelectItem key={question.id} value={question.id}>
                    {question.title}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={statusFilter} onValueChange={(value) => setFilter('status', value)}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Status')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                {Object.entries(answerStatusLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {translateText(label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={visibilityFilter}
              onValueChange={(value) => setFilter('visibility', value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Visibility')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All visibility</SelectItem>
                {Object.entries(visibilityScopeLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {translateText(label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={acceptedFilter} onValueChange={(value) => setFilter('accepted', value)}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Accepted state')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All answer states</SelectItem>
                <SelectItem value="true">Accepted only</SelectItem>
                <SelectItem value="false">Not accepted</SelectItem>
              </SelectContent>
            </Select>
            <div className="sm:col-span-2 xl:col-span-4">
              <Select value={sorting} onValueChange={setSorting}>
                <SelectTrigger className="w-full xl:max-w-[240px]">
                  <SelectValue placeholder={translateText('Sort answers')} />
                </SelectTrigger>
                <SelectContent>
                  {sortingOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {translateText(option.label)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        }
        emptyState={
          <EmptyState
            title="No answers in view"
            description="Create an answer candidate to begin ranking and validation."
            action={{ label: 'New answer', to: '/app/answers/new' }}
          />
        }
        errorState={
          answerQuery.isError ? (
            <ErrorState
              title="Unable to load answers"
              error={answerQuery.error}
              retry={() => void answerQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          answerQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={answerQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={answerQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
