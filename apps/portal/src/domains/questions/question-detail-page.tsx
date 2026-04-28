import { startTransition, useDeferredValue, useMemo, useState } from "react";
import {
  Activity,
  CheckCircle2,
  GitFork,
  Link2,
  MessageSquareText,
  Pencil,
  Plus,
  Tags,
  Trash2,
  TriangleAlert,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useActivityList } from "@/domains/activity/hooks";
import type { ActivityDto } from "@/domains/activity/types";
import { useCreateAnswer, useDeleteAnswer } from "@/domains/answers/hooks";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import {
  useQuestion,
  useAddQuestionSource,
  useAddQuestionTag,
  useApproveQuestion,
  useDeleteQuestion,
  useEscalateQuestion,
  useRejectQuestion,
  useRemoveQuestionSource,
  useRemoveQuestionTag,
  useSubmitQuestion,
  useUpdateQuestion,
  useQuestionList,
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
  SpaceKind,
  VisibilityScope,
  answerStatusLabels,
  sourceRoleLabels,
} from "@/shared/constants/backend-enums";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
  SectionGrid,
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
    duplicateOfQuestionId: question.duplicateOfQuestionId ?? null,
    ...overrides,
  };
}

function getQuestionStatusAfterClearingResolution(question: QuestionDetailDto) {
  if (question.duplicateOfQuestionId) {
    return QuestionStatus.Duplicate;
  }

  if (question.acceptedAnswerId) {
    return question.validatedAtUtc
      ? QuestionStatus.Validated
      : QuestionStatus.Answered;
  }

  if (question.validatedAtUtc) {
    return QuestionStatus.Validated;
  }

  return QuestionStatus.Open;
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

function buildQuestionOption(question: {
  id: string;
  title: string;
  spaceSlug?: string;
}) {
  return {
    value: question.id,
    label: question.title,
    description: question.spaceSlug,
    keywords: [question.title, question.spaceSlug ?? ""],
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
  const submitQuestion = useSubmitQuestion();
  const approveQuestion = useApproveQuestion();
  const rejectQuestion = useRejectQuestion();
  const escalateQuestion = useEscalateQuestion();
  const addTag = useAddQuestionTag(id ?? "");
  const removeTag = useRemoveQuestionTag(id ?? "");
  const addSource = useAddQuestionSource(id ?? "");
  const removeSource = useRemoveQuestionSource(id ?? "");
  const [selectedTagId, setSelectedTagId] = useState("");
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [tagSearch, setTagSearch] = useState("");
  const [sourceSearch, setSourceSearch] = useState("");
  const [duplicateSearch, setDuplicateSearch] = useState("");
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.SupportingContext),
  );
  const [selectedAnswerId, setSelectedAnswerId] = useState("");
  const [selectedDuplicateId, setSelectedDuplicateId] = useState("");
  const [newAnswerHeadline, setNewAnswerHeadline] = useState("");
  const [newAnswerBody, setNewAnswerBody] = useState("");
  const [relationshipTab, setRelationshipTab] = useState("answers");
  const deferredTagSearch = useDeferredValue(tagSearch.trim());
  const deferredSourceSearch = useDeferredValue(sourceSearch.trim());
  const deferredDuplicateSearch = useDeferredValue(duplicateSearch.trim());
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
  const relatedQuestionOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 20,
    sorting: "Title ASC",
    searchText: deferredDuplicateSearch || undefined,
    spaceId: questionQuery.data?.spaceId,
  });
  const activityListQuery = useActivityList({
    page: 1,
    pageSize: 100,
    sorting: "OccurredAtUtc DESC",
    questionId: id,
  });
  const selectedTagQuery = useTag(selectedTagId || undefined);
  const selectedSourceQuery = useSource(selectedSourceId || undefined);
  const selectedDuplicateQuestionQuery = useQuestion(
    selectedDuplicateId ||
      questionQuery.data?.duplicateOfQuestionId ||
      undefined,
  );

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

  const duplicateOptions = useMemo(
    () =>
      (relatedQuestionOptionsQuery.data?.items ?? []).filter(
        (question) => question.id !== id,
      ),
    [id, relatedQuestionOptionsQuery.data?.items],
  );

  const showLoadingState =
    !questionQuery.data &&
    (questionQuery.isLoading ||
      spaceQuery.isLoading ||
      sourceOptionsQuery.isLoading ||
      tagOptionsQuery.isLoading);

  const duplicateTarget =
    duplicateOptions.find(
      (question) => question.id === questionQuery.data?.duplicateOfQuestionId,
    ) ?? selectedDuplicateQuestionQuery.data;
  const selectedDuplicateQuestion =
    duplicateOptions.find((question) => question.id === selectedDuplicateId) ??
    selectedDuplicateQuestionQuery.data;
  const tagOptions = availableTags.map(buildTagOption);
  const sourceOptions = availableSources.map(buildSourceOption);
  const duplicateQuestionOptions = duplicateOptions.map(buildQuestionOption);
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
  const selectedDuplicateQuestionOption = selectedDuplicateQuestion
    ? buildQuestionOption(selectedDuplicateQuestion)
    : null;
  const acceptedAnswerOptions = (questionQuery.data?.answers ?? []).filter(
    (answer) =>
      answer.status === AnswerStatus.Published ||
      answer.status === AnswerStatus.Validated,
  );
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
  const questionActivity = (activityListQuery.data?.items ??
    questionQuery.data?.activity ??
    []) as ActivityDto[];
  const tagsPagination = useLocalPagination({
    items: questionQuery.data?.tags ?? [],
  });
  const sourcesPagination = useLocalPagination({
    items: questionQuery.data?.sources ?? [],
  });
  const answersPagination = useLocalPagination({
    items: questionQuery.data?.answers ?? [],
  });
  const activityPagination = useLocalPagination({ items: questionActivity });

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
  const canSubmit = questionQuery.data?.status === QuestionStatus.Draft;
  const canApprove =
    questionQuery.data?.status === QuestionStatus.PendingReview ||
    questionQuery.data?.status === QuestionStatus.Escalated;
  const canReject =
    questionQuery.data?.status !== QuestionStatus.Draft ||
    questionQuery.data?.visibility !== VisibilityScope.Internal;
  const canEscalate =
    questionQuery.data?.status !== QuestionStatus.Escalated &&
    questionQuery.data?.status !== QuestionStatus.Duplicate &&
    questionQuery.data?.status !== QuestionStatus.Archived;
  const workflowSummary =
    questionQuery.data?.status === QuestionStatus.Draft
      ? spaceQuery.data?.kind === SpaceKind.ControlledPublication ||
        spaceQuery.data?.kind === SpaceKind.ModeratedCollaboration
        ? translateText(
            "Submitting this question will move it into pending review.",
          )
        : translateText("Submitting this question will open it immediately.")
      : translateText(
          "Current status controls which workflow actions are available.",
        );

  return (
    <DetailLayout
      header={
        <PageHeader
          title={questionQuery.data?.title ?? "Question"}
          description="Operate thread workflow, accepted answers, duplicate routing, source links, and activity from one place."
          descriptionMode="hint"
          backTo={
            questionQuery.data?.spaceId
              ? `/app/spaces/${questionQuery.data.spaceId}`
              : "/app/spaces"
          }
        />
      }
      sidebar={
        <>
          <ActionPanel description="Thread actions and navigation.">
            {spaceBlocksAnswers ? (
              <ActionButton disabled>
                <Plus className="size-4" />
                {translateText("New answer")}
              </ActionButton>
            ) : (
              <ActionButton
                type="button"
                tone="primary"
                onClick={() => setRelationshipTab("answers")}
              >
                <Plus className="size-4" />
                {translateText("New answer")}
              </ActionButton>
            )}
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
                name:
                  questionQuery.data?.title ?? translateText("this question"),
              })}
              description={translateText(
                "This removes the thread, its accepted-answer state, and any public-signal aggregation from the portal view.",
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
                <ActionButton tone="danger" span="full">
                  <Trash2 className="size-4" />
                  {translateText("Delete")}
                </ActionButton>
              }
            />
            {spaceBlocksAnswers ? (
              <p className="col-span-2 text-xs text-muted-foreground">
                {translateText("This space does not accept new answers.")}
              </p>
            ) : null}
          </ActionPanel>
          {showLoadingState ? (
            <SidebarSummarySkeleton />
          ) : questionQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle>{translateText("Thread overview")}</CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: "Space",
                      value:
                        spaceQuery.data?.name ?? questionQuery.data.spaceSlug,
                    },
                    {
                      label: "Feedback score",
                      value: String(questionQuery.data.feedbackScore),
                    },
                    {
                      label: "Accepted answer",
                      value:
                        questionQuery.data.acceptedAnswer?.headline || "None",
                    },
                    {
                      label: "Last activity",
                      value: formatOptionalDateTimeInTimeZone(
                        questionQuery.data.lastActivityAtUtc,
                        portalTimeZone,
                        translateText("Not set"),
                      ),
                    },
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
          {showLoadingState ? null : questionQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Thread settings")}</span>
                    <ContextHint
                      content={translateText(
                        "These values describe the thread and the intake path that created it.",
                      )}
                      label={translateText("Thread settings details")}
                    />
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: "Origin channel",
                      value: (
                        <ChannelKindBadge
                          kind={questionQuery.data.originChannel}
                        />
                      ),
                    },
                    {
                      label: "AI confidence",
                      value: String(questionQuery.data.aiConfidenceScore),
                    },
                    {
                      label: "Sort",
                      value: String(questionQuery.data.sort),
                    },
                    {
                      label: "Duplicate target",
                      value: duplicateTarget?.title || "No duplicate target",
                    },
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
          {showLoadingState ? null : questionQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle>{translateText("Context and timing")}</CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: "Answered at",
                      value: formatOptionalDateTimeInTimeZone(
                        questionQuery.data.answeredAtUtc,
                        portalTimeZone,
                        translateText("Not set"),
                      ),
                    },
                    {
                      label: "Validated at",
                      value: formatOptionalDateTimeInTimeZone(
                        questionQuery.data.validatedAtUtc,
                        portalTimeZone,
                        translateText("Not set"),
                      ),
                    },
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
        </>
      }
    >
      {questionQuery.isError ? (
        <ErrorState
          title="Unable to load question"
          error={questionQuery.error}
          retry={() => void questionQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={6} />
      ) : questionQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Status",
                value: (
                  <QuestionStatusBadge status={questionQuery.data.status} />
                ),
                icon: MessageSquareText,
              },
              {
                title: "Visibility",
                value: (
                  <VisibilityBadge visibility={questionQuery.data.visibility} />
                ),
                icon: MessageSquareText,
              },
              {
                title: "Signals",
                value: translateText("Feedback {value}", {
                  value: questionQuery.data.feedbackScore,
                }),
                description: translateText(
                  "Public feedback is aggregated into activity and score",
                ),
                icon: Activity,
              },
              {
                title: "Answers",
                value: questionQuery.data.answers.length,
                description: translateText(
                  "Accepted, draft, and review candidates",
                ),
                icon: CheckCircle2,
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Workflow actions")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">{workflowSummary}</p>
              <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
                <ActionButton
                  tone="primary"
                  onClick={() => void submitQuestion.mutateAsync(id)}
                  disabled={!canSubmit || submitQuestion.isPending}
                >
                  {translateText("Submit for review")}
                </ActionButton>
                <ActionButton
                  tone="primary"
                  onClick={() => void approveQuestion.mutateAsync(id)}
                  disabled={!canApprove || approveQuestion.isPending}
                >
                  {translateText("Approve")}
                </ActionButton>
                <ActionButton
                  tone="danger"
                  onClick={() => void rejectQuestion.mutateAsync({ id })}
                  disabled={!canReject || rejectQuestion.isPending}
                >
                  {translateText("Reject")}
                </ActionButton>
                <ActionButton
                  tone="danger"
                  onClick={() => void escalateQuestion.mutateAsync({ id })}
                  disabled={!canEscalate || escalateQuestion.isPending}
                >
                  <TriangleAlert className="size-4" />
                  {translateText("Escalate")}
                </ActionButton>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Summary and context")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="rounded-lg border border-border bg-muted/10 p-4">
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Summary")}
                </p>
                <p className="mt-2 text-sm leading-6">
                  {questionQuery.data.summary ||
                    translateText("No summary provided.")}
                </p>
              </div>
              <div className="rounded-lg border border-border bg-muted/10 p-4">
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Context note")}
                </p>
                <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                  {questionQuery.data.contextNote ||
                    translateText("No context note recorded.")}
                </p>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>
                  {translateText("Accepted answer and duplicate routing")}
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-3">
                  <p className="text-sm text-muted-foreground">
                    {translateText(
                      "Choose the accepted answer that should anchor the canonical resolution for this thread.",
                    )}
                  </p>
                  <SearchSelect
                    value={acceptedAnswerSelectValue}
                    onValueChange={setSelectedAnswerId}
                    options={acceptedAnswerOptions.map(buildAnswerOption)}
                    selectedOption={selectedAcceptedAnswerOption}
                    placeholder={translateText("Select accepted answer")}
                    searchPlaceholder={translateText("Search answers...")}
                    emptyMessage={translateText(
                      "No published or validated answers found.",
                    )}
                    resultCountHint={translateText(
                      "Only published and validated answers can become accepted.",
                    )}
                  />
                  {acceptedAnswerOptions.length ? (
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
                  ) : (
                    <p className="text-sm text-muted-foreground">
                      {translateText(
                        "Publish or validate an answer before it can be accepted.",
                      )}
                    </p>
                  )}
                  {questionQuery.data.acceptedAnswerId ? (
                    <Button
                      variant="outline"
                      disabled={updateQuestion.isPending}
                      onClick={() =>
                        updateQuestion.mutateAsync(
                          buildQuestionUpdateBody(questionQuery.data, {
                            acceptedAnswerId: null,
                            status: getQuestionStatusAfterClearingResolution({
                              ...questionQuery.data,
                              acceptedAnswerId: null,
                            }),
                          }),
                        )
                      }
                    >
                      {translateText("Clear accepted answer")}
                    </Button>
                  ) : null}
                </div>
                <div className="space-y-3">
                  <p className="text-sm text-muted-foreground">
                    {translateText(
                      "Mark this thread as a duplicate when another canonical question should own the resolution.",
                    )}
                  </p>
                  <SearchSelect
                    value={selectedDuplicateId}
                    onValueChange={setSelectedDuplicateId}
                    options={duplicateQuestionOptions}
                    selectedOption={selectedDuplicateQuestionOption}
                    placeholder={translateText("Select duplicate target")}
                    searchPlaceholder={translateText("Search questions")}
                    emptyMessage={
                      deferredDuplicateSearch
                        ? translateText("No questions match this search.")
                        : translateText("No related questions available.")
                    }
                    loading={relatedQuestionOptionsQuery.isFetching}
                    searchValue={duplicateSearch}
                    onSearchChange={(value) =>
                      startTransition(() => setDuplicateSearch(value))
                    }
                    allowClear
                  />
                  <Button
                    disabled={!selectedDuplicateId || updateQuestion.isPending}
                    onClick={() =>
                      updateQuestion
                        .mutateAsync(
                          buildQuestionUpdateBody(questionQuery.data, {
                            duplicateOfQuestionId: selectedDuplicateId,
                          }),
                        )
                        .then(() => setSelectedDuplicateId(""))
                    }
                  >
                    <GitFork className="size-4" />
                    {translateText("Set duplicate target")}
                  </Button>
                  {questionQuery.data.duplicateOfQuestionId ? (
                    <Button
                      variant="outline"
                      disabled={updateQuestion.isPending}
                      onClick={() =>
                        updateQuestion.mutateAsync(
                          buildQuestionUpdateBody(questionQuery.data, {
                            duplicateOfQuestionId: null,
                            status: getQuestionStatusAfterClearingResolution({
                              ...questionQuery.data,
                              duplicateOfQuestionId: null,
                            }),
                          }),
                        )
                      }
                    >
                      {translateText("Clear duplicate target")}
                    </Button>
                  ) : null}
                </div>
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
                  "Write, publish, validate, or retire answer candidates for this thread.",
                icon: CheckCircle2,
                count: questionQuery.data?.answers.length ?? 0,
              },
              {
                key: "sources",
                label: "Sources",
                description:
                  "Attach evidence so the accepted answer remains defensible.",
                icon: Link2,
                count: questionQuery.data?.sources.length ?? 0,
              },
              {
                key: "tags",
                label: "Tags",
                description:
                  "Attach taxonomy that helps operators find related threads.",
                icon: Tags,
                count: questionQuery.data?.tags.length ?? 0,
              },
              {
                key: "activity",
                label: "Activity",
                description:
                  "Review moderation, reports, votes, feedback, and workflow history.",
                icon: Activity,
                count:
                  activityListQuery.data?.totalCount ??
                  questionQuery.data?.activity.length ??
                  0,
              },
            ]}
          />

          {relationshipTab === "tags" ? (
            <Card>
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
                    description="Attach tags so operators can group and find related threads faster."
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
            <Card>
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
                                {sourceLink.source.isAuthoritative ? (
                                  <Badge variant="primary">
                                    {translateText("Authoritative")}
                                  </Badge>
                                ) : null}
                                {sourceLink.source.allowsPublicCitation ? (
                                  <Badge variant="success" appearance="outline">
                                    {translateText("Public citation")}
                                  </Badge>
                                ) : null}
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
                    description="Attach origin, evidence, or citation material to strengthen the thread."
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
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Answers")}</span>
                    <Badge variant="outline">
                      {translateText("{count} answers", {
                        count: questionQuery.data.answers.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {!spaceBlocksAnswers ? (
                  <form
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
                          score: 1,
                          sort: questionQuery.data.answers.length + 1,
                        })
                        .then(() => {
                          setNewAnswerHeadline("");
                          setNewAnswerBody("");
                        });
                    }}
                  >
                    <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_auto]">
                      <div className="space-y-3">
                        <div>
                          <p className="text-sm font-semibold text-mono">
                            {translateText("Draft answer in this thread")}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {translateText(
                              "Write the answer here, then publish or validate it from this same thread.",
                            )}
                          </p>
                        </div>
                        <Input
                          value={newAnswerHeadline}
                          onChange={(event) =>
                            setNewAnswerHeadline(event.target.value)
                          }
                          placeholder={translateText("Answer headline")}
                          aria-label={translateText("Answer headline")}
                        />
                        <Textarea
                          value={newAnswerBody}
                          onChange={(event) =>
                            setNewAnswerBody(event.target.value)
                          }
                          placeholder={translateText("Answer body")}
                          aria-label={translateText("Answer body")}
                          rows={5}
                        />
                      </div>
                      <div className="flex items-end">
                        <Button
                          type="submit"
                          disabled={
                            !newAnswerHeadline.trim() || createAnswer.isPending
                          }
                        >
                          <Plus className="size-4" />
                          {translateText("Save draft")}
                        </Button>
                      </div>
                    </div>
                  </form>
                ) : null}
                {questionQuery.data.answers.length ? (
                  answersPagination.pagedItems.map((answer) => (
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
                        : "Create the first answer candidate for this thread."
                    }
                  />
                )}
                <ChildListPagination
                  page={answersPagination.page}
                  pageSize={answersPagination.pageSize}
                  totalCount={answersPagination.totalCount}
                  onPageChange={answersPagination.setPage}
                  onPageSizeChange={answersPagination.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "activity" ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Activity")}</span>
                    <Badge variant="outline">
                      {translateText("{count} events", {
                        count: questionActivity.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-3">
                {questionActivity.length ? (
                  activityPagination.pagedItems.map((event) => (
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
                      <Button asChild variant="outline" size="sm">
                        <Link to={`/app/activity/${event.id}`}>
                          <Activity className="size-4" />
                          {translateText("Open event")}
                        </Link>
                      </Button>
                    </div>
                  ))
                ) : (
                  <EmptyState
                    title="No activity yet"
                    description="Workflow events, public signals, and moderation actions will appear here."
                  />
                )}
                <ChildListPagination
                  page={activityPagination.page}
                  pageSize={activityPagination.pageSize}
                  totalCount={activityPagination.totalCount}
                  isFetching={activityListQuery.isFetching}
                  onPageChange={activityPagination.setPage}
                  onPageSizeChange={activityPagination.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}
        </>
      ) : null}
    </DetailLayout>
  );
}
