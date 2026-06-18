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
  CircleHelp,
  CircleOff,
  Link2,
  Pencil,
  Plus,
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
import {
  useCreateQuestion,
  useDeleteQuestion,
  useQuestion,
  useQuestionList,
  useUpdateQuestionStatus,
} from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useSpace } from "@/domains/spaces/hooks";
import { useSource, useSourceList } from "@/domains/sources/hooks";
import {
  AnswerStatus,
  ChannelKind,
  QuestionStatus,
  SourceRole,
  SpaceStatus,
  VisibilityScope,
  activityKindLabels,
  actorKindLabels,
  questionStatusLabels,
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
  ActionPanelEndGroup,
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
  AnswerKindBadge,
  AnswerStatusBadge,
  QuestionStatusBadge,
  SourceRoleBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { useLocalPagination } from "@/shared/lib/use-local-pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { clampPage } from "@/shared/lib/pagination";
import { useRelationshipListState } from "@/shared/lib/use-relationship-list-state";

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
  const parentSpaceQuery = useSpace(questionQuery.data?.spaceId);
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
  const followUpQuestionQuery = useQuestionList({
    page: questionListState.page,
    pageSize: questionListState.pageSize,
    sorting: questionListState.sorting,
    searchText: questionListState.debouncedSearch || undefined,
    parentAnswerId: answerId,
    status:
      questionStatusFilter === "all" ? undefined : Number(questionStatusFilter),
    visibility:
      questionVisibilityFilter === "all"
        ? undefined
        : Number(questionVisibilityFilter),
    enabled: Boolean(answerId),
  });
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
  const createQuestion = useCreateQuestion();
  const deleteQuestion = useDeleteQuestion();
  const updateQuestionStatus = useUpdateQuestionStatus();
  const { resolveActivationVisibility, ActivationVisibilityDialog } =
    useActivationVisibilityPrompt();
  const addSource = useAddAnswerSource(answerId);
  const removeSource = useRemoveAnswerSource(answerId);
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [sourceSearch, setSourceSearch] = useState("");
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.Evidence),
  );
  const [newQuestionTitle, setNewQuestionTitle] = useState("");
  const [newQuestionSummary, setNewQuestionSummary] = useState("");
  const [relationshipTab, setRelationshipTab] = useState("sources");
  const [questionCreateOpen, setQuestionCreateOpen] = useState(false);
  const [questionFiltersOpen, setQuestionFiltersOpen] = useState(false);
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
  const followUpQuestions = followUpQuestionQuery.data?.items ?? [];
  const followUpQuestionCount =
    followUpQuestionQuery.data?.totalCount ??
    answerQuery.data?.followUpQuestions.length ??
    0;
  const isActiveSpace = parentSpaceQuery.data?.status === SpaceStatus.Active;
  const blocksQuestions = parentSpaceQuery.data
    ? isActiveSpace && !parentSpaceQuery.data.acceptsQuestions
    : false;
  const sourcesPagination = useLocalPagination({
    items: answerQuery.data?.sources ?? [],
  });
  const canCreateFollowUpQuestion =
    Boolean(answerQuery.data && questionQuery.data?.spaceId) &&
    !blocksQuestions;

  useEffect(() => {
    const totalCount = followUpQuestionQuery.data?.totalCount;

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
    followUpQuestionQuery.data?.totalCount,
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
  const regularLifecycleActionOptions = lifecycleActionOptions.filter(
    (option) => option.variant !== "destructive",
  );
  const destructiveLifecycleActionOptions = lifecycleActionOptions.filter(
    (option) => option.variant === "destructive",
  );
  const handleFollowUpQuestionStatusChange = async (
    question: (typeof followUpQuestions)[number],
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
          text: "This draft is not reusable yet. Activate it when the content is ready.",
        }
      : answerQuery.data.status === AnswerStatus.Archived
        ? {
            label: "Reactivate answer",
            run: activateCurrentAnswer,
            disabled: updateAnswerStatus.isPending,
            text: "This answer is archived. Reactivate it only if it should return to the usable knowledge set.",
          }
        : {
            label: "Review activity",
            tab: "activity",
            text: "This answer is active. Review recent events before changing lifecycle state.",
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
          description="Manage lifecycle status, optional evidence links, and trust signals for this answer candidate."
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
                  ...(questionQuery.data?.parentAnswerId
                    ? [
                        {
                          label: "Answer",
                          to: `/app/answers/${questionQuery.data.parentAnswerId}`,
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
                description:
                  "Lifecycle state that controls whether this answer is draft, active, or archived.",
                value: <AnswerStatusBadge status={answerQuery.data.status} />,
              },
              {
                label: "Visibility",
                description:
                  "Controls internal, authenticated external, or public answer exposure.",
                value: (
                  <VisibilityBadge visibility={answerQuery.data.visibility} />
                ),
              },
              {
                label: "Type",
                description:
                  "Classifies the answer so workflow and badges present it correctly.",
                value: <AnswerKindBadge kind={answerQuery.data.kind} />,
              },
              {
                label: "Signals",
                description:
                  "Vote and optional accepted-answer signals for this candidate.",
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
                description: "The question this answer candidate belongs to.",
                value: questionQuery.data?.title || answerQuery.data.questionId,
              },
              {
                label: "Author label",
                description:
                  "Optional attribution shown with the answer when authorship should be clear.",
                value: answerQuery.data.authorLabel || "Not set",
              },
              {
                label: "Score",
                description:
                  "Ranking signal used to compare answer candidates before acceptance overrides ordering.",
                value: String(answerQuery.data.score),
              },
              {
                label: "Follow-up questions",
                description:
                  "Optional questions that continue the Q&A path after this answer.",
                value: String(followUpQuestionCount),
              },
              {
                label: "Sort",
                description:
                  "Lower numbers surface first when acceptance does not override ordering.",
                value: String(answerQuery.data.sort),
              },
              {
                label: "Vote score",
                description:
                  "Net voting signal captured for this answer candidate.",
                value: String(answerQuery.data.voteScore),
              },
              {
                label: "AI confidence",
                description:
                  "Model confidence score available for this answer candidate.",
                value: String(answerQuery.data.aiConfidenceScore),
              },
              {
                label: "Created date",
                description: "Record creation timestamp.",
                value: formatOptionalDateTimeInTimeZone(
                  answerQuery.data.createdAtUtc,
                  portalTimeZone,
                  translateText("Not set"),
                ),
              },
              {
                label: "Update date",
                description: "Most recent record update timestamp.",
                value: formatOptionalDateTimeInTimeZone(
                  answerQuery.data.lastUpdatedAtUtc,
                  portalTimeZone,
                  translateText("Not set"),
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
        {regularLifecycleActionOptions.map((option) => (
          <Button
            key={option.status}
            size="sm"
            variant={option.variant}
            onClick={() => void option.run()}
            disabled={option.isPending}
          >
            {translateText(option.label)}
          </Button>
        ))}
        {destructiveLifecycleActionOptions.length > 0 ? (
          <ActionPanelEndGroup>
            {destructiveLifecycleActionOptions.map((option) => (
              <Button
                key={option.status}
                size="sm"
                variant={option.variant}
                data-action-tone="danger"
                onClick={() => void option.run()}
                disabled={option.isPending}
              >
                {translateText(option.label)}
              </Button>
            ))}
            <ConfirmAction
              title={translateText('Delete answer "{name}"?', {
                name:
                  answerQuery.data?.headline ?? translateText("this answer"),
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
                <ActionButton tone="danger" data-action-align="grouped">
                  <Trash2 className="size-4" />
                  {translateText("Delete")}
                </ActionButton>
              }
            />
          </ActionPanelEndGroup>
        ) : (
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
              <ActionButton tone="danger">
                <Trash2 className="size-4" />
                {translateText("Delete")}
              </ActionButton>
            }
          />
        )}
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
                <CardTitle>{translateText("Details")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <DetailFieldList
                items={[
                  {
                    label: "Headline",
                    description:
                      "The short answer that should appear as the candidate title.",
                    value: answerQuery.data.headline,
                    valueClassName: "text-base font-medium text-mono",
                  },
                  {
                    label: "Body",
                    description:
                      "The full guidance, including steps, caveats, and nuance.",
                    value:
                      answerQuery.data.body || translateText("No body yet"),
                    valueClassName: "whitespace-pre-wrap",
                  },
                  {
                    label: "Context note",
                    description:
                      "Optional note explaining why, when, or how this answer applies.",
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
                  "Attach optional evidence or reusable references for this answer.",
                icon: ShieldCheck,
                count: answerQuery.data?.sources.length ?? 0,
              },
              {
                key: "questions",
                label: "Follow-up questions",
                description:
                  "Open recursive questions that continue from this answer.",
                icon: CircleHelp,
                count: followUpQuestionCount,
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
                    description="Optional evidence or reusable references can be attached when this answer needs more context."
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

          {relationshipTab === "questions" ? (
            <Card id="answer-questions-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Follow-up questions")}</span>
                    <Badge variant="outline">
                      {translateText("{count} questions", {
                        count: followUpQuestionCount,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
                <div className="flex flex-wrap items-center gap-2">
                  {canCreateFollowUpQuestion ? (
                    <Button
                      type="button"
                      variant={questionCreateOpen ? "primary" : "outline"}
                      size="sm"
                      className="h-8 gap-1.5 px-2.5 text-xs"
                      aria-expanded={questionCreateOpen}
                      aria-controls="answer-follow-up-question-create-form"
                      onClick={() => setQuestionCreateOpen((open) => !open)}
                    >
                      <Plus className="size-4" />
                      {translateText("New question")}
                    </Button>
                  ) : null}
                  <RelationshipFilterButton
                    activeFilterCount={questionListState.activeFilterCount}
                    isLoading={followUpQuestionQuery.isFetching}
                    open={questionFiltersOpen}
                    onClick={() => setQuestionFiltersOpen((open) => !open)}
                  />
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {questionFiltersOpen ? (
                  <div className="rounded-lg border border-border/70 bg-muted/20 p-3">
                    <ListFilterToolbar
                      isLoading={followUpQuestionQuery.isFetching}
                    >
                      <ListFilterSearch
                        value={questionListState.search}
                        onChange={questionListState.setSearch}
                        placeholder="Search questions by title or summary"
                        activeFilterCount={questionListState.activeFilterCount}
                        onClear={questionListState.resetFilters}
                        isLoading={followUpQuestionQuery.isFetching}
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
                              <SelectItem value="all">
                                All question states
                              </SelectItem>
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
                {canCreateFollowUpQuestion && questionCreateOpen ? (
                  <form
                    id="answer-follow-up-question-create-form"
                    className="rounded-xl border border-primary/15 bg-primary/[0.025] p-4"
                    onSubmit={(event) => {
                      event.preventDefault();
                      const title = newQuestionTitle.trim();

                      if (!title || !questionQuery.data?.spaceId) {
                        return;
                      }

                      void createQuestion
                        .mutateAsync({
                          spaceId: questionQuery.data.spaceId,
                          title,
                          summary: newQuestionSummary.trim() || undefined,
                          contextNote: undefined,
                          status: QuestionStatus.Draft,
                          visibility: VisibilityScope.Internal,
                          originChannel: ChannelKind.Manual,
                          sort:
                            (followUpQuestionQuery.data?.totalCount ?? 0) + 1,
                          parentAnswerId: answerId,
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
                            {translateText("Create question")}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {translateText(
                              "Optionally link questions that should appear after this answer.",
                            )}
                          </p>
                        </div>
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-1.5">
                            <Label htmlFor="new-follow-up-question-title">
                              {translateText("Title")}
                            </Label>
                            <ContextHint
                              content={translateText(
                                "Use the canonical question wording.",
                              )}
                              label={translateText("Field details")}
                            />
                          </div>
                          <Input
                            id="new-follow-up-question-title"
                            value={newQuestionTitle}
                            onChange={(event) =>
                              setNewQuestionTitle(event.target.value)
                            }
                            placeholder={translateText("Title")}
                            aria-label={translateText("Title")}
                          />
                        </div>
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-1.5">
                            <Label htmlFor="new-follow-up-question-summary">
                              {translateText("Summary")}
                            </Label>
                            <ContextHint
                              content={translateText(
                                "A compact explanation of the question before the full context.",
                              )}
                              label={translateText("Field details")}
                            />
                          </div>
                          <Textarea
                            id="new-follow-up-question-summary"
                            value={newQuestionSummary}
                            onChange={(event) =>
                              setNewQuestionSummary(event.target.value)
                            }
                            placeholder={translateText("Summary")}
                            aria-label={translateText("Summary")}
                            rows={3}
                          />
                        </div>
                      </div>
                      <div className="flex items-end">
                        <Button
                          type="submit"
                          disabled={
                            !newQuestionTitle.trim() ||
                            createQuestion.isPending
                          }
                        >
                          <Plus className="size-4" />
                          {translateText("Create question")}
                        </Button>
                      </div>
                    </div>
                  </form>
                ) : null}
                {followUpQuestions.length ? (
                  <div className="space-y-3">
                    {followUpQuestions.map((question) => (
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
                            <Badge variant="outline">
                              {question.spaceSlug}
                            </Badge>
                            <QuestionStatusBadge status={question.status} />
                            <VisibilityBadge
                              visibility={question.visibility}
                            />
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
                              void handleFollowUpQuestionStatusChange(question)
                            }
                          >
                            {question.status === QuestionStatus.Active ? (
                              <CircleOff className="size-4" />
                            ) : (
                              <CheckCircle2 className="size-4" />
                            )}
                            {translateText(
                              question.status === QuestionStatus.Active
                                ? "Archive"
                                : "Activate",
                            )}
                          </Button>
                          <ConfirmAction
                            title={translateText(
                              'Delete question "{name}"?',
                              {
                                name: question.title,
                              },
                            )}
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
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title="No follow-up questions linked"
                    description="Optionally link questions that should appear after this answer."
                  />
                )}
                <ChildListPagination
                  page={questionListState.page}
                  pageSize={questionListState.pageSize}
                  totalCount={followUpQuestionQuery.data?.totalCount ?? 0}
                  isFetching={followUpQuestionQuery.isFetching}
                  onPageChange={questionListState.setPage}
                  onPageSizeChange={questionListState.setPageSize}
                />
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
