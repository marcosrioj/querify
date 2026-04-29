import { startTransition, useDeferredValue, useMemo, useState } from "react";
import {
  Activity,
  BookOpen,
  CheckCircle2,
  Link2,
  MessageSquareText,
  Pencil,
  Plus,
  Tags,
  Trash2,
  Waypoints,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useActivityList } from "@/domains/activity/hooks";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { useCreateQuestion, useQuestionList } from "@/domains/questions/hooks";
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
  Label,
  SearchSelect,
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
  const { id } = useParams();
  const spaceQuery = useSpace(id);
  const questionQuery = useQuestionList({
    page: 1,
    pageSize: 100,
    sorting: "LastActivityAtUtc DESC",
    spaceId: id,
  });
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 100,
    sorting: "OccurredAtUtc DESC",
  });
  const deleteSpace = useDeleteSpace();
  const createQuestion = useCreateQuestion();
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
    (question) =>
      question.status === QuestionStatus.Draft ||
      (!question.acceptedAnswerId &&
        !question.duplicateOfQuestionId &&
        question.status !== QuestionStatus.Archived),
  );
  const visibleQuestionIds = new Set(
    spaceQuestions.map((question) => question.id),
  );
  const contextualActivity = (activityQuery.data?.items ?? []).filter((entry) =>
    visibleQuestionIds.has(entry.questionId),
  );
  const tagsPagination = useLocalPagination({
    items: spaceQuery.data?.tags ?? [],
  });
  const sourcesPagination = useLocalPagination({
    items: spaceQuery.data?.curatedSources ?? [],
  });
  const questionsPagination = useLocalPagination({ items: spaceQuestions });
  const activityPagination = useLocalPagination({ items: contextualActivity });

  const activateRelationshipTab = (tab: string, focusTargetId?: string) => {
    setRelationshipTab(tab);

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
          label: "Resolve first question",
          tab: "questions",
          text: "A question needs an accepted answer or duplicate routing.",
        }
      : (spaceQuery.data?.curatedSources.length ?? 0) === 0
        ? {
            label: "Attach source",
            tab: "sources",
            text: "This Space has no curated evidence yet. Attach a reusable Source before scaling answers.",
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
          description="Review state, intake rules, questions needing action, connected tags and sources, and the next recommended move."
          descriptionMode="hint"
          backTo="/app/spaces"
        />
      }
      sidebar={
        <>
          <ActionPanel description="Record-level actions for this Space.">
            {blocksQuestions ? (
              <ActionButton disabled>
                <Plus className="size-4" />
                {translateText("New question")}
              </ActionButton>
            ) : (
              <ActionButton
                type="button"
                tone="primary"
                onClick={() =>
                  activateRelationshipTab("questions", "new-question-title")
                }
              >
                <Plus className="size-4" />
                {translateText("New question")}
              </ActionButton>
            )}
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
                <ActionButton tone="danger" span="full">
                  <Trash2 className="size-4" />
                  {translateText("Delete")}
                </ActionButton>
              }
            />
            {blocksQuestions ? (
              <p className="col-span-2 text-xs text-muted-foreground">
                {translateText("This space does not accept new questions.")}
              </p>
            ) : null}
          </ActionPanel>
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
                        "This summarizes status and the major workflow gates.",
                      )}
                      label={translateText("Overview details")}
                    />
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    { label: "Slug", value: spaceQuery.data.slug },
                    { label: "Language", value: spaceQuery.data.language },
                    {
                      label: "Questions",
                      value: String(spaceQuery.data.questionCount),
                    },
                    {
                      label: "Curated sources",
                      value: String(spaceQuery.data.curatedSources.length),
                    },
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
          {showLoadingState ? null : spaceQuery.data ? (
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
                            spaceQuery.data.acceptsQuestions
                              ? "success"
                              : "mono"
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
                      label: "Status",
                      value: (
                        <SpaceStatusBadge status={spaceQuery.data.status} />
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
                value: <SpaceStatusBadge status={spaceQuery.data.status} />,
                icon: BookOpen,
              },
              {
                title: "Accepts",
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
              {"to" in nextAction ? (
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
                        : nextAction.tab === "sources"
                          ? "space-source-picker"
                          : undefined,
                    )
                  }
                >
                  {translateText(nextAction.label)}
                </Button>
              )}
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
                  "Attach reusable evidence that should be trusted inside this Space.",
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
                count: contextualActivity.length,
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
                          <p className="font-medium text-mono">
                            {source.label || translateText("Untitled source")}
                          </p>
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
                    description="Attach reusable material that should anchor this space."
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
                  <CardTitle>
                    {translateText("Questions in this Space")}
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {!blocksQuestions ? (
                  <form
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
                          status: QuestionStatus.Active,
                          visibility: VisibilityScope.Public,
                          originChannel: ChannelKind.Manual,
                          sort: spaceQuestions.length + 1,
                        })
                        .then(() => {
                          setNewQuestionTitle("");
                          setNewQuestionSummary("");
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
                  questionsPagination.pagedItems.map((question) => (
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
                          !question.duplicateOfQuestionId ? (
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
                            {translateText("Open question")}
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
                        : "Create the first question in this space to start the QnA workflow."
                    }
                  />
                )}
                <ChildListPagination
                  page={questionsPagination.page}
                  pageSize={questionsPagination.pageSize}
                  totalCount={questionsPagination.totalCount}
                  isFetching={questionQuery.isFetching}
                  onPageChange={questionsPagination.setPage}
                  onPageSizeChange={questionsPagination.setPageSize}
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
                        count: contextualActivity.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-3">
                {contextualActivity.length ? (
                  activityPagination.pagedItems.map((event) => (
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
                    description="Activity appears here after questions in this Space receive status changes, votes, or feedback."
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
