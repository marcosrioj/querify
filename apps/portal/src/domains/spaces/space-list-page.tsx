import { useEffect } from "react";
import { Eye, FolderKanban, Pencil, Plus, Trash2 } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useDeleteSpace, useSpaceList } from "@/domains/spaces/hooks";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  SpaceStatus,
  VisibilityScope,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import { ListLayout, PageHeader } from "@/shared/layout/page-layouts";
import { clampPage } from "@/shared/lib/pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { translateText } from "@/shared/lib/i18n-core";
import {
  Badge,
  Button,
  ConfirmAction,
  ListFilterChip,
  ListFilterChipRail,
  ListFilterDisclosure,
  ListFilterField,
  ListFilterSearch,
  ListFilterSection,
  ListFilterToolbar,
  ListResultSummary,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui";
import { SpaceStatusBadge, VisibilityBadge } from "@/shared/ui/status-badges";

const sortingOptions = [
  { value: "LastUpdatedAtUtc DESC", label: "Last update newest" },
  { value: "LastUpdatedAtUtc ASC", label: "Last update oldest" },
  { value: "Name ASC", label: "Name A-Z" },
  { value: "Name DESC", label: "Name Z-A" },
  { value: "Slug ASC", label: "Slug A-Z" },
  { value: "Slug DESC", label: "Slug Z-A" },
  { value: "QuestionCount DESC", label: "Questions high-low" },
  { value: "QuestionCount ASC", label: "Questions low-high" },
  { value: "Status ASC", label: "Status A-Z" },
  { value: "Status DESC", label: "Status Z-A" },
];

const SPACE_FILTER_DEFAULTS = {
  visibility: "all",
  status: "all",
  acceptsQuestions: "all",
  acceptsAnswers: "all",
} as const;

const spaceStatusBuckets = [
  { label: "All", value: "all" },
  {
    label: "Draft",
    value: String(SpaceStatus.Draft),
  },
  {
    label: "Active",
    value: String(SpaceStatus.Active),
  },
  {
    label: "Archived",
    value: String(SpaceStatus.Archived),
  },
] as const;

export function SpaceListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
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
    filterDefaults: SPACE_FILTER_DEFAULTS,
  });
  const visibilityFilter = filters.visibility;
  const statusFilter = filters.status;
  const acceptsQuestionsFilter = filters.acceptsQuestions;
  const acceptsAnswersFilter = filters.acceptsAnswers;
  const apiVisibility =
    visibilityFilter === "all" ? undefined : Number(visibilityFilter);
  const apiStatus = statusFilter === "all" ? undefined : Number(statusFilter);
  const apiAcceptsQuestions =
    acceptsQuestionsFilter === "all"
      ? undefined
      : acceptsQuestionsFilter === "true";
  const apiAcceptsAnswers =
    acceptsAnswersFilter === "all"
      ? undefined
      : acceptsAnswersFilter === "true";
  const quickAllActive =
    statusFilter === "all" &&
    acceptsQuestionsFilter === "all" &&
    acceptsAnswersFilter === "all";
  const activeFilterCount = [
    search.trim(),
    visibilityFilter !== "all",
    statusFilter !== "all",
    acceptsQuestionsFilter !== "all",
    acceptsAnswersFilter !== "all",
  ].filter(Boolean).length;
  const refinementFilterCount = [
    visibilityFilter !== "all",
    statusFilter !== "all",
    acceptsQuestionsFilter !== "all",
    acceptsAnswersFilter !== "all",
  ].filter(Boolean).length;
  const clearFilters = () => resetFilters();

  const spaceQuery = useSpaceList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    visibility: apiVisibility,
    status: apiStatus,
    acceptsQuestions: apiAcceptsQuestions,
    acceptsAnswers: apiAcceptsAnswers,
  });

  useEffect(() => {
    const totalCount = spaceQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [page, pageSize, setPage, spaceQuery.data?.totalCount]);

  const deleteSpace = useDeleteSpace();
  const spaceRows = spaceQuery.data?.items ?? [];
  const publicCount = spaceRows.filter(
    (space) => space.visibility === VisibilityScope.Public,
  ).length;
  const activeCount = spaceRows.filter(
    (space) => space.status === SpaceStatus.Active,
  ).length;
  const questionIntakeCount = spaceRows.filter(
    (space) => space.acceptsQuestions,
  ).length;

  const columns: DataTableColumn<SpaceDto>[] = [
    {
      key: "name",
      header: "Space",
      className: "xl:min-w-[360px]",
      cell: (space) => (
        <div className="flex min-w-0 gap-3">
          <span className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-lg border border-primary/15 bg-primary/[0.055] text-primary">
            <FolderKanban className="size-4" />
          </span>
          <div className="min-w-0 space-y-1">
            <div className="min-w-0 break-words font-medium text-mono [overflow-wrap:anywhere]">
              {space.name}
            </div>
            <div className="text-sm text-muted-foreground">
              {space.slug} • {space.language}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: "status",
      header: "Status",
      className: "xl:w-[170px]",
      cell: (space) => (
        <div className="space-y-2">
          <SpaceStatusBadge status={space.status} />
          <div className="flex flex-wrap gap-2">
            <Badge
              variant={space.acceptsQuestions ? "success" : "mono"}
              appearance="outline"
            >
              {translateText(
                space.acceptsQuestions
                  ? "Questions enabled"
                  : "Questions disabled",
              )}
            </Badge>
            <Badge
              variant={space.acceptsAnswers ? "success" : "mono"}
              appearance="outline"
            >
              {translateText(
                space.acceptsAnswers ? "Answers enabled" : "Answers disabled",
              )}
            </Badge>
          </div>
        </div>
      ),
    },
    {
      key: "visibility",
      header: "Visibility",
      className: "xl:w-[120px]",
      cell: (space) => <VisibilityBadge visibility={space.visibility} />,
    },
    {
      key: "questions",
      header: "Questions",
      className: "xl:w-[92px]",
      cell: (space) => (
        <div className="text-sm font-medium text-foreground">
          {space.questionCount}
        </div>
      ),
    },
    {
      key: "lastUpdatedAtUtc",
      header: "Last update",
      className: "xl:w-[128px]",
      cell: (space) => (
        <span className="break-words text-sm text-muted-foreground">
          {formatOptionalDateTimeInTimeZone(
            space.lastUpdatedAtUtc,
            portalTimeZone,
            translateText("No update"),
          )}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "xl:w-[108px]",
      cell: (space) => (
        <div
          className="flex min-w-0 flex-wrap items-center justify-start gap-1 lg:justify-end"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="outline" size="sm" mode="icon">
            <Link to={`/app/spaces/${space.id}`}>
              <Eye className="size-4" />
              <span className="sr-only">{translateText("Open")}</span>
            </Link>
          </Button>
          <Button asChild variant="primary" size="sm" mode="icon">
            <Link to={`/app/spaces/${space.id}/edit`}>
              <Pencil className="size-4" />
              <span className="sr-only">{translateText("Edit")}</span>
            </Link>
          </Button>
          <ConfirmAction
            title={translateText('Delete space "{name}"?', {
              name: space.name,
            })}
            description={translateText(
              "This removes the space, its workflow configuration, and any curated links from the portal.",
            )}
            confirmLabel={translateText("Delete space")}
            isPending={deleteSpace.isPending}
            onConfirm={() => deleteSpace.mutateAsync(space.id)}
            trigger={
              <Button variant="ghost" size="sm" mode="icon">
                <Trash2 className="size-4 text-destructive" />
                <span className="sr-only">{translateText("Delete")}</span>
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
            title="Spaces"
            description="Operate QnA spaces by status, visibility, and intake capability."
            descriptionMode="hint"
          />
        </>
      }
      filters={
        <ListFilterDisclosure
          search={
            <ListFilterSearch
              value={search}
              onChange={setSearch}
              placeholder="Search spaces by name, slug, summary, or language"
              activeFilterCount={activeFilterCount}
              onClear={clearFilters}
              isLoading={spaceQuery.isFetching}
            />
          }
          activeFilterCount={refinementFilterCount}
          isLoading={spaceQuery.isFetching}
        >
          <div className="space-y-3">
            <ListFilterSection
              label="Quick filters"
              activeFilterCount={refinementFilterCount}
              emptyLabel="All spaces"
            >
              <ListFilterChipRail>
                {spaceStatusBuckets.map((bucket) => (
                  <ListFilterChip
                    key={bucket.value}
                    active={
                      bucket.value === "all"
                        ? quickAllActive
                        : statusFilter === bucket.value
                    }
                    onClick={() => {
                      if (bucket.value === "all") {
                        setFilters({
                          status: "all",
                          acceptsQuestions: "all",
                          acceptsAnswers: "all",
                        });
                        return;
                      }

                      setFilter("status", bucket.value);
                    }}
                  >
                    {translateText(bucket.label)}
                  </ListFilterChip>
                ))}
                <ListFilterChip
                  active={acceptsQuestionsFilter === "true"}
                  onClick={() =>
                    setFilter(
                      "acceptsQuestions",
                      acceptsQuestionsFilter === "true" ? "all" : "true",
                    )
                  }
                >
                  {translateText("Questions enabled")}
                </ListFilterChip>
                <ListFilterChip
                  active={acceptsAnswersFilter === "true"}
                  onClick={() =>
                    setFilter(
                      "acceptsAnswers",
                      acceptsAnswersFilter === "true" ? "all" : "true",
                    )
                  }
                >
                  {translateText("Answers enabled")}
                </ListFilterChip>
              </ListFilterChipRail>
            </ListFilterSection>
            <ListFilterToolbar isLoading={spaceQuery.isFetching}>
              <div className="grid w-full gap-3 md:grid-cols-2 xl:grid-cols-4">
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
                <ListFilterField label="Questions">
                  <Select
                    value={acceptsQuestionsFilter}
                    onValueChange={(value) =>
                      setFilter("acceptsQuestions", value)
                    }
                  >
                    <SelectTrigger className="w-full" size="lg">
                      <SelectValue
                        placeholder={translateText("Question intake")}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All question states</SelectItem>
                      <SelectItem value="true">Questions enabled</SelectItem>
                      <SelectItem value="false">Questions disabled</SelectItem>
                    </SelectContent>
                  </Select>
                </ListFilterField>
                <ListFilterField label="Answers">
                  <Select
                    value={acceptsAnswersFilter}
                    onValueChange={(value) =>
                      setFilter("acceptsAnswers", value)
                    }
                  >
                    <SelectTrigger className="w-full" size="lg">
                      <SelectValue
                        placeholder={translateText("Answer intake")}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All answer states</SelectItem>
                      <SelectItem value="true">Answers enabled</SelectItem>
                      <SelectItem value="false">Answers disabled</SelectItem>
                    </SelectContent>
                  </Select>
                </ListFilterField>
                <ListFilterField label="Sort">
                  <Select value={sorting} onValueChange={setSorting}>
                    <SelectTrigger className="w-full" size="lg">
                      <SelectValue placeholder={translateText("Sort spaces")} />
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
          </div>
        </ListFilterDisclosure>
      }
    >
      <DataTable
        title="Spaces"
        description="Open a space to review its status, curated sources, and question volume."
        descriptionMode="hint"
        columns={columns}
        rows={spaceRows}
        getRowId={(row) => row.id}
        loading={spaceQuery.isLoading}
        onRowClick={(space) => navigate(`/app/spaces/${space.id}`)}
        toolbar={
          <div className="flex w-full min-w-0 flex-wrap items-start gap-2 sm:items-center">
            <ListResultSummary
              className="flex-1 basis-0"
              isLoading={spaceQuery.isLoading && spaceQuery.data === undefined}
              items={[
                {
                  label: "Results",
                  value: spaceQuery.data?.totalCount ?? 0,
                  description: debouncedSearch
                    ? translateText("Search: {value}", {
                        value: debouncedSearch,
                      })
                    : "QnA spaces in this workspace",
                  tone: "primary",
                },
                {
                  label: "Public",
                  value: publicCount,
                  description: "Spaces visible outside internal operations",
                  tone: "info",
                },
                {
                  label: "Active",
                  value: activeCount,
                  description: "Spaces available for active QnA work",
                  tone: "success",
                },
                {
                  label: "Accepting questions",
                  value: questionIntakeCount,
                  description: "Spaces accepting new questions",
                  tone: "warning",
                },
              ]}
            />
            <Button asChild className="ms-auto shrink-0">
              <Link to="/app/spaces/new">
                <Plus className="size-4" />
                {translateText("New space")}
              </Link>
            </Button>
          </div>
        }
        emptyState={
          <EmptyState
            title="No spaces in view"
            description="Create the first QnA space to define status, exposure, and question ownership."
            action={{ label: "New space", to: "/app/spaces/new" }}
          />
        }
        errorState={
          spaceQuery.isError ? (
            <ErrorState
              title="Unable to load spaces"
              error={spaceQuery.error}
              retry={() => void spaceQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          spaceQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={spaceQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={spaceQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
