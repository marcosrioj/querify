import { startTransition, useDeferredValue, useMemo, useState } from "react";
import {
  Activity,
  CheckCircle2,
  Link2,
  Pencil,
  ShieldCheck,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useActivityList } from "@/domains/activity/hooks";
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
import { useSource, useSourceList } from "@/domains/sources/hooks";
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
  const answerQuery = useAnswer(id);
  const questionQuery = useQuestion(answerQuery.data?.questionId);
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 100,
    sorting: "OccurredAtUtc DESC",
    answerId: id,
  });
  const deleteAnswer = useDeleteAnswer();
  const publishAnswer = usePublishAnswer();
  const validateAnswer = useValidateAnswer();
  const rejectAnswer = useRejectAnswer();
  const retireAnswer = useRetireAnswer();
  const addSource = useAddAnswerSource(id ?? "");
  const removeSource = useRemoveAnswerSource(id ?? "");
  const [selectedSourceId, setSelectedSourceId] = useState("");
  const [sourceSearch, setSourceSearch] = useState("");
  const [selectedSourceRole, setSelectedSourceRole] = useState(
    String(SourceRole.Evidence),
  );
  const [relationshipTab, setRelationshipTab] = useState("sources");
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
  const activityPagination = useLocalPagination({ items: answerActivity });

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
          description="Manage publication, validation, evidence links, and retirement for this answer candidate."
          descriptionMode="hint"
          backTo={
            answerQuery.data?.questionId
              ? `/app/questions/${answerQuery.data.questionId}`
              : "/app/spaces"
          }
        />
      }
      sidebar={
        <>
          <ActionPanel description="Answer actions and parent navigation.">
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
                <ActionButton tone="danger" span="full">
                  <Trash2 className="size-4" />
                  {translateText("Delete")}
                </ActionButton>
              }
            />
          </ActionPanel>
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
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
          {showLoadingState ? null : answerQuery.data ? (
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
              <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
                <ActionButton
                  tone="primary"
                  onClick={() => void publishAnswer.mutateAsync(id)}
                  disabled={!canPublish || publishAnswer.isPending}
                >
                  {translateText("Publish")}
                </ActionButton>
                <ActionButton
                  tone="primary"
                  onClick={() => void validateAnswer.mutateAsync(id)}
                  disabled={!canValidate || validateAnswer.isPending}
                >
                  {translateText("Validate")}
                </ActionButton>
                <ActionButton
                  tone="danger"
                  onClick={() => void rejectAnswer.mutateAsync(id)}
                  disabled={!canReject || rejectAnswer.isPending}
                >
                  {translateText("Reject")}
                </ActionButton>
                <ActionButton
                  tone="danger"
                  onClick={() => void retireAnswer.mutateAsync(id)}
                  disabled={!canRetire || retireAnswer.isPending}
                >
                  {translateText("Retire")}
                </ActionButton>
              </div>
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

          <QnaModuleNav
            eyebrow="Answer relationships"
            activeKey={relationshipTab}
            onActiveKeyChange={setRelationshipTab}
            items={[
              {
                key: "sources",
                label: "Sources",
                description:
                  "Attach evidence before validating this answer as trusted knowledge.",
                icon: ShieldCheck,
                count: answerQuery.data?.sources.length ?? 0,
              },
              {
                key: "activity",
                label: "Activity",
                description:
                  "Review events scoped to this answer when validation or retirement needs context.",
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
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Activity")}</span>
                    <Badge variant="outline">
                      {translateText("{count} events", {
                        count: answerActivity.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-3">
                {activityQuery.isError ? (
                  <ErrorState
                    title="Unable to load answer activity"
                    error={activityQuery.error}
                    retry={() => void activityQuery.refetch()}
                  />
                ) : answerActivity.length ? (
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
                      <span className="text-sm text-muted-foreground">
                        {formatOptionalDateTimeInTimeZone(
                          event.occurredAtUtc,
                          portalTimeZone,
                          translateText("Not set"),
                        )}
                      </span>
                    </div>
                  ))
                ) : (
                  <EmptyState
                    title="No answer activity yet"
                    description="Publication, validation, rejection, retirement, and source changes will appear here."
                  />
                )}
                <ChildListPagination
                  page={activityPagination.page}
                  pageSize={activityPagination.pageSize}
                  totalCount={activityPagination.totalCount}
                  isFetching={activityQuery.isFetching}
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
