import { useEffect, useMemo } from "react";
import { Link2, Pencil, Plus, Trash2, Waypoints } from "lucide-react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAnswer, useRemoveAnswerSource } from "@/domains/answers/hooks";
import {
  useQuestion,
  useRemoveQuestionSource,
} from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import {
  useDeleteSource,
  useSourceList,
  useSourceUploadStatusListNotifications,
} from "@/domains/sources/hooks";
import type { SourceDto } from "@/domains/sources/types";
import { useRemoveSpaceSource, useSpace } from "@/domains/spaces/hooks";
import {
  SourceRole,
  SourceUploadStatus,
  sourceUploadStatusLabels,
} from "@/shared/constants/backend-enums";
import { ListLayout, PageHeader } from "@/shared/layout/page-layouts";
import { translateText } from "@/shared/lib/i18n-core";
import { clampPage } from "@/shared/lib/pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
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
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  SourceRoleBadge,
  SourceUploadStatusBadge,
} from "@/shared/ui/status-badges";

const sortingOptions = [
  { value: "LastUpdatedAtUtc DESC", label: "Last update newest" },
  { value: "LastUpdatedAtUtc ASC", label: "Last update oldest" },
  { value: "Label ASC", label: "Label A-Z" },
  { value: "Label DESC", label: "Label Z-A" },
  { value: "Locator ASC", label: "Locator" },
  { value: "Locator DESC", label: "Locator Z-A" },
  { value: "LinkedRecordCount DESC", label: "Linked records high-low" },
  { value: "LinkedRecordCount ASC", label: "Linked records low-high" },
  { value: "SpaceUsageCount DESC", label: "Spaces high-low" },
  { value: "SpaceUsageCount ASC", label: "Spaces low-high" },
  { value: "QuestionUsageCount DESC", label: "Questions high-low" },
  { value: "QuestionUsageCount ASC", label: "Questions low-high" },
  { value: "AnswerUsageCount DESC", label: "Answers high-low" },
  { value: "AnswerUsageCount ASC", label: "Answers low-high" },
];

const SOURCE_FILTER_DEFAULTS = {
  uploadStatus: "all",
} as const;

type SourceRelationshipKind = "space" | "question" | "answer";

type SourceListRow = SourceDto & {
  relationship?: {
    parentKind: SourceRelationshipKind;
    linkId?: string;
    role?: SourceRole;
    order?: number;
  };
};

function sourceMatchesSearch(source: SourceListRow, searchText: string) {
  if (!searchText) {
    return true;
  }

  const normalizedSearch = searchText.toLowerCase();
  return [
    source.label,
    source.locator,
    source.language,
    source.contextNote,
    source.externalId,
  ]
    .filter(Boolean)
    .some((value) => String(value).toLowerCase().includes(normalizedSearch));
}

function isExternalUrl(locator: string | null | undefined) {
  return /^https?:\/\//i.test(locator?.trim() ?? "");
}

function shouldShowExternalUploadStatusOnly(source: SourceDto) {
  return (
    !source.storageKey &&
    source.uploadStatus === SourceUploadStatus.None &&
    isExternalUrl(source.locator)
  );
}

function sourceMatchesFilters(
  source: SourceListRow,
  {
    searchText,
    uploadStatusFilter,
  }: {
    searchText: string;
    uploadStatusFilter: string;
  },
) {
  const matchesUploadStatus =
    uploadStatusFilter === "all" ||
    source.uploadStatus === Number(uploadStatusFilter);

  return matchesUploadStatus && sourceMatchesSearch(source, searchText);
}

function compareText(
  left: string | null | undefined,
  right: string | null | undefined,
) {
  return (left ?? "").localeCompare(right ?? "");
}

function compareDate(
  left: string | null | undefined,
  right: string | null | undefined,
) {
  return (
    (left ? new Date(left).getTime() : 0) -
    (right ? new Date(right).getTime() : 0)
  );
}

function sourceLinkedRecordCount(source: SourceListRow) {
  return (
    source.spaceUsageCount + source.questionUsageCount + source.answerUsageCount
  );
}

function sortSources(sources: SourceListRow[], sorting: string) {
  const normalizedSorting = sorting.trim().toLowerCase();
  const sortedSources = [...sources];

  sortedSources.sort((left, right) => {
    switch (normalizedSorting) {
      case "label asc":
      case "label":
        return (
          compareText(left.label, right.label) ||
          compareText(left.locator, right.locator)
        );
      case "label desc":
        return (
          compareText(right.label, left.label) ||
          compareText(left.locator, right.locator)
        );
      case "locator asc":
      case "locator":
        return compareText(left.locator, right.locator);
      case "locator desc":
        return compareText(right.locator, left.locator);
      case "linkedrecordcount asc":
      case "linkedrecordcount":
        return sourceLinkedRecordCount(left) - sourceLinkedRecordCount(right);
      case "linkedrecordcount desc":
        return sourceLinkedRecordCount(right) - sourceLinkedRecordCount(left);
      case "spaceusagecount asc":
      case "spaceusagecount":
        return left.spaceUsageCount - right.spaceUsageCount;
      case "spaceusagecount desc":
        return right.spaceUsageCount - left.spaceUsageCount;
      case "questionusagecount asc":
      case "questionusagecount":
        return left.questionUsageCount - right.questionUsageCount;
      case "questionusagecount desc":
        return right.questionUsageCount - left.questionUsageCount;
      case "answerusagecount asc":
      case "answerusagecount":
        return left.answerUsageCount - right.answerUsageCount;
      case "answerusagecount desc":
        return right.answerUsageCount - left.answerUsageCount;
      case "lastupdatedatutc asc":
      case "lastupdatedatutc":
        return compareDate(left.lastUpdatedAtUtc, right.lastUpdatedAtUtc);
      default:
        return (
          compareDate(right.lastUpdatedAtUtc, left.lastUpdatedAtUtc) ||
          compareText(left.label, right.label)
        );
    }
  });

  return sortedSources;
}

export function SourceListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  useSourceUploadStatusListNotifications();
  const [searchParams] = useSearchParams();
  const spaceId = searchParams.get("spaceId") ?? "";
  const questionId = searchParams.get("questionId") ?? "";
  const answerId = searchParams.get("answerId") ?? "";
  const relationshipKind: SourceRelationshipKind | undefined = spaceId
    ? "space"
    : questionId
      ? "question"
      : answerId
        ? "answer"
        : undefined;
  const relationshipActive = Boolean(relationshipKind);
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
    filterDefaults: SOURCE_FILTER_DEFAULTS,
  });
  const uploadStatusFilter = filters.uploadStatus;
  const quickAllActive = uploadStatusFilter === "all";
  const activeFilterCount = [
    search.trim(),
    uploadStatusFilter !== "all",
  ].filter(Boolean).length;
  const refinementFilterCount = [uploadStatusFilter !== "all"].filter(
    Boolean,
  ).length;
  const clearFilters = () => resetFilters();

  const spaceQuery = useSpace(spaceId || undefined);
  const questionQuery = useQuestion(questionId || undefined);
  const answerQuery = useAnswer(answerId || undefined);
  const sourceQuery = useSourceList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    enabled: !relationshipActive,
  });
  const deleteSource = useDeleteSource();
  const removeSpaceSource = useRemoveSpaceSource(spaceId);
  const removeQuestionSource = useRemoveQuestionSource(questionId);
  const removeAnswerSource = useRemoveAnswerSource(answerId);

  const relationshipRows = useMemo<SourceListRow[]>(() => {
    if (spaceId) {
      return (spaceQuery.data?.curatedSources ?? []).map((source) => ({
        ...source,
        relationship: { parentKind: "space" },
      }));
    }

    if (questionId) {
      return (questionQuery.data?.sources ?? [])
        .map((link) =>
          link.source
            ? {
                ...link.source,
                relationship: {
                  parentKind: "question" as const,
                  linkId: link.id,
                  role: link.role,
                  order: link.order,
                },
              }
            : null,
        )
        .filter((source): source is SourceListRow => Boolean(source));
    }

    if (answerId) {
      return (answerQuery.data?.sources ?? [])
        .map((link) =>
          link.source
            ? {
                ...link.source,
                relationship: {
                  parentKind: "answer" as const,
                  linkId: link.id,
                  role: link.role,
                  order: link.order,
                },
              }
            : null,
        )
        .filter((source): source is SourceListRow => Boolean(source));
    }

    return [];
  }, [
    answerId,
    answerQuery.data?.sources,
    questionId,
    questionQuery.data?.sources,
    spaceId,
    spaceQuery.data?.curatedSources,
  ]);

  const sourceRows = relationshipActive
    ? sortSources(
        relationshipRows.filter((source) =>
          sourceMatchesFilters(source, {
            searchText: debouncedSearch,
            uploadStatusFilter,
          }),
        ),
        sorting,
      )
    : ((sourceQuery.data?.items ?? []) as SourceListRow[]).filter(
        (source) =>
          uploadStatusFilter === "all" ||
          source.uploadStatus === Number(uploadStatusFilter),
      );
  const relationshipLoading =
    (spaceId && spaceQuery.isLoading && !spaceQuery.data) ||
    (questionId && questionQuery.isLoading && !questionQuery.data) ||
    (answerId && answerQuery.isLoading && !answerQuery.data);
  const relationshipFetching =
    (spaceId && spaceQuery.isFetching) ||
    (questionId && questionQuery.isFetching) ||
    (answerId && answerQuery.isFetching);
  const filtersLoading = relationshipActive
    ? Boolean(relationshipFetching)
    : sourceQuery.isFetching;
  const relationshipError =
    (spaceId && spaceQuery.isError) ||
    (questionId && questionQuery.isError) ||
    (answerId && answerQuery.isError);
  const scopeName =
    spaceQuery.data?.name ??
    questionQuery.data?.title ??
    answerQuery.data?.headline ??
    "";
  const scopeParentTo = spaceId
    ? `/app/spaces/${spaceId}`
    : questionId
      ? `/app/questions/${questionId}`
      : answerId
        ? `/app/answers/${answerId}`
        : undefined;
  const scopeLabel = spaceId
    ? "Space"
    : questionId
      ? "Question"
      : answerId
        ? "Answer"
        : undefined;

  useEffect(() => {
    if (relationshipActive) {
      return;
    }

    const totalCount = sourceQuery.data?.totalCount;

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
    relationshipActive,
    setPage,
    sourceQuery.data?.totalCount,
  ]);

  const linkedRecordCount = sourceRows.reduce(
    (total, source) =>
      total +
      source.spaceUsageCount +
      source.questionUsageCount +
      source.answerUsageCount,
    0,
  );

  const detachSource = (source: SourceListRow) => {
    if (source.relationship?.parentKind === "space") {
      return removeSpaceSource.mutateAsync(source.id);
    }

    if (
      source.relationship?.parentKind === "question" &&
      source.relationship.linkId
    ) {
      return removeQuestionSource.mutateAsync(source.relationship.linkId);
    }

    if (
      source.relationship?.parentKind === "answer" &&
      source.relationship.linkId
    ) {
      return removeAnswerSource.mutateAsync(source.relationship.linkId);
    }

    return Promise.resolve();
  };

  const columns: DataTableColumn<SourceListRow>[] = [
    {
      key: "source",
      header: "Source",
      className: "xl:min-w-[420px]",
      cell: (source) => (
        <div className="flex min-w-0 gap-3">
          <span className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-lg border border-cyan-500/15 bg-cyan-500/[0.055] text-cyan-600 dark:text-cyan-300">
            <Waypoints className="size-4" />
          </span>
          <div className="min-w-0 space-y-1">
            <div className="min-w-0 break-words font-medium text-mono [overflow-wrap:anywhere]">
              {source.label || translateText("Untitled source")}
            </div>
            <div className="break-all text-sm text-muted-foreground">
              {source.locator}
              {source.language ? ` • ${source.language}` : null}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: "origin",
      header: "Origin",
      className: "xl:w-[130px]",
      cell: (source) => (
        <div className="space-y-2">
          {shouldShowExternalUploadStatusOnly(source) ? null : (
            <Badge
              variant={source.storageKey ? "info" : "outline"}
              appearance="outline"
            >
              {translateText(source.storageKey ? "File" : "URL")}
            </Badge>
          )}
          <SourceUploadStatusBadge status={source.uploadStatus} />
        </div>
      ),
    },
    {
      key: "usage",
      header: relationshipActive ? "Relationship" : "Where used",
      className: relationshipActive ? "xl:w-[150px]" : "xl:w-[190px]",
      cell: (source) =>
        source.relationship ? (
          <div className="flex min-w-0 flex-wrap gap-2">
            <Badge variant="primary" appearance="outline">
              {translateText(scopeLabel ?? "Scoped")}
            </Badge>
            {source.relationship.role !== undefined ? (
              <SourceRoleBadge role={source.relationship.role} />
            ) : null}
            {source.relationship.order !== undefined ? (
              <Badge variant="outline">
                {translateText("Order {value}", {
                  value: source.relationship.order,
                })}
              </Badge>
            ) : null}
          </div>
        ) : (
          <div className="flex min-w-0 flex-wrap gap-2">
            <Badge variant={source.spaceUsageCount > 0 ? "primary" : "outline"}>
              {translateText("{count} spaces", {
                count: source.spaceUsageCount,
              })}
            </Badge>
            <Badge
              variant={source.questionUsageCount > 0 ? "secondary" : "outline"}
            >
              {translateText("{count} questions", {
                count: source.questionUsageCount,
              })}
            </Badge>
            <Badge
              variant={source.answerUsageCount > 0 ? "success" : "outline"}
            >
              {translateText("{count} answers", {
                count: source.answerUsageCount,
              })}
            </Badge>
          </div>
        ),
    },
    {
      key: "lastUpdatedAtUtc",
      header: "Last update",
      className: "xl:w-[128px]",
      cell: (source) => (
        <span className="break-words text-sm text-muted-foreground">
          {formatOptionalDateTimeInTimeZone(
            source.lastUpdatedAtUtc,
            portalTimeZone,
            translateText("No update"),
          )}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: relationshipActive ? "xl:w-[82px]" : "xl:w-[108px]",
      cell: (source) => (
        <div
          className="flex min-w-0 flex-wrap items-center justify-start gap-1 lg:justify-end"
          onClick={(event) => event.stopPropagation()}
        >
          {relationshipActive ? (
            <>
              <Button asChild variant="outline" size="sm" mode="icon">
                <Link to={`/app/sources/${source.id}`}>
                  <Link2 className="size-4" />
                  <span className="sr-only">{translateText("Open")}</span>
                </Link>
              </Button>
              <Button
                variant="ghost"
                size="sm"
                mode="icon"
                disabled={
                  removeSpaceSource.isPending ||
                  removeQuestionSource.isPending ||
                  removeAnswerSource.isPending
                }
                onClick={() => void detachSource(source)}
              >
                <Trash2 className="size-4" />
                <span className="sr-only">{translateText("Detach")}</span>
              </Button>
            </>
          ) : (
            <>
              <Button asChild variant="outline" size="sm" mode="icon">
                <Link to={`/app/sources/${source.id}`}>
                  <Link2 className="size-4" />
                  <span className="sr-only">{translateText("Open")}</span>
                </Link>
              </Button>
              <Button asChild variant="primary" size="sm" mode="icon">
                <Link to={`/app/sources/${source.id}/edit`}>
                  <Pencil className="size-4" />
                  <span className="sr-only">{translateText("Edit")}</span>
                </Link>
              </Button>
              <ConfirmAction
                title={translateText('Delete source "{name}"?', {
                  name: source.label || source.locator,
                })}
                description={translateText(
                  "This removes the source from the portal catalog and from future attachment flows.",
                )}
                confirmLabel={translateText("Delete source")}
                isPending={deleteSource.isPending}
                onConfirm={() => deleteSource.mutateAsync(source.id)}
                trigger={
                  <Button variant="ghost" size="sm" mode="icon">
                    <Trash2 className="size-4 text-destructive" />
                    <span className="sr-only">{translateText("Delete")}</span>
                  </Button>
                }
              />
            </>
          )}
        </div>
      ),
    },
  ];

  return (
    <ListLayout
      header={
        <>
          <PageHeader
            title={
              relationshipActive
                ? translateText("Sources for this {kind}", {
                    kind: scopeLabel?.toLowerCase() ?? "record",
                  })
                : "Sources"
            }
            description={
              relationshipActive
                ? scopeName
                  ? translateText(
                      "Showing only source links attached to {name}.",
                      {
                        name: scopeName,
                      },
                    )
                  : translateText("Showing only source links in this context.")
                : "Maintain the evidence and reusable reference material behind questions and answers."
            }
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
              placeholder="Search sources"
              activeFilterCount={activeFilterCount}
              onClear={clearFilters}
              isLoading={filtersLoading}
            />
          }
          activeFilterCount={refinementFilterCount}
          isLoading={filtersLoading}
        >
          <div className="space-y-3">
            <ListFilterSection
              label="Quick filters"
              activeFilterCount={refinementFilterCount}
              emptyLabel={
                relationshipActive ? "All related sources" : "All sources"
              }
            >
              <ListFilterChipRail>
                <ListFilterChip
                  active={quickAllActive}
                  onClick={() => {
                    setFilters({
                      uploadStatus: "all",
                    });
                  }}
                >
                  {translateText("All")}
                </ListFilterChip>
              </ListFilterChipRail>
            </ListFilterSection>
            <ListFilterToolbar isLoading={filtersLoading}>
              <div className="grid w-full gap-3 md:grid-cols-2">
                <ListFilterField label="Upload status">
                  <Select
                    value={uploadStatusFilter}
                    onValueChange={(value) => setFilter("uploadStatus", value)}
                  >
                    <SelectTrigger className="w-full" size="lg">
                      <SelectValue
                        placeholder={translateText("Upload status")}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All upload status</SelectItem>
                      {Object.entries(sourceUploadStatusLabels).map(
                        ([value, label]) => (
                          <SelectItem key={value} value={value}>
                            {translateText(label)}
                          </SelectItem>
                        ),
                      )}
                    </SelectContent>
                  </Select>
                </ListFilterField>
                <ListFilterField label="Sort">
                  <Select value={sorting} onValueChange={setSorting}>
                    <SelectTrigger className="w-full" size="lg">
                      <SelectValue
                        placeholder={translateText("Sort sources")}
                      />
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
        title={relationshipActive ? "Related sources" : "Sources"}
        description={
          relationshipActive
            ? "This list is scoped to the selected relationship. Detach removes the link, not the source record."
            : "Open a source to review locator, metadata, and external identifiers."
        }
        descriptionMode="hint"
        columns={columns}
        rows={sourceRows}
        getRowId={(row) =>
          row.relationship?.linkId
            ? `${row.relationship.parentKind}:${row.relationship.linkId}`
            : row.id
        }
        loading={
          relationshipActive
            ? Boolean(relationshipLoading)
            : sourceQuery.isLoading
        }
        onRowClick={(source) => navigate(`/app/sources/${source.id}`)}
        toolbar={
          <div className="flex w-full min-w-0 flex-wrap items-start gap-2 sm:items-center">
            <ListResultSummary
              className="flex-1 basis-0"
              isLoading={
                relationshipActive
                  ? Boolean(relationshipLoading)
                  : sourceQuery.isLoading && sourceQuery.data === undefined
              }
              items={[
                {
                  label: relationshipActive ? "Related" : "Results",
                  value: relationshipActive
                    ? sourceRows.length
                    : (sourceQuery.data?.totalCount ?? 0),
                  description: relationshipActive
                    ? "Source links in the current context"
                    : debouncedSearch
                      ? translateText("Search: {value}", {
                          value: debouncedSearch,
                        })
                      : "Reusable source records in this workspace",
                  tone: "primary",
                },
                {
                  label: relationshipActive ? "Context" : "Linked records",
                  value: relationshipActive
                    ? translateText(scopeLabel ?? "Scoped")
                    : linkedRecordCount,
                  description: relationshipActive
                    ? "Filtered from a relationship section"
                    : "Attachments across spaces, questions, and answers",
                  tone: "success",
                },
              ]}
            />
            {relationshipActive && scopeParentTo ? (
              <Button asChild className="ms-auto shrink-0">
                <Link to={scopeParentTo}>
                  <Link2 className="size-4" />
                  {translateText("Manage relationship")}
                </Link>
              </Button>
            ) : (
              <Button asChild className="ms-auto shrink-0">
                <Link to="/app/sources/new">
                  <Plus className="size-4" />
                  {translateText("New source")}
                </Link>
              </Button>
            )}
          </div>
        }
        emptyState={
          <EmptyState
            title={
              relationshipActive
                ? "No sources linked here"
                : "No sources in view"
            }
            description={
              relationshipActive
                ? "Open the parent record to attach an existing source to this relationship."
                : "Create a source record so questions and answers can cite stable evidence."
            }
            action={
              relationshipActive && scopeParentTo
                ? { label: "Manage relationship", to: scopeParentTo }
                : { label: "New source", to: "/app/sources/new" }
            }
          />
        }
        errorState={
          relationshipActive && relationshipError ? (
            <ErrorState
              title="Unable to load related sources"
              description="The parent record could not be loaded for this relationship view."
            />
          ) : !relationshipActive && sourceQuery.isError ? (
            <ErrorState
              title="Unable to load sources"
              error={sourceQuery.error}
              retry={() => void sourceQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          !relationshipActive && sourceQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={sourceQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={sourceQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
