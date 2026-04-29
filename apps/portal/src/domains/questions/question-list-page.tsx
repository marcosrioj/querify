import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  CheckCircle2,
  CircleDot,
  GitFork,
  Pencil,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useQuestionList, useDeleteQuestion } from "@/domains/questions/hooks";
import type { QuestionDto } from "@/domains/questions/types";
import { useSpace, useSpaceList } from "@/domains/spaces/hooks";
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
  SearchSelect,
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

const statusBuckets = [
  { label: "All", value: "all" },
  { label: "Draft", value: String(QuestionStatus.Draft) },
  { label: "Active", value: String(QuestionStatus.Active) },
  { label: "Archived", value: String(QuestionStatus.Archived) },
] as const;

type QuestionListRow = QuestionDto & {
  sources?: Array<{ sourceId: string }>;
  tags?: Array<{ id: string }>;
};

function buildSpaceOption(space: { id: string; name: string; slug: string }) {
  return {
    value: space.id,
    label: space.name,
    description: space.slug,
    keywords: [space.name, space.slug],
  };
}

export function QuestionListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const [searchParams] = useSearchParams();
  const sourceId = searchParams.get("sourceId") ?? undefined;
  const tagId = searchParams.get("tagId") ?? undefined;
  const scopedToRelationship = Boolean(sourceId || tagId);
  const [spaceSearch, setSpaceSearch] = useState("");
  const deferredSpaceSearch = useDeferredValue(spaceSearch.trim());
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
    page: scopedToRelationship ? 1 : page,
    pageSize: scopedToRelationship ? 100 : pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    status: apiStatus,
    visibility: apiVisibility,
    spaceId: apiSpaceId,
    includeSources: Boolean(sourceId),
    includeTags: Boolean(tagId),
  });
  const spaceOptionsQuery = useSpaceList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });
  const spaceSearchOptionsQuery = useSpaceList({
    page: 1,
    pageSize: 20,
    sorting: "Name ASC",
    searchText: deferredSpaceSearch || undefined,
    enabled: Boolean(deferredSpaceSearch),
  });
  const selectedSpaceQuery = useSpace(apiSpaceId);

  useEffect(() => {
    if (scopedToRelationship) {
      return;
    }

    const totalCount = questionQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [
    page,
    pageSize,
    questionQuery.data?.totalCount,
    scopedToRelationship,
    setPage,
  ]);

  const deleteQuestion = useDeleteQuestion();
  const questionRows = (
    (questionQuery.data?.items ?? []) as QuestionListRow[]
  ).filter((question) => {
    const matchesSource = sourceId
      ? question.sources?.some((link) => link.sourceId === sourceId)
      : true;
    const matchesTag = tagId
      ? question.tags?.some((tag) => tag.id === tagId)
      : true;

    return matchesSource && matchesTag;
  });
  const acceptedAnswerCount = questionRows.filter(
    (question) => Boolean(question.acceptedAnswerId),
  ).length;
  const activeCount = questionRows.filter(
    (question) => question.status === QuestionStatus.Active,
  ).length;
  const duplicateCount = questionRows.filter(
    (question) => Boolean(question.duplicateOfQuestionId),
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
  const spaceOptionItems = deferredSpaceSearch
    ? (spaceSearchOptionsQuery.data?.items ?? [])
    : (spaceOptionsQuery.data?.items ?? []);
  const spaceOptions = spaceOptionItems.map(buildSpaceOption);
  const selectedSpace =
    spaceOptionItems.find((space) => space.id === apiSpaceId) ??
    selectedSpaceQuery.data;
  const selectedSpaceOption = selectedSpace
    ? buildSpaceOption(selectedSpace)
    : null;
  const showMetricsLoadingState =
    questionQuery.isLoading && questionQuery.data === undefined;

  const columns: DataTableColumn<QuestionListRow>[] = [
    {
      key: "title",
      header: "Question",
      cell: (question) => (
        <div className="min-w-0 space-y-1">
          <div className="min-w-0 break-words font-medium text-mono [overflow-wrap:anywhere]">
            {question.title}
          </div>
          <div className="min-w-0 break-words text-sm text-muted-foreground [overflow-wrap:anywhere]">
            {spaceLookup[question.spaceId] ?? question.spaceSlug}
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
        <div className="min-w-0 space-y-1 break-words text-sm text-muted-foreground">
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
        <span className="break-words text-sm text-muted-foreground">
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
          className="flex min-w-0 flex-wrap items-center justify-start gap-1 lg:justify-end"
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
              "This removes the question from the portal and breaks any accepted-answer linkage.",
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
            title={scopedToRelationship ? "Related questions" : "Questions"}
            description={
              sourceId
                ? "Showing only questions linked to the selected Source."
                : tagId
                  ? "Showing only questions attached to the selected Tag."
                  : "Questions are operated from their owning Space. Use this scoped view only to triage questions that already have parent context."
            }
            descriptionMode="inline"
            backTo="/app/spaces"
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
              value: scopedToRelationship
                ? questionRows.length
                : (questionQuery.data?.totalCount ?? 0),
              description: scopedToRelationship
                ? translateText("Filtered by relationship")
                : debouncedSearch
                  ? translateText("Search: {value}", {
                      value: debouncedSearch,
                    })
                  : translateText("Questions currently in this workspace"),
              icon: CircleDot,
            },
            {
              title: "Active",
              value: activeCount,
              description: translateText(
                "Questions available for normal QnA work",
              ),
              icon: CheckCircle2,
            },
            {
              title: "Accepted",
              value: acceptedAnswerCount,
              description: translateText(
                "Questions with an accepted answer",
              ),
              icon: CheckCircle2,
            },
            {
              title: "Duplicates",
              value: duplicateCount,
              description: translateText(
                "Questions redirected to a canonical question",
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
        headingControl={
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder={translateText("Search questions")}
          />
        }
        toolbar={
          <div className="grid w-full gap-3 xl:min-w-[680px]">
            <div className="flex flex-wrap gap-1.5 rounded-xl border border-border/70 bg-muted/30 p-1">
              {statusBuckets.map((bucket) => (
                <Button
                  key={bucket.value}
                  type="button"
                  variant={
                    statusFilter === bucket.value ? "secondary" : "ghost"
                  }
                  size="sm"
                  className="shrink-0"
                  onClick={() => setFilter("status", bucket.value)}
                >
                  {translateText(bucket.label)}
                </Button>
              ))}
            </div>
            <div className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-[minmax(220px,1fr)_180px_180px_200px]">
              <SearchSelect
                value={apiSpaceId ?? ""}
                onValueChange={(value) => setFilter("spaceId", value || "all")}
                options={spaceOptions}
                selectedOption={selectedSpaceOption}
                placeholder={translateText("All spaces")}
                searchPlaceholder={translateText("Search spaces")}
                emptyMessage={
                  deferredSpaceSearch
                    ? translateText("No spaces match this search.")
                    : translateText("No spaces available.")
                }
                loading={
                  deferredSpaceSearch
                    ? spaceSearchOptionsQuery.isFetching
                    : spaceOptionsQuery.isFetching
                }
                searchValue={spaceSearch}
                onSearchChange={(value) =>
                  startTransition(() => setSpaceSearch(value))
                }
                allowClear
                clearLabel={translateText("All spaces")}
              />
              <Select
                value={statusFilter}
                onValueChange={(value) => setFilter("status", value)}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder={translateText("Status")} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All statuses</SelectItem>
                  {Object.entries(questionStatusLabels).map(
                    ([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {translateText(label)}
                      </SelectItem>
                    ),
                  )}
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
                  {Object.entries(visibilityScopeLabels).map(
                    ([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {translateText(label)}
                      </SelectItem>
                    ),
                  )}
                </SelectContent>
              </Select>
              <Select value={sorting} onValueChange={setSorting}>
                <SelectTrigger className="w-full">
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
            description="Open a Space and create the first question from there so intake rules, visibility, tags, and sources stay attached."
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
          !scopedToRelationship && questionQuery.data ? (
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
