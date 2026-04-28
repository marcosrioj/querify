import { useEffect } from "react";
import {
  Eye,
  FolderKanban,
  MessageSquarePlus,
  Pencil,
  Plus,
  ShieldCheck,
  Trash2,
} from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useDeleteSpace, useSpaceList } from "@/domains/spaces/hooks";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  SpaceKind,
  VisibilityScope,
  spaceKindLabels,
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
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { translateText } from "@/shared/lib/i18n-core";
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
import { SpaceKindBadge, VisibilityBadge } from "@/shared/ui/status-badges";

const sortingOptions = [
  { value: "Name ASC", label: "Name A-Z" },
  { value: "QuestionCount DESC", label: "Question count" },
  { value: "PublishedAtUtc DESC", label: "Recently published" },
  { value: "LastValidatedAtUtc DESC", label: "Recently validated" },
];

const SPACE_FILTER_DEFAULTS = {
  visibility: "all",
  kind: "all",
  acceptsQuestions: "all",
  acceptsAnswers: "all",
} as const;

export function SpaceListPage() {
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
    defaultSorting: "Name ASC",
    filterDefaults: SPACE_FILTER_DEFAULTS,
  });
  const visibilityFilter = filters.visibility;
  const kindFilter = filters.kind;
  const acceptsQuestionsFilter = filters.acceptsQuestions;
  const acceptsAnswersFilter = filters.acceptsAnswers;
  const apiVisibility =
    visibilityFilter === "all" ? undefined : Number(visibilityFilter);
  const apiKind = kindFilter === "all" ? undefined : Number(kindFilter);
  const apiAcceptsQuestions =
    acceptsQuestionsFilter === "all"
      ? undefined
      : acceptsQuestionsFilter === "true";
  const apiAcceptsAnswers =
    acceptsAnswersFilter === "all"
      ? undefined
      : acceptsAnswersFilter === "true";

  const spaceQuery = useSpaceList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    visibility: apiVisibility,
    kind: apiKind,
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
    (space) => space.visibility >= VisibilityScope.Public,
  ).length;
  const reviewGatedCount = spaceRows.filter(
    (space) =>
      space.kind === SpaceKind.ControlledPublication ||
      space.kind === SpaceKind.ModeratedCollaboration,
  ).length;
  const questionIntakeCount = spaceRows.filter(
    (space) => space.acceptsQuestions,
  ).length;
  const showMetricsLoadingState =
    spaceQuery.isLoading && spaceQuery.data === undefined;

  const columns: DataTableColumn<SpaceDto>[] = [
    {
      key: "name",
      header: "Space",
      cell: (space) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{space.name}</div>
          <div className="text-sm text-muted-foreground">
            {space.key} • {space.language}
          </div>
          {space.summary ? (
            <div className="line-clamp-2 text-sm text-muted-foreground">
              {space.summary}
            </div>
          ) : null}
        </div>
      ),
    },
    {
      key: "kind",
      header: "Model",
      className: "lg:w-[160px]",
      cell: (space) => (
        <div className="space-y-2">
          <SpaceKindBadge kind={space.kind} />
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
      className: "lg:w-[180px]",
      cell: (space) => <VisibilityBadge visibility={space.visibility} />,
    },
    {
      key: "questions",
      header: "Questions",
      className: "lg:w-[120px]",
      cell: (space) => (
        <div className="text-sm font-medium text-foreground">
          {space.questionCount}
        </div>
      ),
    },
    {
      key: "validated",
      header: "Last validated",
      className: "lg:w-[170px]",
      cell: (space) => (
        <span className="text-sm text-muted-foreground">
          {formatNumericDateTimeInTimeZone(
            space.lastValidatedAtUtc,
            portalTimeZone,
          )}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "lg:w-[120px]",
      cell: (space) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/spaces/${space.id}/edit`}>
              <Pencil className="size-4" />
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
            title="Spaces"
            description="Operate QnA spaces by operating mode, visibility, and intake capability."
            descriptionMode="inline"
            actions={
              <Button asChild>
                <Link to="/app/spaces/new">
                  <Plus className="size-4" />
                  {translateText("New space")}
                </Link>
              </Button>
            }
          />
          <QnaModuleNav
            activeKey="spaces"
            intent="Spaces are the parent boundary for every question, answer, source attachment, tag, and activity event."
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
              value: spaceQuery.data?.totalCount ?? 0,
              description: debouncedSearch
                ? translateText("Search: {value}", { value: debouncedSearch })
                : translateText("Active QnA spaces in this workspace"),
              icon: FolderKanban,
            },
            {
              title: "Public",
              value: publicCount,
              description: translateText(
                "Spaces visible outside internal operations",
              ),
              icon: Eye,
            },
            {
              title: "Moderated",
              value: reviewGatedCount,
              description: translateText(
                "Controlled or moderated operating modes",
              ),
              icon: ShieldCheck,
            },
            {
              title: "Questions",
              value: questionIntakeCount,
              description: translateText("Spaces accepting new questions"),
              icon: MessageSquarePlus,
            },
          ]}
        />
      )}
      <DataTable
        title="Spaces"
        description="Open a space to review its operating mode, curated sources, and thread volume."
        descriptionMode="hint"
        columns={columns}
        rows={spaceRows}
        getRowId={(row) => row.id}
        loading={spaceQuery.isLoading}
        onRowClick={(space) => navigate(`/app/spaces/${space.id}`)}
        toolbar={
          <div className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-[minmax(280px,1fr)_180px_190px_180px_180px]">
            <div className="sm:col-span-2 xl:col-span-1">
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder={translateText("Search spaces")}
                className="w-full"
              />
            </div>
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
            <Select
              value={kindFilter}
              onValueChange={(value) => setFilter("kind", value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText("Operating mode")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">
                  {translateText("All modes")}
                </SelectItem>
                {Object.entries(spaceKindLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {translateText(label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={acceptsQuestionsFilter}
              onValueChange={(value) => setFilter("acceptsQuestions", value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText("Question intake")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All question states</SelectItem>
                <SelectItem value="true">Questions enabled</SelectItem>
                <SelectItem value="false">Questions disabled</SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={acceptsAnswersFilter}
              onValueChange={(value) => setFilter("acceptsAnswers", value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText("Answer intake")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All answer states</SelectItem>
                <SelectItem value="true">Answers enabled</SelectItem>
                <SelectItem value="false">Answers disabled</SelectItem>
              </SelectContent>
            </Select>
            <Select value={sorting} onValueChange={setSorting}>
              <SelectTrigger className="w-full">
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
          </div>
        }
        emptyState={
          <EmptyState
            title="No spaces in view"
            description="Create the first QnA space to define mode, exposure, and thread ownership."
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
