import { useEffect, useMemo } from "react";
import {
  AlertTriangle,
  CheckCircle2,
  CircleDot,
  GitFork,
  Pencil,
  Trash2,
} from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useQuestionList, useDeleteQuestion } from "@/domains/questions/hooks";
import type { QuestionDto } from "@/domains/questions/types";
import { useSpaceList } from "@/domains/spaces/hooks";
import {
  QuestionStatus,
  questionStatusLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  ListLayout,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { clampPage } from "@/shared/lib/pagination";
import { formatNumericDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import { translateText } from "@/shared/lib/i18n-core";
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  Badge,
  Button,
  ConfirmAction,
  Input,
  SectionGridSkeleton,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui";
import {
  ChannelKindBadge,
  QuestionStatusBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";

const sortingOptions = [
  { value: "LastActivityAtUtc DESC", label: "Latest activity" },
  { value: "Title ASC", label: "Title A-Z" },
  { value: "FeedbackScore DESC", label: "Feedback score" },
  { value: "AiConfidenceScore DESC", label: "AI confidence" },
  { value: "Sort ASC", label: "Sort" },
];

const QUESTION_FILTER_DEFAULTS = {
  status: "all",
  visibility: "all",
  spaceId: "all",
} as const;

export function QuestionListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const {
    debouncedSearch,
    filters,
    page,
    pageSize,
    search,
    setFilter,
    setPage,
    setPageSize,
    setSearch,
    setSorting,
    sorting,
  } = useListQueryState({
    defaultSorting: "LastActivityAtUtc DESC",
    filterDefaults: QUESTION_FILTER_DEFAULTS,
  });
  const statusFilter = filters.status;
  const visibilityFilter = filters.visibility;
  const spaceFilter = filters.spaceId;
  const apiStatus = statusFilter === "all" ? undefined : Number(statusFilter);
  const apiVisibility =
    visibilityFilter === "all" ? undefined : Number(visibilityFilter);
  const apiSpaceId = spaceFilter === "all" ? undefined : spaceFilter;

  const questionQuery = useQuestionList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    status: apiStatus,
    visibility: apiVisibility,
    spaceId: apiSpaceId,
  });
  const spaceOptionsQuery = useSpaceList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });

  useEffect(() => {
    const totalCount = questionQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [page, pageSize, questionQuery.data?.totalCount, setPage]);

  const deleteQuestion = useDeleteQuestion();
  const questionRows = questionQuery.data?.items ?? [];
  const answeredCount = questionRows.filter(
    (question) =>
      question.status === QuestionStatus.Answered ||
      question.status === QuestionStatus.Validated,
  ).length;
  const escalatedCount = questionRows.filter(
    (question) => question.status === QuestionStatus.Escalated,
  ).length;
  const duplicateCount = questionRows.filter(
    (question) => question.status === QuestionStatus.Duplicate,
  ).length;
  const spaceLookup = useMemo(
    () =>
      Object.fromEntries(
        (spaceOptionsQuery.data?.items ?? []).map((space) => [
          space.id,
          space.name,
        ]),
      ),
    [spaceOptionsQuery.data?.items],
  );
  const showMetricsLoadingState =
    questionQuery.isLoading && questionQuery.data === undefined;

  const columns: DataTableColumn<QuestionDto>[] = [
    {
      key: "title",
      header: "Question",
      cell: (question) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{question.title}</div>
          <div className="text-sm text-muted-foreground">
            {spaceLookup[question.spaceId] ?? question.spaceKey}
          </div>
          <div className="flex flex-wrap gap-2">
            <ChannelKindBadge kind={question.originChannel} />
          </div>
          {question.summary ? (
            <div className="line-clamp-2 text-sm text-muted-foreground">
              {question.summary}
            </div>
          ) : null}
        </div>
      ),
    },
    {
      key: "status",
      header: "Status",
      className: "lg:w-[160px]",
      cell: (question) => (
        <div className="space-y-2">
          <QuestionStatusBadge status={question.status} />
          <VisibilityBadge visibility={question.visibility} />
        </div>
      ),
    },
    {
      key: "signals",
      header: "Signals",
      className: "lg:w-[160px]",
      cell: (question) => (
        <div className="space-y-1 text-sm text-muted-foreground">
          <div>
            {translateText("Feedback {value}", {
              value: question.feedbackScore,
            })}
          </div>
          <div>
            {translateText("Confidence {value}", {
              value: question.aiConfidenceScore,
            })}
          </div>
          {question.acceptedAnswerId ? (
            <Badge variant="success" appearance="outline">
              {translateText("Accepted answer")}
            </Badge>
          ) : null}
          {question.duplicateOfQuestionId ? (
            <Badge variant="mono" appearance="outline">
              {translateText("Duplicate")}
            </Badge>
          ) : null}
        </div>
      ),
    },
    {
      key: "lastActivityAtUtc",
      header: "Last activity",
      className: "lg:w-[160px]",
      cell: (question) => (
        <span className="text-sm text-muted-foreground">
          {formatNumericDateTimeInTimeZone(
            question.lastActivityAtUtc,
            portalTimeZone,
          )}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "lg:w-[120px]",
      cell: (question) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/questions/${question.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <ConfirmAction
            title={translateText('Delete question "{name}"?', {
              name: question.title,
            })}
            description={translateText(
              "This removes the thread from the portal and breaks any accepted-answer linkage.",
            )}
            confirmLabel={translateText("Delete question")}
            isPending={deleteQuestion.isPending}
            onConfirm={() => deleteQuestion.mutateAsync(question.id)}
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
        <>
          <PageHeader
            title="Questions"
            description="Questions are operated from their owning Space. Use this scoped view only to triage threads that already have parent context."
            descriptionMode="inline"
            backTo="/app/spaces"
          />
          <QnaModuleNav
            activeKey="spaces"
            intent="Questions are not a standalone workspace. Open the Space first, then create or resolve question threads inside that operating context."
          />
        </>
      }
    >
      {showMetricsLoadingState ? (
        <SectionGridSkeleton />
      ) : (
        <SectionGrid
          items={[
            {
              title: "Total",
              value: questionQuery.data?.totalCount ?? 0,
              description: debouncedSearch
                ? translateText("Search: {value}", { value: debouncedSearch })
                : translateText("Threads currently in this workspace"),
              icon: CircleDot,
            },
            {
              title: "Answered",
              value: answeredCount,
              description: translateText(
                "Questions already resolved enough to use",
              ),
              icon: CheckCircle2,
            },
            {
              title: "Escalated",
              value: escalatedCount,
              description: translateText(
                "Threads routed outside normal QnA resolution",
              ),
              icon: AlertTriangle,
            },
            {
              title: "Duplicates",
              value: duplicateCount,
              description: translateText(
                "Threads redirected to a canonical question",
              ),
              icon: GitFork,
            },
          ]}
        />
      )}
      <DataTable
        title="Questions"
        description="Open a question to manage workflow, answers, source links, tags, and activity."
        descriptionMode="hint"
        columns={columns}
        rows={questionRows}
        getRowId={(row) => row.id}
        loading={questionQuery.isLoading}
        onRowClick={(question) => navigate(`/app/questions/${question.id}`)}
        toolbar={
          <div className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-[minmax(240px,1fr)_220px_220px]">
            <div className="sm:col-span-2 xl:col-span-1">
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder={translateText("Search questions")}
              />
            </div>
            <Select
              value={spaceFilter}
              onValueChange={(value) => setFilter("spaceId", value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText("Space")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All spaces</SelectItem>
                {(spaceOptionsQuery.data?.items ?? []).map((space) => (
                  <SelectItem key={space.id} value={space.id}>
                    {space.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={statusFilter}
              onValueChange={(value) => setFilter("status", value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText("Status")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                {Object.entries(questionStatusLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {translateText(label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={visibilityFilter}
              onValueChange={(value) => setFilter("visibility", value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText("Visibility")} />
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
            <div className="sm:col-span-2 xl:col-span-3">
              <Select value={sorting} onValueChange={setSorting}>
                <SelectTrigger className="w-full xl:max-w-[240px]">
                  <SelectValue placeholder={translateText("Sort questions")} />
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
            title="No questions in view"
            description="Open a Space and create the first thread from there so intake rules, visibility, tags, and sources stay attached."
            action={{ label: "Open spaces", to: "/app/spaces" }}
          />
        }
        errorState={
          questionQuery.isError ? (
            <ErrorState
              title="Unable to load questions"
              error={questionQuery.error}
              retry={() => void questionQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          questionQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={questionQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={questionQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
