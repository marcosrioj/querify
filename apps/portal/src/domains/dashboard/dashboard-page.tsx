import {
  Activity,
  ArrowRight,
  Clock3,
  FolderKanban,
  MessageSquareText,
  Plus,
  RadioTower,
  ShieldCheck,
  UsersRound,
  Waypoints,
} from "lucide-react";
import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  XAxis,
  YAxis,
} from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { useActivityList } from "@/domains/activity/hooks";
import type { ActivityDto } from "@/domains/activity/types";
import { useAnswerList } from "@/domains/answers/hooks";
import type { AnswerDto } from "@/domains/answers/types";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useQuestionList } from "@/domains/questions/hooks";
import type { QuestionDto } from "@/domains/questions/types";
import { useSourceList } from "@/domains/sources/hooks";
import type { SourceDto } from "@/domains/sources/types";
import { useSpaceList } from "@/domains/spaces/hooks";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  useCurrentWorkspace,
  useTenantWorkspace,
} from "@/domains/tenants/hooks";
import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  ProgressChecklistCard,
  SectionGridSkeleton,
  Skeleton,
} from "@/shared/ui";
import {
  PageHeader,
  PageSurface,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { formatNumericDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { translateText } from "@/shared/lib/i18n-core";
import {
  ActivityKindBadge,
  ActorKindBadge,
  AnswerStatusBadge,
  QuestionStatusBadge,
  SourceKindBadge,
  SpaceKindBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import { AnswerStatus, QuestionStatus } from "@/shared/constants/backend-enums";

function DashboardSection({
  title,
  description,
  action,
  children,
}: {
  title: string;
  description: string;
  action?: { label: string; to: string };
  children: ReactNode;
}) {
  return (
    <Card className="min-h-full overflow-hidden">
      <CardHeader className="gap-4">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <CardHeading>
            <CardTitle>{translateText(title)}</CardTitle>
            <CardDescription>{translateText(description)}</CardDescription>
          </CardHeading>
          {action ? (
            <Button asChild variant="ghost" size="sm">
              <Link to={action.to}>
                {translateText(action.label)}
                <ArrowRight className="size-4" />
              </Link>
            </Button>
          ) : null}
        </div>
      </CardHeader>
      <CardContent className="space-y-3">{children}</CardContent>
    </Card>
  );
}

function DashboardSectionSkeleton() {
  return (
    <Card className="min-h-full overflow-hidden">
      <CardHeader className="gap-4">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="space-y-2">
            <Skeleton className="h-5 w-36" />
            <Skeleton className="h-4 w-56" />
          </div>
          <Skeleton className="h-7 w-24" />
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {Array.from({ length: 3 }, (_, index) => (
          <div
            key={`dashboard-section-skeleton-${index}`}
            className="rounded-lg border border-border/70 bg-background/70 px-4 py-3"
          >
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0 flex-1 space-y-2">
                <Skeleton className="h-4 w-2/3" />
                <Skeleton className="h-4 w-1/3" />
                <Skeleton className="h-4 w-full" />
              </div>
              <Skeleton className="h-6 w-20 rounded-full" />
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

function DashboardChartCard({
  title,
  description,
  children,
}: {
  title: string;
  description: string;
  children: ReactNode;
}) {
  return (
    <Card className="min-h-full overflow-hidden">
      <CardHeader>
        <CardHeading>
          <CardTitle>{translateText(title)}</CardTitle>
          <CardDescription>{translateText(description)}</CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  );
}

function ModuleSignalCard({
  title,
  description,
  value,
  icon: Icon,
}: {
  title: string;
  description: string;
  value: ReactNode;
  icon: typeof FolderKanban;
}) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/75 p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 space-y-1">
          <p className="text-sm font-semibold text-mono">
            {translateText(title)}
          </p>
          <p className="text-sm leading-6 text-muted-foreground">
            {translateText(description)}
          </p>
        </div>
        <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary ring-1 ring-primary/15">
          <Icon className="size-4" />
        </div>
      </div>
      <div className="mt-4 border-t border-border/70 pt-3 text-sm font-medium text-primary">
        {typeof value === "string" ? translateText(value) : value}
      </div>
    </div>
  );
}

function DashboardValueTracker({
  items,
}: {
  items: Array<{
    label: string;
    value: string | number;
    description: string;
    tone: string;
  }>;
}) {
  return (
    <Card className="overflow-hidden border-primary/20 bg-linear-to-br from-background via-background to-primary/[0.06]">
      <CardHeader>
        <CardHeading>
          <CardTitle>{translateText("Value tracker")}</CardTitle>
          <CardDescription>
            {translateText(
              "Proof that QnA is moving from raw intake toward reusable, trusted knowledge.",
            )}
          </CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        {items.map((item) => (
          <div
            key={item.label}
            className="rounded-lg border border-border/70 bg-background/80 p-4"
          >
            <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
              {translateText(item.label)}
            </p>
            <div className="mt-3 flex items-end justify-between gap-3">
              <p className="text-2xl font-semibold text-mono">
                {translateText(String(item.value))}
              </p>
              <span className={`h-2 w-14 rounded-full ${item.tone}`} />
            </div>
            <p className="mt-3 text-sm leading-6 text-muted-foreground">
              {translateText(item.description)}
            </p>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

function DashboardLoadingState() {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="QnA dashboard"
        description="Operate the workspace around spaces, question workflow, answer publication, curated sources, and activity signals."
      />
      <Card className="border-primary/20 bg-linear-to-br from-primary/[0.08] via-background to-sky-500/[0.05]">
        <CardContent className="space-y-5 p-5 lg:p-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
            <div className="space-y-3">
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-6 w-48" />
              <Skeleton className="h-4 w-full max-w-xl" />
            </div>
            <Skeleton className="h-16 w-36 rounded-lg" />
          </div>
          <Skeleton className="h-2 w-full" />
          <div className="grid gap-3 lg:grid-cols-2">
            {Array.from({ length: 4 }, (_, index) => (
              <Skeleton
                key={`readiness-step-skeleton-${index}`}
                className="h-20 rounded-lg"
              />
            ))}
          </div>
        </CardContent>
      </Card>
      <SectionGridSkeleton items={4} />
      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        {Array.from({ length: 4 }, (_, index) => (
          <DashboardSectionSkeleton key={`dashboard-card-skeleton-${index}`} />
        ))}
      </div>
    </PageSurface>
  );
}

function SpaceRow({ space, timeZone }: { space: SpaceDto; timeZone: string }) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/spaces/${space.id}`}
            className="block truncate text-sm font-medium text-mono hover:text-primary"
          >
            {space.name}
          </Link>
          <p className="text-sm text-muted-foreground">
            {space.key} • {space.language}
          </p>
          {space.summary ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">
              {space.summary}
            </p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <SpaceKindBadge kind={space.kind} />
          <VisibilityBadge visibility={space.visibility} />
        </div>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">
        {translateText("Questions {count} • Last validated {value}", {
          count: space.questionCount,
          value: formatNumericDateTimeInTimeZone(
            space.lastValidatedAtUtc,
            timeZone,
          ),
        })}
      </p>
    </div>
  );
}

function QuestionRow({
  question,
  timeZone,
}: {
  question: QuestionDto;
  timeZone: string;
}) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/questions/${question.id}`}
            className="block text-sm font-medium text-mono hover:text-primary"
          >
            {question.title}
          </Link>
          <p className="text-sm text-muted-foreground">{question.spaceKey}</p>
          {question.summary ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">
              {question.summary}
            </p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <QuestionStatusBadge status={question.status} />
          <VisibilityBadge visibility={question.visibility} />
        </div>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        <span>
          {translateText("Feedback {value}", { value: question.feedbackScore })}
        </span>
        <span>
          {translateText("Confidence {value}", {
            value: question.aiConfidenceScore,
          })}
        </span>
        {question.acceptedAnswerId ? (
          <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-emerald-700">
            {translateText("Accepted")}
          </span>
        ) : null}
        {question.duplicateOfQuestionId ? (
          <span className="rounded-full border border-border bg-muted px-2 py-0.5">
            {translateText("Duplicate")}
          </span>
        ) : null}
        <span>
          {translateText("Last activity {value}", {
            value: formatNumericDateTimeInTimeZone(
              question.lastActivityAtUtc,
              timeZone,
            ),
          })}
        </span>
      </div>
    </div>
  );
}

function AnswerRow({
  answer,
  timeZone,
}: {
  answer: AnswerDto;
  timeZone: string;
}) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/answers/${answer.id}`}
            className="block text-sm font-medium text-mono hover:text-primary"
          >
            {answer.headline}
          </Link>
          {answer.body ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">
              {answer.body}
            </p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <AnswerStatusBadge status={answer.status} />
          <VisibilityBadge visibility={answer.visibility} />
        </div>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        <span>{translateText("Score {value}", { value: answer.score })}</span>
        <span>{translateText("Sort {value}", { value: answer.sort })}</span>
        <span>
          {translateText("Vote score {value}", { value: answer.voteScore })}
        </span>
        <span>
          {translateText("Confidence {value}", {
            value: answer.aiConfidenceScore,
          })}
        </span>
        {answer.isAccepted ? (
          <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-emerald-700">
            {translateText("Accepted")}
          </span>
        ) : null}
        <span>
          {translateText("Published {value}", {
            value: formatNumericDateTimeInTimeZone(
              answer.publishedAtUtc,
              timeZone,
            ),
          })}
        </span>
      </div>
    </div>
  );
}

function SourceRow({
  source,
  timeZone,
}: {
  source: SourceDto;
  timeZone: string;
}) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/sources/${source.id}`}
            className="block truncate text-sm font-medium text-mono hover:text-primary"
          >
            {source.label || source.locator}
          </Link>
          <p className="truncate text-sm text-muted-foreground">
            {source.locator}
          </p>
        </div>
        <div className="space-y-2 text-right">
          <SourceKindBadge kind={source.kind} />
          <VisibilityBadge visibility={source.visibility} />
        </div>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        {source.isAuthoritative ? (
          <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-emerald-700">
            {translateText("Authoritative")}
          </span>
        ) : null}
        {source.allowsPublicCitation ? (
          <span className="rounded-full border border-border bg-muted px-2 py-0.5">
            {translateText("Public citation")}
          </span>
        ) : null}
        <span>
          {translateText("Verified {value}", {
            value: formatNumericDateTimeInTimeZone(
              source.lastVerifiedAtUtc,
              timeZone,
            ),
          })}
        </span>
      </div>
    </div>
  );
}

function ActivityRow({
  entry,
  timeZone,
}: {
  entry: ActivityDto;
  timeZone: string;
}) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/activity/${entry.id}`}
            className="block text-sm font-medium text-mono hover:text-primary"
          >
            {entry.userPrint}
          </Link>
          <p className="text-sm text-muted-foreground">
            {entry.actorLabel || translateText("Unlabeled actor")}
          </p>
          {entry.notes ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">
              {entry.notes}
            </p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <ActivityKindBadge kind={entry.kind} />
          <ActorKindBadge kind={entry.actorKind} />
        </div>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">
        {formatNumericDateTimeInTimeZone(entry.occurredAtUtc, timeZone)}
      </p>
    </div>
  );
}

export function DashboardPage() {
  const timeZone = usePortalTimeZone();
  const workspace = useCurrentWorkspace();
  const { clientKeyQuery } = useTenantWorkspace();

  const spacesQuery = useSpaceList({
    page: 1,
    pageSize: 4,
    sorting: "PublishedAtUtc DESC",
  });
  const questionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: "LastActivityAtUtc DESC",
    includeAnswers: true,
  });
  const pendingQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.PendingReview,
  });
  const answersQuery = useAnswerList({
    page: 1,
    pageSize: 5,
    sorting: "PublishedAtUtc DESC",
  });
  const publishedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: "PublishedAtUtc DESC",
    status: AnswerStatus.Published,
  });
  const validatedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: "ValidatedAtUtc DESC",
    status: AnswerStatus.Validated,
  });
  const sourcesQuery = useSourceList({
    page: 1,
    pageSize: 5,
    sorting: "LastVerifiedAtUtc DESC",
  });
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 6,
    sorting: "OccurredAtUtc DESC",
  });

  const isInitialDashboardLoading =
    spacesQuery.isLoading ||
    questionsQuery.isLoading ||
    pendingQuestionsQuery.isLoading ||
    answersQuery.isLoading ||
    publishedAnswersQuery.isLoading ||
    validatedAnswersQuery.isLoading ||
    sourcesQuery.isLoading ||
    activityQuery.isLoading ||
    clientKeyQuery.isLoading;
  const hasCriticalError =
    spacesQuery.isError ||
    questionsQuery.isError ||
    pendingQuestionsQuery.isError ||
    answersQuery.isError ||
    publishedAnswersQuery.isError ||
    validatedAnswersQuery.isError ||
    sourcesQuery.isError ||
    activityQuery.isError;

  if (isInitialDashboardLoading) {
    return <DashboardLoadingState />;
  }

  if (hasCriticalError) {
    const error =
      spacesQuery.error ??
      questionsQuery.error ??
      pendingQuestionsQuery.error ??
      answersQuery.error ??
      publishedAnswersQuery.error ??
      validatedAnswersQuery.error ??
      sourcesQuery.error ??
      activityQuery.error;

    return (
      <PageSurface>
        <PageHeader
          title="QnA dashboard"
          description="Track spaces, questions, answers, sources, and activity for the current workspace."
        />
        <ErrorState
          title="Unable to load the QnA dashboard"
          error={error}
          retry={() => {
            void spacesQuery.refetch();
            void questionsQuery.refetch();
            void pendingQuestionsQuery.refetch();
            void answersQuery.refetch();
            void publishedAnswersQuery.refetch();
            void validatedAnswersQuery.refetch();
            void sourcesQuery.refetch();
            void activityQuery.refetch();
          }}
        />
      </PageSurface>
    );
  }

  const recentSpaces = spacesQuery.data?.items ?? [];
  const recentQuestions = questionsQuery.data?.items ?? [];
  const recentAnswers = answersQuery.data?.items ?? [];
  const recentSources = sourcesQuery.data?.items ?? [];
  const recentActivity = activityQuery.data?.items ?? [];
  const workspaceReady = Boolean(workspace);
  const clientKeyReady = Boolean(clientKeyQuery.data);
  const spaceCount = spacesQuery.data?.totalCount ?? 0;
  const questionCount = questionsQuery.data?.totalCount ?? 0;
  const answerCount = answersQuery.data?.totalCount ?? 0;
  const pendingQuestionCount = pendingQuestionsQuery.data?.totalCount ?? 0;
  const publishedAnswerCount = publishedAnswersQuery.data?.totalCount ?? 0;
  const validatedAnswerCount = validatedAnswersQuery.data?.totalCount ?? 0;
  const sourceCount = sourcesQuery.data?.totalCount ?? 0;
  const activityCount = activityQuery.data?.totalCount ?? 0;
  const acceptedQuestionCount = recentQuestions.filter((question) =>
    Boolean(question.acceptedAnswerId),
  ).length;
  const duplicateQuestionCount = recentQuestions.filter((question) =>
    Boolean(question.duplicateOfQuestionId),
  ).length;
  const reviewPressurePercent = questionCount
    ? Math.round((pendingQuestionCount / questionCount) * 100)
    : 0;
  const trustedAnswerPercent = answerCount
    ? Math.round(
        ((publishedAnswerCount + validatedAnswerCount) / answerCount) * 100,
      )
    : 0;
  const readinessProgress =
    [
      workspaceReady,
      clientKeyReady,
      spaceCount > 0,
      validatedAnswerCount > 0,
    ].filter(Boolean).length * 25;
  const readinessComplete =
    workspaceReady &&
    clientKeyReady &&
    spaceCount > 0 &&
    validatedAnswerCount > 0;
  const readinessActionTarget = !workspaceReady
    ? "/app/settings/tenant"
    : !clientKeyReady
      ? "/app/settings/tenant"
      : spaceCount === 0
        ? "/app/spaces/new"
        : validatedAnswerCount === 0
          ? "/app/answers"
          : "/app/activity";
  const qnaFlowData = [
    {
      stage: "Spaces",
      value: spaceCount,
      fill: "var(--primary)",
    },
    {
      stage: "Questions",
      value: questionCount,
      fill: "var(--color-blue-500)",
    },
    {
      stage: "Answers",
      value: answerCount,
      fill: "var(--color-cyan-500)",
    },
    {
      stage: "Sources",
      value: sourceCount,
      fill: "var(--color-amber-500)",
    },
    {
      stage: "Activity",
      value: activityCount,
      fill: "var(--color-violet-500)",
    },
  ];
  const governanceFunnelData = [
    { stage: "Intake", value: questionCount },
    { stage: "Review", value: pendingQuestionCount },
    { stage: "Published", value: publishedAnswerCount },
    { stage: "Validated", value: validatedAnswerCount },
  ];
  const signalMixData = [
    {
      name: "Accepted",
      value: acceptedQuestionCount,
      fill: "var(--primary)",
    },
    {
      name: "Duplicates",
      value: duplicateQuestionCount,
      fill: "var(--color-amber-500)",
    },
    {
      name: "Open sample",
      value: Math.max(
        recentQuestions.length - acceptedQuestionCount - duplicateQuestionCount,
        0,
      ),
      fill: "var(--color-blue-500)",
    },
  ];
  const valueTrackerItems = [
    {
      label: "Readiness",
      value: `${readinessProgress}%`,
      description:
        "Workspace, client key, spaces, and validated answers aligned",
      tone: "bg-emerald-500",
    },
    {
      label: "Review pressure",
      value: `${reviewPressurePercent}%`,
      description: "Share of all questions currently waiting for review",
      tone: "bg-amber-500",
    },
    {
      label: "Trusted answers",
      value: `${trustedAnswerPercent}%`,
      description: "Answers already published or validated against the catalog",
      tone: "bg-cyan-500",
    },
    {
      label: "Evidence base",
      value: sourceCount,
      description:
        "Reusable sources available for spaces, questions, and answers",
      tone: "bg-blue-500",
    },
  ];

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="QnA dashboard"
        description="Operate the workspace around spaces, question workflow, answer publication, curated sources, and activity signals."
        actions={
          <>
            <Button asChild variant="outline">
              <Link to="/app/spaces/new">
                <Plus className="size-4" />
                {translateText("New space")}
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/app/questions/new">
                <Plus className="size-4" />
                {translateText("New question")}
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/app/answers/new">
                <Plus className="size-4" />
                {translateText("New answer")}
              </Link>
            </Button>
          </>
        }
      />

      <QnaModuleNav
        activeKey="dashboard"
        intent="This dashboard is the macro view for the QnA module. Use Spaces as the entry point, then follow child records through workflow, trust, and activity."
      />

      <ProgressChecklistCard
        hideWhenComplete={false}
        title="Workspace readiness"
        description="Keep the operational prerequisites aligned before exposing QnA publicly."
        steps={[
          {
            id: "workspace",
            label: "Workspace",
            description: workspaceReady
              ? "Current selection"
              : "Pick a workspace to load tenant-scoped QnA data.",
            complete: workspaceReady,
          },
          {
            id: "client-key",
            label: "Public client key",
            description: clientKeyReady
              ? "Public QnA previews and widgets can authenticate."
              : "Generate a client key before depending on public feedback or vote flows.",
            complete: clientKeyReady,
          },
          {
            id: "spaces",
            label: "Spaces",
            description:
              "Create a space to define operating mode, exposure, and curated source rules.",
            complete: spaceCount > 0,
          },
          {
            id: "validated-answers",
            label: "Validated answers",
            description:
              "Validated answers are the strongest candidates for trusted public visibility.",
            complete: validatedAnswerCount > 0,
          },
        ]}
        action={{
          label: readinessComplete ? "Review activity" : "Start here",
          to: readinessActionTarget,
        }}
      />

      <SectionGrid
        items={[
          {
            title: "Spaces",
            value: spaceCount,
            description: "Configured knowledge spaces for this workspace",
            icon: FolderKanban,
          },
          {
            title: "Questions in review",
            value: pendingQuestionCount,
            description: "Threads waiting for moderation or approval",
            icon: Clock3,
            iconToneClassName:
              "bg-amber-500/10 text-amber-600 ring-amber-500/15 dark:bg-amber-500/15 dark:text-amber-300",
          },
          {
            title: "Published answers",
            value: publishedAnswerCount,
            description: "Answers already live in the operational flow",
            icon: MessageSquareText,
            iconToneClassName:
              "bg-emerald-500/10 text-emerald-600 ring-emerald-500/15 dark:bg-emerald-500/15 dark:text-emerald-300",
          },
          {
            title: "Sources",
            value: sourceCount,
            description: "Reusable evidence, citations, and references",
            icon: Waypoints,
            iconToneClassName:
              "bg-sky-500/10 text-sky-600 ring-sky-500/15 dark:bg-sky-500/15 dark:text-sky-300",
          },
        ]}
      />

      <DashboardValueTracker items={valueTrackerItems} />

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_360px] lg:gap-7.5">
        <DashboardChartCard
          title="QnA operating flow"
          description="Volume by parent and child record type across the current workspace."
        >
          <ChartContainer
            config={{
              value: {
                label: translateText("Records"),
                color: "var(--primary)",
              },
            }}
            className="aspect-auto h-[260px]"
          >
            <BarChart data={qnaFlowData} margin={{ left: 0, right: 8 }}>
              <CartesianGrid vertical={false} strokeDasharray="3 3" />
              <XAxis dataKey="stage" tickLine={false} axisLine={false} />
              <YAxis tickLine={false} axisLine={false} allowDecimals={false} />
              <ChartTooltip content={<ChartTooltipContent hideLabel />} />
              <Bar dataKey="value" radius={[8, 8, 0, 0]}>
                {qnaFlowData.map((entry) => (
                  <Cell key={entry.stage} fill={entry.fill} />
                ))}
              </Bar>
            </BarChart>
          </ChartContainer>
        </DashboardChartCard>

        <DashboardChartCard
          title="Governance funnel"
          description="How much knowledge is still intake, in review, published, or validated."
        >
          <ChartContainer
            config={{
              value: {
                label: translateText("Records"),
                color: "var(--primary)",
              },
            }}
            className="aspect-auto h-[260px]"
          >
            <AreaChart
              data={governanceFunnelData}
              margin={{ left: 0, right: 8 }}
            >
              <defs>
                <linearGradient id="governanceFill" x1="0" x2="0" y1="0" y2="1">
                  <stop
                    offset="5%"
                    stopColor="var(--primary)"
                    stopOpacity={0.35}
                  />
                  <stop
                    offset="95%"
                    stopColor="var(--primary)"
                    stopOpacity={0.02}
                  />
                </linearGradient>
              </defs>
              <CartesianGrid vertical={false} strokeDasharray="3 3" />
              <XAxis dataKey="stage" tickLine={false} axisLine={false} />
              <YAxis tickLine={false} axisLine={false} allowDecimals={false} />
              <ChartTooltip content={<ChartTooltipContent hideLabel />} />
              <Area
                dataKey="value"
                type="monotone"
                stroke="var(--primary)"
                strokeWidth={2}
                fill="url(#governanceFill)"
              />
            </AreaChart>
          </ChartContainer>
        </DashboardChartCard>

        <DashboardChartCard
          title="Resolution signal mix"
          description="Accepted, duplicate, and open states in the latest question sample."
        >
          {signalMixData.some((item) => item.value > 0) ? (
            <ChartContainer
              config={{
                value: {
                  label: translateText("Questions"),
                  color: "var(--primary)",
                },
              }}
              className="aspect-auto h-[260px]"
            >
              <PieChart>
                <ChartTooltip content={<ChartTooltipContent hideLabel />} />
                <Pie
                  data={signalMixData}
                  dataKey="value"
                  nameKey="name"
                  innerRadius={54}
                  outerRadius={92}
                  paddingAngle={3}
                >
                  {signalMixData.map((entry) => (
                    <Cell key={entry.name} fill={entry.fill} />
                  ))}
                </Pie>
              </PieChart>
            </ChartContainer>
          ) : (
            <EmptyState
              title="No resolution signals yet"
              description="Accepted answers, duplicate routing, and open threads will appear once questions accumulate."
            />
          )}
        </DashboardChartCard>
      </div>

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>{translateText("Business module health")}</CardTitle>
            <CardDescription>
              {translateText(
                "BaseFAQ modules connected through the QnA knowledge loop: tenant setup, reusable knowledge, private resolution, public reuse, and trust.",
              )}
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="grid gap-3 md:grid-cols-2 xl:grid-cols-5">
          <ModuleSignalCard
            title="Tenant"
            description="Workspace identity and public client key readiness."
            value={translateText("{value}% ready", {
              value: readinessProgress,
            })}
            icon={UsersRound}
          />
          <ModuleSignalCard
            title="QnA"
            description="Spaces, questions, answers, sources, and activity in one operating loop."
            value={translateText("{count} spaces", { count: spaceCount })}
            icon={FolderKanban}
          />
          <ModuleSignalCard
            title="Direct"
            description="Private unresolved asks that should become reusable answers."
            value={translateText("{count} in review", {
              count: pendingQuestionCount,
            })}
            icon={MessageSquareText}
          />
          <ModuleSignalCard
            title="Broadcast"
            description="Published answers ready for public or community distribution."
            value={translateText("{count} published", {
              count: publishedAnswerCount,
            })}
            icon={RadioTower}
          />
          <ModuleSignalCard
            title="Trust"
            description="Validated answers and curated sources protecting answer quality."
            value={translateText("{count} validated", {
              count: validatedAnswerCount,
            })}
            icon={ShieldCheck}
          />
        </CardContent>
      </Card>

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <DashboardSection
          title="Recent spaces"
          description="Spaces drive operating mode, visibility, and curated source policy."
          action={{ label: "Open spaces", to: "/app/spaces" }}
        >
          {recentSpaces.length ? (
            recentSpaces.map((space) => (
              <SpaceRow key={space.id} space={space} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No spaces yet"
              description="Create a space to define operating mode, exposure, and curated source rules."
              action={{ label: "Create first space", to: "/app/spaces/new" }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Question workflow"
          description="Watch pending review, duplicates, accepted answers, and customer feedback signals."
          action={{ label: "Open questions", to: "/app/questions" }}
        >
          {recentQuestions.length ? (
            recentQuestions.map((question) => (
              <QuestionRow
                key={question.id}
                question={question}
                timeZone={timeZone}
              />
            ))
          ) : (
            <EmptyState
              title="No questions yet"
              description="Create the first question thread so answers, votes, and activity can accumulate around it."
              action={{
                label: "Create first question",
                to: "/app/questions/new",
              }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Answer publication"
          description="Track ranking, confidence, accepted answers, and answer lifecycle."
          action={{ label: "Open answers", to: "/app/answers" }}
        >
          {recentAnswers.length ? (
            recentAnswers.map((answer) => (
              <AnswerRow key={answer.id} answer={answer} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No answers yet"
              description="Add an answer candidate so the question workflow can move toward publication and validation."
              action={{ label: "Create first answer", to: "/app/answers/new" }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Curated sources"
          description="Sources feed evidence, citations, and public trust signals across spaces, questions, and answers."
          action={{ label: "Open sources", to: "/app/sources" }}
        >
          {recentSources.length ? (
            recentSources.map((source) => (
              <SourceRow key={source.id} source={source} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No sources yet"
              description="Register a reusable source before attaching evidence or citations to QnA records."
              action={{ label: "Create first source", to: "/app/sources/new" }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Latest activity"
          description="This feed reflects workflow operations, public feedback, votes, and audit events."
          action={{ label: "Open activity", to: "/app/activity" }}
        >
          {recentActivity.length ? (
            recentActivity.map((entry) => (
              <ActivityRow key={entry.id} entry={entry} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No activity yet"
              description="Once questions, answers, feedback, or votes start flowing, the activity timeline will appear here."
            />
          )}
        </DashboardSection>
      </div>

      <Card className="border-dashed bg-muted/20">
        <CardContent className="flex flex-col gap-4 p-5 lg:flex-row lg:items-center lg:justify-between">
          <div className="space-y-1">
            <p className="text-sm font-semibold text-mono">
              {translateText("Operate QnA end to end")}
            </p>
            <p className="text-sm text-muted-foreground">
              {translateText(
                "Spaces define operating boundaries, questions drive workflow, answers hold publication state, sources ground evidence, and activity captures moderation plus public signals.",
              )}
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <Button asChild variant="outline">
              <Link to="/app/tags">
                <ShieldCheck className="size-4" />
                {translateText("Manage tags")}
              </Link>
            </Button>
            <Button asChild>
              <Link to="/app/activity">
                <Activity className="size-4" />
                {translateText("Review activity")}
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </PageSurface>
  );
}
