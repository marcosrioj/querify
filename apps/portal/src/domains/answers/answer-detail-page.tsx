import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  Activity,
  Link2,
  Pencil,
  ShieldCheck,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ActivityRelationshipActions } from "@/domains/activity/activity-relationship-actions";
import { useActivityList } from "@/domains/activity/hooks";
import {
  useAnswer,
  useArchiveAnswer,
  useAddAnswerSource,
  useDeleteAnswer,
  useRemoveAnswerSource,
  useUpdateAnswerStatus,
} from "@/domains/answers/hooks";
import { useActivationVisibilityPrompt } from "@/domains/qna/activation-visibility";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { RecommendedNextActionCard } from "@/domains/qna/recommended-next-action-card";
import { useQuestion } from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useSource, useSourceList } from "@/domains/sources/hooks";
import {
  AnswerStatus,
  SourceRole,
  activityKindLabels,
  actorKindLabels,
  sourceRoleLabels,
} from "@/shared/constants/backend-enums";
import {
  DetailOverviewCard,
  DetailFieldList,
  DetailLayout,
  PageHeader,
} from "@/shared/layout/page-layouts";
import {
  ActionButton,
  ActionPanel,
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ChildListPagination,
  ConfirmAction,
  ContextHint,
  DetailPageSkeleton,
  ListFilterField,
  ListFilterSearch,
  ListFilterToolbar,
  RelationshipFilterButton,
  SearchSelect,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  ActivityKindBadge,
  ActorKindBadge,
  AnswerKindBadge,
  AnswerStatusBadge,
  SourceRoleBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { useLocalPagination } from "@/shared/lib/use-local-pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { clampPage } from "@/shared/lib/pagination";
import { useRelationshipListState } from "@/shared/lib/use-relationship-list-state";

const activitySortingOptions = [
  { value: "OccurredAtUtc DESC", label: "Latest activity" },
  { value: "OccurredAtUtc ASC", label: "Oldest activity" },
  { value: "Kind ASC", label: "Event kind A-Z" },
  { value: "Kind DESC", label: "Event kind Z-A" },
  { value: "ActorKind ASC", label: "Actor kind A-Z" },
  { value: "ActorKind DESC", label: "Actor kind Z-A" },
];

const ACTIVITY_RELATIONSHIP_FILTER_DEFAULTS: Record<
  "kind" | "actorKind",
  string
> = {
  kind: "all",
  actorKind: "all",
};

function buildSourceOption(source: {
  id: string;
  label?: string | null;
  locator: string;
}) {
  const label = source.label || source.locator;

  return {
    value: source.id,
    label,
    description: source.locator,
    keywords: [label, source.locator],
  };
}

export function AnswerDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const answerId = id ?? "";
  const answerQuery = useAnswer(id);
  const questionQuery = useQuestion(answerQuery.data?.questionId);
  const activityListState = useRelationshipListState({
    defaultSorting: "OccurredAtUtc DESC",
    filterDefaults: ACTIVITY_RELATIONSHIP_FILTER_DEFAULTS,
  });
  const activityKindFilter = activityListState.filters.kind;
  const activityActorKindFilter = activityListState.filters.actorKind;
  const activityQuery = useActivityList({
    page: activityListState.page,
    pageSize: activityListState.pageSize,
    sorting: activityListState.sorting,
    searchText: activityListState.debouncedSearch || undefined,
    answerId: id,
    kind: activityKindFilter === "all" ? undefined : Number(activityKindFilter),
    actorKind:
      activityActorKindFilter === "all"
        ? undefined
        : Number(activityActorKindFilter),
    enabled: Boolean(id),
  });
  const deleteAnswer = useDeleteAnswer();
  const updateAnswerStatus = useUpdateAnswerStatus();
  const archiveAnswer = useArchiveAnswer();
  const { resolveActivationVisibility, ActivationVisibilityDialog } =
    useActivationVisibilityPrompt();
  const addSource = useAddAnswerSource(answerId);
  const removeSource = useRemoveAnswerSource(answerId);
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [sourceSearch, setSourceSearch] = useState("");
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.Evidence),
  );
  const [relationshipTab, setRelationshipTab] = useState("sources");
  const [activityFiltersOpen, setActivityFiltersOpen] = useState(false);
  const deferredSourceSearch = useDeferredValue(sourceSearch.trim());
  const sourceOptionsQuery = useSourceList({
    page: 1,
    pageSize: 20,
    sorting: "Label ASC",
    searchText: deferredSourceSearch || undefined,
  });
  const selectedSourceQuery = useSource(selectedSourceId || undefined);

  const availableSources = useMemo(() => {
    const existing = new Set(
      (answerQuery.data?.sources ?? []).map((link) => link.sourceId),
    );
    return (sourceOptionsQuery.data?.items ?? []).filter(
      (source) => !existing.has(source.id),
    );
  }, [answerQuery.data?.sources, sourceOptionsQuery.data?.items]);
  const sourceOptions = availableSources.map(buildSourceOption);
  const selectedSource =
    availableSources.find((source) => source.id === selectedSourceId) ??
    selectedSourceQuery.data;
  const selectedSourceOption = selectedSource
    ? buildSourceOption(selectedSource)
    : null;
  const answerActivity = activityQuery.data?.items ?? [];
  const sourcesPagination = useLocalPagination({
    items: answerQuery.data?.sources ?? [],
  });

  useEffect(() => {
    const totalCount = activityQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(
      activityListState.page,
      totalCount,
      activityListState.pageSize,
    );
    if (nextPage !== activityListState.page) {
      activityListState.setPage(nextPage);
    }
  }, [
    activityListState.page,
    activityListState.pageSize,
    activityListState.setPage,
    activityQuery.data?.totalCount,
  ]);

  const showLoadingState =
    !answerQuery.data &&
    (answerQuery.isLoading ||
      questionQuery.isLoading ||
      sourceOptionsQuery.isLoading);
  const currentAnswerStatus = answerQuery.data?.status;
  const activateCurrentAnswer = async () => {
    if (!answerQuery.data) {
      return Promise.resolve();
    }

    const visibility = await resolveActivationVisibility(
      answerQuery.data.visibility,
    );
    if (visibility === null) {
      return Promise.resolve();
    }

    return updateAnswerStatus.mutateAsync({
      answer: answerQuery.data,
      status: AnswerStatus.Active,
      visibility,
    });
  };
  const lifecycleActionOptions = [
    {
      status: AnswerStatus.Active,
      label: "Activate",
      variant: "primary" as const,
      isPending: updateAnswerStatus.isPending,
      run: activateCurrentAnswer,
    },
    {
      status: AnswerStatus.Archived,
      label: "Archive",
      variant: "destructive" as const,
      isPending: archiveAnswer.isPending,
      run: () => archiveAnswer.mutateAsync(answerId),
    },
  ].filter((option) => option.status !== currentAnswerStatus);
  const hasDestructiveLifecycleAction = lifecycleActionOptions.some(
    (option) => option.variant === "destructive",
  );
  const lifecycleSummary =
    currentAnswerStatus === AnswerStatus.Draft
      ? translateText(
          "Activate the answer before exposing it publicly or accepting it.",
        )
      : translateText(
          "Current status controls which lifecycle actions are available.",
        );
  const activateRelationshipTab = (tab: string, focusTargetId?: string) => {
    setRelationshipTab(tab);

    window.requestAnimationFrame(() => {
      window.requestAnimationFrame(() => {
        const section = document.getElementById(`answer-${tab}-section`);
        const focusTarget = focusTargetId
          ? document.getElementById(focusTargetId)
          : null;
        const scrollTarget = focusTarget ?? section;

        scrollTarget?.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });

        if (focusTarget instanceof HTMLElement) {
          focusTarget.focus({ preventScroll: true });
        }
      });
    });
  };
  const answerNextAction = !answerQuery.data
    ? {
        label: "Open spaces",
        to: "/app/spaces",
        text: "Return to Spaces while this answer loads.",
      }
    : answerQuery.data.status === AnswerStatus.Draft
      ? {
          label: "Activate answer",
          run: activateCurrentAnswer,
          disabled: updateAnswerStatus.isPending,
          text: "This draft is not available for accepted-answer selection yet. Activate it when the content is ready.",
        }
      : answerQuery.data.status === AnswerStatus.Archived
        ? {
            label: "Reactivate answer",
            run: activateCurrentAnswer,
            disabled: updateAnswerStatus.isPending,
            text: "This answer is archived. Reactivate it only if it should return to the usable knowledge set.",
          }
        : answerQuery.data.sources.length === 0
          ? {
              label: "Attach source",
              tab: "sources",
              focusTargetId: "answer-source-picker",
              text: "Active answers need evidence links before they can be trusted as durable guidance.",
            }
          : !answerQuery.data.isAccepted
            ? {
                label: "Open question",
                to: `/app/questions/${answerQuery.data.questionId}`,
                text: "This answer has supporting evidence. Open the parent question when it should become the accepted resolution.",
              }
            : {
                label: "Review activity",
                tab: "activity",
                text: "This accepted answer is active and sourced. Review recent events before changing lifecycle state.",
              };

  if (!id) {
    return (
      <ErrorState
        title="Invalid answer route"
        description="Answer detail routes need an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title={answerQuery.data?.headline ?? "Answer"}
          description="Manage lifecycle status, evidence links, and trust signals for this answer candidate."
          descriptionMode="hint"
          backTo={
            answerQuery.data?.questionId
              ? `/app/questions/${answerQuery.data.questionId}`
              : "/app/spaces"
          }
          breadcrumbs={
            answerQuery.data
              ? [
                  ...(questionQuery.data?.spaceId
                    ? [
                        {
                          label: "Space",
                          to: `/app/spaces/${questionQuery.data.spaceId}`,
                        },
                      ]
                    : []),
                  {
                    label: "Question",
                    to: `/app/questions/${answerQuery.data.questionId}`,
                  },
                  { label: "Answer" },
                ]
              : undefined
          }
        />
      }
      sidebar={
        showLoadingState ? (
          <SidebarSummarySkeleton />
        ) : answerQuery.data ? (
          <DetailOverviewCard
            description="This summarizes lifecycle, visibility, type, and trust signals for this answer."
            highlights={[
              {
                label: "Status",
                value: <AnswerStatusBadge status={answerQuery.data.status} />,
              },
              {
                label: "Visibility",
                value: (
                  <VisibilityBadge visibility={answerQuery.data.visibility} />
                ),
              },
              {
                label: "Type",
                value: <AnswerKindBadge kind={answerQuery.data.kind} />,
              },
              {
                label: "Signals",
                value: (
                  <div className="flex flex-wrap gap-2">
                    <Badge variant="outline">
                      {translateText("Votes {value}", {
                        value: answerQuery.data.voteScore,
                      })}
                    </Badge>
                    <Badge
                      variant={answerQuery.data.isAccepted ? "success" : "mono"}
                    >
                      {translateText(
                        answerQuery.data.isAccepted
                          ? "Accepted"
                          : "Not accepted",
                      )}
                    </Badge>
                  </div>
                ),
              },
            ]}
            items={[
              {
                label: "Question",
                value: questionQuery.data?.title || answerQuery.data.questionId,
              },
              {
                label: "Author label",
                value: answerQuery.data.authorLabel || "Not set",
              },
              { label: "Score", value: String(answerQuery.data.score) },
              { label: "Sort", value: String(answerQuery.data.sort) },
              {
                label: "Vote score",
                value: String(answerQuery.data.voteScore),
              },
              {
                label: "AI confidence",
                value: String(answerQuery.data.aiConfidenceScore),
              },
            ]}
          />
        ) : null
      }
    >
      {ActivationVisibilityDialog}
      <ActionPanel
        layout="bar"
        description="Answer actions and parent navigation."
      >
        <ActionButton asChild tone="primary">
          <Link to={`/app/answers/${id}/edit`}>
            <Pencil className="size-4" />
            {translateText("Edit")}
          </Link>
        </ActionButton>
        {answerQuery.data?.questionId ? (
          <ActionButton asChild tone="secondary">
            <Link to={`/app/questions/${answerQuery.data.questionId}`}>
              <Link2 className="size-4" />
              {translateText("Open question")}
            </Link>
          </ActionButton>
        ) : null}
        {lifecycleActionOptions.map((option) => (
          <Button
            key={option.status}
            size="sm"
            variant={option.variant}
            data-action-tone={
              option.variant === "destructive" ? "danger" : undefined
            }
            data-action-align={
              option.variant === "destructive" ? "end" : undefined
            }
            onClick={() => void option.run()}
            disabled={option.isPending}
          >
            {translateText(option.label)}
          </Button>
        ))}
        <ConfirmAction
          title={translateText('Delete answer "{name}"?', {
            name: answerQuery.data?.headline ?? translateText("this answer"),
          })}
          description={translateText(
            "This removes the answer candidate and any ranking signals attached to it.",
          )}
          confirmLabel={translateText("Delete answer")}
          isPending={deleteAnswer.isPending}
          onConfirm={() =>
            deleteAnswer
              .mutateAsync(id)
              .then(() =>
                navigate(
                  answerQuery.data?.questionId
                    ? `/app/questions/${answerQuery.data.questionId}`
                    : "/app/spaces",
                ),
              )
          }
          trigger={
            <ActionButton
              tone="danger"
              data-action-align={
                hasDestructiveLifecycleAction ? "grouped" : undefined
              }
            >
              <Trash2 className="size-4" />
              {translateText("Delete")}
            </ActionButton>
          }
        />
      </ActionPanel>
      {answerQuery.isError ? (
        <ErrorState
          title="Unable to load answer"
          error={answerQuery.error}
          retry={() => void answerQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={5} metrics={0} />
      ) : answerQuery.data ? (
        <>
          <RecommendedNextActionCard
            label={answerNextAction.label}
            text={answerNextAction.text}
            action={
              "run" in answerNextAction ? (
                <Button
                  type="button"
                  onClick={() => void answerNextAction.run()}
                  disabled={answerNextAction.disabled}
                >
                  {translateText(answerNextAction.label)}
                </Button>
              ) : "to" in answerNextAction ? (
                <Button asChild>
                  <Link to={answerNextAction.to}>
                    {translateText(answerNextAction.label)}
                  </Link>
                </Button>
              ) : (
                <Button
                  type="button"
                  onClick={() =>
                    activateRelationshipTab(
                      answerNextAction.tab,
                      answerNextAction.focusTargetId,
                    )
                  }
                >
                  {translateText(answerNextAction.label)}
                </Button>
              )
            }
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex items-center gap-2">
                  <span>{translateText("Lifecycle")}</span>
                  <ContextHint
                    content={translateText(
                      "Activate or archive the answer as product truth evolves.",
                    )}
                    label={translateText("Lifecycle details")}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                {lifecycleSummary}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Details")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <DetailFieldList
                items={[
                  {
                    label: "Headline",
                    value: answerQuery.data.headline,
                    valueClassName: "text-base font-medium text-mono",
                  },
                  {
                    label: "Body",
                    value:
                      answerQuery.data.body || translateText("No body yet"),
                    valueClassName: "whitespace-pre-wrap",
                  },
                  {
                    label: "Context note",
                    value:
                      answerQuery.data.contextNote ||
                      translateText("No context note recorded."),
                    valueClassName: "whitespace-pre-wrap",
                  },
                ]}
              />
            </CardContent>
          </Card>

          <QnaModuleNav
            eyebrow="Answer relationships"
            activeKey={relationshipTab}
            onActiveKeyChange={setRelationshipTab}
            items={[
              {
                key: "sources",
                label: "Sources",
                description:
                  "Attach evidence before using this answer as active knowledge.",
                icon: ShieldCheck,
                count: answerQuery.data?.sources.length ?? 0,
              },
              {
                key: "activity",
                label: "Activity",
                description:
                  "Review events scoped to this answer when status changes need context.",
                icon: Activity,
                count: activityQuery.data?.totalCount ?? 0,
              },
            ]}
          />

          {relationshipTab === "sources" ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Source links")}</span>
                    <Badge variant="outline">
                      {translateText("{count} sources", {
                        count: answerQuery.data.sources.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {answerQuery.data.sources.length ? (
                  <div className="space-y-3">
                    {sourcesPagination.pagedItems.map((sourceLink) => (
                      <div
                        key={sourceLink.id}
                        className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                      >
                        <div className="min-w-0">
                          <p className="font-medium text-mono">
                            {sourceLink.source?.label ||
                              sourceLink.source?.locator ||
                              sourceLink.sourceId}
                          </p>
                          <div className="mt-2 flex flex-wrap items-center gap-2">
                            <SourceRoleBadge role={sourceLink.role} />
                            <Badge variant="outline">
                              {translateText("Order {value}", {
                                value: sourceLink.order,
                              })}
                            </Badge>
                            {sourceLink.source ? (
                              <>
                                <VisibilityBadge
                                  visibility={sourceLink.source.visibility}
                                />
                              </>
                            ) : null}
                          </div>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          {sourceLink.source ? (
                            <Button asChild variant="outline" size="sm">
                              <Link to={`/app/sources/${sourceLink.source.id}`}>
                                <Link2 className="size-4" />
                                {translateText("Open source")}
                              </Link>
                            </Button>
                          ) : null}
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() =>
                              void removeSource.mutateAsync(sourceLink.id)
                            }
                          >
                            <Trash2 className="size-4" />
                            {translateText("Detach")}
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title="No sources linked yet"
                    description="Attach evidence or reusable references for this answer."
                  />
                )}
                <ChildListPagination
                  page={sourcesPagination.page}
                  pageSize={sourcesPagination.pageSize}
                  totalCount={sourcesPagination.totalCount}
                  onPageChange={sourcesPagination.setPage}
                  onPageSizeChange={sourcesPagination.setPageSize}
                />
                <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_220px_160px]">
                  <SearchSelect
                    id="answer-source-picker"
                    value={selectedSourceId}
                    onValueChange={setSelectedSourceId}
                    options={sourceOptions}
                    selectedOption={selectedSourceOption}
                    placeholder={translateText("Attach existing source")}
                    searchPlaceholder={translateText("Search sources")}
                    emptyMessage={
                      deferredSourceSearch
                        ? translateText("No sources match this search.")
                        : translateText("No sources available.")
                    }
                    loading={sourceOptionsQuery.isFetching}
                    searchValue={sourceSearch}
                    onSearchChange={(value) =>
                      startTransition(() => setSourceSearch(value))
                    }
                    allowClear
                  />
                  <Select
                    value={selectedSourceRole}
                    onValueChange={setSelectedSourceRole}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder={translateText("Source role")} />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(sourceRoleLabels).map(
                        ([value, label]) => (
                          <SelectItem key={value} value={value}>
                            {translateText(label)}
                          </SelectItem>
                        ),
                      )}
                    </SelectContent>
                  </Select>
                  <Button
                    disabled={!selectedSourceId || addSource.isPending}
                    onClick={() =>
                      addSource
                        .mutateAsync({
                          answerId: id,
                          sourceId: selectedSourceId,
                          role: Number(selectedSourceRole) as SourceRole,
                          order: answerQuery.data.sources.length + 1,
                        })
                        .then(() => setSelectedSourceId(""))
                    }
                  >
                    {translateText("Attach source")}
                  </Button>
                </div>
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "activity" ? (
            <Card id="answer-activity-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Activity")}</span>
                    <Badge variant="outline">
                      {translateText("{count} events", {
                        count: activityQuery.data?.totalCount ?? 0,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
                <RelationshipFilterButton
                  activeFilterCount={activityListState.activeFilterCount}
                  isLoading={activityQuery.isFetching}
                  open={activityFiltersOpen}
                  onClick={() => setActivityFiltersOpen((open) => !open)}
                />
              </CardHeader>
              <CardContent className="space-y-3">
                {activityFiltersOpen ? (
                  <div className="rounded-lg border border-border/70 bg-muted/20 p-3">
                    <ListFilterToolbar isLoading={activityQuery.isFetching}>
                      <ListFilterSearch
                        value={activityListState.search}
                        onChange={activityListState.setSearch}
                        placeholder="Search activity by actor, notes, or subject"
                        activeFilterCount={activityListState.activeFilterCount}
                        onClear={activityListState.resetFilters}
                        isLoading={activityQuery.isFetching}
                      />
                      <div className="grid gap-3 md:grid-cols-3">
                        <ListFilterField label="Event kind">
                          <Select
                            value={activityKindFilter}
                            onValueChange={(value) =>
                              activityListState.setFilter("kind", value)
                            }
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Event kind")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="all">
                                All event kinds
                              </SelectItem>
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
                            value={activityActorKindFilter}
                            onValueChange={(value) =>
                              activityListState.setFilter("actorKind", value)
                            }
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Actor kind")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="all">All actors</SelectItem>
                              {Object.entries(actorKindLabels).map(
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
                          <Select
                            value={activityListState.sorting}
                            onValueChange={activityListState.setSorting}
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Sort events")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              {activitySortingOptions.map((option) => (
                                <SelectItem
                                  key={option.value}
                                  value={option.value}
                                >
                                  {translateText(option.label)}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </ListFilterField>
                      </div>
                    </ListFilterToolbar>
                  </div>
                ) : null}
                {activityQuery.isError ? (
                  <ErrorState
                    title="Unable to load answer activity"
                    error={activityQuery.error}
                    retry={() => void activityQuery.refetch()}
                  />
                ) : answerActivity.length ? (
                  answerActivity.map((event) => (
                    <div
                      key={event.id}
                      className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                    >
                      <div className="min-w-0 space-y-2">
                        <div className="flex flex-wrap items-center gap-2">
                          <ActivityKindBadge kind={event.kind} />
                          <ActorKindBadge kind={event.actorKind} />
                        </div>
                        <div className="text-sm text-muted-foreground">
                          {event.notes || translateText("No notes recorded.")}
                        </div>
                      </div>
                      <div className="flex flex-col items-start gap-2 sm:items-end">
                        <span className="text-sm text-muted-foreground">
                          {formatOptionalDateTimeInTimeZone(
                            event.occurredAtUtc,
                            portalTimeZone,
                            translateText("Not set"),
                          )}
                        </span>
                        <ActivityRelationshipActions event={event} />
                      </div>
                    </div>
                  ))
                ) : (
                  <EmptyState
                    title="No answer activity yet"
                    description="Status and source changes will appear here."
                  />
                )}
                <ChildListPagination
                  page={activityListState.page}
                  pageSize={activityListState.pageSize}
                  totalCount={activityQuery.data?.totalCount ?? 0}
                  isFetching={activityQuery.isFetching}
                  onPageChange={activityListState.setPage}
                  onPageSizeChange={activityListState.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}
        </>
      ) : null}
    </DetailLayout>
  );
}
