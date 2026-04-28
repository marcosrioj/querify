import { useMemo, useState } from "react";
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
import { useDeleteAnswer } from "@/domains/answers/hooks";
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
import { useSourceList } from "@/domains/sources/hooks";
import { useSpace } from "@/domains/spaces/hooks";
import { useTagList } from "@/domains/tags/hooks";
import {
  AnswerStatus,
  QuestionStatus,
  SourceRole,
  SpaceKind,
  VisibilityScope,
  sourceRoleLabels,
} from "@/shared/constants/backend-enums";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ConfirmAction,
  ContextHint,
  DetailPageSkeleton,
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
  AnswerStatusBadge,
  ChannelKindBadge,
  QuestionStatusBadge,
  SourceRoleBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
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

export function QuestionDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const questionQuery = useQuestion(id);
  const spaceQuery = useSpace(questionQuery.data?.spaceId);
  const sourceOptionsQuery = useSourceList({
    page: 1,
    pageSize: 100,
    sorting: "Label ASC",
  });
  const tagOptionsQuery = useTagList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });
  const relatedQuestionOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 100,
    sorting: "Title ASC",
    spaceId: questionQuery.data?.spaceId,
  });
  const activityListQuery = useActivityList({
    page: 1,
    pageSize: 20,
    sorting: "OccurredAtUtc DESC",
    questionId: id,
  });
  const deleteQuestion = useDeleteQuestion();
  const updateQuestion = useUpdateQuestion(id ?? "");
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
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.SupportingContext),
  );
  const [selectedAnswerId, setSelectedAnswerId] = useState("");
  const [selectedDuplicateId, setSelectedDuplicateId] = useState("");

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

  if (!id) {
    return (
      <ErrorState
        title="Invalid question route"
        description="Question detail routes need an identifier."
      />
    );
  }

  const showLoadingState =
    !questionQuery.data &&
    (questionQuery.isLoading ||
      spaceQuery.isLoading ||
      sourceOptionsQuery.isLoading ||
      tagOptionsQuery.isLoading);

  const duplicateTarget = duplicateOptions.find(
    (question) => question.id === questionQuery.data?.duplicateOfQuestionId,
  );
  const acceptedAnswerOptions = (questionQuery.data?.answers ?? []).filter(
    (answer) =>
      answer.status === AnswerStatus.Published ||
      answer.status === AnswerStatus.Validated,
  );
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
        <>
          <PageHeader
            title={questionQuery.data?.title ?? "Question"}
            description="Operate thread workflow, accepted answers, duplicate routing, source links, and activity from one place."
            descriptionMode="hint"
            backTo="/app/questions"
          />
          <QnaModuleNav
            activeKey="questions"
            intent="This question sits under a space. Keep accepted answers, duplicate routing, sources, tags, and activity tied to that parent."
          />
        </>
      }
      sidebar={
        <>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-3">
              {spaceBlocksAnswers ? (
                <Button size="sm" className="w-full justify-start" disabled>
                  <Plus className="size-4" />
                  {translateText("New answer")}
                </Button>
              ) : (
                <Button asChild size="sm" className="w-full justify-start">
                  <Link to={`/app/answers/new?questionId=${id}`}>
                    <Plus className="size-4" />
                    {translateText("New answer")}
                  </Link>
                </Button>
              )}
              <Button
                asChild
                variant="outline"
                size="sm"
                className="w-full justify-start"
              >
                <Link to={`/app/questions/${id}/edit`}>
                  <Pencil className="size-4" />
                  {translateText("Edit")}
                </Link>
              </Button>
              {questionQuery.data?.spaceId ? (
                <Button
                  asChild
                  variant="outline"
                  size="sm"
                  className="w-full justify-start"
                >
                  <Link to={`/app/spaces/${questionQuery.data.spaceId}`}>
                    <Link2 className="size-4" />
                    {translateText("Open space")}
                  </Link>
                </Button>
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
                    .then(() => navigate("/app/questions"))
                }
                trigger={
                  <Button
                    variant="destructive"
                    size="sm"
                    className="col-span-2 w-full justify-start"
                  >
                    <Trash2 className="size-4" />
                    {translateText("Delete")}
                  </Button>
                }
              />
              {spaceBlocksAnswers ? (
                <p className="col-span-2 text-xs text-muted-foreground">
                  {translateText("This space does not accept new answers.")}
                </p>
              ) : null}
            </CardContent>
          </Card>
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
                        spaceQuery.data?.name ?? questionQuery.data.spaceKey,
                    },
                    {
                      label: "Feedback score",
                      value: String(questionQuery.data.feedbackScore),
                    },
                    {
                      label: "AI confidence",
                      value: String(questionQuery.data.aiConfidenceScore),
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

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Workflow actions")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">{workflowSummary}</p>
              <div className="flex flex-wrap gap-3">
                <Button
                  variant="outline"
                  onClick={() => void submitQuestion.mutateAsync(id)}
                  disabled={!canSubmit || submitQuestion.isPending}
                >
                  {translateText("Submit for review")}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => void approveQuestion.mutateAsync(id)}
                  disabled={!canApprove || approveQuestion.isPending}
                >
                  {translateText("Approve")}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => void rejectQuestion.mutateAsync({ id })}
                  disabled={!canReject || rejectQuestion.isPending}
                >
                  {translateText("Reject")}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => void escalateQuestion.mutateAsync({ id })}
                  disabled={!canEscalate || escalateQuestion.isPending}
                >
                  <TriangleAlert className="size-4" />
                  {translateText("Escalate")}
                </Button>
              </div>
            </CardContent>
          </Card>

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
                  <Select
                    value={selectedAnswerId}
                    onValueChange={setSelectedAnswerId}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue
                        placeholder={translateText("Select accepted answer")}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {acceptedAnswerOptions.map((answer) => (
                        <SelectItem key={answer.id} value={answer.id}>
                          {answer.headline}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {acceptedAnswerOptions.length ? (
                    <Button
                      disabled={!selectedAnswerId || updateQuestion.isPending}
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
                  <Select
                    value={selectedDuplicateId}
                    onValueChange={setSelectedDuplicateId}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue
                        placeholder={translateText("Select duplicate target")}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {duplicateOptions.map((question) => (
                        <SelectItem key={question.id} value={question.id}>
                          {question.title}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
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
                  {questionQuery.data.tags.map((tag) => (
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
              <div className="flex flex-col gap-3 sm:flex-row">
                <Select value={selectedTagId} onValueChange={setSelectedTagId}>
                  <SelectTrigger className="w-full">
                    <SelectValue
                      placeholder={translateText("Attach existing tag")}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {availableTags.map((tag) => (
                      <SelectItem key={tag.id} value={tag.id}>
                        {tag.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
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
                  {questionQuery.data.sources.map((sourceLink) => (
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
              <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_220px_160px]">
                <Select
                  value={selectedSourceId}
                  onValueChange={setSelectedSourceId}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue
                      placeholder={translateText("Attach existing source")}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {availableSources.map((source) => (
                      <SelectItem key={source.id} value={source.id}>
                        {source.label || source.locator}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Select
                  value={selectedSourceRole}
                  onValueChange={setSelectedSourceRole}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder={translateText("Source role")} />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(sourceRoleLabels).map(([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {translateText(label)}
                      </SelectItem>
                    ))}
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
            <CardContent className="space-y-3">
              {questionQuery.data.answers.length ? (
                questionQuery.data.answers.map((answer) => (
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
                  action={
                    spaceBlocksAnswers
                      ? undefined
                      : {
                          label: "New answer",
                          to: `/app/answers/new?questionId=${id}`,
                        }
                  }
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex items-center gap-2">
                  <span>{translateText("Activity")}</span>
                  <Badge variant="outline">
                    {translateText("{count} events", {
                      count: (
                        activityListQuery.data?.items ??
                        questionQuery.data.activity
                      ).length,
                    })}
                  </Badge>
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {(
                (activityListQuery.data?.items ??
                  questionQuery.data.activity) as ActivityDto[]
              ).length ? (
                (
                  (activityListQuery.data?.items ??
                    questionQuery.data.activity) as ActivityDto[]
                ).map((event) => (
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
            </CardContent>
          </Card>
        </>
      ) : null}
    </DetailLayout>
  );
}
