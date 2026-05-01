import { Link, useParams } from "react-router-dom";
import { useActivity } from "@/domains/activity/hooks";
import { RecommendedNextActionCard } from "@/domains/qna/recommended-next-action-card";
import { useQuestion } from "@/domains/questions/hooks";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import {
  DetailOverviewCard,
  DetailLayout,
  KeyValueList,
  PageHeader,
} from "@/shared/layout/page-layouts";
import {
  ActionButton,
  ActionPanel,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ContextHint,
  DetailPageSkeleton,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { ErrorState } from "@/shared/ui/placeholder-state";
import { ActivityKindBadge, ActorKindBadge } from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";

function formatMetadataJson(metadataJson?: string | null) {
  const value = metadataJson?.trim();

  if (!value) {
    return null;
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}

function ActivitySubjectValue({
  fallback,
  value,
}: {
  fallback: string;
  value?: string | null;
}) {
  return (
    <span className="block text-base font-medium leading-snug text-foreground [overflow-wrap:anywhere] sm:text-lg">
      {value?.trim() || fallback}
    </span>
  );
}

export function ActivityDetailPage() {
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const activityQuery = useActivity(id);
  const questionQuery = useQuestion(activityQuery.data?.questionId);
  const formattedMetadataJson = formatMetadataJson(
    activityQuery.data?.metadataJson,
  );
  const activityNextAction = !activityQuery.data
    ? {
        label: "Open spaces",
        to: "/app/spaces",
        text: "Return to Spaces while this event loads.",
      }
    : activityQuery.data.answerId
      ? {
          label: "Open answer",
          to: `/app/answers/${activityQuery.data.answerId}`,
          text: "This event is scoped to an answer. Open it to review lifecycle, sources, and acceptance state.",
        }
      : {
          label: "Open question",
          to: `/app/questions/${activityQuery.data.questionId}`,
          text: "This event is scoped to a question. Open it to review answers, sources, tags, and workflow state.",
        };

  if (!id) {
    return (
      <ErrorState
        title="Invalid activity route"
        description="Activity detail routes need an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title="Activity"
          description="Inspect actor context, notes, metadata, and the question or answer behind this audit entry."
          descriptionMode="hint"
          backTo={
            activityQuery.data?.answerId
              ? `/app/answers/${activityQuery.data.answerId}`
              : activityQuery.data?.questionId
                ? `/app/questions/${activityQuery.data.questionId}`
                : "/app/spaces"
          }
          breadcrumbs={
            activityQuery.data
              ? [
                  ...(questionQuery.data?.spaceId
                    ? [
                        {
                          label: "Space",
                          to: `/app/spaces/${questionQuery.data.spaceId}`,
                        },
                      ]
                    : []),
                  {
                    label: "Question",
                    to: `/app/questions/${activityQuery.data.questionId}`,
                  },
                  ...(activityQuery.data.answerId
                    ? [
                        {
                          label: "Answer",
                          to: `/app/answers/${activityQuery.data.answerId}`,
                        },
                      ]
                    : []),
                ]
              : undefined
          }
        />
      }
      sidebar={
        activityQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : activityQuery.data ? (
          <DetailOverviewCard
            description="This summarizes the event type, actor scope, and linked records."
            highlights={[
              {
                label: "Event",
                description:
                  "The workflow, moderation, status, vote, or feedback event recorded in activity history.",
                value: <ActivityKindBadge kind={activityQuery.data.kind} />,
              },
              {
                label: "Actor",
                description:
                  "The actor scope captured when the event was recorded.",
                value: <ActorKindBadge kind={activityQuery.data.actorKind} />,
              },
            ]}
            items={[
              {
                label: "Question",
                description:
                  "Question record connected to this activity event.",
                value: (
                  <ActivitySubjectValue
                    value={activityQuery.data.questionTitle}
                    fallback={activityQuery.data.questionId}
                  />
                ),
              },
              {
                label: "Answer",
                description:
                  "Answer record connected to this activity event, when the event has answer scope.",
                value: (
                  <ActivitySubjectValue
                    value={activityQuery.data.answerHeadline}
                    fallback={
                      activityQuery.data.answerId ||
                      translateText("No answer scope")
                    }
                  />
                ),
              },
            ]}
          />
        ) : null
      }
    >
      {activityQuery.data ? (
        <ActionPanel
          layout="bar"
          title="Jump to subject"
          description="Open the record connected to this event."
        >
          <ActionButton asChild tone="primary">
            <Link to={`/app/questions/${activityQuery.data.questionId}`}>
              {translateText("Open question")}
            </Link>
          </ActionButton>
          {activityQuery.data.answerId ? (
            <ActionButton asChild tone="secondary">
              <Link to={`/app/answers/${activityQuery.data.answerId}`}>
                {translateText("Open answer")}
              </Link>
            </ActionButton>
          ) : null}
        </ActionPanel>
      ) : null}
      {activityQuery.isError ? (
        <ErrorState
          title="Unable to load activity event"
          error={activityQuery.error}
          retry={() => void activityQuery.refetch()}
        />
      ) : activityQuery.isLoading ? (
        <DetailPageSkeleton cards={3} metrics={0} />
      ) : activityQuery.data ? (
        <>
          <RecommendedNextActionCard
            label={activityNextAction.label}
            text={activityNextAction.text}
            action={
              <Button asChild>
                <Link to={activityNextAction.to}>
                  {translateText(activityNextAction.label)}
                </Link>
              </Button>
            }
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex items-center gap-2">
                  <span>{translateText("Actor context")}</span>
                  <ContextHint
                    content={translateText(
                      "This is the actor identity captured when the event was recorded.",
                    )}
                    label={translateText("Actor context details")}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: "Actor label",
                    value: activityQuery.data.actorLabel || "Not set",
                  },
                  { label: "User print", value: activityQuery.data.userPrint },
                  { label: "IP", value: activityQuery.data.ip || "Not set" },
                  {
                    label: "User agent",
                    value: activityQuery.data.userAgent || "Not set",
                  },
                  {
                    label: "Occurred at",
                    value: formatOptionalDateTimeInTimeZone(
                      activityQuery.data.occurredAtUtc,
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
                <CardTitle>{translateText("Notes and metadata")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Notes")}
                </p>
                <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                  {activityQuery.data.notes ||
                    translateText("No notes recorded.")}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Metadata JSON")}
                </p>
                {formattedMetadataJson ? (
                  <pre className="mt-2 max-h-[32rem] overflow-auto rounded-lg border border-border bg-muted/10 p-4 font-mono text-xs leading-5">
                    {formattedMetadataJson}
                  </pre>
                ) : (
                  <p className="mt-2 text-sm text-muted-foreground">
                    {translateText("No metadata recorded.")}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        </>
      ) : null}
    </DetailLayout>
  );
}
