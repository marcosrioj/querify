import { Link, useParams } from "react-router-dom";
import { useActivity } from "@/domains/activity/hooks";
import { RecommendedNextActionCard } from "@/domains/qna/recommended-next-action-card";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
  SectionGrid,
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

export function ActivityDetailPage() {
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const activityQuery = useActivity(id);
  const activityNextAction = !activityQuery.data
    ? {
        label: "Back to activity",
        to: "/app/activity",
        text: "Return to the activity stream while this event loads.",
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
          title="Activity event"
          description="Inspect actor context, notes, metadata, and the question identifiers behind this audit entry."
          descriptionMode="hint"
          backTo={
            activityQuery.data?.answerId
              ? `/app/answers/${activityQuery.data.answerId}`
              : activityQuery.data?.questionId
                ? `/app/questions/${activityQuery.data.questionId}`
                : "/app/spaces"
          }
        />
      }
      sidebar={
        activityQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : activityQuery.data ? (
          <ActionPanel
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
        ) : null
      }
    >
      {activityQuery.isError ? (
        <ErrorState
          title="Unable to load activity event"
          error={activityQuery.error}
          retry={() => void activityQuery.refetch()}
        />
      ) : activityQuery.isLoading ? (
        <DetailPageSkeleton cards={3} />
      ) : activityQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Event",
                value: <ActivityKindBadge kind={activityQuery.data.kind} />,
              },
              {
                title: "Actor",
                value: <ActorKindBadge kind={activityQuery.data.actorKind} />,
              },
              {
                title: "Question",
                value: activityQuery.data.questionId,
              },
              {
                title: "Answer",
                value: activityQuery.data.answerId || "No answer scope",
              },
            ]}
          />

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
                {activityQuery.data.metadataJson ? (
                  <pre className="mt-2 overflow-x-auto rounded-lg border border-border bg-muted/10 p-4 text-sm">
                    {activityQuery.data.metadataJson}
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
