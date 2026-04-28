import { useMemo, useState } from "react";
import { CheckCircle2, Link2, Pencil, ShieldCheck, Trash2 } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  useAnswer,
  useAddAnswerSource,
  useDeleteAnswer,
  usePublishAnswer,
  useRejectAnswer,
  useRemoveAnswerSource,
  useRetireAnswer,
  useValidateAnswer,
} from "@/domains/answers/hooks";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { useQuestion } from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useSourceList } from "@/domains/sources/hooks";
import {
  AnswerStatus,
  SourceRole,
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
  AnswerKindBadge,
  AnswerStatusBadge,
  SourceRoleBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";

export function AnswerDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const answerQuery = useAnswer(id);
  const questionQuery = useQuestion(answerQuery.data?.questionId);
  const sourceOptionsQuery = useSourceList({
    page: 1,
    pageSize: 100,
    sorting: "Label ASC",
  });
  const deleteAnswer = useDeleteAnswer();
  const publishAnswer = usePublishAnswer();
  const validateAnswer = useValidateAnswer();
  const rejectAnswer = useRejectAnswer();
  const retireAnswer = useRetireAnswer();
  const addSource = useAddAnswerSource(id ?? "");
  const removeSource = useRemoveAnswerSource(id ?? "");
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.Evidence),
  );

  const availableSources = useMemo(() => {
    const existing = new Set(
      (answerQuery.data?.sources ?? []).map((link) => link.sourceId),
    );
    return (sourceOptionsQuery.data?.items ?? []).filter(
      (source) => !existing.has(source.id),
    );
  }, [answerQuery.data?.sources, sourceOptionsQuery.data?.items]);

  if (!id) {
    return (
      <ErrorState
        title="Invalid answer route"
        description="Answer detail routes need an identifier."
      />
    );
  }

  const showLoadingState =
    !answerQuery.data &&
    (answerQuery.isLoading ||
      questionQuery.isLoading ||
      sourceOptionsQuery.isLoading);
  const canPublish =
    answerQuery.data?.status === AnswerStatus.Draft ||
    answerQuery.data?.status === AnswerStatus.PendingReview ||
    answerQuery.data?.status === AnswerStatus.Rejected;
  const canValidate = answerQuery.data?.status === AnswerStatus.Published;
  const canReject = answerQuery.data?.status !== AnswerStatus.Rejected;
  const canRetire =
    answerQuery.data?.status === AnswerStatus.Published ||
    answerQuery.data?.status === AnswerStatus.Validated ||
    answerQuery.data?.status === AnswerStatus.Rejected;
  const lifecycleSummary =
    answerQuery.data?.status === AnswerStatus.Draft ||
    answerQuery.data?.status === AnswerStatus.PendingReview
      ? translateText(
          "Publish the answer before exposing it publicly or accepting it.",
        )
      : translateText(
          "Current status controls which lifecycle actions are available.",
        );

  return (
    <DetailLayout
      header={
        <>
          <PageHeader
            title={answerQuery.data?.headline ?? "Answer"}
            description="Manage publication, validation, evidence links, and retirement for this answer candidate."
            descriptionMode="hint"
            backTo={
              answerQuery.data?.questionId
                ? `/app/questions/${answerQuery.data.questionId}`
                : "/app/spaces"
            }
          />
          <QnaModuleNav
            activeKey="spaces"
            intent="This answer belongs to a Question. Publish, validate, or retire it only after checking the parent thread and evidence links."
          />
        </>
      }
      sidebar={
        <>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-3">
              <Button asChild size="sm" className="w-full justify-start">
                <Link to={`/app/answers/${id}/edit`}>
                  <Pencil className="size-4" />
                  {translateText("Edit")}
                </Link>
              </Button>
              {answerQuery.data?.questionId ? (
                <Button
                  asChild
                  variant="outline"
                  size="sm"
                  className="w-full justify-start"
                >
                  <Link to={`/app/questions/${answerQuery.data.questionId}`}>
                    <Link2 className="size-4" />
                    {translateText("Open question")}
                  </Link>
                </Button>
              ) : null}
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
            </CardContent>
          </Card>
          {showLoadingState ? (
            <SidebarSummarySkeleton />
          ) : answerQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle>{translateText("Overview")}</CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: "Question",
                      value:
                        questionQuery.data?.title ||
                        answerQuery.data.questionId,
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
                    {
                      label: "Published at",
                      value: formatOptionalDateTimeInTimeZone(
                        answerQuery.data.publishedAtUtc,
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
      {answerQuery.isError ? (
        <ErrorState
          title="Unable to load answer"
          error={answerQuery.error}
          retry={() => void answerQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={5} />
      ) : answerQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Status",
                value: <AnswerStatusBadge status={answerQuery.data.status} />,
                icon: CheckCircle2,
              },
              {
                title: "Visibility",
                value: (
                  <VisibilityBadge visibility={answerQuery.data.visibility} />
                ),
                icon: CheckCircle2,
              },
              {
                title: "Type",
                value: <AnswerKindBadge kind={answerQuery.data.kind} />,
                icon: ShieldCheck,
              },
              {
                title: "Signals",
                value: translateText("Votes {value}", {
                  value: answerQuery.data.voteScore,
                }),
                description: answerQuery.data.isAccepted
                  ? translateText("This answer is currently accepted")
                  : translateText("Not currently accepted"),
                icon: ShieldCheck,
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex items-center gap-2">
                  <span>{translateText("Lifecycle actions")}</span>
                  <ContextHint
                    content={translateText(
                      "Publish, validate, reject, or retire the answer as product truth evolves.",
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
              <div className="flex flex-wrap gap-3">
                <Button
                  variant="outline"
                  onClick={() => void publishAnswer.mutateAsync(id)}
                  disabled={!canPublish || publishAnswer.isPending}
                >
                  {translateText("Publish")}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => void validateAnswer.mutateAsync(id)}
                  disabled={!canValidate || validateAnswer.isPending}
                >
                  {translateText("Validate")}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => void rejectAnswer.mutateAsync(id)}
                  disabled={!canReject || rejectAnswer.isPending}
                >
                  {translateText("Reject")}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => void retireAnswer.mutateAsync(id)}
                  disabled={!canRetire || retireAnswer.isPending}
                >
                  {translateText("Retire")}
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
                    label: "Author label",
                    value: answerQuery.data.authorLabel || "Not set",
                  },
                  {
                    label: "Score",
                    value: String(answerQuery.data.score),
                  },
                  {
                    label: "Published at",
                    value: formatOptionalDateTimeInTimeZone(
                      answerQuery.data.publishedAtUtc,
                      portalTimeZone,
                      translateText("Not set"),
                    ),
                  },
                  {
                    label: "Validated at",
                    value: formatOptionalDateTimeInTimeZone(
                      answerQuery.data.validatedAtUtc,
                      portalTimeZone,
                      translateText("Not set"),
                    ),
                  },
                  {
                    label: "Retired at",
                    value: formatOptionalDateTimeInTimeZone(
                      answerQuery.data.retiredAtUtc,
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
                <CardTitle>{translateText("Body and trust")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-5">
              {answerQuery.data.body ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    {translateText("Body")}
                  </p>
                  <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                    {answerQuery.data.body}
                  </p>
                </div>
              ) : (
                <EmptyState
                  title="No body yet"
                  description="This answer currently only has the headline-level guidance."
                />
              )}
              <div className="grid gap-4 md:grid-cols-2">
                <div className="rounded-lg border border-border bg-muted/10 p-4">
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    {translateText("Context note")}
                  </p>
                  <p className="mt-2 text-sm leading-6">
                    {answerQuery.data.contextNote ||
                      translateText("No context note recorded.")}
                  </p>
                </div>
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
                      count: answerQuery.data.sources.length,
                    })}
                  </Badge>
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              {answerQuery.data.sources.length ? (
                <div className="space-y-3">
                  {answerQuery.data.sources.map((sourceLink) => (
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
                              {sourceLink.source.allowsPublicExcerpt ? (
                                <Badge variant="outline">
                                  {translateText("Public excerpt")}
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
                  description="Attach evidence, citations, or canonical references for this answer."
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
        </>
      ) : null}
    </DetailLayout>
  );
}
