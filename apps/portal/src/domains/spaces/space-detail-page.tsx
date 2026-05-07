import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  Activity,
  CheckCircle2,
  CircleOff,
  Link2,
  MessageSquareText,
  Pencil,
  Plus,
  Tags,
  Trash2,
  Waypoints,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ActivityRelationshipActions } from "@/domains/activity/activity-relationship-actions";
import { useActivityList } from "@/domains/activity/hooks";
import { useActivationVisibilityPrompt } from "@/domains/qna/activation-visibility";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { RecommendedNextActionCard } from "@/domains/qna/recommended-next-action-card";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import {
  useCreateQuestion,
  useDeleteQuestion,
  useQuestionList,
  useUpdateQuestionStatus,
} from "@/domains/questions/hooks";
import { useSource, useSourceList } from "@/domains/sources/hooks";
import {
  useSpace,
  useAddSpaceSource,
  useAddSpaceTag,
  useDeleteSpace,
  useRemoveSpaceSource,
  useRemoveSpaceTag,
} from "@/domains/spaces/hooks";
import { useTag, useTagList } from "@/domains/tags/hooks";
import {
  ChannelKind,
  QuestionStatus,
  VisibilityScope,
  activityKindLabels,
  actorKindLabels,
  questionStatusLabels,
  visibilityScopeLabels,
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
  Input,
  Label,
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
  Textarea,
} from "@/shared/ui";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  ActivityKindBadge,
  ActorKindBadge,
  QuestionStatusBadge,
  SpaceStatusBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { useLocalPagination } from "@/shared/lib/use-local-pagination";
import { useRelationshipListState } from "@/shared/lib/use-relationship-list-state";
import { clampPage } from "@/shared/lib/pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";

const questionSortingOptions = [
  { value: "LastActivityAtUtc DESC", label: "Latest activity" },
  { value: "LastActivityAtUtc ASC", label: "Oldest activity" },
  { value: "Title ASC", label: "Title A-Z" },
  { value: "Title DESC", label: "Title Z-A" },
  { value: "FeedbackScore DESC", label: "Feedback high-low" },
  { value: "FeedbackScore ASC", label: "Feedback low-high" },
  { value: "Sort ASC", label: "Sort low-high" },
  { value: "Sort DESC", label: "Sort high-low" },
];

const activitySortingOptions = [
  { value: "OccurredAtUtc DESC", label: "Latest activity" },
  { value: "OccurredAtUtc ASC", label: "Oldest activity" },
  { value: "Kind ASC", label: "Event kind A-Z" },
  { value: "Kind DESC", label: "Event kind Z-A" },
  { value: "ActorKind ASC", label: "Actor kind A-Z" },
  { value: "ActorKind DESC", label: "Actor kind Z-A" },
];

const QUESTION_RELATIONSHIP_FILTER_DEFAULTS: Record<
  "status" | "visibility",
  string
> = {
  status: "all",
  visibility: "all",
};

const ACTIVITY_RELATIONSHIP_FILTER_DEFAULTS: Record<
  "kind" | "actorKind",
  string
> = {
  kind: "all",
  actorKind: "all",
};

function buildTagOption(tag: {
  id: string;
  name: string;
  spaceUsageCount?: number;
  questionUsageCount?: number;
}) {
  const description =
    tag.spaceUsageCount !== undefined || tag.questionUsageCount !== undefined
      ? translateText("{spaces} spaces • {questions} questions", {
          spaces: tag.spaceUsageCount ?? 0,
          questions: tag.questionUsageCount ?? 0,
        })
      : undefined;

  return {
    value: tag.id,
    label: tag.name,
    description,
    keywords: [tag.name],
  };
}

function buildSourceOption(source: {
  id: string;
  label?: string | null;
  locator: string;
  kind?: number;
}) {
  const label = source.label || source.locator;

  return {
    value: source.id,
    label,
    description: source.locator,
    keywords: [label, source.locator],
  };
}

export function SpaceDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const spaceQuery = useSpace(id);
  const questionListState = useRelationshipListState({
    defaultSorting: "LastActivityAtUtc DESC",
    filterDefaults: QUESTION_RELATIONSHIP_FILTER_DEFAULTS,
  });
  const activityListState = useRelationshipListState({
    defaultSorting: "OccurredAtUtc DESC",
    filterDefaults: ACTIVITY_RELATIONSHIP_FILTER_DEFAULTS,
  });
  const questionStatusFilter = questionListState.filters.status;
  const questionVisibilityFilter = questionListState.filters.visibility;
  const activityKindFilter = activityListState.filters.kind;
  const activityActorKindFilter = activityListState.filters.actorKind;
  const questionQuery = useQuestionList({
    page: questionListState.page,
    pageSize: questionListState.pageSize,
    sorting: questionListState.sorting,
    searchText: questionListState.debouncedSearch || undefined,
    spaceId: id,
    status:
      questionStatusFilter === "all" ? undefined : Number(questionStatusFilter),
    visibility:
      questionVisibilityFilter === "all"
        ? undefined
        : Number(questionVisibilityFilter),
    enabled: Boolean(id),
  });
  const activityQuery = useActivityList({
    page: activityListState.page,
    pageSize: activityListState.pageSize,
    sorting: activityListState.sorting,
    searchText: activityListState.debouncedSearch || undefined,
    spaceId: id,
    kind: activityKindFilter === "all" ? undefined : Number(activityKindFilter),
    actorKind:
      activityActorKindFilter === "all"
        ? undefined
        : Number(activityActorKindFilter),
    enabled: Boolean(id),
  });
  const deleteSpace = useDeleteSpace();
  const createQuestion = useCreateQuestion();
  const deleteQuestion = useDeleteQuestion();
  const updateQuestionStatus = useUpdateQuestionStatus();
  const { resolveActivationVisibility, ActivationVisibilityDialog } =
    useActivationVisibilityPrompt();
  const addTag = useAddSpaceTag(id ?? "");
  const removeTag = useRemoveSpaceTag(id ?? "");
  const addSource = useAddSpaceSource(id ?? "");
  const removeSource = useRemoveSpaceSource(id ?? "");
  const [selectedTagId, setSelectedTagId] = useState("");
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [tagSearch, setTagSearch] = useState("");
  const [sourceSearch, setSourceSearch] = useState("");
  const [newQuestionTitle, setNewQuestionTitle] = useState("");
  const [newQuestionSummary, setNewQuestionSummary] = useState("");
  const [relationshipTab, setRelationshipTab] = useState("questions");
  const [questionCreateOpen, setQuestionCreateOpen] = useState(false);
  const [questionFiltersOpen, setQuestionFiltersOpen] = useState(false);
  const [activityFiltersOpen, setActivityFiltersOpen] = useState(false);
  const deferredTagSearch = useDeferredValue(tagSearch.trim());
  const deferredSourceSearch = useDeferredValue(sourceSearch.trim());
  const sourceOptionsQuery = useSourceList({
    page: 1,
    pageSize: 20,
    sorting: "Label ASC",
    searchText: deferredSourceSearch || undefined,
  });
  const tagOptionsQuery = useTagList({
    page: 1,
    pageSize: 20,
    sorting: "Name ASC",
    searchText: deferredTagSearch || undefined,
  });
  const selectedTagQuery = useTag(selectedTagId || undefined);
  const selectedSourceQuery = useSource(selectedSourceId || undefined);

  const availableTags = useMemo(() => {
    const existing = new Set(
      (spaceQuery.data?.tags ?? []).map((tag) => tag.id),
    );
    return (tagOptionsQuery.data?.items ?? []).filter(
      (tag) => !existing.has(tag.id),
    );
  }, [spaceQuery.data?.tags, tagOptionsQuery.data?.items]);

  const availableSources = useMemo(() => {
    const existing = new Set(
      (spaceQuery.data?.curatedSources ?? []).map((source) => source.id),
    );
    return (sourceOptionsQuery.data?.items ?? []).filter(
      (source) => !existing.has(source.id),
    );
  }, [sourceOptionsQuery.data?.items, spaceQuery.data?.curatedSources]);
  const tagOptions = availableTags.map(buildTagOption);
  const sourceOptions = availableSources.map(buildSourceOption);
  const selectedTag =
    availableTags.find((tag) => tag.id === selectedTagId) ??
    selectedTagQuery.data;
  const selectedSource =
    availableSources.find((source) => source.id === selectedSourceId) ??
    selectedSourceQuery.data;
  const selectedTagOption = selectedTag ? buildTagOption(selectedTag) : null;
  const selectedSourceOption = selectedSource
    ? buildSourceOption(selectedSource)
    : null;

  const showLoadingState =
    !spaceQuery.data &&
    (spaceQuery.isLoading ||
      questionQuery.isLoading ||
      sourceOptionsQuery.isLoading);
  const blocksQuestions = spaceQuery.data
    ? !spaceQuery.data.acceptsQuestions
    : false;
  const blocksAnswers = spaceQuery.data
    ? !spaceQuery.data.acceptsAnswers
    : false;
  const spaceQuestions = questionQuery.data?.items ?? [];
  const questionsNeedingAction = spaceQuestions.filter(
    (question) => question.status === QuestionStatus.Draft,
  );
  const contextualActivity = activityQuery.data?.items ?? [];
  const tagsPagination = useLocalPagination({
    items: spaceQuery.data?.tags ?? [],
  });
  const sourcesPagination = useLocalPagination({
    items: spaceQuery.data?.curatedSources ?? [],
  });

  useEffect(() => {
    const totalCount = questionQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(
      questionListState.page,
      totalCount,
      questionListState.pageSize,
    );
    if (nextPage !== questionListState.page) {
      questionListState.setPage(nextPage);
    }
  }, [
    questionListState.page,
    questionListState.pageSize,
    questionListState.setPage,
    questionQuery.data?.totalCount,
  ]);

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
  const handleQuestionStatusChange = async (
    question: (typeof spaceQuestions)[number],
  ) => {
    const status =
      question.status === QuestionStatus.Active
        ? QuestionStatus.Archived
        : QuestionStatus.Active;
    let visibility: VisibilityScope | undefined;
    if (question.status !== QuestionStatus.Active) {
      const resolvedVisibility = await resolveActivationVisibility(
        question.visibility,
      );
      if (resolvedVisibility === null) {
        return;
      }

      visibility = resolvedVisibility;
    }

    updateQuestionStatus.mutate({
      question,
      status,
      visibility,
    });
  };

  const activateRelationshipTab = (tab: string, focusTargetId?: string) => {
    setRelationshipTab(tab);
    if (tab === "questions" && focusTargetId === "new-question-title") {
      setQuestionCreateOpen(true);
    }

    window.requestAnimationFrame(() => {
      window.requestAnimationFrame(() => {
        const section = document.getElementById(`space-${tab}-section`);
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

  if (!id) {
    return (
      <ErrorState
        title="Invalid space route"
        description="Space detail routes need an identifier."
      />
    );
  }

  const nextAction = blocksQuestions
    ? {
        label: "Review intake rules",
        to: `/app/spaces/${id}/edit`,
        text: "Question intake is closed. Reopen it or keep routing work to another Space.",
      }
    : questionsNeedingAction.length > 0
      ? {
          label: "Review draft question",
          tab: "questions",
          text: "Draft questions need activation or archive review.",
        }
      : {
          label: "Create question",
          tab: "questions",
          text: "The Space is ready for the next operational question.",
        };

  return (
    <DetailLayout
      header={
        <PageHeader
          title={spaceQuery.data?.name ?? "Space"}
          description="Review state, intake rules, draft questions, connected tags and sources, and the next recommended move."
          descriptionMode="hint"
          backTo="/app/spaces"
          breadcrumbs={[{ label: "Space", to: "/app/spaces" }]}
        />
      }
      sidebar={
        showLoadingState ? (
          <SidebarSummarySkeleton />
        ) : spaceQuery.data ? (
          <DetailOverviewCard
            description="This summarizes status and the major workflow gates."
            highlights={[
              {
                label: "Status",
                description:
                  "Public spaces must be active before they are exposed.",
                value: <SpaceStatusBadge status={spaceQuery.data.status} />,
              },
              {
                label: "Visibility",
                description:
                  "Choose the strongest audience exposure the space should allow.",
                value: (
                  <VisibilityBadge visibility={spaceQuery.data.visibility} />
                ),
              },
              {
                label: "Accepts",
                description:
                  "Shows whether this space currently accepts new questions and answers.",
                value: (
                  <div className="flex flex-wrap gap-2">
                    <Badge variant={blocksQuestions ? "mono" : "success"}>
                      {translateText(
                        blocksQuestions ? "No questions" : "Questions",
                      )}
                    </Badge>
                    <Badge variant={blocksAnswers ? "mono" : "success"}>
                      {translateText(blocksAnswers ? "No answers" : "Answers")}
                    </Badge>
                  </div>
                ),
              },
              {
                label: "Needs action",
                description:
                  "Questions in this space that need operator attention before the knowledge set is ready.",
                value: String(questionsNeedingAction.length),
              },
            ]}
            items={[
              {
                label: "Slug",
                description: "Use a stable slug for routing and integrations.",
                value: spaceQuery.data.slug,
              },
              {
                label: "Language",
                description:
                  "Use the main locale for the questions and answers in this space.",
                value: spaceQuery.data.language,
              },
              {
                label: "Questions",
                description:
                  "Questions currently routed through this operating space.",
                value: String(spaceQuery.data.questionCount),
              },
              {
                label: "Curated sources",
                description:
                  "Optional reusable material that can add context to this space.",
                value: String(spaceQuery.data.curatedSources.length),
              },
              {
                label: "Accepts questions",
                description:
                  "Disable this for frozen or read-only knowledge spaces.",
                value: (
                  <Badge
                    variant={
                      spaceQuery.data.acceptsQuestions ? "success" : "mono"
                    }
                  >
                    {translateText(
                      spaceQuery.data.acceptsQuestions ? "Enabled" : "Disabled",
                    )}
                  </Badge>
                ),
              },
              {
                label: "Accepts answers",
                description:
                  "Disable this if questions should route elsewhere instead of collecting answers.",
                value: (
                  <Badge
                    variant={
                      spaceQuery.data.acceptsAnswers ? "success" : "mono"
                    }
                  >
                    {translateText(
                      spaceQuery.data.acceptsAnswers ? "Enabled" : "Disabled",
                    )}
                  </Badge>
                ),
              },
            ]}
          />
        ) : null
      }
    >
      {ActivationVisibilityDialog}
      <ActionPanel
        layout="bar"
        description="Record-level actions for this Space."
      >
        <ActionButton asChild tone="secondary">
          <Link to={`/app/spaces/${id}/edit`}>
            <Pencil className="size-4" />
            {translateText("Edit")}
          </Link>
        </ActionButton>
        <ConfirmAction
          title={translateText('Delete space "{name}"?', {
            name: spaceQuery.data?.name ?? translateText("this space"),
          })}
          description={translateText(
            "This removes the space and its operating rules from the workspace.",
          )}
          confirmLabel={translateText("Delete space")}
          isPending={deleteSpace.isPending}
          onConfirm={() =>
            deleteSpace.mutateAsync(id).then(() => navigate("/app/spaces"))
          }
          trigger={
            <ActionButton tone="danger">
              <Trash2 className="size-4" />
              {translateText("Delete")}
            </ActionButton>
          }
        />
      </ActionPanel>
      {spaceQuery.isError ? (
        <ErrorState
          title="Unable to load space"
          error={spaceQuery.error}
          retry={() => void spaceQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={5} metrics={0} />
      ) : spaceQuery.data ? (
        <>
          <RecommendedNextActionCard
            label={nextAction.label}
            text={nextAction.text}
            action={
              "to" in nextAction ? (
                <Button asChild>
                  <Link to={nextAction.to}>
                    {translateText(nextAction.label)}
                  </Link>
                </Button>
              ) : (
                <Button
                  type="button"
                  onClick={() =>
                    activateRelationshipTab(
                      nextAction.tab,
                      nextAction.tab === "questions" &&
                        nextAction.label === "Create question"
                        ? "new-question-title"
                        : undefined,
                    )
                  }
                >
                  {translateText(nextAction.label)}
                </Button>
              )
            }
          />

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
                    label: "Name",
                    description:
                      "Use the operational name teammates will recognize.",
                    value: spaceQuery.data.name,
                    valueClassName: "text-base font-medium text-mono",
                  },
                  {
                    label: "Summary",
                    description:
                      "Explain what the space covers and when teams should route content here.",
                    value:
                      spaceQuery.data.summary ||
                      translateText("No summary provided."),
                    valueClassName: "whitespace-pre-wrap",
                  },
                ]}
              />
            </CardContent>
          </Card>

          <QnaModuleNav
            eyebrow="Space relationships"
            activeKey={relationshipTab}
            onActiveKeyChange={setRelationshipTab}
            items={[
              {
                key: "questions",
                label: "Questions",
                description:
                  "Review and create questions that inherit this Space's intake and visibility rules.",
                icon: MessageSquareText,
                count: questionQuery.data?.totalCount ?? 0,
              },
              {
                key: "sources",
                label: "Curated sources",
                description:
                  "Attach optional reusable references for this Space.",
                icon: Waypoints,
                count: spaceQuery.data?.curatedSources.length ?? 0,
              },
              {
                key: "tags",
                label: "Tags",
                description:
                  "Attach reusable taxonomy so this Space stays easy to scan and route.",
                icon: Tags,
                count: spaceQuery.data?.tags.length ?? 0,
              },
              {
                key: "activity",
                label: "Activity",
                description:
                  "Inspect events from questions that belong to this Space.",
                icon: Activity,
                count: activityQuery.data?.totalCount ?? 0,
              },
            ]}
          />

          {relationshipTab === "tags" ? (
            <Card id="space-tags-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Tags")}</span>
                    <Badge variant="outline">
                      {translateText("{count} tags", {
                        count: spaceQuery.data.tags.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {spaceQuery.data.tags.length ? (
                  <div className="flex flex-wrap gap-2">
                    {tagsPagination.pagedItems.map((tag) => (
                      <Badge
                        key={tag.id}
                        variant="outline"
                        className="gap-2 px-3 py-1.5"
                      >
                        <span>{tag.name}</span>
                        <button
                          type="button"
                          className="text-muted-foreground hover:text-foreground"
                          onClick={() => void removeTag.mutateAsync(tag.id)}
                        >
                          ×
                        </button>
                      </Badge>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title="No tags yet"
                    description="Attach reusable taxonomy so operators can group and find the space faster."
                  />
                )}
                <ChildListPagination
                  page={tagsPagination.page}
                  pageSize={tagsPagination.pageSize}
                  totalCount={tagsPagination.totalCount}
                  onPageChange={tagsPagination.setPage}
                  onPageSizeChange={tagsPagination.setPageSize}
                />
                <div className="flex flex-col gap-3 sm:flex-row">
                  <SearchSelect
                    value={selectedTagId}
                    onValueChange={setSelectedTagId}
                    options={tagOptions}
                    selectedOption={selectedTagOption}
                    placeholder={translateText("Attach existing tag")}
                    searchPlaceholder={translateText("Search tags")}
                    emptyMessage={
                      deferredTagSearch
                        ? translateText("No tags match this search.")
                        : translateText("No tags available.")
                    }
                    loading={tagOptionsQuery.isFetching}
                    searchValue={tagSearch}
                    onSearchChange={(value) =>
                      startTransition(() => setTagSearch(value))
                    }
                    allowClear
                  />
                  <Button
                    disabled={!selectedTagId || addTag.isPending}
                    onClick={() =>
                      addTag
                        .mutateAsync({ spaceId: id, tagId: selectedTagId })
                        .then(() => setSelectedTagId(""))
                    }
                  >
                    {translateText("Attach tag")}
                  </Button>
                </div>
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "sources" ? (
            <Card id="space-sources-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Curated sources")}</span>
                    <Badge variant="outline">
                      {translateText("{count} sources", {
                        count: spaceQuery.data.curatedSources.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {spaceQuery.data.curatedSources.length ? (
                  <div className="space-y-3">
                    {sourcesPagination.pagedItems.map((source) => (
                      <div
                        key={source.id}
                        className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                      >
                        <div className="min-w-0">
                          <Link
                            to={`/app/sources/${source.id}`}
                            className="font-medium text-mono hover:text-primary"
                          >
                            {source.label || translateText("Untitled source")}
                          </Link>
                          <p className="mt-1 break-all text-sm text-muted-foreground">
                            {source.locator}
                          </p>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          <VisibilityBadge visibility={source.visibility} />
                          <Button asChild variant="outline" size="sm">
                            <Link to={`/app/sources/${source.id}`}>
                              <Link2 className="size-4" />
                              {translateText("Open source")}
                            </Link>
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() =>
                              void removeSource.mutateAsync(source.id)
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
                    title="No curated sources yet"
                    description="Optional reusable material can be attached when this space needs shared context."
                  />
                )}
                <ChildListPagination
                  page={sourcesPagination.page}
                  pageSize={sourcesPagination.pageSize}
                  totalCount={sourcesPagination.totalCount}
                  onPageChange={sourcesPagination.setPage}
                  onPageSizeChange={sourcesPagination.setPageSize}
                />
                <div className="flex flex-col gap-3 sm:flex-row">
                  <SearchSelect
                    id="space-source-picker"
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
                  <Button
                    disabled={!selectedSourceId || addSource.isPending}
                    onClick={() =>
                      addSource
                        .mutateAsync({
                          spaceId: id,
                          sourceId: selectedSourceId,
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

          {relationshipTab === "questions" ? (
            <Card id="space-questions-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Questions in this Space")}</span>
                    <Badge variant="outline">
                      {translateText("{count} questions", {
                        count: questionQuery.data?.totalCount ?? 0,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
                <div className="flex flex-wrap items-center gap-2">
                  {!blocksQuestions ? (
                    <Button
                      type="button"
                      variant={questionCreateOpen ? "primary" : "outline"}
                      size="sm"
                      className="h-8 gap-1.5 px-2.5 text-xs"
                      aria-expanded={questionCreateOpen}
                      aria-controls="space-question-create-form"
                      onClick={() => setQuestionCreateOpen((open) => !open)}
                    >
                      <Plus className="size-4" />
                      {translateText("New question")}
                    </Button>
                  ) : null}
                  <RelationshipFilterButton
                    activeFilterCount={questionListState.activeFilterCount}
                    isLoading={questionQuery.isFetching}
                    open={questionFiltersOpen}
                    onClick={() => setQuestionFiltersOpen((open) => !open)}
                  />
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {questionFiltersOpen ? (
                  <div className="rounded-lg border border-border/70 bg-muted/20 p-3">
                    <ListFilterToolbar isLoading={questionQuery.isFetching}>
                      <ListFilterSearch
                        value={questionListState.search}
                        onChange={questionListState.setSearch}
                        placeholder="Search questions by title or summary"
                        activeFilterCount={questionListState.activeFilterCount}
                        onClear={questionListState.resetFilters}
                        isLoading={questionQuery.isFetching}
                      />
                      <div className="grid gap-3 md:grid-cols-3">
                        <ListFilterField label="Status">
                          <Select
                            value={questionStatusFilter}
                            onValueChange={(value) =>
                              questionListState.setFilter("status", value)
                            }
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Status")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="all">All</SelectItem>
                              {Object.entries(questionStatusLabels).map(
                                ([value, label]) => (
                                  <SelectItem key={value} value={value}>
                                    {translateText(label)}
                                  </SelectItem>
                                ),
                              )}
                            </SelectContent>
                          </Select>
                        </ListFilterField>
                        <ListFilterField label="Visibility">
                          <Select
                            value={questionVisibilityFilter}
                            onValueChange={(value) =>
                              questionListState.setFilter("visibility", value)
                            }
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Visibility")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="all">
                                All visibility
                              </SelectItem>
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
                        <ListFilterField label="Sort">
                          <Select
                            value={questionListState.sorting}
                            onValueChange={questionListState.setSorting}
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Sort questions")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              {questionSortingOptions.map((option) => (
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
                {!blocksQuestions && questionCreateOpen ? (
                  <form
                    id="space-question-create-form"
                    className="rounded-xl border border-primary/15 bg-primary/[0.025] p-4"
                    onSubmit={(event) => {
                      event.preventDefault();
                      const title = newQuestionTitle.trim();

                      if (!title) {
                        return;
                      }

                      void createQuestion
                        .mutateAsync({
                          spaceId: id,
                          title,
                          summary: newQuestionSummary.trim() || undefined,
                          contextNote: undefined,
                          status: QuestionStatus.Draft,
                          visibility: VisibilityScope.Internal,
                          originChannel: ChannelKind.Manual,
                          sort: (questionQuery.data?.totalCount ?? 0) + 1,
                        })
                        .then(() => {
                          setNewQuestionTitle("");
                          setNewQuestionSummary("");
                          setQuestionCreateOpen(false);
                        });
                    }}
                  >
                    <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_auto]">
                      <div className="space-y-3">
                        <div>
                          <p className="text-sm font-semibold text-mono">
                            {translateText("Create question in this Space")}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {translateText(
                              "Capture the question here so it inherits this Space's intake and visibility rules.",
                            )}
                          </p>
                        </div>
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-1.5">
                            <Label htmlFor="new-question-title">
                              {translateText("Question title")}
                            </Label>
                            <ContextHint
                              content={translateText(
                                "Canonical question wording saved into this Space.",
                              )}
                              label={translateText("Field details")}
                            />
                          </div>
                          <Input
                            id="new-question-title"
                            value={newQuestionTitle}
                            onChange={(event) =>
                              setNewQuestionTitle(event.target.value)
                            }
                            placeholder={translateText("Question title")}
                            aria-label={translateText("Question title")}
                          />
                        </div>
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-1.5">
                            <Label htmlFor="new-question-summary">
                              {translateText("Question summary")}
                            </Label>
                            <ContextHint
                              content={translateText(
                                "Short context paragraph stored with the new question.",
                              )}
                              label={translateText("Field details")}
                            />
                          </div>
                          <Textarea
                            id="new-question-summary"
                            value={newQuestionSummary}
                            onChange={(event) =>
                              setNewQuestionSummary(event.target.value)
                            }
                            placeholder={translateText("Optional summary")}
                            aria-label={translateText("Question summary")}
                            rows={3}
                          />
                        </div>
                      </div>
                      <div className="flex items-end">
                        <Button
                          type="submit"
                          disabled={
                            !newQuestionTitle.trim() || createQuestion.isPending
                          }
                        >
                          <Plus className="size-4" />
                          {translateText("Create question")}
                        </Button>
                      </div>
                    </div>
                  </form>
                ) : null}
                {spaceQuestions.length ? (
                  spaceQuestions.map((question) => (
                    <div
                      key={question.id}
                      className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                    >
                      <div className="min-w-0">
                        <Link
                          to={`/app/questions/${question.id}`}
                          className="font-medium text-mono hover:text-primary"
                        >
                          {question.title}
                        </Link>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {question.summary ||
                            translateText("No summary provided.")}
                        </p>
                        <div className="mt-2 flex flex-wrap items-center gap-2">
                          <QuestionStatusBadge status={question.status} />
                          <VisibilityBadge visibility={question.visibility} />
                          <span className="text-xs text-muted-foreground">
                            {translateText("Last update {value}", {
                              value: formatOptionalDateTimeInTimeZone(
                                question.lastUpdatedAtUtc,
                                portalTimeZone,
                                translateText("Not set"),
                              ),
                            })}
                          </span>
                          {question.acceptedAnswerId ? (
                            <Badge variant="success" appearance="outline">
                              {translateText("Accepted answer")}
                            </Badge>
                          ) : null}
                        </div>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/questions/${question.id}`}>
                            <Link2 className="size-4" />
                            {translateText("Open")}
                          </Link>
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={updateQuestionStatus.isPending}
                          onClick={() =>
                            void handleQuestionStatusChange(question)
                          }
                        >
                          {question.status === QuestionStatus.Active ? (
                            <CircleOff className="size-4" />
                          ) : (
                            <CheckCircle2 className="size-4" />
                          )}
                          {translateText(
                            question.status === QuestionStatus.Active
                              ? "Desativar"
                              : "Ativar",
                          )}
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
                          onConfirm={() =>
                            deleteQuestion.mutateAsync(question.id)
                          }
                          trigger={
                            <Button variant="ghost" size="sm">
                              <Trash2 className="size-4" />
                              {translateText("Delete")}
                            </Button>
                          }
                        />
                      </div>
                    </div>
                  ))
                ) : (
                  <EmptyState
                    title="No questions yet"
                    description={
                      blocksQuestions
                        ? "Question intake is disabled for this space."
                        : "Create the first question in this space to start the QnA workflow."
                    }
                  />
                )}
                <ChildListPagination
                  page={questionListState.page}
                  pageSize={questionListState.pageSize}
                  totalCount={questionQuery.data?.totalCount ?? 0}
                  isFetching={questionQuery.isFetching}
                  onPageChange={questionListState.setPage}
                  onPageSizeChange={questionListState.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "activity" ? (
            <Card id="space-activity-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Activity in this Space")}</span>
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
                {contextualActivity.length ? (
                  contextualActivity.map((event) => (
                    <div
                      key={event.id}
                      className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                    >
                      <div className="min-w-0 space-y-2">
                        <div className="flex flex-wrap gap-2">
                          <ActivityKindBadge kind={event.kind} />
                          <ActorKindBadge kind={event.actorKind} />
                        </div>
                        <p className="line-clamp-2 text-sm text-muted-foreground">
                          {event.notes || event.actorLabel || event.userPrint}
                        </p>
                      </div>
                      <ActivityRelationshipActions event={event} />
                    </div>
                  ))
                ) : (
                  <EmptyState
                    title="No contextual activity yet"
                    description="Activity appears here after questions in this Space receive status changes, votes, or feedback."
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
