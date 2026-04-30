import { useEffect } from "react";
import { Activity, Eye, ShieldCheck } from "lucide-react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useActivityList } from "@/domains/activity/hooks";
import type { ActivityDto } from "@/domains/activity/types";
import {
  ActivityKind,
  ActorKind,
  activityKindLabels,
  actorKindLabels,
} from "@/shared/constants/backend-enums";
import {
  ListLayout,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { clampPage } from "@/shared/lib/pagination";
import { formatNumericDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { translateText } from "@/shared/lib/i18n-core";
import {
  ListFilterField,
  ListFilterSearch,
  ListFilterToolbar,
  SectionGridSkeleton,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui";
import { ActivityKindBadge, ActorKindBadge } from "@/shared/ui/status-badges";

const sortingOptions = [
  { value: "OccurredAtUtc DESC", label: "Latest activity" },
  { value: "OccurredAtUtc ASC", label: "Oldest activity" },
  { value: "Kind ASC", label: "Event kind A-Z" },
  { value: "Kind DESC", label: "Event kind Z-A" },
  { value: "ActorKind ASC", label: "Actor kind A-Z" },
  { value: "ActorKind DESC", label: "Actor kind Z-A" },
];

const ACTIVITY_FILTER_DEFAULTS = {
  kind: "all",
  actorKind: "all",
} as const;

export function ActivityListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const [searchParams] = useSearchParams();
  const {
    debouncedSearch,
    filters,
    page,
    pageSize,
    resetFilters,
    search,
    setFilter,
    setPage,
    setPageSize,
    setSearch,
    setSorting,
    sorting,
  } = useListQueryState({
    defaultSorting: "OccurredAtUtc DESC",
    filterDefaults: ACTIVITY_FILTER_DEFAULTS,
  });
  const kindFilter = filters.kind;
  const actorKindFilter = filters.actorKind;
  const apiKind = kindFilter === "all" ? undefined : Number(kindFilter);
  const apiActorKind =
    actorKindFilter === "all" ? undefined : Number(actorKindFilter);
  const activeFilterCount = [
    search.trim(),
    kindFilter !== "all",
    actorKindFilter !== "all",
  ].filter(Boolean).length;
  const clearFilters = () => resetFilters();
  const questionId = searchParams.get("questionId") ?? undefined;
  const answerId = searchParams.get("answerId") ?? undefined;
  const spaceId = searchParams.get("spaceId") ?? undefined;

  const activityQuery = useActivityList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    spaceId,
    questionId,
    answerId,
    kind: apiKind,
    actorKind: apiActorKind,
  });
  const filtersLoading = activityQuery.isFetching;

  useEffect(() => {
    const totalCount = activityQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [activityQuery.data?.totalCount, page, pageSize, setPage]);

  const rows = activityQuery.data?.items ?? [];
  const moderationCount = rows.filter(
    (event) => event.actorKind === ActorKind.Moderator,
  ).length;
  const signalCount = rows.filter(
    (event) =>
      event.kind === ActivityKind.FeedbackReceived ||
      event.kind === ActivityKind.VoteReceived,
  ).length;
  const workflowCount = rows.filter(
    (event) =>
      event.kind === ActivityKind.QuestionDraft ||
      event.kind === ActivityKind.QuestionActive ||
      event.kind === ActivityKind.QuestionArchived ||
      event.kind === ActivityKind.AnswerDraft ||
      event.kind === ActivityKind.AnswerActive ||
      event.kind === ActivityKind.AnswerArchived,
  ).length;

  const columns: DataTableColumn<ActivityDto>[] = [
    {
      key: "kind",
      header: "Event",
      cell: (event) => (
        <div className="space-y-2">
          <ActivityKindBadge kind={event.kind} />
          <ActorKindBadge kind={event.actorKind} />
        </div>
      ),
    },
    {
      key: "notes",
      header: "Details",
      cell: (event) => (
        <div className="min-w-0 space-y-1">
          <div className="min-w-0 break-words font-medium text-mono [overflow-wrap:anywhere]">
            {event.actorLabel || event.userPrint}
          </div>
          <div className="line-clamp-2 text-sm text-muted-foreground">
            {event.notes || translateText("No notes recorded.")}
          </div>
        </div>
      ),
    },
    {
      key: "subject",
      header: "Subject",
      className: "lg:w-[200px]",
      cell: (event) => (
        <div className="min-w-0 space-y-1 text-sm text-muted-foreground">
          <div className="break-all">
            {translateText("Question {value}", { value: event.questionId })}
          </div>
          {event.answerId ? (
            <div className="break-all">
              {translateText("Answer {value}", { value: event.answerId })}
            </div>
          ) : null}
        </div>
      ),
    },
    {
      key: "occurredAtUtc",
      header: "Occurred",
      className: "lg:w-[160px]",
      cell: (event) => (
        <span className="break-words text-sm text-muted-foreground">
          {formatNumericDateTimeInTimeZone(event.occurredAtUtc, portalTimeZone)}
        </span>
      ),
    },
  ];

  return (
    <ListLayout
      header={
        <>
          <PageHeader
            title="Activity"
            description={
              spaceId
                ? "Review events from questions that belong to this Space."
                : "Review moderation actions, workflow transitions, and public signals across QnA questions."
            }
            descriptionMode="inline"
          />
        </>
      }
    >
      {activityQuery.isLoading && activityQuery.data === undefined ? (
        <SectionGridSkeleton />
      ) : (
        <SectionGrid
          items={[
            {
              title: "Total",
              value: activityQuery.data?.totalCount ?? 0,
              description: spaceId
                ? translateText("Scoped to the current Space")
                : questionId
                  ? translateText("Scoped to the current question")
                  : answerId
                    ? translateText("Scoped to the current answer")
                    : translateText("Audit trail and public-signal events"),
              icon: Activity,
            },
            {
              title: "Moderation",
              value: moderationCount,
              description: translateText("Events caused by moderators"),
              icon: ShieldCheck,
            },
            {
              title: "Signals",
              value: signalCount,
              description: translateText("Feedback and votes"),
              icon: Eye,
            },
            {
              title: "Workflow",
              value: workflowCount,
              description: translateText("Question lifecycle events"),
              icon: Activity,
            },
          ]}
        />
      )}
      <DataTable
        title="Activity"
        description="Open an event to inspect actor, metadata, and subject identifiers."
        descriptionMode="hint"
        columns={columns}
        rows={rows}
        getRowId={(row) => row.id}
        loading={activityQuery.isLoading}
        onRowClick={(row) => navigate(`/app/activity/${row.id}`)}
        headingControl={
          <ListFilterSearch
            value={search}
            onChange={setSearch}
            placeholder="Search activity by actor, notes, or subject"
            activeFilterCount={activeFilterCount}
            onClear={clearFilters}
            isLoading={activityQuery.isFetching}
          />
        }
        toolbar={
          <ListFilterToolbar isLoading={filtersLoading}>
            <div className="grid w-full gap-3 md:grid-cols-2 xl:grid-cols-3">
              <ListFilterField label="Event kind">
                <Select
                  value={kindFilter}
                  onValueChange={(value) => setFilter("kind", value)}
                >
                  <SelectTrigger className="w-full" size="lg">
                    <SelectValue placeholder={translateText("Event kind")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All event kinds</SelectItem>
                    {Object.entries(activityKindLabels).map(
                      ([value, label]) => (
                        <SelectItem key={value} value={value}>
                          {translateText(label)}
                        </SelectItem>
                      ),
                    )}
                  </SelectContent>
                </Select>
              </ListFilterField>
              <ListFilterField label="Actor kind">
                <Select
                  value={actorKindFilter}
                  onValueChange={(value) => setFilter("actorKind", value)}
                >
                  <SelectTrigger className="w-full" size="lg">
                    <SelectValue placeholder={translateText("Actor kind")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All actors</SelectItem>
                    {Object.entries(actorKindLabels).map(([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {translateText(label)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </ListFilterField>
              <ListFilterField
                label="Sort"
                className="md:col-span-2 xl:col-span-1"
              >
                <Select value={sorting} onValueChange={setSorting}>
                  <SelectTrigger className="w-full" size="lg">
                    <SelectValue placeholder={translateText("Sort events")} />
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
            title="No activity in view"
            description="Status changes, votes, and feedback will appear here."
          />
        }
        errorState={
          activityQuery.isError ? (
            <ErrorState
              title="Unable to load activity"
              error={activityQuery.error}
              retry={() => {
                void activityQuery.refetch();
              }}
            />
          ) : undefined
        }
        footer={
          activityQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={activityQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={activityQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
