import { useMemo, useState } from "react";
import {
  Activity,
  BookOpen,
  CheckCircle2,
  Link2,
  Pencil,
  Plus,
  Trash2,
  Waypoints,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useActivityList } from "@/domains/activity/hooks";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { useQuestionList } from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useSourceList } from "@/domains/sources/hooks";
import {
  useSpace,
  useAddSpaceSource,
  useAddSpaceTag,
  useDeleteSpace,
  useRemoveSpaceSource,
  useRemoveSpaceTag,
} from "@/domains/spaces/hooks";
import { useTagList } from "@/domains/tags/hooks";
import { QuestionStatus, SpaceKind } from "@/shared/constants/backend-enums";
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
  QuestionStatusBadge,
  SpaceKindBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";

export function SpaceDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const spaceQuery = useSpace(id);
  const questionQuery = useQuestionList({
    page: 1,
    pageSize: 8,
    sorting: "LastActivityAtUtc DESC",
    spaceId: id,
  });
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 20,
    sorting: "OccurredAtUtc DESC",
  });
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
  const deleteSpace = useDeleteSpace();
  const addTag = useAddSpaceTag(id ?? "");
  const removeTag = useRemoveSpaceTag(id ?? "");
  const addSource = useAddSpaceSource(id ?? "");
  const removeSource = useRemoveSpaceSource(id ?? "");
  const [selectedTagId, setSelectedTagId] = useState("");
  const [selectedSourceId, setSelectedSourceId] = useState("");

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

  if (!id) {
    return (
      <ErrorState
        title="Invalid space route"
        description="Space detail routes need an identifier."
      />
    );
  }

  const showLoadingState =
    !spaceQuery.data &&
    (spaceQuery.isLoading ||
      questionQuery.isLoading ||
      sourceOptionsQuery.isLoading);
  const blocksQuestions = spaceQuery.data
    ? !spaceQuery.data.acceptsQuestions
    : false;
  const blocksAnswers = spaceQuery.data ? !spaceQuery.data.acceptsAnswers : false;
  const reviewGated = spaceQuery.data
    ? spaceQuery.data.kind === SpaceKind.ControlledPublication ||
      spaceQuery.data.kind === SpaceKind.ModeratedCollaboration
    : false;
  const spaceQuestions = questionQuery.data?.items ?? [];
  const questionsNeedingAction = spaceQuestions.filter(
    (question) =>
      question.status === QuestionStatus.Draft ||
      question.status === QuestionStatus.PendingReview ||
      question.status === QuestionStatus.Escalated ||
      (!question.acceptedAnswerId &&
        question.status !== QuestionStatus.Duplicate &&
        question.status !== QuestionStatus.Archived),
  );
  const visibleQuestionIds = new Set(spaceQuestions.map((question) => question.id));
  const contextualActivity = (activityQuery.data?.items ?? []).filter((entry) =>
    visibleQuestionIds.has(entry.questionId),
  );
  const nextAction = blocksQuestions
    ? {
        label: "Review intake rules",
        to: `/app/spaces/${id}/edit`,
        text: "Question intake is closed. Reopen it or keep routing work to another Space.",
      }
    : questionsNeedingAction.length > 0
      ? {
          label: "Resolve first thread",
          to: `/app/questions/${questionsNeedingAction[0].id}`,
          text: "A question needs moderation, an accepted answer, duplicate routing, or escalation review.",
        }
      : (spaceQuery.data?.curatedSources.length ?? 0) === 0
        ? {
            label: "Attach source",
            to: "/app/sources",
            text: "This Space has no curated evidence yet. Attach a reusable Source before scaling answers.",
          }
        : {
            label: "Create question",
            to: `/app/questions/new?spaceId=${id}`,
            text: "The Space is ready for the next operational thread.",
          };

  return (
    <DetailLayout
      header={
        <>
          <PageHeader
            title={spaceQuery.data?.name ?? "Space"}
            description="Review state, intake rules, questions needing action, connected tags and sources, and the next recommended move."
            descriptionMode="hint"
            backTo="/app/spaces"
          />
          <QnaModuleNav
            activeKey="spaces"
            intent="This space is the parent context. Review its rules before creating child questions, curation links, or taxonomy."
          />
        </>
      }
      sidebar={
        <>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-3">
              {blocksQuestions ? (
                <Button size="sm" className="w-full justify-start" disabled>
                  <Plus className="size-4" />
                  {translateText("New question")}
                </Button>
              ) : (
                <Button asChild size="sm" className="w-full justify-start">
                  <Link to={`/app/questions/new?spaceId=${id}`}>
                    <Plus className="size-4" />
                    {translateText("New question")}
                  </Link>
                </Button>
              )}
              <Button
                asChild
                variant="outline"
                size="sm"
                className="w-full justify-start"
              >
                <Link to={`/app/spaces/${id}/edit`}>
                  <Pencil className="size-4" />
                  {translateText("Edit")}
                </Link>
              </Button>
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
                  deleteSpace
                    .mutateAsync(id)
                    .then(() => navigate("/app/spaces"))
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
              {blocksQuestions ? (
                <p className="col-span-2 text-xs text-muted-foreground">
                  {translateText("This space does not accept new questions.")}
                </p>
              ) : null}
            </CardContent>
          </Card>
          {showLoadingState ? (
            <SidebarSummarySkeleton />
          ) : spaceQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Overview")}</span>
                    <ContextHint
                      content={translateText(
                        "This summarizes the operating model and the major workflow gates.",
                      )}
                      label={translateText("Overview details")}
                    />
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    { label: "Key", value: spaceQuery.data.key },
                    { label: "Language", value: spaceQuery.data.language },
                    {
                      label: "Questions",
                      value: String(spaceQuery.data.questionCount),
                    },
                    {
                      label: "Curated sources",
                      value: String(spaceQuery.data.curatedSources.length),
                    },
                    {
                      label: "Published at",
                      value: formatOptionalDateTimeInTimeZone(
                        spaceQuery.data.publishedAtUtc,
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
      {spaceQuery.isError ? (
        <ErrorState
          title="Unable to load space"
          error={spaceQuery.error}
          retry={() => void spaceQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={4} />
      ) : spaceQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "State",
                value: (
                  <Badge
                    variant={
                      blocksQuestions && blocksAnswers ? "destructive" : "success"
                    }
                  >
                    {translateText(
                      blocksQuestions && blocksAnswers
                        ? "Intake closed"
                        : "Operational",
                    )}
                  </Badge>
                ),
                icon: BookOpen,
              },
              {
                title: "Accepts",
                value: (
                  <div className="flex flex-wrap gap-2">
                    <Badge variant={blocksQuestions ? "mono" : "success"}>
                      {translateText(blocksQuestions ? "No questions" : "Questions")}
                    </Badge>
                    <Badge variant={blocksAnswers ? "mono" : "success"}>
                      {translateText(blocksAnswers ? "No answers" : "Answers")}
                    </Badge>
                  </div>
                ),
                icon: CheckCircle2,
              },
              {
                title: "Needs action",
                value: questionsNeedingAction.length,
                description: translateText(
                  "Questions in this Space waiting for an operator decision",
                ),
                icon: Waypoints,
              },
              {
                title: "Visibility",
                value: (
                  <VisibilityBadge visibility={spaceQuery.data.visibility} />
                ),
                icon: BookOpen,
              },
            ]}
          />

          <Card className="border-emerald-500/20 bg-linear-to-br from-background via-background to-emerald-500/[0.06]">
            <CardContent className="flex flex-col gap-4 p-5 lg:flex-row lg:items-center lg:justify-between">
              <div className="space-y-1">
                <p className="text-xs font-medium uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-300">
                  {translateText("Recommended next action")}
                </p>
                <p className="text-lg font-semibold text-mono">
                  {translateText(nextAction.label)}
                </p>
                <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
                  {translateText(nextAction.text)}
                </p>
              </div>
              <Button asChild className="bg-emerald-600 text-white shadow-lg shadow-emerald-600/25 hover:bg-emerald-700">
                <Link to={nextAction.to}>
                  {translateText(nextAction.label)}
                </Link>
              </Button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Workflow rules")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: "Accepts questions",
                    value: (
                      <Badge
                        variant={
                          spaceQuery.data.acceptsQuestions ? "success" : "mono"
                        }
                      >
                        {translateText(
                          spaceQuery.data.acceptsQuestions
                            ? "Enabled"
                            : "Disabled",
                        )}
                      </Badge>
                    ),
                  },
                  {
                    label: "Accepts answers",
                    value: (
                      <Badge
                        variant={
                          spaceQuery.data.acceptsAnswers ? "success" : "mono"
                        }
                      >
                        {translateText(
                          spaceQuery.data.acceptsAnswers
                            ? "Enabled"
                            : "Disabled",
                        )}
                      </Badge>
                    ),
                  },
                  {
                    label: "Review gate",
                    value: (
                      <Badge variant={reviewGated ? "warning" : "secondary"}>
                        {translateText(
                          reviewGated ? "Required by mode" : "Open by mode",
                        )}
                      </Badge>
                    ),
                  },
                ]}
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Publishing")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: "Published at",
                    value: formatOptionalDateTimeInTimeZone(
                      spaceQuery.data.publishedAtUtc,
                      portalTimeZone,
                      translateText("Not set"),
                    ),
                  },
                  {
                    label: "Last validated",
                    value: formatOptionalDateTimeInTimeZone(
                      spaceQuery.data.lastValidatedAtUtc,
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
                  {spaceQuery.data.tags.map((tag) => (
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
                      .mutateAsync({ spaceId: id, tagId: selectedTagId })
                      .then(() => setSelectedTagId(""))
                  }
                >
                  {translateText("Attach tag")}
                </Button>
              </div>
            </CardContent>
          </Card>

          <Card>
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
                  {spaceQuery.data.curatedSources.map((source) => (
                    <div
                      key={source.id}
                      className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                    >
                      <div className="min-w-0">
                        <p className="font-medium text-mono">
                          {source.label || translateText("Untitled source")}
                        </p>
                        <p className="mt-1 break-all text-sm text-muted-foreground">
                          {source.locator}
                        </p>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <VisibilityBadge visibility={source.visibility} />
                        {source.isAuthoritative ? (
                          <Badge variant="primary">
                            {translateText("Authoritative")}
                          </Badge>
                        ) : null}
                        {source.allowsPublicCitation ? (
                          <Badge variant="success" appearance="outline">
                            {translateText("Public citation")}
                          </Badge>
                        ) : null}
                        {source.allowsPublicExcerpt ? (
                          <Badge variant="outline">
                            {translateText("Public excerpt")}
                          </Badge>
                        ) : null}
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
                  description="Attach authoritative or reusable material that should anchor this space."
                />
              )}
              <div className="flex flex-col gap-3 sm:flex-row">
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
                <Button
                  disabled={!selectedSourceId || addSource.isPending}
                  onClick={() =>
                    addSource
                      .mutateAsync({ spaceId: id, sourceId: selectedSourceId })
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
                <CardTitle>{translateText("Questions in this Space")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
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
                      <div className="mt-2 flex flex-wrap gap-2">
                        {!question.acceptedAnswerId &&
                        question.status !== QuestionStatus.Duplicate ? (
                          <Badge variant="warning" appearance="outline">
                            {translateText("Needs answer decision")}
                          </Badge>
                        ) : null}
                        {question.duplicateOfQuestionId ? (
                          <Badge variant="mono" appearance="outline">
                            {translateText("Duplicate")}
                          </Badge>
                        ) : null}
                      </div>
                    </div>
                    <div className="flex shrink-0 flex-col gap-2 sm:items-end">
                      <QuestionStatusBadge status={question.status} />
                      <Button asChild variant="outline" size="sm">
                        <Link to={`/app/questions/${question.id}`}>
                          {translateText("Open thread")}
                        </Link>
                      </Button>
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No questions yet"
                  description={
                    blocksQuestions
                      ? "Question intake is disabled for this space."
                      : "Create the first thread in this space to start the QnA workflow."
                  }
                  action={
                    blocksQuestions
                      ? undefined
                      : {
                          label: "New question",
                          to: `/app/questions/new?spaceId=${id}`,
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
                  <span>{translateText("Activity in this Space")}</span>
                  <Badge variant="outline">
                    {translateText("{count} events", {
                      count: contextualActivity.length,
                    })}
                  </Badge>
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {contextualActivity.length ? (
                contextualActivity.slice(0, 6).map((event) => (
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
                    <Button asChild variant="outline" size="sm">
                      <Link
                        to={
                          event.answerId
                            ? `/app/answers/${event.answerId}`
                            : `/app/questions/${event.questionId}`
                        }
                      >
                        <Activity className="size-4" />
                        {translateText("Open context")}
                      </Link>
                    </Button>
                  </div>
                ))
              ) : (
                <EmptyState
                  title="No contextual activity yet"
                  description="Activity appears here after questions in this Space receive workflow changes, moderation, votes, feedback, or reports."
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : null}
    </DetailLayout>
  );
}
