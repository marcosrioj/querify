import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  CheckCircle2,
  Medal,
  Pencil,
  ShieldCheck,
  Trash2,
  Vote,
} from "lucide-react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAnswerList, useDeleteAnswer } from "@/domains/answers/hooks";
import type { AnswerDto } from "@/domains/answers/types";
import { useQuestion, useQuestionList } from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useSpace, useSpaceList } from "@/domains/spaces/hooks";
import {
  AnswerStatus,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  ListLayout,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { clampPage } from "@/shared/lib/pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import { translateText } from "@/shared/lib/i18n-core";
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  Badge,
  Button,
  ConfirmAction,
  ListFilterChip,
  ListFilterChipRail,
  ListFilterClearButton,
  ListFilterField,
  ListFilterSearch,
  ListFilterSection,
  ListFilterToolbar,
  SectionGridSkeleton,
  SearchSelect,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui";
import {
  AnswerKindBadge,
  AnswerStatusBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";

const sortingOptions = [
  { value: "LastUpdatedAtUtc DESC", label: "Last update newest" },
  { value: "LastUpdatedAtUtc ASC", label: "Last update oldest" },
  { value: "Headline ASC", label: "Headline A-Z" },
  { value: "Headline DESC", label: "Headline Z-A" },
  { value: "Score DESC", label: "Score high-low" },
  { value: "Score ASC", label: "Score low-high" },
  { value: "AiConfidenceScore DESC", label: "AI confidence high-low" },
  { value: "AiConfidenceScore ASC", label: "AI confidence low-high" },
  { value: "Sort ASC", label: "Sort low-high" },
  { value: "Sort DESC", label: "Sort high-low" },
];

const ANSWER_FILTER_DEFAULTS = {
  status: "all",
  spaceId: "all",
  questionId: "all",
  accepted: "all",
  visibility: "all",
} as const;

const answerStatusBuckets = [
  { label: "All", value: "all" },
  { label: "Draft", value: String(AnswerStatus.Draft) },
  { label: "Active", value: String(AnswerStatus.Active) },
  { label: "Archived", value: String(AnswerStatus.Archived) },
] as const;

function buildQuestionOption(question: {
  id: string;
  title: string;
  spaceSlug?: string;
}) {
  return {
    value: question.id,
    label: question.title,
    description: question.spaceSlug,
    keywords: [question.title, question.spaceSlug ?? ""],
  };
}

function buildSpaceOption(space: { id: string; name: string; slug: string }) {
  return {
    value: space.id,
    label: space.name,
    description: space.slug,
    keywords: [space.name, space.slug],
  };
}

export function AnswerListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const [searchParams] = useSearchParams();
  const sourceId = searchParams.get("sourceId") ?? undefined;
  const scopedToSource = Boolean(sourceId);
  const [questionSearch, setQuestionSearch] = useState("");
  const deferredQuestionSearch = useDeferredValue(questionSearch.trim());
  const [spaceSearch, setSpaceSearch] = useState("");
  const deferredSpaceSearch = useDeferredValue(spaceSearch.trim());
  const {
    debouncedSearch,
    filters,
    page,
    pageSize,
    resetFilters,
    search,
    setFilter,
    setFilters,
    setPage,
    setPageSize,
    setSearch,
    setSorting,
    sorting,
  } = useListQueryState({
    defaultSorting: "LastUpdatedAtUtc DESC",
    filterDefaults: ANSWER_FILTER_DEFAULTS,
  });
  const statusFilter = filters.status;
  const spaceFilter = filters.spaceId;
  const questionFilter = filters.questionId;
  const acceptedFilter = filters.accepted;
  const visibilityFilter = filters.visibility;
  const apiStatus = statusFilter === "all" ? undefined : Number(statusFilter);
  const apiSpaceId = spaceFilter === "all" ? undefined : spaceFilter;
  const apiQuestionId = questionFilter === "all" ? undefined : questionFilter;
  const apiAccepted =
    acceptedFilter === "all" ? undefined : acceptedFilter === "true";
  const apiVisibility =
    visibilityFilter === "all" ? undefined : Number(visibilityFilter);
  const activeFilterCount = [
    search.trim(),
    statusFilter !== "all",
    spaceFilter !== "all",
    questionFilter !== "all",
    acceptedFilter !== "all",
    visibilityFilter !== "all",
  ].filter(Boolean).length;
  const clearFilters = () => resetFilters();

  const answerQuery = useAnswerList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    spaceId: apiSpaceId,
    sourceId,
    status: apiStatus,
    questionId: apiQuestionId,
    isAccepted: apiAccepted,
    visibility: apiVisibility,
  });
  const questionOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 100,
    sorting: "Title ASC",
    spaceId: apiSpaceId,
  });
  const questionSearchOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 20,
    sorting: "Title ASC",
    searchText: deferredQuestionSearch || undefined,
    spaceId: apiSpaceId,
    enabled: Boolean(deferredQuestionSearch),
  });
  const selectedQuestionQuery = useQuestion(apiQuestionId);
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
  const activeCount = answerRows.filter(
    (answer) => answer.status === AnswerStatus.Active,
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
  const questionOptionItems = deferredQuestionSearch
    ? (questionSearchOptionsQuery.data?.items ?? [])
    : (questionOptionsQuery.data?.items ?? []);
  const questionOptions = questionOptionItems.map(buildQuestionOption);
  const selectedQuestion =
    questionOptionItems.find((question) => question.id === apiQuestionId) ??
    selectedQuestionQuery.data;
  const selectedQuestionOption = selectedQuestion
    ? buildQuestionOption(selectedQuestion)
    : null;
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
    answerQuery.isLoading && answerQuery.data === undefined;
  const filtersLoading =
    answerQuery.isFetching ||
    questionOptionsQuery.isFetching ||
    questionSearchOptionsQuery.isFetching ||
    selectedQuestionQuery.isFetching ||
    spaceOptionsQuery.isFetching ||
    spaceSearchOptionsQuery.isFetching ||
    selectedSpaceQuery.isFetching;

  const columns: DataTableColumn<AnswerDto>[] = [
    {
      key: "headline",
      header: "Answer",
      cell: (answer) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{answer.headline}</div>
          <div className="text-sm text-muted-foreground">
            {questionLookup[answer.questionId] ?? answer.questionId}
          </div>
          {answer.body ? (
            <div className="line-clamp-2 text-sm text-muted-foreground">
              {answer.body}
            </div>
          ) : null}
        </div>
      ),
    },
    {
      key: "status",
      header: "Status",
      className: "lg:w-[160px]",
      cell: (answer) => (
        <div className="space-y-2">
          <AnswerStatusBadge status={answer.status} />
          <VisibilityBadge visibility={answer.visibility} />
        </div>
      ),
    },
    {
      key: "kind",
      header: "Type",
      className: "lg:w-[160px]",
      cell: (answer) => (
        <div className="space-y-2">
          <AnswerKindBadge kind={answer.kind} />
          {answer.isAccepted ? (
            <Badge variant="success">{translateText("Accepted")}</Badge>
          ) : null}
        </div>
      ),
    },
    {
      key: "signals",
      header: "Signals",
      className: "lg:w-[160px]",
      cell: (answer) => (
        <div className="space-y-1 text-sm text-muted-foreground">
          <div>{translateText("Score {value}", { value: answer.score })}</div>
          <div>{translateText("Sort {value}", { value: answer.sort })}</div>
          <div>
            {translateText("Votes {value}", { value: answer.voteScore })}
          </div>
          <div>
            {translateText("Confidence {value}", {
              value: answer.aiConfidenceScore,
            })}
          </div>
        </div>
      ),
    },
    {
      key: "lastUpdatedAtUtc",
      header: "Last update",
      className: "lg:w-[160px]",
      cell: (answer) => (
        <span className="break-words text-sm text-muted-foreground">
          {formatOptionalDateTimeInTimeZone(
            answer.lastUpdatedAtUtc,
            portalTimeZone,
            translateText("No update"),
          )}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "lg:w-[120px]",
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
              "This removes the answer candidate and any vote-based ranking attached to it.",
            )}
            confirmLabel={translateText("Delete answer")}
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
        <>
          <PageHeader
            title={scopedToSource ? "Answers linked to source" : "Answers"}
            description={
              scopedToSource
                ? "Showing only answer candidates that cite the selected Source."
                : "Answers are operated from their parent Question. Use this scoped view only to triage candidates that already belong to a question."
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
              value: answerQuery.data?.totalCount ?? 0,
              description: scopedToSource
                ? translateText("Filtered by source relationship")
                : debouncedSearch
                  ? translateText("Search: {value}", {
                      value: debouncedSearch,
                    })
                  : translateText("Answer candidates in this workspace"),
              icon: Medal,
            },
            {
              title: "Active",
              value: activeCount,
              description: translateText("Active answers ready for use"),
              icon: ShieldCheck,
            },
            {
              title: "Accepted",
              value: acceptedCount,
              description: translateText(
                "Currently chosen question resolutions",
              ),
              icon: CheckCircle2,
            },
            {
              title: "Official",
              value: officialCount,
              description: translateText(
                "Brand-owned or operationally official answers",
              ),
              icon: Vote,
            },
          ]}
        />
      )}
      <DataTable
        title="Answers"
        description="Open an answer to manage source links, lifecycle status, and question context."
        descriptionMode="hint"
        columns={columns}
        rows={answerRows}
        getRowId={(row) => row.id}
        loading={answerQuery.isLoading}
        onRowClick={(answer) => navigate(`/app/answers/${answer.id}`)}
        headingControl={
          <ListFilterSearch
            value={search}
            onChange={setSearch}
            placeholder="Search answers by headline or body"
            activeFilterCount={activeFilterCount}
            onClear={clearFilters}
            isLoading={answerQuery.isFetching}
          />
        }
        toolbar={
          <ListFilterToolbar isLoading={filtersLoading}>
            <ListFilterSection
              label="Status"
              activeFilterCount={activeFilterCount}
              emptyLabel="All answers"
              action={
                <ListFilterClearButton
                  activeFilterCount={activeFilterCount}
                  onClear={clearFilters}
                  size="sm"
                  className="w-auto"
                />
              }
            >
              <ListFilterChipRail>
                {answerStatusBuckets.map((bucket) => (
                  <ListFilterChip
                    key={bucket.value}
                    active={statusFilter === bucket.value}
                    onClick={() => setFilter("status", bucket.value)}
                  >
                    {translateText(bucket.label)}
                  </ListFilterChip>
                ))}
              </ListFilterChipRail>
            </ListFilterSection>
            <div className="grid w-full gap-3 md:grid-cols-2 xl:grid-cols-[minmax(220px,1fr)_minmax(220px,1fr)_180px_180px_200px]">
              <ListFilterField
                label="Space"
                className="md:col-span-2 xl:col-span-1"
              >
                <SearchSelect
                  value={apiSpaceId ?? ""}
                  onValueChange={(value) =>
                    setFilters({
                      spaceId: value || "all",
                      questionId: "all",
                    })
                  }
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
                  className="w-full"
                />
              </ListFilterField>
              <ListFilterField
                label="Question"
                className="md:col-span-2 xl:col-span-1"
              >
                <SearchSelect
                  value={apiQuestionId ?? ""}
                  onValueChange={(value) =>
                    setFilter("questionId", value || "all")
                  }
                  options={questionOptions}
                  selectedOption={selectedQuestionOption}
                  placeholder={translateText("All questions")}
                  searchPlaceholder={translateText("Search questions")}
                  emptyMessage={
                    deferredQuestionSearch
                      ? translateText("No questions match this search.")
                      : translateText("No questions available.")
                  }
                  loading={
                    deferredQuestionSearch
                      ? questionSearchOptionsQuery.isFetching
                      : questionOptionsQuery.isFetching
                  }
                  searchValue={questionSearch}
                  onSearchChange={(value) =>
                    startTransition(() => setQuestionSearch(value))
                  }
                  allowClear
                  clearLabel={translateText("All questions")}
                  className="w-full"
                />
              </ListFilterField>
              <ListFilterField label="Visibility">
                <Select
                  value={visibilityFilter}
                  onValueChange={(value) => setFilter("visibility", value)}
                >
                  <SelectTrigger className="w-full" size="lg">
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
              </ListFilterField>
              <ListFilterField label="Accepted">
                <Select
                  value={acceptedFilter}
                  onValueChange={(value) => setFilter("accepted", value)}
                >
                  <SelectTrigger className="w-full" size="lg">
                    <SelectValue
                      placeholder={translateText("Accepted state")}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All answer states</SelectItem>
                    <SelectItem value="true">Accepted only</SelectItem>
                    <SelectItem value="false">Not accepted</SelectItem>
                  </SelectContent>
                </Select>
              </ListFilterField>
              <ListFilterField label="Sort">
                <Select value={sorting} onValueChange={setSorting}>
                  <SelectTrigger className="w-full" size="lg">
                    <SelectValue placeholder={translateText("Sort answers")} />
                  </SelectTrigger>
                  <SelectContent>
                    {sortingOptions.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {translateText(option.label)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </ListFilterField>
            </div>
          </ListFilterToolbar>
        }
        emptyState={
          <EmptyState
            title="No answers in view"
            description="Open a Question from its Space before creating an answer candidate."
            action={{ label: "Open spaces", to: "/app/spaces" }}
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
