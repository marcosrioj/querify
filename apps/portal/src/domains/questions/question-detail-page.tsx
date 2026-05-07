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
  Pencil,
  Plus,
  Tags,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ActivityRelationshipActions } from "@/domains/activity/activity-relationship-actions";
import { useActivityList } from "@/domains/activity/hooks";
import {
  useAnswerList,
  useCreateAnswer,
  useDeleteAnswer,
  useUpdateAnswerStatus,
} from "@/domains/answers/hooks";
import type { AnswerDto } from "@/domains/answers/types";
import { useActivationVisibilityPrompt } from "@/domains/qna/activation-visibility";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { RecommendedNextActionCard } from "@/domains/qna/recommended-next-action-card";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import {
  useQuestion,
  useAddQuestionSource,
  useAddQuestionTag,
  useDeleteQuestion,
  useRemoveQuestionSource,
  useRemoveQuestionTag,
  useUpdateQuestion,
} from "@/domains/questions/hooks";
import type {
  QuestionDetailDto,
  QuestionUpdateRequestDto,
} from "@/domains/questions/types";
import { useSource, useSourceList } from "@/domains/sources/hooks";
import { useSpace } from "@/domains/spaces/hooks";
import { useTag, useTagList } from "@/domains/tags/hooks";
import {
  AnswerKind,
  AnswerStatus,
  QuestionStatus,
  SourceRole,
  VisibilityScope,
  activityKindLabels,
  actorKindLabels,
  answerStatusLabels,
  sourceRoleLabels,
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
  AnswerStatusBadge,
  ChannelKindBadge,
  QuestionStatusBadge,
  SourceRoleBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { useLocalPagination } from "@/shared/lib/use-local-pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { clampPage } from "@/shared/lib/pagination";
import { useRelationshipListState } from "@/shared/lib/use-relationship-list-state";

const answerSortingOptions = [
  { value: "LastUpdatedAtUtc DESC", label: "Last update newest" },
  { value: "LastUpdatedAtUtc ASC", label: "Last update oldest" },
  { value: "Headline ASC", label: "Headline A-Z" },
  { value: "Headline DESC", label: "Headline Z-A" },
  { value: "Score DESC", label: "Score high-low" },
  { value: "Score ASC", label: "Score low-high" },
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

const ANSWER_RELATIONSHIP_FILTER_DEFAULTS: Record<
  "status" | "visibility" | "accepted",
  string
> = {
  status: "all",
  visibility: "all",
  accepted: "all",
};

const ACTIVITY_RELATIONSHIP_FILTER_DEFAULTS: Record<
  "kind" | "actorKind",
  string
> = {
  kind: "all",
  actorKind: "all",
};

function buildQuestionUpdateBody(
  question: QuestionDetailDto,
  overrides: Partial<QuestionUpdateRequestDto> = {},
): QuestionUpdateRequestDto {
  return {
    title: question.title,
    summary: question.summary ?? undefined,
    contextNote: question.contextNote ?? undefined,
    status: question.status,
    visibility: question.visibility,
    originChannel: question.originChannel,
    sort: question.sort,
    acceptedAnswerId: question.acceptedAnswerId ?? null,
    ...overrides,
  };
}

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
}) {
  const label = source.label || source.locator;

  return {
    value: source.id,
    label,
    description: source.locator,
    keywords: [label, source.locator],
  };
}

function buildAnswerOption(answer: {
  id: string;
  headline: string;
  body?: string | null;
  status: AnswerStatus;
}) {
  return {
    value: answer.id,
    label: answer.headline,
    description: answerStatusLabels[answer.status],
    keywords: [
      answer.headline,
      answer.body ?? "",
      answerStatusLabels[answer.status],
    ],
  };
}

export function QuestionDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const questionQuery = useQuestion(id);
  const spaceQuery = useSpace(questionQuery.data?.spaceId);
  const deleteQuestion = useDeleteQuestion();
  const updateQuestion = useUpdateQuestion(id ?? "");
  const createAnswer = useCreateAnswer();
  const deleteAnswer = useDeleteAnswer();
  const updateAnswerStatus = useUpdateAnswerStatus();
  const { resolveActivationVisibility, ActivationVisibilityDialog } =
    useActivationVisibilityPrompt();
  const addTag = useAddQuestionTag(id ?? "");
  const removeTag = useRemoveQuestionTag(id ?? "");
  const addSource = useAddQuestionSource(id ?? "");
  const removeSource = useRemoveQuestionSource(id ?? "");
  const [selectedTagId, setSelectedTagId] = useState("");
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [tagSearch, setTagSearch] = useState("");
  const [sourceSearch, setSourceSearch] = useState("");
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.Context),
  );
  const [selectedAnswerId, setSelectedAnswerId] = useState("");
  const [acceptedAnswerSearch, setAcceptedAnswerSearch] = useState("");
  const [newAnswerHeadline, setNewAnswerHeadline] = useState("");
  const [newAnswerBody, setNewAnswerBody] = useState("");
  const [relationshipTab, setRelationshipTab] = useState("answers");
  const [answerCreateOpen, setAnswerCreateOpen] = useState(false);
  const [answerFiltersOpen, setAnswerFiltersOpen] = useState(false);
  const [activityFiltersOpen, setActivityFiltersOpen] = useState(false);
  const answerListState = useRelationshipListState({
    defaultSorting: "LastUpdatedAtUtc DESC",
    filterDefaults: ANSWER_RELATIONSHIP_FILTER_DEFAULTS,
  });
  const activityListState = useRelationshipListState({
    defaultSorting: "OccurredAtUtc DESC",
    filterDefaults: ACTIVITY_RELATIONSHIP_FILTER_DEFAULTS,
  });
  const deferredTagSearch = useDeferredValue(tagSearch.trim());
  const deferredSourceSearch = useDeferredValue(sourceSearch.trim());
  const deferredAcceptedAnswerSearch = useDeferredValue(
    acceptedAnswerSearch.trim(),
  );
  const answerStatusFilter = answerListState.filters.status;
  const answerVisibilityFilter = answerListState.filters.visibility;
  const answerAcceptedFilter = answerListState.filters.accepted;
  const activityKindFilter = activityListState.filters.kind;
  const activityActorKindFilter = activityListState.filters.actorKind;
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
  const answerListQuery = useAnswerList({
    page: answerListState.page,
    pageSize: answerListState.pageSize,
    sorting: answerListState.sorting,
    searchText: answerListState.debouncedSearch || undefined,
    questionId: id,
    status:
      answerStatusFilter === "all" ? undefined : Number(answerStatusFilter),
    visibility:
      answerVisibilityFilter === "all"
        ? undefined
        : Number(answerVisibilityFilter),
    isAccepted:
      answerAcceptedFilter === "all"
        ? undefined
        : answerAcceptedFilter === "true",
    enabled: Boolean(id),
  });
  const acceptedAnswerOptionsQuery = useAnswerList({
    page: 1,
    pageSize: 20,
    sorting: "Headline ASC",
    searchText: deferredAcceptedAnswerSearch || undefined,
    questionId: id,
    status: AnswerStatus.Active,
    enabled: Boolean(id),
  });
  const activityListQuery = useActivityList({
    page: activityListState.page,
    pageSize: activityListState.pageSize,
    sorting: activityListState.sorting,
    searchText: activityListState.debouncedSearch || undefined,
    questionId: id,
    kind: activityKindFilter === "all" ? undefined : Number(activityKindFilter),
    actorKind:
      activityActorKindFilter === "all"
        ? undefined
        : Number(activityActorKindFilter),
    enabled: Boolean(id),
  });
  const selectedTagQuery = useTag(selectedTagId || undefined);
  const selectedSourceQuery = useSource(selectedSourceId || undefined);

  const availableTags = useMemo(() => {
    const existing = new Set(
      (questionQuery.data?.tags ?? []).map((tag) => tag.id),
    );
    return (tagOptionsQuery.data?.items ?? []).filter(
      (tag) => !existing.has(tag.id),
    );
  }, [questionQuery.data?.tags, tagOptionsQuery.data?.items]);

  const availableSources = useMemo(() => {
    const existing = new Set(
      (questionQuery.data?.sources ?? []).map((link) => link.sourceId),
    );
    return (sourceOptionsQuery.data?.items ?? []).filter(
      (source) => !existing.has(source.id),
    );
  }, [questionQuery.data?.sources, sourceOptionsQuery.data?.items]);

  const showLoadingState =
    !questionQuery.data &&
    (questionQuery.isLoading ||
      spaceQuery.isLoading ||
      sourceOptionsQuery.isLoading ||
      tagOptionsQuery.isLoading);

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
  const acceptedAnswerOptions = acceptedAnswerOptionsQuery.data?.items ?? [];
  const acceptedAnswerSelectValue =
    selectedAnswerId || questionQuery.data?.acceptedAnswerId || "";
  const selectedAcceptedAnswer =
    acceptedAnswerOptions.find(
      (answer) => answer.id === acceptedAnswerSelectValue,
    ) ??
    questionQuery.data?.acceptedAnswer ??
    null;
  const selectedAcceptedAnswerOption = selectedAcceptedAnswer
    ? buildAnswerOption(selectedAcceptedAnswer)
    : null;
  const questionActivity = activityListQuery.data?.items ?? [];
  const questionAnswers = answerListQuery.data?.items ?? [];
  const tagsPagination = useLocalPagination({
    items: questionQuery.data?.tags ?? [],
  });
  const sourcesPagination = useLocalPagination({
    items: questionQuery.data?.sources ?? [],
  });

  useEffect(() => {
    const totalCount = answerListQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(
      answerListState.page,
      totalCount,
      answerListState.pageSize,
    );
    if (nextPage !== answerListState.page) {
      answerListState.setPage(nextPage);
    }
  }, [
    answerListQuery.data?.totalCount,
    answerListState.page,
    answerListState.pageSize,
    answerListState.setPage,
  ]);

  useEffect(() => {
    const totalCount = activityListQuery.data?.totalCount;

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
    activityListQuery.data?.totalCount,
    activityListState.page,
    activityListState.pageSize,
    activityListState.setPage,
  ]);

  const handleAnswerStatusChange = async (answer: AnswerDto) => {
    const status =
      answer.status === AnswerStatus.Active
        ? AnswerStatus.Archived
        : AnswerStatus.Active;
    let visibility: VisibilityScope | undefined;
    if (answer.status !== AnswerStatus.Active) {
      const resolvedVisibility = await resolveActivationVisibility(
        answer.visibility,
      );
      if (resolvedVisibility === null) {
        return;
      }

      visibility = resolvedVisibility;
    }

    updateAnswerStatus.mutate({
      answer,
      status,
      visibility,
    });
  };

  const activateRelationshipTab = (tab: string, focusTargetId?: string) => {
    setRelationshipTab(tab);
    if (tab === "answers" && focusTargetId === "new-answer-headline") {
      setAnswerCreateOpen(true);
    }

    window.requestAnimationFrame(() => {
      window.requestAnimationFrame(() => {
        const section = document.getElementById(`question-${tab}-section`);
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

  const scrollToDetailTarget = (targetId: string) => {
    window.requestAnimationFrame(() => {
      const target = document.getElementById(targetId);

      target?.scrollIntoView({
        behavior: "smooth",
        block: "start",
      });

      if (target instanceof HTMLElement) {
        target.focus({ preventScroll: true });
      }
    });
  };

  if (!id) {
    return (
      <ErrorState
        title="Invalid question route"
        description="Question detail routes need an identifier."
      />
    );
  }

  const spaceBlocksAnswers = spaceQuery.data
    ? !spaceQuery.data.acceptsAnswers
    : false;
  const questionNextAction = !questionQuery.data
    ? {
        label: "Open spaces",
        to: "/app/spaces",
        text: "Return to Spaces while this question loads.",
      }
    : questionQuery.data.status === QuestionStatus.Draft
      ? {
          label: "Review status",
          to: `/app/questions/${id}/edit`,
          text: "This draft question is not reusable yet. Activate it when it should be available for operators and automation.",
        }
      : questionQuery.data.status === QuestionStatus.Archived
        ? {
            label: "Review status",
            to: `/app/questions/${id}/edit`,
            text: "This question is archived. Review the status before returning it to active use.",
          }
        : {
            label: "Review activity",
            tab: "activity",
            text: "This question is active and available for operators and automation.",
          };

  return (
    <DetailLayout
      header={
        <PageHeader
          title={questionQuery.data?.title ?? "Question"}
          description="Operate question workflow, answers, optional accepted answer, source links, tags, and activity from one place."
          descriptionMode="hint"
          backTo={
            questionQuery.data?.spaceId
              ? `/app/spaces/${questionQuery.data.spaceId}`
              : "/app/spaces"
          }
          breadcrumbs={
            questionQuery.data
              ? [
                  {
                    label: "Space",
                    to: `/app/spaces/${questionQuery.data.spaceId}`,
                  },
                  { label: "Question" },
                ]
              : undefined
          }
        />
      }
      sidebar={
        showLoadingState ? (
          <SidebarSummarySkeleton />
        ) : questionQuery.data ? (
          <DetailOverviewCard
            title="Overview"
            description="These values describe the question state, intake path, and workflow signals."
            highlights={[
              {
                label: "Status",
                description:
                  "Controls whether the question is draft, active, or archived.",
                value: (
                  <QuestionStatusBadge status={questionQuery.data.status} />
                ),
              },
              {
                label: "Visibility",
                description:
                  "Controls internal, authenticated external, or public question exposure.",
                value: (
                  <VisibilityBadge visibility={questionQuery.data.visibility} />
                ),
              },
              {
                label: "Signals",
                description:
                  "Public and operator feedback signals captured for this question.",
                value: translateText("Feedback {value}", {
                  value: questionQuery.data.feedbackScore,
                }),
              },
              {
                label: "Answers",
                description:
                  "Answer candidates currently attached to this question.",
                value: String(answerListQuery.data?.totalCount ?? 0),
              },
            ]}
            items={[
              {
                label: "Space",
                description:
                  "The space controls exposure and how the question should be operated.",
                value: spaceQuery.data?.name ?? questionQuery.data.spaceSlug,
              },
              {
                label: "Accepted answer",
                description:
                  "Optional answer selected as the canonical response for this question.",
                value: questionQuery.data.acceptedAnswer?.headline || "None",
              },
              {
                label: "Last activity",
                description:
                  "Most recent workflow, source, answer, vote, or feedback activity recorded for this question.",
                value: formatOptionalDateTimeInTimeZone(
                  questionQuery.data.lastActivityAtUtc,
                  portalTimeZone,
                  translateText("Not set"),
                ),
              },
              {
                label: "Origin channel",
                description:
                  "Records where the question came from for reporting and routing.",
                value: (
                  <ChannelKindBadge kind={questionQuery.data.originChannel} />
                ),
              },
              {
                label: "AI confidence",
                description:
                  "Model confidence score available for this question's generated or assisted content.",
                value: String(questionQuery.data.aiConfidenceScore),
              },
              {
                label: "Sort",
                description: "Lower values appear earlier in curated ordering.",
                value: String(questionQuery.data.sort),
              },
            ]}
          />
        ) : null
      }
    >
      {ActivationVisibilityDialog}
      <ActionPanel layout="bar" description="Question actions and navigation.">
        <ActionButton asChild tone="secondary">
          <Link to={`/app/questions/${id}/edit`}>
            <Pencil className="size-4" />
            {translateText("Edit")}
          </Link>
        </ActionButton>
        {questionQuery.data?.spaceId ? (
          <ActionButton asChild tone="secondary">
            <Link to={`/app/spaces/${questionQuery.data.spaceId}`}>
              <Link2 className="size-4" />
              {translateText("Open space")}
            </Link>
          </ActionButton>
        ) : null}
        <ConfirmAction
          title={translateText('Delete question "{name}"?', {
            name: questionQuery.data?.title ?? translateText("this question"),
          })}
          description={translateText(
            "This removes the question, its accepted-answer state, and any public-signal aggregation from the portal view.",
          )}
          confirmLabel={translateText("Delete question")}
          isPending={deleteQuestion.isPending}
          onConfirm={() =>
            deleteQuestion
              .mutateAsync(id)
              .then(() =>
                navigate(
                  questionQuery.data?.spaceId
                    ? `/app/spaces/${questionQuery.data.spaceId}`
                    : "/app/spaces",
                ),
              )
          }
          trigger={
            <ActionButton tone="danger">
              <Trash2 className="size-4" />
              {translateText("Delete")}
            </ActionButton>
          }
        />
      </ActionPanel>
      {questionQuery.isError ? (
        <ErrorState
          title="Unable to load question"
          error={questionQuery.error}
          retry={() => void questionQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={6} metrics={0} />
      ) : questionQuery.data ? (
        <>
          <RecommendedNextActionCard
            label={questionNextAction.label}
            text={questionNextAction.text}
            action={
              "to" in questionNextAction ? (
                <Button asChild>
                  <Link to={questionNextAction.to}>
                    {translateText(questionNextAction.label)}
                  </Link>
                </Button>
              ) : "targetId" in questionNextAction ? (
                <Button
                  type="button"
                  onClick={() =>
                    scrollToDetailTarget(questionNextAction.targetId)
                  }
                >
                  {translateText(questionNextAction.label)}
                </Button>
              ) : (
                <Button
                  type="button"
                  onClick={() =>
                    activateRelationshipTab(
                      questionNextAction.tab,
                      questionNextAction.focusTargetId,
                    )
                  }
                >
                  {translateText(questionNextAction.label)}
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
                    label: "Title",
                    description: "Use the canonical question wording.",
                    value: questionQuery.data.title,
                    valueClassName: "text-base font-medium text-mono",
                  },
                  {
                    label: "Summary",
                    description:
                      "A compact explanation of the question before the full context.",
                    value:
                      questionQuery.data.summary ||
                      translateText("No summary provided."),
                  },
                  {
                    label: "Context note",
                    description:
                      "Operational nuance that answer authors should understand.",
                    value:
                      questionQuery.data.contextNote ||
                      translateText("No context note recorded."),
                    valueClassName: "whitespace-pre-wrap",
                  },
                ]}
              />
            </CardContent>
          </Card>

          <Card id="question-accepted-answer-section">
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Accepted answer")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <p className="text-sm text-muted-foreground">
                  {translateText(
                    "Optionally choose the accepted answer that should anchor the canonical response for this question.",
                  )}
                </p>
                <SearchSelect
                  id="question-accepted-answer-picker"
                  value={acceptedAnswerSelectValue}
                  onValueChange={setSelectedAnswerId}
                  options={acceptedAnswerOptions.map(buildAnswerOption)}
                  selectedOption={selectedAcceptedAnswerOption}
                  placeholder={translateText("Select accepted answer")}
                  searchPlaceholder={translateText("Search answers...")}
                  emptyMessage={translateText("No active answers found.")}
                  loading={acceptedAnswerOptionsQuery.isFetching}
                  searchValue={acceptedAnswerSearch}
                  onSearchChange={(value) =>
                    startTransition(() => setAcceptedAnswerSearch(value))
                  }
                  resultCountHint={translateText(
                    "Only active answers can be selected.",
                  )}
                />
                {(acceptedAnswerOptionsQuery.data?.totalCount ?? 0) ? null : (
                  <p className="text-sm text-muted-foreground">
                    {translateText(
                      "Activate an answer before selecting it here.",
                    )}
                  </p>
                )}
                {(acceptedAnswerOptionsQuery.data?.totalCount ?? 0) ||
                questionQuery.data.acceptedAnswerId ? (
                  <div className="flex flex-wrap gap-2">
                    {(acceptedAnswerOptionsQuery.data?.totalCount ?? 0) ? (
                      <Button
                        disabled={
                          !selectedAnswerId ||
                          selectedAnswerId ===
                            questionQuery.data.acceptedAnswerId ||
                          updateQuestion.isPending
                        }
                        onClick={() =>
                          updateQuestion
                            .mutateAsync(
                              buildQuestionUpdateBody(questionQuery.data, {
                                acceptedAnswerId: selectedAnswerId,
                              }),
                            )
                            .then(() => setSelectedAnswerId(""))
                        }
                      >
                        {translateText("Set accepted answer")}
                      </Button>
                    ) : null}
                    {questionQuery.data.acceptedAnswerId ? (
                      <Button
                        variant="outline"
                        disabled={updateQuestion.isPending}
                        onClick={() =>
                          updateQuestion.mutateAsync(
                            buildQuestionUpdateBody(questionQuery.data, {
                              acceptedAnswerId: null,
                            }),
                          )
                        }
                      >
                        {translateText("Clear accepted answer")}
                      </Button>
                    ) : null}
                  </div>
                ) : null}
              </div>
            </CardContent>
          </Card>

          <QnaModuleNav
            eyebrow="Question relationships"
            activeKey={relationshipTab}
            onActiveKeyChange={setRelationshipTab}
            items={[
              {
                key: "answers",
                label: "Answers",
                description:
                  "Write, activate, or archive answer candidates for this question.",
                icon: CheckCircle2,
                count: answerListQuery.data?.totalCount ?? 0,
              },
              {
                key: "sources",
                label: "Sources",
                description:
                  "Attach optional evidence or reusable references when this question needs additional context.",
                icon: Link2,
                count: questionQuery.data?.sources.length ?? 0,
              },
              {
                key: "tags",
                label: "Tags",
                description:
                  "Attach taxonomy that helps operators find related questions.",
                icon: Tags,
                count: questionQuery.data?.tags.length ?? 0,
              },
              {
                key: "activity",
                label: "Activity",
                description:
                  "Review status changes, votes, feedback, and workflow history.",
                icon: Activity,
                count: activityListQuery.data?.totalCount ?? 0,
              },
            ]}
          />

          {relationshipTab === "tags" ? (
            <Card id="question-tags-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Tags")}</span>
                    <Badge variant="outline">
                      {translateText("{count} tags", {
                        count: questionQuery.data.tags.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {questionQuery.data.tags.length ? (
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
                    description="Attach tags so operators can group and find related questions faster."
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
                        .mutateAsync({ questionId: id, tagId: selectedTagId })
                        .then(() => setSelectedTagId(""))
                    }
                  >
                    <Tags className="size-4" />
                    {translateText("Attach tag")}
                  </Button>
                </div>
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "sources" ? (
            <Card id="question-sources-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Source links")}</span>
                    <Badge variant="outline">
                      {translateText("{count} sources", {
                        count: questionQuery.data.sources.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {questionQuery.data.sources.length ? (
                  <div className="space-y-3">
                    {sourcesPagination.pagedItems.map((sourceLink) => (
                      <div
                        key={sourceLink.id}
                        className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                      >
                        <div className="min-w-0">
                          {sourceLink.source ? (
                            <Link
                              to={`/app/sources/${sourceLink.source.id}`}
                              className="font-medium text-mono hover:text-primary"
                            >
                              {sourceLink.source.label ||
                                sourceLink.source.locator ||
                                sourceLink.sourceId}
                            </Link>
                          ) : (
                            <p className="font-medium text-mono">
                              {sourceLink.sourceId}
                            </p>
                          )}
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
                    description="Attach origin, evidence, or reusable reference material to strengthen the question."
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
                    id="question-source-picker"
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
                          questionId: id,
                          sourceId: selectedSourceId,
                          role: Number(selectedSourceRole) as SourceRole,
                          order: questionQuery.data.sources.length + 1,
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

          {relationshipTab === "answers" ? (
            <Card id="question-answers-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Answers")}</span>
                    <Badge variant="outline">
                      {translateText("{count} answers", {
                        count: answerListQuery.data?.totalCount ?? 0,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
                <div className="flex flex-wrap items-center gap-2">
                  {!spaceBlocksAnswers ? (
                    <Button
                      type="button"
                      variant={answerCreateOpen ? "primary" : "outline"}
                      size="sm"
                      className="h-8 gap-1.5 px-2.5 text-xs"
                      aria-expanded={answerCreateOpen}
                      aria-controls="question-answer-create-form"
                      onClick={() => setAnswerCreateOpen((open) => !open)}
                    >
                      <Plus className="size-4" />
                      {translateText("New answer")}
                    </Button>
                  ) : null}
                  <RelationshipFilterButton
                    activeFilterCount={answerListState.activeFilterCount}
                    isLoading={answerListQuery.isFetching}
                    open={answerFiltersOpen}
                    onClick={() => setAnswerFiltersOpen((open) => !open)}
                  />
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {answerFiltersOpen ? (
                  <div className="rounded-lg border border-border/70 bg-muted/20 p-3">
                    <ListFilterToolbar isLoading={answerListQuery.isFetching}>
                      <ListFilterSearch
                        value={answerListState.search}
                        onChange={answerListState.setSearch}
                        placeholder="Search answers by headline or body"
                        activeFilterCount={answerListState.activeFilterCount}
                        onClear={answerListState.resetFilters}
                        isLoading={answerListQuery.isFetching}
                      />
                      <div className="grid gap-3 md:grid-cols-4">
                        <ListFilterField label="Status">
                          <Select
                            value={answerStatusFilter}
                            onValueChange={(value) =>
                              answerListState.setFilter("status", value)
                            }
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Status")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="all">All</SelectItem>
                              {Object.entries(answerStatusLabels).map(
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
                            value={answerVisibilityFilter}
                            onValueChange={(value) =>
                              answerListState.setFilter("visibility", value)
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
                        <ListFilterField label="Accepted">
                          <Select
                            value={answerAcceptedFilter}
                            onValueChange={(value) =>
                              answerListState.setFilter("accepted", value)
                            }
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Accepted state")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="all">
                                All answer states
                              </SelectItem>
                              <SelectItem value="true">
                                Accepted only
                              </SelectItem>
                              <SelectItem value="false">
                                Not accepted
                              </SelectItem>
                            </SelectContent>
                          </Select>
                        </ListFilterField>
                        <ListFilterField label="Sort">
                          <Select
                            value={answerListState.sorting}
                            onValueChange={answerListState.setSorting}
                          >
                            <SelectTrigger className="w-full" size="lg">
                              <SelectValue
                                placeholder={translateText("Sort answers")}
                              />
                            </SelectTrigger>
                            <SelectContent>
                              {answerSortingOptions.map((option) => (
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
                {!spaceBlocksAnswers && answerCreateOpen ? (
                  <form
                    id="question-answer-create-form"
                    className="rounded-xl border border-primary/15 bg-primary/[0.025] p-4"
                    onSubmit={(event) => {
                      event.preventDefault();
                      const headline = newAnswerHeadline.trim();

                      if (!headline) {
                        return;
                      }

                      void createAnswer
                        .mutateAsync({
                          questionId: id,
                          headline,
                          body: newAnswerBody.trim() || undefined,
                          kind: AnswerKind.Official,
                          status: AnswerStatus.Draft,
                          visibility: VisibilityScope.Internal,
                          contextNote: undefined,
                          authorLabel: undefined,
                          sort: (answerListQuery.data?.totalCount ?? 0) + 1,
                        })
                        .then(() => {
                          setNewAnswerHeadline("");
                          setNewAnswerBody("");
                          setAnswerCreateOpen(false);
                        });
                    }}
                  >
                    <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_auto]">
                      <div className="space-y-3">
                        <div>
                          <p className="text-sm font-semibold text-mono">
                            {translateText("Create answer for this question")}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {translateText(
                              "Write the answer here, then activate it from the list below.",
                            )}
                          </p>
                        </div>
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-1.5">
                            <Label htmlFor="new-answer-headline">
                              {translateText("Answer headline")}
                            </Label>
                            <ContextHint
                              content={translateText(
                                "Short answer title saved into this question.",
                              )}
                              label={translateText("Field details")}
                            />
                          </div>
                          <Input
                            id="new-answer-headline"
                            value={newAnswerHeadline}
                            onChange={(event) =>
                              setNewAnswerHeadline(event.target.value)
                            }
                            placeholder={translateText("Answer headline")}
                            aria-label={translateText("Answer headline")}
                          />
                        </div>
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-1.5">
                            <Label htmlFor="new-answer-body">
                              {translateText("Answer body")}
                            </Label>
                            <ContextHint
                              content={translateText(
                                "Optional full answer body for steps, caveats, and detail.",
                              )}
                              label={translateText("Field details")}
                            />
                          </div>
                          <Textarea
                            id="new-answer-body"
                            value={newAnswerBody}
                            onChange={(event) =>
                              setNewAnswerBody(event.target.value)
                            }
                            placeholder={translateText("Answer body")}
                            aria-label={translateText("Answer body")}
                            rows={5}
                          />
                        </div>
                      </div>
                      <div className="flex items-end">
                        <Button
                          type="submit"
                          disabled={
                            !newAnswerHeadline.trim() || createAnswer.isPending
                          }
                        >
                          <Plus className="size-4" />
                          {translateText("Create answer")}
                        </Button>
                      </div>
                    </div>
                  </form>
                ) : null}
                {questionAnswers.length ? (
                  questionAnswers.map((answer) => (
                    <div
                      key={answer.id}
                      className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                    >
                      <div className="min-w-0">
                        <Link
                          to={`/app/answers/${answer.id}`}
                          className="font-medium text-mono hover:text-primary"
                        >
                          {answer.headline}
                        </Link>
                        <div className="mt-2 flex flex-wrap items-center gap-2">
                          <AnswerStatusBadge status={answer.status} />
                          <VisibilityBadge visibility={answer.visibility} />
                          <span className="text-xs text-muted-foreground">
                            {translateText("Last update {value}", {
                              value: formatOptionalDateTimeInTimeZone(
                                answer.lastUpdatedAtUtc,
                                portalTimeZone,
                                translateText("Not set"),
                              ),
                            })}
                          </span>
                          {answer.isAccepted ? (
                            <Badge variant="success">
                              {translateText("Accepted")}
                            </Badge>
                          ) : null}
                          {answer.isOfficial ? (
                            <Badge variant="primary">
                              {translateText("Official")}
                            </Badge>
                          ) : null}
                          <Badge variant="outline">
                            {translateText("Votes {value}", {
                              value: answer.voteScore,
                            })}
                          </Badge>
                        </div>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/answers/${answer.id}`}>
                            <Link2 className="size-4" />
                            {translateText("Open")}
                          </Link>
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={updateAnswerStatus.isPending}
                          onClick={() => void handleAnswerStatusChange(answer)}
                        >
                          {answer.status === AnswerStatus.Active ? (
                            <CircleOff className="size-4" />
                          ) : (
                            <CheckCircle2 className="size-4" />
                          )}
                          {translateText(
                            answer.status === AnswerStatus.Active
                              ? "Archive"
                              : "Activate",
                          )}
                        </Button>
                        <ConfirmAction
                          title={translateText('Delete answer "{name}"?', {
                            name: answer.headline,
                          })}
                          description={translateText(
                            "This removes the answer candidate and any vote history tied to it.",
                          )}
                          confirmLabel={translateText("Delete answer")}
                          isPending={deleteAnswer.isPending}
                          onConfirm={() => deleteAnswer.mutateAsync(answer.id)}
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
                    title="No answers yet"
                    description={
                      spaceBlocksAnswers
                        ? "This space currently blocks new answer creation."
                        : "Optional answers can be added when this question needs reusable response content."
                    }
                  />
                )}
                <ChildListPagination
                  page={answerListState.page}
                  pageSize={answerListState.pageSize}
                  totalCount={answerListQuery.data?.totalCount ?? 0}
                  isFetching={answerListQuery.isFetching}
                  onPageChange={answerListState.setPage}
                  onPageSizeChange={answerListState.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "activity" ? (
            <Card id="question-activity-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Activity")}</span>
                    <Badge variant="outline">
                      {translateText("{count} events", {
                        count: activityListQuery.data?.totalCount ?? 0,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
                <RelationshipFilterButton
                  activeFilterCount={activityListState.activeFilterCount}
                  isLoading={activityListQuery.isFetching}
                  open={activityFiltersOpen}
                  onClick={() => setActivityFiltersOpen((open) => !open)}
                />
              </CardHeader>
              <CardContent className="space-y-3">
                {activityFiltersOpen ? (
                  <div className="rounded-lg border border-border/70 bg-muted/20 p-3">
                    <ListFilterToolbar isLoading={activityListQuery.isFetching}>
                      <ListFilterSearch
                        value={activityListState.search}
                        onChange={activityListState.setSearch}
                        placeholder="Search activity by actor, notes, or subject"
                        activeFilterCount={activityListState.activeFilterCount}
                        onClear={activityListState.resetFilters}
                        isLoading={activityListQuery.isFetching}
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
                {questionActivity.length ? (
                  questionActivity.map((event) => (
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
                      <ActivityRelationshipActions event={event} />
                    </div>
                  ))
                ) : (
                  <EmptyState
                    title="No activity yet"
                    description="Workflow events, public signals, and moderation actions will appear here."
                  />
                )}
                <ChildListPagination
                  page={activityListState.page}
                  pageSize={activityListState.pageSize}
                  totalCount={activityListQuery.data?.totalCount ?? 0}
                  isFetching={activityListQuery.isFetching}
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
