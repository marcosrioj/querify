import {
  Activity,
  ArrowRight,
  Clock3,
  FolderKanban,
  MessageSquareText,
  Plus,
  ShieldCheck,
  Waypoints,
  type LucideIcon,
} from "lucide-react";
import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { useActivityList } from "@/domains/activity/hooks";
import type { ActivityDto } from "@/domains/activity/types";
import { useAnswerList } from "@/domains/answers/hooks";
import type { AnswerDto } from "@/domains/answers/types";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { useQuestionList } from "@/domains/questions/hooks";
import type { QuestionDto } from "@/domains/questions/types";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useSourceList } from "@/domains/sources/hooks";
import { useSpaceList } from "@/domains/spaces/hooks";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  useCurrentWorkspace,
  useTenantWorkspace,
} from "@/domains/tenants/hooks";
import {
  AnswerStatus,
  QuestionStatus,
  VisibilityScope,
} from "@/shared/constants/backend-enums";
import { PageHeader, PageSurface } from "@/shared/layout/page-layouts";
import { formatNumericDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { translateText } from "@/shared/lib/i18n-core";
import {
  ActivityKindBadge,
  ActorKindBadge,
  AnswerStatusBadge,
  QuestionStatusBadge,
  SpaceKindBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import {
  Badge,
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
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";

type DashboardActionCardProps = {
  title: string;
  value: ReactNode;
  description: string;
  actionLabel: string;
  to: string;
  icon: LucideIcon;
  tone: "emerald" | "amber" | "sky" | "rose";
};

const toneClasses: Record<
  DashboardActionCardProps["tone"],
  {
    card: string;
    icon: string;
    line: string;
  }
> = {
  emerald: {
    card: "border-emerald-500/20 bg-linear-to-br from-background via-background to-emerald-500/[0.08]",
    icon: "bg-emerald-500/10 text-emerald-600 ring-emerald-500/20 dark:text-emerald-300",
    line: "bg-emerald-500",
  },
  amber: {
    card: "border-amber-500/20 bg-linear-to-br from-background via-background to-amber-500/[0.08]",
    icon: "bg-amber-500/10 text-amber-600 ring-amber-500/20 dark:text-amber-300",
    line: "bg-amber-500",
  },
  sky: {
    card: "border-sky-500/20 bg-linear-to-br from-background via-background to-sky-500/[0.08]",
    icon: "bg-sky-500/10 text-sky-600 ring-sky-500/20 dark:text-sky-300",
    line: "bg-sky-500",
  },
  rose: {
    card: "border-rose-500/20 bg-linear-to-br from-background via-background to-rose-500/[0.08]",
    icon: "bg-rose-500/10 text-rose-600 ring-rose-500/20 dark:text-rose-300",
    line: "bg-rose-500",
  },
};

function DashboardActionCard({
  title,
  value,
  description,
  actionLabel,
  to,
  icon: Icon,
  tone,
}: DashboardActionCardProps) {
  const classes = toneClasses[tone];

  return (
    <Card className={classes.card}>
      <CardContent className="relative flex min-h-48 flex-col justify-between gap-5 p-5">
        <span
          aria-hidden="true"
          className={`absolute inset-x-5 top-0 h-1 rounded-b-full ${classes.line}`}
        />
        <div className="flex items-start justify-between gap-4 pt-2">
          <div className="min-w-0 space-y-2">
            <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
              {translateText(title)}
            </p>
            <div className="text-3xl font-semibold text-mono">{value}</div>
          </div>
          <div
            className={`flex size-10 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset ${classes.icon}`}
          >
            <Icon className="size-5" />
          </div>
        </div>
        <div className="space-y-4">
          <p className="text-sm leading-6 text-muted-foreground">
            {translateText(description)}
          </p>
          <Button asChild variant="outline" size="sm">
            <Link to={to}>
              {translateText(actionLabel)}
              <ArrowRight className="size-4" />
            </Link>
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

function DashboardPanel({
  title,
  description,
  children,
}: {
  title: string;
  description: string;
  children: ReactNode;
}) {
  return (
    <Card className="min-h-full">
      <CardHeader>
        <CardHeading>
          <CardTitle>{translateText(title)}</CardTitle>
          <CardDescription>{translateText(description)}</CardDescription>
        </CardHeading>
      </CardHeader>
      <CardContent className="space-y-3">{children}</CardContent>
    </Card>
  );
}

function SpaceAttentionRow({ space }: { space: SpaceDto }) {
  const blocksWork = !space.acceptsQuestions && !space.acceptsAnswers;
  const needsValidation = !space.lastValidatedAtUtc;

  return (
    <div className="rounded-lg border border-border/70 bg-background/75 p-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
          <Link
            to={`/app/spaces/${space.id}`}
            className="font-medium text-mono hover:text-primary"
          >
            {space.name}
          </Link>
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {space.summary || translateText("No summary recorded.")}
          </p>
          <div className="flex flex-wrap gap-2">
            <SpaceKindBadge kind={space.kind} />
            <VisibilityBadge visibility={space.visibility} />
            <Badge
              variant={space.acceptsQuestions ? "success" : "mono"}
              appearance="outline"
            >
              {translateText(
                space.acceptsQuestions
                  ? "Questions enabled"
                  : "Questions disabled",
              )}
            </Badge>
            <Badge
              variant={space.acceptsAnswers ? "success" : "mono"}
              appearance="outline"
            >
              {translateText(
                space.acceptsAnswers ? "Answers enabled" : "Answers disabled",
              )}
            </Badge>
          </div>
        </div>
        <div className="flex shrink-0 flex-col gap-2 sm:items-end">
          <Badge
            variant={blocksWork ? "destructive" : needsValidation ? "warning" : "success"}
          >
            {translateText(
              blocksWork
                ? "Intake closed"
                : needsValidation
                  ? "Needs validation"
                  : "Ready",
            )}
          </Badge>
          <Button asChild size="sm">
            <Link to={`/app/spaces/${space.id}`}>
              {translateText("Open space")}
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}

function QuestionAttentionRow({
  question,
  timeZone,
}: {
  question: QuestionDto;
  timeZone: string;
}) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/75 p-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
          <Link
            to={`/app/questions/${question.id}`}
            className="font-medium text-mono hover:text-primary"
          >
            {question.title}
          </Link>
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {question.summary || translateText("No summary provided.")}
          </p>
          <div className="flex flex-wrap gap-2">
            <QuestionStatusBadge status={question.status} />
            <VisibilityBadge visibility={question.visibility} />
            {question.duplicateOfQuestionId ? (
              <Badge variant="mono" appearance="outline">
                {translateText("Duplicate")}
              </Badge>
            ) : null}
          </div>
        </div>
        <div className="flex shrink-0 flex-col gap-2 sm:items-end">
          <p className="text-xs text-muted-foreground">
            {formatNumericDateTimeInTimeZone(
              question.lastActivityAtUtc,
              timeZone,
            )}
          </p>
          <Button asChild size="sm">
            <Link to={`/app/questions/${question.id}`}>
              {translateText("Resolve thread")}
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}

function AnswerAttentionRow({ answer }: { answer: AnswerDto }) {
  return (
    <div className="rounded-lg border border-border/70 bg-background/75 p-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
          <Link
            to={`/app/answers/${answer.id}`}
            className="font-medium text-mono hover:text-primary"
          >
            {answer.headline}
          </Link>
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {answer.body || translateText("No answer body recorded.")}
          </p>
          <div className="flex flex-wrap gap-2">
            <AnswerStatusBadge status={answer.status} />
            <VisibilityBadge visibility={answer.visibility} />
            {answer.isAccepted ? (
              <Badge variant="success">{translateText("Accepted")}</Badge>
            ) : null}
            <Badge variant="outline">
              {translateText("{count} sources", {
                count: answer.sources.length,
              })}
            </Badge>
          </div>
        </div>
        <Button asChild size="sm" className="shrink-0">
          <Link to={`/app/answers/${answer.id}`}>
            {translateText("Validate answer")}
          </Link>
        </Button>
      </div>
    </div>
  );
}

function ActivitySignalRow({
  entry,
  timeZone,
}: {
  entry: ActivityDto;
  timeZone: string;
}) {
  const target = entry.answerId
    ? `/app/answers/${entry.answerId}`
    : `/app/questions/${entry.questionId}`;

  return (
    <div className="rounded-lg border border-border/70 bg-background/75 p-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap gap-2">
            <ActivityKindBadge kind={entry.kind} />
            <ActorKindBadge kind={entry.actorKind} />
          </div>
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {entry.notes || entry.actorLabel || entry.userPrint}
          </p>
          <p className="text-xs text-muted-foreground">
            {formatNumericDateTimeInTimeZone(entry.occurredAtUtc, timeZone)}
          </p>
        </div>
        <Button asChild variant="outline" size="sm" className="shrink-0">
          <Link to={target}>{translateText("Open context")}</Link>
        </Button>
      </div>
    </div>
  );
}

function DashboardLoadingState() {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Dashboard"
        description="QnA overview focused on operational action."
      />
      <Card className="border-emerald-500/20 bg-linear-to-br from-background via-background to-emerald-500/[0.06]">
        <CardContent className="space-y-5 p-5 lg:p-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
            <div className="space-y-3">
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-6 w-56" />
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
      <div className="grid gap-5 xl:grid-cols-3 lg:gap-7.5">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton
            key={`dashboard-panel-skeleton-${index}`}
            className="h-80 rounded-lg"
          />
        ))}
      </div>
    </PageSurface>
  );
}

export function DashboardPage() {
  const timeZone = usePortalTimeZone();
  const workspace = useCurrentWorkspace();
  const { clientKeyQuery } = useTenantWorkspace();

  const spacesQuery = useSpaceList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });
  const pendingQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.PendingReview,
  });
  const recentQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: "LastActivityAtUtc DESC",
  });
  const unvalidatedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 5,
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
    pageSize: 1,
    sorting: "LastVerifiedAtUtc DESC",
  });
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 6,
    sorting: "OccurredAtUtc DESC",
  });

  const isInitialDashboardLoading =
    spacesQuery.isLoading ||
    pendingQuestionsQuery.isLoading ||
    recentQuestionsQuery.isLoading ||
    unvalidatedAnswersQuery.isLoading ||
    validatedAnswersQuery.isLoading ||
    sourcesQuery.isLoading ||
    activityQuery.isLoading ||
    clientKeyQuery.isLoading;
  const hasCriticalError =
    spacesQuery.isError ||
    pendingQuestionsQuery.isError ||
    recentQuestionsQuery.isError ||
    unvalidatedAnswersQuery.isError ||
    validatedAnswersQuery.isError ||
    sourcesQuery.isError ||
    activityQuery.isError;

  if (isInitialDashboardLoading) {
    return <DashboardLoadingState />;
  }

  if (hasCriticalError) {
    const error =
      spacesQuery.error ??
      pendingQuestionsQuery.error ??
      recentQuestionsQuery.error ??
      unvalidatedAnswersQuery.error ??
      validatedAnswersQuery.error ??
      sourcesQuery.error ??
      activityQuery.error;

    return (
      <PageSurface>
        <PageHeader
          title="Dashboard"
          description="QnA overview focused on operational action."
        />
        <ErrorState
          title="Unable to load the QnA dashboard"
          error={error}
          retry={() => {
            void spacesQuery.refetch();
            void pendingQuestionsQuery.refetch();
            void recentQuestionsQuery.refetch();
            void unvalidatedAnswersQuery.refetch();
            void validatedAnswersQuery.refetch();
            void sourcesQuery.refetch();
            void activityQuery.refetch();
          }}
        />
      </PageSurface>
    );
  }

  const spaces = spacesQuery.data?.items ?? [];
  const pendingQuestions = pendingQuestionsQuery.data?.items ?? [];
  const recentQuestions = recentQuestionsQuery.data?.items ?? [];
  const unvalidatedAnswers = unvalidatedAnswersQuery.data?.items ?? [];
  const recentActivity = activityQuery.data?.items ?? [];
  const workspaceReady = Boolean(workspace);
  const clientKeyReady = Boolean(clientKeyQuery.data);
  const spaceCount = spacesQuery.data?.totalCount ?? 0;
  const activeSpaceCount = spaces.filter(
    (space) => space.acceptsQuestions || space.acceptsAnswers,
  ).length;
  const intakeClosedCount = spaces.filter(
    (space) => !space.acceptsQuestions && !space.acceptsAnswers,
  ).length;
  const publicSpaceCount = spaces.filter(
    (space) => space.visibility >= VisibilityScope.Public,
  ).length;
  const pendingQuestionCount = pendingQuestionsQuery.data?.totalCount ?? 0;
  const unvalidatedAnswerCount =
    unvalidatedAnswersQuery.data?.totalCount ?? 0;
  const validatedAnswerCount = validatedAnswersQuery.data?.totalCount ?? 0;
  const sourceCount = sourcesQuery.data?.totalCount ?? 0;
  const signalTarget = recentActivity[0]
    ? recentActivity[0].answerId
      ? `/app/answers/${recentActivity[0].answerId}`
      : `/app/questions/${recentActivity[0].questionId}`
    : "/app/spaces";
  const readinessActionTarget =
    spaceCount === 0
      ? "/app/spaces/new"
      : pendingQuestionCount > 0
        ? `/app/questions?status=${QuestionStatus.PendingReview}`
        : unvalidatedAnswerCount > 0
          ? `/app/answers?status=${AnswerStatus.Published}`
          : sourceCount === 0
            ? "/app/sources/new"
            : "/app/spaces";
  const riskItems = [
    {
      title: "Questions waiting for decision",
      value: pendingQuestionCount,
      description:
        "Moderation backlog blocks reusable answers and public confidence.",
      to: `/app/questions?status=${QuestionStatus.PendingReview}`,
      action: "Review threads",
      tone: pendingQuestionCount > 0 ? "warning" : "success",
    },
    {
      title: "Published answers not validated",
      value: unvalidatedAnswerCount,
      description:
        "Published answers need validation before they become trusted knowledge.",
      to: `/app/answers?status=${AnswerStatus.Published}`,
      action: "Validate answers",
      tone: unvalidatedAnswerCount > 0 ? "warning" : "success",
    },
    {
      title: "Spaces with intake closed",
      value: intakeClosedCount,
      description:
        "Closed intake can be intentional, but it should be visible before operators route work.",
      to: "/app/spaces?acceptsQuestions=false&acceptsAnswers=false",
      action: "Review spaces",
      tone: intakeClosedCount > 0 ? "warning" : "success",
    },
    {
      title: "Evidence catalog",
      value: sourceCount,
      description:
        "Answers need reusable sources before citation and audit can scale.",
      to: sourceCount > 0 ? "/app/sources" : "/app/sources/new",
      action: sourceCount > 0 ? "Open sources" : "Create source",
      tone: sourceCount > 0 ? "success" : "warning",
    },
  ] as const;

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Dashboard"
        description="QnA overview: active Spaces, pending decisions, unvalidated answers, recent signals, and operational risk."
        actions={
          <>
            <Button asChild>
              <Link to="/app/spaces">
                <FolderKanban className="size-4" />
                {translateText("Start with Spaces")}
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/app/spaces/new">
                <Plus className="size-4" />
                {translateText("New space")}
              </Link>
            </Button>
          </>
        }
      />

      <QnaModuleNav
        activeKey="dashboard"
        intent="This is the QnA macro view. It only shows what helps an operator decide where to act next."
      />

      <ProgressChecklistCard
        hideWhenComplete={false}
        eyebrow="Start here"
        title="Workspace path to trusted answers"
        description="Progress is measured by business readiness: context exists, intake works, questions are handled, answers are validated, and sources can be cited."
        steps={[
          {
            id: "workspace",
            label: "Workspace selected",
            description: workspaceReady
              ? "Current workspace context is available."
              : "Pick a workspace before operating QnA records.",
            complete: workspaceReady,
          },
          {
            id: "client-key",
            label: "Public client key",
            description: clientKeyReady
              ? "Public feedback and widget flows can authenticate."
              : "Generate a client key before relying on public QnA signals.",
            complete: clientKeyReady,
          },
          {
            id: "spaces",
            label: "Active Spaces",
            description:
              "Spaces define where questions, answers, tags, sources, and activity belong.",
            complete: activeSpaceCount > 0,
          },
          {
            id: "trusted-answers",
            label: "Validated answers",
            description:
              "Validated answers are ready to be reused as trusted knowledge.",
            complete: validatedAnswerCount > 0,
          },
        ]}
        action={{
          label:
            spaceCount === 0
              ? "Start here"
              : pendingQuestionCount > 0
                ? "Review queue"
                : "Open spaces",
          to: readinessActionTarget,
        }}
        secondaryAction={{ label: "Manage sources", to: "/app/sources" }}
      />

      <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
        <DashboardActionCard
          title="Active Spaces"
          value={activeSpaceCount}
          description={`${publicSpaceCount} public or indexed. Open a Space to see rules, questions, sources, tags, and next action.`}
          actionLabel="Open spaces"
          to="/app/spaces"
          icon={FolderKanban}
          tone="emerald"
        />
        <DashboardActionCard
          title="Pending questions"
          value={pendingQuestionCount}
          description="These threads need moderation, approval, duplicate routing, or an accepted answer decision."
          actionLabel="Review by Space"
          to={`/app/questions?status=${QuestionStatus.PendingReview}`}
          icon={Clock3}
          tone={pendingQuestionCount > 0 ? "amber" : "emerald"}
        />
        <DashboardActionCard
          title="Answers without validation"
          value={unvalidatedAnswerCount}
          description="Published answers are useful, but validation turns them into trusted reusable knowledge."
          actionLabel="Validate answers"
          to={`/app/answers?status=${AnswerStatus.Published}`}
          icon={MessageSquareText}
          tone={unvalidatedAnswerCount > 0 ? "rose" : "emerald"}
        />
        <DashboardActionCard
          title="Recent signals"
          value={recentActivity.length}
          description="Feedback, votes, moderation, and workflow events. Open the latest signal in its own Question or Answer context."
          actionLabel="Open latest context"
          to={signalTarget}
          icon={Activity}
          tone="sky"
        />
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)] lg:gap-7.5">
        <DashboardPanel
          title="Spaces needing attention"
          description="Open the Space first. It is the operating boundary for questions, answers, sources, tags, and activity."
        >
          {spaces.length ? (
            spaces
              .slice()
              .sort((first, second) => {
                const firstScore =
                  (!first.acceptsQuestions && !first.acceptsAnswers ? 2 : 0) +
                  (!first.lastValidatedAtUtc ? 1 : 0);
                const secondScore =
                  (!second.acceptsQuestions && !second.acceptsAnswers ? 2 : 0) +
                  (!second.lastValidatedAtUtc ? 1 : 0);

                return secondScore - firstScore;
              })
              .slice(0, 4)
              .map((space) => (
                <SpaceAttentionRow key={space.id} space={space} />
              ))
          ) : (
            <EmptyState
              title="No Spaces yet"
              description="Create a Space before questions, answers, tags, sources, or activity can be operated safely."
              action={{ label: "Create Space", to: "/app/spaces/new" }}
            />
          )}
        </DashboardPanel>

        <DashboardPanel
          title="Questions needing action"
          description="Pending review comes first; recent unresolved threads remain visible as secondary context."
        >
          {(pendingQuestions.length ? pendingQuestions : recentQuestions).length ? (
            (pendingQuestions.length ? pendingQuestions : recentQuestions).map(
              (question) => (
                <QuestionAttentionRow
                  key={question.id}
                  question={question}
                  timeZone={timeZone}
                />
              ),
            )
          ) : (
            <EmptyState
              title="No question action needed"
              description="When a Space receives questions, pending review and unresolved threads will appear here."
              action={{ label: "Open spaces", to: "/app/spaces" }}
            />
          )}
        </DashboardPanel>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px] lg:gap-7.5">
        <DashboardPanel
          title="Answers waiting for validation"
          description="An Answer becomes reliable only when publication, validation, quality, and sources line up."
        >
          {unvalidatedAnswers.length ? (
            unvalidatedAnswers.map((answer) => (
              <AnswerAttentionRow key={answer.id} answer={answer} />
            ))
          ) : (
            <EmptyState
              title="No validation backlog"
              description="Published answers that still need validation will appear here."
            />
          )}
        </DashboardPanel>

        <DashboardPanel
          title="Operational risks"
          description="Only risks that can change an operator decision are shown."
        >
          <div className="space-y-3">
            {riskItems.map((risk) => (
              <div
                key={risk.title}
                className="rounded-lg border border-border/70 bg-background/75 p-4"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0 space-y-1">
                    <p className="text-sm font-medium text-mono">
                      {translateText(risk.title)}
                    </p>
                    <p className="text-sm leading-6 text-muted-foreground">
                      {translateText(risk.description)}
                    </p>
                  </div>
                  <Badge
                    variant={risk.tone === "warning" ? "warning" : "success"}
                  >
                    {risk.value}
                  </Badge>
                </div>
                <Button asChild variant="ghost" size="sm" className="mt-3 px-0">
                  <Link to={risk.to}>
                    {translateText(risk.action)}
                    <ArrowRight className="size-4" />
                  </Link>
                </Button>
              </div>
            ))}
          </div>
        </DashboardPanel>
      </div>

      <DashboardPanel
        title="Recent signals"
        description="Macro feed only. Every item sends the operator back to the Question or Answer that owns the event."
      >
        {recentActivity.length ? (
          <div className="grid gap-3 lg:grid-cols-2">
            {recentActivity.map((entry) => (
              <ActivitySignalRow
                key={entry.id}
                entry={entry}
                timeZone={timeZone}
              />
            ))}
          </div>
        ) : (
          <EmptyState
            title="No recent signals"
            description="Feedback, votes, reports, moderation, and workflow transitions will appear here once operators start working."
          />
        )}
      </DashboardPanel>

      <Card className="border-dashed bg-muted/20">
        <CardContent className="flex flex-col gap-4 p-5 lg:flex-row lg:items-center lg:justify-between">
          <div className="space-y-1">
            <p className="text-sm font-semibold text-mono">
              {translateText("Business rule")}
            </p>
            <p className="text-sm text-muted-foreground">
              {translateText(
                "Questions are created from Spaces. Answers are created from Questions. Activity is audit context, not a standalone work queue.",
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
              <Link to="/app/sources">
                <Waypoints className="size-4" />
                {translateText("Manage sources")}
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </PageSurface>
  );
}
