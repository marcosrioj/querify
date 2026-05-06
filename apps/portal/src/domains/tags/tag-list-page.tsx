import { useEffect, useMemo } from "react";
import { Link2, Pencil, Plus, Tags, Trash2 } from "lucide-react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useQuestion, useRemoveQuestionTag } from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useRemoveSpaceTag, useSpace } from "@/domains/spaces/hooks";
import { useDeleteTag, useTagList } from "@/domains/tags/hooks";
import type { TagDto } from "@/domains/tags/types";
import { ListLayout, PageHeader } from "@/shared/layout/page-layouts";
import { translateText } from "@/shared/lib/i18n-core";
import { clampPage } from "@/shared/lib/pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import {
  Badge,
  Button,
  ConfirmAction,
  ListFilterField,
  ListFilterSearch,
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

const TAG_FILTER_DEFAULTS = {} as const;

const sortingOptions = [
  { value: "LastUpdatedAtUtc DESC", label: "Last update newest" },
  { value: "LastUpdatedAtUtc ASC", label: "Last update oldest" },
  { value: "Name ASC", label: "Name A-Z" },
  { value: "Name DESC", label: "Name Z-A" },
  { value: "LinkedRecordCount DESC", label: "Linked records high-low" },
  { value: "LinkedRecordCount ASC", label: "Linked records low-high" },
  { value: "SpaceUsageCount DESC", label: "Spaces high-low" },
  { value: "SpaceUsageCount ASC", label: "Spaces low-high" },
  { value: "QuestionUsageCount DESC", label: "Questions high-low" },
  { value: "QuestionUsageCount ASC", label: "Questions low-high" },
];

type TagRelationshipKind = "space" | "question";

type TagListRow = TagDto & {
  relationship?: {
    parentKind: TagRelationshipKind;
  };
};

function tagMatchesSearch(tag: TagListRow, searchText: string) {
  if (!searchText) {
    return true;
  }

  return tag.name.toLowerCase().includes(searchText.toLowerCase());
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

function tagLinkedRecordCount(tag: TagListRow) {
  return tag.spaceUsageCount + tag.questionUsageCount;
}

function sortTags(tags: TagListRow[], sorting: string) {
  const normalizedSorting = sorting.trim().toLowerCase();
  const sortedTags = [...tags];

  sortedTags.sort((left, right) => {
    switch (normalizedSorting) {
      case "name asc":
      case "name":
        return left.name.localeCompare(right.name);
      case "name desc":
        return right.name.localeCompare(left.name);
      case "linkedrecordcount asc":
      case "linkedrecordcount":
        return tagLinkedRecordCount(left) - tagLinkedRecordCount(right);
      case "linkedrecordcount desc":
        return tagLinkedRecordCount(right) - tagLinkedRecordCount(left);
      case "spaceusagecount desc":
        return right.spaceUsageCount - left.spaceUsageCount;
      case "spaceusagecount asc":
      case "spaceusagecount":
        return left.spaceUsageCount - right.spaceUsageCount;
      case "questionusagecount desc":
        return right.questionUsageCount - left.questionUsageCount;
      case "questionusagecount asc":
      case "questionusagecount":
        return left.questionUsageCount - right.questionUsageCount;
      case "lastupdatedatutc asc":
      case "lastupdatedatutc":
        return compareDate(left.lastUpdatedAtUtc, right.lastUpdatedAtUtc);
      default:
        return (
          compareDate(right.lastUpdatedAtUtc, left.lastUpdatedAtUtc) ||
          left.name.localeCompare(right.name)
        );
    }
  });

  return sortedTags;
}

export function TagListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const [searchParams] = useSearchParams();
  const spaceId = searchParams.get("spaceId") ?? "";
  const questionId = searchParams.get("questionId") ?? "";
  const relationshipKind: TagRelationshipKind | undefined = spaceId
    ? "space"
    : questionId
      ? "question"
      : undefined;
  const relationshipActive = Boolean(relationshipKind);
  const {
    debouncedSearch,
    page,
    pageSize,
    resetFilters,
    search,
    setPage,
    setPageSize,
    setSearch,
    setSorting,
    sorting,
  } = useListQueryState({
    defaultSorting: "LastUpdatedAtUtc DESC",
    filterDefaults: TAG_FILTER_DEFAULTS,
  });
  const activeFilterCount = search.trim() ? 1 : 0;
  const clearFilters = () => resetFilters();

  const spaceQuery = useSpace(spaceId || undefined);
  const questionQuery = useQuestion(questionId || undefined);
  const tagQuery = useTagList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    enabled: !relationshipActive,
  });
  const deleteTag = useDeleteTag();
  const removeSpaceTag = useRemoveSpaceTag(spaceId);
  const removeQuestionTag = useRemoveQuestionTag(questionId);

  const relationshipRows = useMemo<TagListRow[]>(() => {
    if (spaceId) {
      return (spaceQuery.data?.tags ?? []).map((tag) => ({
        ...tag,
        relationship: { parentKind: "space" },
      }));
    }

    if (questionId) {
      return (questionQuery.data?.tags ?? []).map((tag) => ({
        ...tag,
        relationship: { parentKind: "question" },
      }));
    }

    return [];
  }, [questionId, questionQuery.data?.tags, spaceId, spaceQuery.data?.tags]);
  const tagRows = relationshipActive
    ? sortTags(
        relationshipRows.filter((tag) =>
          tagMatchesSearch(tag, debouncedSearch),
        ),
        sorting,
      )
    : ((tagQuery.data?.items ?? []) as TagListRow[]);
  const relationshipLoading =
    (spaceId && spaceQuery.isLoading && !spaceQuery.data) ||
    (questionId && questionQuery.isLoading && !questionQuery.data);
  const relationshipFetching =
    (spaceId && spaceQuery.isFetching) ||
    (questionId && questionQuery.isFetching);
  const filtersLoading = relationshipActive
    ? Boolean(relationshipFetching)
    : tagQuery.isFetching;
  const relationshipError =
    (spaceId && spaceQuery.isError) || (questionId && questionQuery.isError);
  const scopeName = spaceQuery.data?.name ?? questionQuery.data?.title ?? "";
  const scopeParentTo = spaceId
    ? `/app/spaces/${spaceId}`
    : questionId
      ? `/app/questions/${questionId}`
      : undefined;
  const scopeLabel = spaceId ? "Space" : questionId ? "Question" : undefined;

  useEffect(() => {
    if (relationshipActive) {
      return;
    }

    const totalCount = tagQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [page, pageSize, relationshipActive, setPage, tagQuery.data?.totalCount]);

  const spaceUsageCount = tagRows.reduce(
    (total, tag) => total + tag.spaceUsageCount,
    0,
  );
  const questionUsageCount = tagRows.reduce(
    (total, tag) => total + tag.questionUsageCount,
    0,
  );

  const detachTag = (tag: TagListRow) => {
    if (tag.relationship?.parentKind === "space") {
      return removeSpaceTag.mutateAsync(tag.id);
    }

    if (tag.relationship?.parentKind === "question") {
      return removeQuestionTag.mutateAsync(tag.id);
    }

    return Promise.resolve();
  };

  const columns: DataTableColumn<TagListRow>[] = [
    {
      key: "name",
      header: "Tag",
      cell: (tag) => (
        <div className="flex min-w-0 gap-3">
          <span className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-lg border border-emerald-500/15 bg-emerald-500/[0.055] text-emerald-600 dark:text-emerald-300">
            <Tags className="size-4" />
          </span>
          <div className="min-w-0">
            <div className="min-w-0 break-words font-medium text-mono [overflow-wrap:anywhere]">
              {tag.name}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: "usage",
      header: relationshipActive ? "Relationship" : "Where used",
      className: "lg:w-[260px]",
      cell: (tag) =>
        tag.relationship ? (
          <div className="flex min-w-0 flex-wrap gap-2">
            <Badge variant="primary" appearance="outline">
              {translateText(scopeLabel ?? "Scoped")}
            </Badge>
          </div>
        ) : (
          <div className="flex min-w-0 flex-wrap gap-2">
            <Badge variant={tag.spaceUsageCount > 0 ? "primary" : "outline"}>
              {translateText("{count} spaces", {
                count: tag.spaceUsageCount,
              })}
            </Badge>
            <Badge
              variant={tag.questionUsageCount > 0 ? "secondary" : "outline"}
            >
              {translateText("{count} questions", {
                count: tag.questionUsageCount,
              })}
            </Badge>
          </div>
        ),
    },
    {
      key: "lastUpdatedAtUtc",
      header: "Last update",
      className: "lg:w-[160px]",
      cell: (tag) => (
        <span className="break-words text-sm text-muted-foreground">
          {formatOptionalDateTimeInTimeZone(
            tag.lastUpdatedAtUtc,
            portalTimeZone,
            translateText("No update"),
          )}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "lg:w-[190px]",
      cell: (tag) => (
        <div
          className="flex min-w-0 flex-wrap items-center justify-start gap-1 lg:justify-end"
          onClick={(event) => event.stopPropagation()}
        >
          {relationshipActive ? (
            <>
              <Button asChild variant="outline" size="sm">
                <Link to={`/app/tags/${tag.id}/edit`}>
                  <Pencil className="size-4" />
                  {translateText("Rename")}
                </Link>
              </Button>
              <Button
                variant="ghost"
                size="sm"
                disabled={
                  removeSpaceTag.isPending || removeQuestionTag.isPending
                }
                onClick={() => void detachTag(tag)}
              >
                <Trash2 className="size-4" />
                {translateText("Detach")}
              </Button>
            </>
          ) : (
            <>
              <Button asChild variant="outline" size="sm">
                <Link to={`/app/tags/${tag.id}/edit`}>
                  <Pencil className="size-4" />
                  {translateText("Rename")}
                </Link>
              </Button>
              <ConfirmAction
                title={translateText('Delete tag "{name}"?', {
                  name: tag.name,
                })}
                description={translateText(
                  "This removes the reusable tag from the workspace taxonomy.",
                )}
                confirmLabel={translateText("Delete tag")}
                isPending={deleteTag.isPending}
                onConfirm={() => deleteTag.mutateAsync(tag.id)}
                trigger={
                  <Button variant="ghost" size="sm">
                    <Trash2 className="size-4 text-destructive" />
                    {translateText("Delete")}
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
                ? translateText("Tags for this {kind}", {
                    kind: scopeLabel?.toLowerCase() ?? "record",
                  })
                : "Tags"
            }
            description={
              relationshipActive
                ? scopeName
                  ? translateText("Showing only tags attached to {name}.", {
                      name: scopeName,
                    })
                  : translateText("Showing only tags in this context.")
                : "Maintain the reusable taxonomy that groups spaces and questions."
            }
            descriptionMode="hint"
          />
        </>
      }
      filters={
        <div className="space-y-3">
          <ListFilterSearch
            value={search}
            onChange={setSearch}
            placeholder="Search tags"
            activeFilterCount={activeFilterCount}
            onClear={clearFilters}
            isLoading={filtersLoading}
          />
          <ListFilterToolbar isLoading={filtersLoading}>
            <ListFilterField label="Sort" className="max-w-sm">
              <Select value={sorting} onValueChange={setSorting}>
                <SelectTrigger className="w-full" size="lg">
                  <SelectValue placeholder={translateText("Sort tags")} />
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
          </ListFilterToolbar>
        </div>
      }
    >
      <DataTable
        title={relationshipActive ? "Related tags" : "Tags"}
        description={
          relationshipActive
            ? "This list is scoped to the selected relationship. Detach removes the link, not the tag."
            : "Open a tag to rename it before attaching it elsewhere."
        }
        descriptionMode="hint"
        columns={columns}
        rows={tagRows}
        getRowId={(row) =>
          row.relationship ? `${row.relationship.parentKind}:${row.id}` : row.id
        }
        loading={
          relationshipActive ? Boolean(relationshipLoading) : tagQuery.isLoading
        }
        onRowClick={(tag) => navigate(`/app/tags/${tag.id}/edit`)}
        toolbar={
          <div className="flex w-full min-w-0 items-center gap-2">
            <ListResultSummary
              className="flex-1"
              isLoading={
                relationshipActive
                  ? Boolean(relationshipLoading)
                  : tagQuery.isLoading && tagQuery.data === undefined
              }
              items={[
                {
                  label: relationshipActive ? "Related" : "Results",
                  value: relationshipActive
                    ? tagRows.length
                    : (tagQuery.data?.totalCount ?? 0),
                  tone: "primary",
                },
                {
                  label: "Spaces",
                  value: spaceUsageCount,
                  tone: "info",
                },
                {
                  label: "Questions",
                  value: questionUsageCount,
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
                <Link to="/app/tags/new">
                  <Plus className="size-4" />
                  {translateText("New tag")}
                </Link>
              </Button>
            )}
          </div>
        }
        emptyState={
          <EmptyState
            title={
              relationshipActive ? "No tags attached here" : "No tags in view"
            }
            description={
              relationshipActive
                ? "Open the parent record to attach a reusable tag to this relationship."
                : "Create the first reusable tag for space and question taxonomy."
            }
            action={
              relationshipActive && scopeParentTo
                ? { label: "Manage relationship", to: scopeParentTo }
                : { label: "New tag", to: "/app/tags/new" }
            }
          />
        }
        errorState={
          relationshipActive && relationshipError ? (
            <ErrorState
              title="Unable to load related tags"
              description="The parent record could not be loaded for this relationship view."
            />
          ) : !relationshipActive && tagQuery.isError ? (
            <ErrorState
              title="Unable to load tags"
              error={tagQuery.error}
              retry={() => void tagQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          !relationshipActive && tagQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={tagQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={tagQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
