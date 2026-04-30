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
  sourceRoleLabels,
} from "@/shared/constants/backend-enums";
import {
  DetailFieldList,
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
  const answerId = id ?? "";
  const answerQuery = useAnswer(id);
  const questionQuery = useQuestion(answerQuery.data?.questionId);
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 100,
    sorting: "OccurredAtUtc DESC",
    answerId: id,
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
        label: "Back to answers",
        to: "/app/answers",
        text: "Return to the answer queue while this answer loads.",
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
            <Card id="answer-sources-section">
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
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
        </>
      }
    >
      {ActivationVisibilityDialog}
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
                  <span>{translateText("Lifecycle actions")}</span>
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
              {lifecycleActionOptions.length ? (
                <div className="flex flex-wrap gap-2">
                  {lifecycleActionOptions.map((option) => (
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
                </div>
              ) : null}
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
