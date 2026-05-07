import { useState } from "react";
import {
  CheckCircle2,
  ExternalLink,
  FolderKanban,
  Link2,
  MessageSquareText,
  Pencil,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { RecommendedNextActionCard } from "@/domains/qna/recommended-next-action-card";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useDeleteSource, useSource } from "@/domains/sources/hooks";
import {
  DetailOverviewCard,
  DetailFieldList,
  DetailLayout,
  KeyValueList,
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
  DetailPageSkeleton,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  AnswerKindBadge,
  AnswerStatusBadge,
  QuestionStatusBadge,
  SourceKindBadge,
  SourceRoleBadge,
  SpaceStatusBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { useLocalPagination } from "@/shared/lib/use-local-pagination";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";

export function SourceDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const sourceQuery = useSource(id);
  const deleteSource = useDeleteSource();
  const [relationshipTab, setRelationshipTab] = useState("spaces");
  const spacesPagination = useLocalPagination({
    items: sourceQuery.data?.spaces ?? [],
  });
  const questionsPagination = useLocalPagination({
    items: sourceQuery.data?.questions ?? [],
  });
  const answersPagination = useLocalPagination({
    items: sourceQuery.data?.answers ?? [],
  });
  const activateRelationshipTab = (tab: string) => {
    setRelationshipTab(tab);

    window.requestAnimationFrame(() => {
      window.requestAnimationFrame(() => {
        document
          .getElementById(`source-${tab}-section`)
          ?.scrollIntoView({ behavior: "smooth", block: "start" });
      });
    });
  };
  const sourceNextAction = !sourceQuery.data
    ? {
        label: "Back to sources",
        to: "/app/sources",
        text: "Return to the source catalog while this source loads.",
      }
    : sourceQuery.data.spaces.length === 0
      ? {
          label: "Open spaces",
          to: "/app/spaces",
          text: "No Space curates this source yet. Attach it from a Space when it should become trusted evidence.",
        }
      : sourceQuery.data.questions.length === 0
        ? {
            label: "Review spaces",
            tab: "spaces",
            text: "This source is curated by a Space. Use those boundaries to decide which questions should reference it.",
          }
        : sourceQuery.data.answers.length === 0
          ? {
              label: "Review question links",
              tab: "questions",
              text: "Questions already use this source. Link it to answers when they need optional evidence or canonical support.",
            }
          : {
              label: "Review answer links",
              tab: "answers",
              text: "This source is cited by answers. Review those links before updating trust metadata.",
            };

  if (!id) {
    return (
      <ErrorState
        title="Invalid source route"
        description="Source detail routes need an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title={sourceQuery.data?.label || "Source"}
          description="Review visibility, verification metadata, and connector identifiers for this reusable source."
          descriptionMode="hint"
          backTo="/app/sources"
        />
      }
      sidebar={
        sourceQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : sourceQuery.data ? (
          <DetailOverviewCard
            description="This summarizes source type, visibility, verification metadata, and connector identifiers."
            highlights={[
              {
                label: "Kind",
                description:
                  "The type of evidence or reusable reference this source represents.",
                value: <SourceKindBadge kind={sourceQuery.data.kind} />,
              },
              {
                label: "Visibility",
                description:
                  "Controls which audiences can see or reuse this source.",
                value: (
                  <VisibilityBadge visibility={sourceQuery.data.visibility} />
                ),
              },
            ]}
            items={[
              {
                label: "External ID",
                description:
                  "Identifier from the upstream connector, repository, or source system.",
                value: sourceQuery.data.externalId || "Not set",
              },
              {
                label: "Checksum",
                description:
                  "Read-only value generated by the backend from the locator.",
                value: sourceQuery.data.checksum || "Not set",
              },
              {
                label: "Language",
                description: "Use the main locale for this source content.",
                value: sourceQuery.data.language || "Not set",
              },
              {
                label: "Last verified",
                description:
                  "Verification timestamp used to show when this source was last checked.",
                value: formatOptionalDateTimeInTimeZone(
                  sourceQuery.data.lastVerifiedAtUtc,
                  portalTimeZone,
                  translateText("Not set"),
                ),
              },
            ]}
          />
        ) : null
      }
    >
      <ActionPanel layout="bar" description="Source actions and risk controls.">
        <ActionButton asChild tone="secondary">
          <Link to={`/app/sources/${id}/edit`}>
            <Pencil className="size-4" />
            {translateText("Edit")}
          </Link>
        </ActionButton>
        <ConfirmAction
          title={translateText('Delete source "{name}"?', {
            name:
              sourceQuery.data?.label ||
              sourceQuery.data?.locator ||
              translateText("this source"),
          })}
          description={translateText(
            "This removes the source from the portal catalog and future linking flows.",
          )}
          confirmLabel={translateText("Delete source")}
          isPending={deleteSource.isPending}
          onConfirm={() =>
            deleteSource.mutateAsync(id).then(() => navigate("/app/sources"))
          }
          trigger={
            <ActionButton tone="danger">
              <Trash2 className="size-4" />
              {translateText("Delete")}
            </ActionButton>
          }
        />
      </ActionPanel>
      {sourceQuery.isError ? (
        <ErrorState
          title="Unable to load source"
          error={sourceQuery.error}
          retry={() => void sourceQuery.refetch()}
        />
      ) : sourceQuery.isLoading ? (
        <DetailPageSkeleton cards={4} metrics={0} />
      ) : sourceQuery.data ? (
        <>
          <RecommendedNextActionCard
            label={sourceNextAction.label}
            text={sourceNextAction.text}
            action={
              "to" in sourceNextAction ? (
                <Button asChild>
                  <Link to={sourceNextAction.to}>
                    {translateText(sourceNextAction.label)}
                  </Link>
                </Button>
              ) : (
                <Button
                  type="button"
                  onClick={() => activateRelationshipTab(sourceNextAction.tab)}
                >
                  {translateText(sourceNextAction.label)}
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
                    label: "Label",
                    description:
                      "Human-readable name shown when operators choose this source.",
                    value: sourceQuery.data.label || translateText("Not set"),
                    valueClassName: "text-base font-medium text-mono",
                  },
                  {
                    label: "Locator",
                    description:
                      "Use the canonical URL, path, repository URI, ticket reference, or document locator.",
                    value: (
                      <>
                        <p className="break-all">{sourceQuery.data.locator}</p>
                        {/^https?:\/\//i.test(sourceQuery.data.locator) ? (
                          <Button asChild variant="outline" size="sm">
                            <a
                              href={sourceQuery.data.locator}
                              target="_blank"
                              rel="noreferrer"
                            >
                              <ExternalLink className="size-4" />
                              {translateText("Open locator")}
                            </a>
                          </Button>
                        ) : null}
                      </>
                    ),
                    valueClassName: "space-y-3",
                  },
                ]}
              />
            </CardContent>
          </Card>

          <QnaModuleNav
            eyebrow="Source relationships"
            activeKey={relationshipTab}
            onActiveKeyChange={setRelationshipTab}
            items={[
              {
                key: "spaces",
                label: "Spaces",
                description:
                  "Spaces curating this source as reusable evidence.",
                icon: FolderKanban,
                count: sourceQuery.data?.spaceUsageCount ?? 0,
              },
              {
                key: "questions",
                label: "Questions",
                description:
                  "Questions linked to this source for context or origin.",
                icon: MessageSquareText,
                count: sourceQuery.data?.questionUsageCount ?? 0,
              },
              {
                key: "answers",
                label: "Answers",
                description:
                  "Answers citing this source as supporting evidence.",
                icon: CheckCircle2,
                count: sourceQuery.data?.answerUsageCount ?? 0,
              },
            ]}
          />

          {relationshipTab === "spaces" ? (
            <Card id="source-spaces-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Spaces")}</span>
                    <Badge variant="outline">
                      {translateText("{count} spaces", {
                        count: sourceQuery.data.spaces.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {sourceQuery.data.spaces.length ? (
                  <div className="space-y-3">
                    {spacesPagination.pagedItems.map((space) => (
                      <div
                        key={space.id}
                        className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                      >
                        <div className="min-w-0 space-y-2">
                          <div>
                            <p className="font-medium text-mono">
                              {space.name}
                            </p>
                            <p className="mt-1 break-words text-sm text-muted-foreground">
                              {space.slug}
                            </p>
                          </div>
                          {space.summary ? (
                            <p className="line-clamp-2 text-sm text-muted-foreground">
                              {space.summary}
                            </p>
                          ) : null}
                          <div className="flex flex-wrap items-center gap-2">
                            <SpaceStatusBadge status={space.status} />
                            <VisibilityBadge visibility={space.visibility} />
                            <Badge variant="outline">
                              {translateText("{count} questions", {
                                count: space.questionCount,
                              })}
                            </Badge>
                            <Badge
                              variant={
                                space.acceptsQuestions ? "success" : "mono"
                              }
                              appearance="outline"
                            >
                              {translateText(
                                space.acceptsQuestions
                                  ? "Questions enabled"
                                  : "Questions disabled",
                              )}
                            </Badge>
                            <Badge
                              variant={
                                space.acceptsAnswers ? "success" : "mono"
                              }
                              appearance="outline"
                            >
                              {translateText(
                                space.acceptsAnswers
                                  ? "Answers enabled"
                                  : "Answers disabled",
                              )}
                            </Badge>
                          </div>
                        </div>
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/spaces/${space.spaceId}`}>
                            <Link2 className="size-4" />
                            {translateText("Open space")}
                          </Link>
                        </Button>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title="No spaces curate this source yet"
                    description="Attach this source from a Space when it should be trusted in that operating boundary."
                  />
                )}
                <ChildListPagination
                  page={spacesPagination.page}
                  pageSize={spacesPagination.pageSize}
                  totalCount={spacesPagination.totalCount}
                  onPageChange={spacesPagination.setPage}
                  onPageSizeChange={spacesPagination.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "questions" ? (
            <Card id="source-questions-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Question links")}</span>
                    <Badge variant="outline">
                      {translateText("{count} questions", {
                        count: sourceQuery.data.questions.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {sourceQuery.data.questions.length ? (
                  <div className="space-y-3">
                    {questionsPagination.pagedItems.map((question) => (
                      <div
                        key={question.id}
                        className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                      >
                        <div className="min-w-0 space-y-2">
                          <div>
                            <p className="font-medium text-mono">
                              {question.title}
                            </p>
                            <p className="mt-1 break-words text-sm text-muted-foreground">
                              {question.spaceSlug}
                            </p>
                          </div>
                          {question.summary ? (
                            <p className="line-clamp-2 text-sm text-muted-foreground">
                              {question.summary}
                            </p>
                          ) : null}
                          <div className="flex flex-wrap items-center gap-2">
                            <QuestionStatusBadge status={question.status} />
                            <VisibilityBadge visibility={question.visibility} />
                            <SourceRoleBadge role={question.role} />
                            <Badge variant="outline">
                              {translateText("Order {value}", {
                                value: question.order,
                              })}
                            </Badge>
                            <Badge variant="outline">
                              {formatOptionalDateTimeInTimeZone(
                                question.lastActivityAtUtc,
                                portalTimeZone,
                                translateText("No activity"),
                              )}
                            </Badge>
                          </div>
                        </div>
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/questions/${question.questionId}`}>
                            <Link2 className="size-4" />
                            {translateText("Open question")}
                          </Link>
                        </Button>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title="No question links yet"
                    description="Attach this source from a Question when it explains origin, context, or supporting evidence."
                  />
                )}
                <ChildListPagination
                  page={questionsPagination.page}
                  pageSize={questionsPagination.pageSize}
                  totalCount={questionsPagination.totalCount}
                  onPageChange={questionsPagination.setPage}
                  onPageSizeChange={questionsPagination.setPageSize}
                />
              </CardContent>
            </Card>
          ) : null}

          {relationshipTab === "answers" ? (
            <Card id="source-answers-section">
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex items-center gap-2">
                    <span>{translateText("Answer links")}</span>
                    <Badge variant="outline">
                      {translateText("{count} answers", {
                        count: sourceQuery.data.answers.length,
                      })}
                    </Badge>
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent className="space-y-4">
                {sourceQuery.data.answers.length ? (
                  <div className="space-y-3">
                    {answersPagination.pagedItems.map((answer) => (
                      <div
                        key={answer.id}
                        className="flex flex-col gap-3 rounded-lg border border-border bg-muted/10 p-4 sm:flex-row sm:items-start sm:justify-between"
                      >
                        <div className="min-w-0 space-y-2">
                          <div>
                            <p className="font-medium text-mono">
                              {answer.headline}
                            </p>
                            <p className="mt-1 break-words text-sm text-muted-foreground">
                              {answer.questionTitle}
                            </p>
                          </div>
                          <div className="flex flex-wrap items-center gap-2">
                            <AnswerStatusBadge status={answer.status} />
                            <AnswerKindBadge kind={answer.kind} />
                            <VisibilityBadge visibility={answer.visibility} />
                            <SourceRoleBadge role={answer.role} />
                            <Badge variant="outline">
                              {translateText("Order {value}", {
                                value: answer.order,
                              })}
                            </Badge>
                            {answer.isAccepted ? (
                              <Badge variant="success">
                                {translateText("Accepted")}
                              </Badge>
                            ) : null}
                          </div>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          <Button asChild variant="outline" size="sm">
                            <Link to={`/app/answers/${answer.answerId}`}>
                              <Link2 className="size-4" />
                              {translateText("Open answer")}
                            </Link>
                          </Button>
                          <Button asChild variant="ghost" size="sm">
                            <Link to={`/app/questions/${answer.questionId}`}>
                              {translateText("Open question")}
                            </Link>
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title="No answer links yet"
                    description="Attach this source from an Answer when it should be cited as evidence or a canonical reference."
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

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>
                  {translateText("Usage impact and metadata")}
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <KeyValueList
                items={[
                  {
                    label: "Context note",
                    value: sourceQuery.data.contextNote || "Not set",
                  },
                  {
                    label: "Media type",
                    value: sourceQuery.data.mediaType || "Not set",
                  },
                  {
                    label: "Last verified",
                    value: formatOptionalDateTimeInTimeZone(
                      sourceQuery.data.lastVerifiedAtUtc,
                      portalTimeZone,
                      translateText("Not set"),
                    ),
                  },
                  {
                    label: "Spaces",
                    value: String(sourceQuery.data.spaceUsageCount),
                  },
                  {
                    label: "Questions",
                    value: String(sourceQuery.data.questionUsageCount),
                  },
                  {
                    label: "Answers",
                    value: String(sourceQuery.data.answerUsageCount),
                  },
                ]}
              />
              {sourceQuery.data.metadataJson ? (
                <pre className="overflow-x-auto rounded-lg border border-border bg-muted/10 p-4 text-sm">
                  {sourceQuery.data.metadataJson}
                </pre>
              ) : (
                <EmptyState
                  title="No metadata JSON"
                  description="Add structured metadata when an integration or ingestion flow needs it."
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : null}
    </DetailLayout>
  );
}
