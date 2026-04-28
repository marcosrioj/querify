import { useEffect, useMemo } from "react";
import {
  FolderKanban,
  Link2,
  MessageSquareText,
  Pencil,
  Plus,
  Tags,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useQuestion, useRemoveQuestionTag } from "@/domains/questions/hooks";
import { useRemoveSpaceTag, useSpace } from "@/domains/spaces/hooks";
import { useDeleteTag, useTagList } from "@/domains/tags/hooks";
import type { TagDto } from "@/domains/tags/types";
import {
  ListLayout,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { translateText } from "@/shared/lib/i18n-core";
import { clampPage } from "@/shared/lib/pagination";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import {
  Badge,
  Button,
  ConfirmAction,
  Input,
  SectionGridSkeleton,
} from "@/shared/ui";
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";

const TAG_FILTER_DEFAULTS = {} as const;

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

export function TagListPage() {
  const navigate = useNavigate();
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
    search,
    setPage,
    setPageSize,
    setSearch,
  } = useListQueryState({
    defaultSorting: "Name ASC",
    filterDefaults: TAG_FILTER_DEFAULTS,
  });

  const spaceQuery = useSpace(spaceId || undefined);
  const questionQuery = useQuestion(questionId || undefined);
  const tagQuery = useTagList({
    page,
    pageSize,
    sorting: "Name ASC",
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
    ? relationshipRows.filter((tag) => tagMatchesSearch(tag, debouncedSearch))
    : ((tagQuery.data?.items ?? []) as TagListRow[]);
  const relationshipLoading =
    (spaceId && spaceQuery.isLoading && !spaceQuery.data) ||
    (questionId && questionQuery.isLoading && !questionQuery.data);
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
  const showMetricsLoadingState = relationshipActive
    ? relationshipLoading
    : tagQuery.isLoading && tagQuery.data === undefined;

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
      cell: (tag) => <div className="font-medium text-mono">{tag.name}</div>,
    },
    {
      key: "usage",
      header: relationshipActive ? "Relationship" : "Where used",
      className: "lg:w-[260px]",
      cell: (tag) =>
        tag.relationship ? (
          <div className="flex flex-wrap gap-2">
            <Badge variant="primary" appearance="outline">
              {translateText(scopeLabel ?? "Scoped")}
            </Badge>
          </div>
        ) : (
          <div className="flex flex-wrap gap-2">
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
      key: "actions",
      header: "Actions",
      className: "lg:w-[140px]",
      cell: (tag) => (
        <div
          className="flex items-center justify-end gap-1"
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
              <Button asChild variant="ghost" mode="icon">
                <Link to={`/app/tags/${tag.id}/edit`}>
                  <Pencil className="size-4" />
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
                  <Button variant="ghost" mode="icon">
                    <Trash2 className="size-4 text-destructive" />
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
            descriptionMode="inline"
            actions={
              relationshipActive && scopeParentTo ? (
                <Button asChild>
                  <Link to={scopeParentTo}>
                    <Link2 className="size-4" />
                    {translateText("Manage relationship")}
                  </Link>
                </Button>
              ) : (
                <Button asChild>
                  <Link to="/app/tags/new">
                    <Plus className="size-4" />
                    {translateText("New tag")}
                  </Link>
                </Button>
              )
            }
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
              title: relationshipActive ? "Related" : "Total",
              value: relationshipActive
                ? tagRows.length
                : (tagQuery.data?.totalCount ?? 0),
              description: relationshipActive
                ? translateText("Tags in the current context")
                : debouncedSearch
                  ? translateText("Search: {value}", { value: debouncedSearch })
                  : translateText("Reusable taxonomy labels"),
              icon: Tags,
            },
            {
              title: "Spaces",
              value: spaceUsageCount,
              description: translateText("Tag attachments on Spaces"),
              icon: FolderKanban,
            },
            {
              title: "Questions",
              value: questionUsageCount,
              description: translateText("Tag attachments on Questions"),
              icon: MessageSquareText,
            },
          ]}
        />
      )}
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
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder={translateText("Search tags")}
            className="w-full max-w-sm"
          />
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
