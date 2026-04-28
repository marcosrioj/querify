import {
  Activity,
  ArrowRight,
  Clock3,
  FileCheck2,
  FolderKanban,
  ShieldCheck,
  Sparkles,
  Users,
  Waypoints,
} from "lucide-react";
import type { ComponentType, ReactNode } from "react";
import { Link } from "react-router-dom";
import {
  getActivationState,
  getActivityMixData,
  getAnswerTrustFunnelData,
  getBillingNeedsAttention,
  getBusinessReadout,
  getDashboardKpis,
  getEvidenceReadinessData,
  getQuestionLifecycleData,
  getRoleAwareNextAction,
  getSetupProgress,
  getSourceUtilizationData,
  getSpaceWorkloadData,
} from "@/domains/dashboard/dashboard-selectors";
import { useActivityList } from "@/domains/activity/hooks";
import type { ActivityDto } from "@/domains/activity/types";
import { useAnswerList } from "@/domains/answers/hooks";
import { useBillingWorkspace } from "@/domains/billing/hooks";
import { useTenantMembers } from "@/domains/members/hooks";
import { useQuestionList } from "@/domains/questions/hooks";
import type { QuestionDto } from "@/domains/questions/types";
import {
  usePortalTimeZone,
  useUserProfile,
} from "@/domains/settings/settings-hooks";
import { useSourceList } from "@/domains/sources/hooks";
import { useSpaceList } from "@/domains/spaces/hooks";
import {
  AnswerStatus,
  QuestionStatus,
  TenantSubscriptionStatus,
  VisibilityScope,
} from "@/shared/constants/backend-enums";
import {
  billingInvoiceStatusPresentation,
  tenantSubscriptionStatusPresentation,
} from "@/shared/constants/enum-ui";
import { PageHeader, PageSurface } from "@/shared/layout/page-layouts";
import { formatNumericDateTimeInTimeZone } from "@/shared/lib/time-zone";
import { translateText } from "@/shared/lib/i18n-core";
import {
  ActivityKindBadge,
  ActorKindBadge,
  AnswerStatusBadge,
  QuestionStatusBadge,
  VisibilityBadge,
} from "@/shared/ui/status-badges";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ContextHint,
  ProgressChecklistCard,
  Skeleton,
} from "@/shared/ui";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";

function DashboardLoadingState() {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-[var(--shadow-premium-card)]">
        <div className="grid gap-5 lg:grid-cols-[minmax(0,1fr)_280px]">
          <div className="space-y-3">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-9 w-72" />
            <Skeleton className="h-4 w-full max-w-2xl" />
            <Skeleton className="h-4 w-full max-w-xl" />
          </div>
          <Skeleton className="h-28 rounded-xl" />
        </div>
      </div>
      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        {Array.from({ length: 6 }).map((_, index) => (
          <Skeleton key={index} className="h-36 rounded-xl" />
        ))}
      </div>
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px]">
        <Skeleton className="h-96 rounded-2xl" />
        <Skeleton className="h-96 rounded-2xl" />
      </div>
    </PageSurface>
  );
}

function HomeHero({
  nextAction,
  setupProgress,
}: {
  nextAction: { label: string; description: string; to: string };
  setupProgress: number;
}) {
  const showSetupProgress = setupProgress < 100;

  return (
    <Card className="overflow-hidden border-primary/15 bg-linear-to-br from-card via-card to-emerald-500/[0.07]">
      <CardContent className="relative p-5 lg:p-7">
        <div
          aria-hidden="true"
          className="absolute inset-x-0 top-0 h-1 bg-linear-to-r from-emerald-500 via-sky-500 to-indigo-500"
        />
        <div
          className={
            showSetupProgress
              ? "grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px] lg:items-center"
              : "grid gap-6"
          }
        >
          <div className="min-w-0 space-y-5">
            <div className="inline-flex items-center gap-2 rounded-full border border-primary/20 bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
              <Sparkles className="size-3.5" />
              {translateText("Activation mode")}
            </div>
            <div className="max-w-3xl space-y-3">
              <h2 className="text-3xl font-semibold tracking-normal text-mono lg:text-4xl">
                {translateText("Start with a clean QnA foundation")}
              </h2>
              <p className="text-sm leading-6 text-muted-foreground lg:text-base">
                {translateText(
                  "Create the first space, attach evidence, and publish the first validated answer without exposing the full enterprise dashboard too early.",
                )}
              </p>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <Button asChild size="lg">
                <Link to={nextAction.to}>
                  {translateText(nextAction.label)}
                  <ArrowRight className="size-4" />
                </Link>
              </Button>
              <Button asChild variant="outline" size="lg">
                <Link to="/app/questions">
                  {translateText("Review questions")}
                </Link>
              </Button>
            </div>
          </div>

          {showSetupProgress ? (
            <div className="rounded-2xl border border-border/70 bg-background/80 p-4 shadow-xs">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-muted-foreground">
                {translateText("Setup progress")}
              </p>
              <div className="mt-3 flex items-end justify-between gap-4">
                <div className="text-4xl font-semibold leading-none text-mono">
                  {setupProgress}%
                </div>
                <Badge variant="primary" appearance="outline">
                  {translateText("In progress")}
                </Badge>
              </div>
              <div className="mt-4 h-2 overflow-hidden rounded-full bg-muted">
                <div
                  className="h-full rounded-full bg-primary transition-[width] duration-500"
                  style={{ width: `${setupProgress}%` }}
                />
              </div>
              <p className="mt-4 text-sm leading-6 text-muted-foreground">
                {translateText(nextAction.description)}
              </p>
            </div>
          ) : null}
        </div>
      </CardContent>
    </Card>
  );
}

function Panel({
  title,
  description,
  action,
  children,
}: {
  title: string;
  description: string;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <Card className="min-h-full">
      <CardHeader className="gap-3">
        <CardHeading className="space-y-0">
          <div className="flex min-w-0 flex-wrap items-center gap-2">
            <CardTitle>{translateText(title)}</CardTitle>
            <ContextHint
              content={description}
              label="More information"
              contentClassName="max-w-72"
            />
          </div>
        </CardHeading>
        {action}
      </CardHeader>
      <CardContent className="space-y-3">{children}</CardContent>
    </Card>
  );
}

type DashboardChartRow = {
  key: string;
  label: string;
  value: number;
  fill: string;
  to?: string;
};

function formatChartNumber(value: number) {
  return new Intl.NumberFormat(undefined, {
    notation: value >= 10000 ? "compact" : "standard",
    maximumFractionDigits: 1,
  }).format(value);
}

function EmptyChart({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="flex min-h-60 flex-col items-center justify-center rounded-xl border border-dashed border-border/70 bg-muted/10 p-6 text-center">
      <p className="font-medium text-mono">{translateText(title)}</p>
      <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
        {translateText(description)}
      </p>
    </div>
  );
}

function HorizontalValueChart({
  data,
  emptyDescription,
  emptyTitle,
  valueLabel,
}: {
  data: DashboardChartRow[];
  emptyDescription: string;
  emptyTitle: string;
  valueLabel: string;
}) {
  if (!data.some((entry) => entry.value > 0)) {
    return <EmptyChart title={emptyTitle} description={emptyDescription} />;
  }

  const chartData = data.map((entry) => ({
    ...entry,
    label: translateText(entry.label),
  }));
  const maxValue = Math.max(...chartData.map((entry) => entry.value), 1);

  return (
    <div className="space-y-3" aria-label={translateText(valueLabel)}>
      {chartData.map((entry) => {
        const barWidth =
          entry.value > 0
            ? Math.max(Math.round((entry.value / maxValue) * 100), 2)
            : 0;
        const content = (
          <>
            <div className="flex min-w-0 items-center justify-between gap-3">
              <span className="min-w-0 truncate text-sm font-medium text-mono">
                {entry.label}
              </span>
              <span className="shrink-0 text-sm font-semibold tabular-nums text-mono">
                {formatChartNumber(entry.value)}
              </span>
            </div>
            <div className="mt-2 h-1.5 overflow-hidden rounded-full bg-muted">
              <span
                className="block h-full rounded-full"
                style={{
                  backgroundColor: entry.fill,
                  width: `${barWidth}%`,
                }}
              />
            </div>
          </>
        );

        if (entry.to) {
          return (
            <Link
              key={entry.key}
              to={entry.to}
              className="block rounded-lg px-1.5 py-1 transition-colors hover:bg-muted/40"
            >
              {content}
            </Link>
          );
        }

        return (
          <div key={entry.key} className="rounded-lg px-1.5 py-1">
            {content}
          </div>
        );
      })}
    </div>
  );
}

function CompactBreakdownChart({
  centerLabel,
  centerValue,
  data,
  emptyDescription,
  emptyTitle,
}: {
  centerLabel: string;
  centerValue: string;
  data: DashboardChartRow[];
  emptyDescription: string;
  emptyTitle: string;
}) {
  if (!data.some((entry) => entry.value > 0)) {
    return <EmptyChart title={emptyTitle} description={emptyDescription} />;
  }

  const chartData = data.map((entry) => ({
    ...entry,
    label: translateText(entry.label),
  }));
  const positiveData = chartData.filter((entry) => entry.value > 0);
  const total = positiveData.reduce((sum, entry) => sum + entry.value, 0);

  return (
    <div className="space-y-4">
      <div className="rounded-xl bg-muted/20 p-4">
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
          {translateText(centerLabel)}
        </p>
        <p className="mt-2 text-4xl font-semibold leading-none text-mono">
          {centerValue}
        </p>
        <div className="mt-5 flex h-2 overflow-hidden rounded-full bg-muted">
          {positiveData.map((entry) => (
            <span
              key={entry.key}
              className="h-full"
              style={{
                backgroundColor: entry.fill,
                width: `${Math.max((entry.value / total) * 100, 4)}%`,
              }}
            />
          ))}
        </div>
      </div>
      <ChartLegendRows data={positiveData} />
    </div>
  );
}

function ChartLegendRows({ data }: { data: DashboardChartRow[] }) {
  return (
    <div className="grid gap-2 sm:grid-cols-2">
      {data.map((entry) => (
        <div
          key={entry.key}
          className="flex min-w-0 items-center justify-between gap-3 px-1"
        >
          <div className="flex min-w-0 items-center gap-2">
            <span
              className="size-2 shrink-0 rounded-full"
              style={{ backgroundColor: entry.fill }}
            />
            <span className="truncate text-sm text-muted-foreground">
              {translateText(entry.label)}
            </span>
          </div>
          <span className="shrink-0 text-sm font-semibold tabular-nums text-mono">
            {formatChartNumber(entry.value)}
          </span>
        </div>
      ))}
    </div>
  );
}

const businessReadoutIcons: Record<
  string,
  ComponentType<{ className?: string }>
> = {
  "Moderation pressure": Clock3,
  "Validation backlog": FileCheck2,
  "Evidence readiness": Waypoints,
  "Knowledge concentration": FolderKanban,
  "Trusted answer base": ShieldCheck,
  "Open answer demand": Activity,
};

function BusinessReadout({
  items,
}: {
  items: Array<{ label: string; value: string; detail: string }>;
}) {
  return (
    <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      {items.map((item) => {
        const Icon = businessReadoutIcons[item.label] ?? Activity;

        return (
          <div
            key={item.label}
            className="rounded-xl border border-border/70 bg-background/75 p-4"
          >
            <div className="flex items-start justify-between gap-3">
              <p className="min-w-0 text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                {translateText(item.label)}
              </p>
              <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary ring-1 ring-inset ring-primary/15">
                <Icon className="size-4" />
              </span>
            </div>
            <p className="mt-2 text-3xl font-semibold leading-none text-mono">
              {item.value}
            </p>
            <p className="mt-3 text-sm leading-6 text-muted-foreground">
              {translateText(item.detail)}
            </p>
          </div>
        );
      })}
    </div>
  );
}

function QuestionQueueRow({
  question,
  timeZone,
}: {
  question: QuestionDto;
  timeZone: string;
}) {
  return (
    <Link
      to={`/app/questions/${question.id}`}
      className="group block rounded-xl border border-border/70 bg-background/75 p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <QuestionStatusBadge status={question.status} />
            <VisibilityBadge visibility={question.visibility} />
            {question.acceptedAnswerId ? (
              <Badge variant="success" appearance="outline">
                {translateText("Accepted answer")}
              </Badge>
            ) : null}
          </div>
          <p className="font-medium text-mono group-hover:text-primary">
            {question.title}
          </p>
          <p className="line-clamp-2 text-sm leading-6 text-muted-foreground">
            {question.summary || translateText("No summary provided.")}
          </p>
        </div>
        <div className="shrink-0 text-sm text-muted-foreground">
          {formatNumericDateTimeInTimeZone(
            question.lastActivityAtUtc,
            timeZone,
          )}
        </div>
      </div>
    </Link>
  );
}

function ActivityRow({
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
    <Link
      to={target}
      className="block rounded-xl border border-border/70 bg-background/75 p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
    >
      <div className="flex flex-wrap items-center gap-2">
        <ActivityKindBadge kind={entry.kind} />
        <ActorKindBadge kind={entry.actorKind} />
      </div>
      <p className="mt-2 line-clamp-2 text-sm leading-6 text-muted-foreground">
        {entry.notes || entry.actorLabel || entry.userPrint}
      </p>
      <p className="mt-2 text-xs text-muted-foreground">
        {formatNumericDateTimeInTimeZone(entry.occurredAtUtc, timeZone)}
      </p>
    </Link>
  );
}

function BillingNotice({
  status,
  invoiceStatus,
}: {
  status?: TenantSubscriptionStatus;
  invoiceStatus?: number;
}) {
  if (!status) {
    return null;
  }

  const presentation = tenantSubscriptionStatusPresentation[status];
  const invoicePresentation =
    invoiceStatus !== undefined
      ? billingInvoiceStatusPresentation[invoiceStatus]
      : undefined;

  return (
    <Card className="border-warning/25 bg-warning/5">
      <CardContent className="flex flex-col gap-4 p-5 sm:flex-row sm:items-center sm:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant={presentation.badgeVariant} appearance="outline">
              {translateText(presentation.label)}
            </Badge>
            {invoicePresentation ? (
              <Badge
                variant={invoicePresentation.badgeVariant}
                appearance="outline"
              >
                {translateText(invoicePresentation.label)}
              </Badge>
            ) : null}
          </div>
          <p className="text-sm leading-6 text-muted-foreground">
            {translateText(
              "Billing needs attention so entitlement and workspace access stay predictable.",
            )}
          </p>
        </div>
        <Button asChild variant="outline">
          <Link to="/app/billing">{translateText("Review billing")}</Link>
        </Button>
      </CardContent>
    </Card>
  );
}

export function DashboardPage() {
  const timeZone = usePortalTimeZone();
  const profileQuery = useUserProfile();
  const membersQuery = useTenantMembers();
  const billing = useBillingWorkspace();

  const spacesQuery = useSpaceList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });
  const questionsSummaryQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: "LastActivityAtUtc DESC",
  });
  const pendingQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Draft,
  });
  const openQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Active,
  });
  const draftQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Draft,
  });
  const duplicateQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Duplicate,
  });
  const archivedQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Archived,
  });
  const recentQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: "LastActivityAtUtc DESC",
  });
  const answersSummaryQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: "PublishedAtUtc DESC",
  });
  const publishedAnswersQuery = useAnswerList({
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
  const acceptedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: "PublishedAtUtc DESC",
    isAccepted: true,
  });
  const sourcesQuery = useSourceList({
    page: 1,
    pageSize: 100,
    sorting: "LastVerifiedAtUtc DESC",
  });
  const publicSourcesQuery = useSourceList({
    page: 1,
    pageSize: 1,
    sorting: "LastVerifiedAtUtc DESC",
    visibility: VisibilityScope.Public,
  });
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 30,
    sorting: "OccurredAtUtc DESC",
  });

  const isInitialDashboardLoading =
    spacesQuery.isLoading ||
    questionsSummaryQuery.isLoading ||
    pendingQuestionsQuery.isLoading ||
    openQuestionsQuery.isLoading ||
    draftQuestionsQuery.isLoading ||
    duplicateQuestionsQuery.isLoading ||
    archivedQuestionsQuery.isLoading ||
    recentQuestionsQuery.isLoading ||
    answersSummaryQuery.isLoading ||
    publishedAnswersQuery.isLoading ||
    validatedAnswersQuery.isLoading ||
    acceptedAnswersQuery.isLoading ||
    sourcesQuery.isLoading ||
    publicSourcesQuery.isLoading ||
    activityQuery.isLoading ||
    membersQuery.isLoading ||
    profileQuery.isLoading ||
    billing.summaryQuery.isLoading;

  const hasCriticalError =
    spacesQuery.isError ||
    questionsSummaryQuery.isError ||
    pendingQuestionsQuery.isError ||
    openQuestionsQuery.isError ||
    draftQuestionsQuery.isError ||
    duplicateQuestionsQuery.isError ||
    archivedQuestionsQuery.isError ||
    recentQuestionsQuery.isError ||
    answersSummaryQuery.isError ||
    publishedAnswersQuery.isError ||
    validatedAnswersQuery.isError ||
    acceptedAnswersQuery.isError ||
    sourcesQuery.isError ||
    publicSourcesQuery.isError ||
    activityQuery.isError;

  if (isInitialDashboardLoading) {
    return <DashboardLoadingState />;
  }

  if (hasCriticalError) {
    const error =
      spacesQuery.error ??
      questionsSummaryQuery.error ??
      pendingQuestionsQuery.error ??
      openQuestionsQuery.error ??
      draftQuestionsQuery.error ??
      duplicateQuestionsQuery.error ??
      archivedQuestionsQuery.error ??
      recentQuestionsQuery.error ??
      answersSummaryQuery.error ??
      publishedAnswersQuery.error ??
      validatedAnswersQuery.error ??
      acceptedAnswersQuery.error ??
      sourcesQuery.error ??
      publicSourcesQuery.error ??
      activityQuery.error;

    return (
      <PageSurface>
        <PageHeader
          title="Home"
          description="Attention, activation, value proof, and recent workflow signals."
        />
        <ErrorState
          title="Unable to load Home"
          error={error}
          retry={() => {
            void spacesQuery.refetch();
            void questionsSummaryQuery.refetch();
            void pendingQuestionsQuery.refetch();
            void openQuestionsQuery.refetch();
            void draftQuestionsQuery.refetch();
            void duplicateQuestionsQuery.refetch();
            void archivedQuestionsQuery.refetch();
            void recentQuestionsQuery.refetch();
            void answersSummaryQuery.refetch();
            void publishedAnswersQuery.refetch();
            void validatedAnswersQuery.refetch();
            void acceptedAnswersQuery.refetch();
            void sourcesQuery.refetch();
            void publicSourcesQuery.refetch();
            void activityQuery.refetch();
          }}
        />
      </PageSurface>
    );
  }

  const spaces = spacesQuery.data?.items ?? [];
  const pendingQuestions = pendingQuestionsQuery.data?.items ?? [];
  const openQuestions = openQuestionsQuery.data?.items ?? [];
  const recentQuestions = recentQuestionsQuery.data?.items ?? [];
  const publishedAnswers = publishedAnswersQuery.data?.items ?? [];
  const recentActivity = activityQuery.data?.items ?? [];
  const recentActivityList = recentActivity.slice(0, 6);
  const sources = sourcesQuery.data?.items ?? [];
  const memberCount = membersQuery.data?.length ?? 0;
  const questionCount = questionsSummaryQuery.data?.totalCount ?? 0;
  const draftQuestionCount = draftQuestionsQuery.data?.totalCount ?? 0;
  const pendingQuestionCount = pendingQuestionsQuery.data?.totalCount ?? 0;
  const openQuestionCount = openQuestionsQuery.data?.totalCount ?? 0;
  const duplicateQuestionCount = duplicateQuestionsQuery.data?.totalCount ?? 0;
  const archivedQuestionCount = archivedQuestionsQuery.data?.totalCount ?? 0;
  const answerCount = answersSummaryQuery.data?.totalCount ?? 0;
  const publishedAnswerCount = publishedAnswersQuery.data?.totalCount ?? 0;
  const acceptedAnswerCount = acceptedAnswersQuery.data?.totalCount ?? 0;
  const sourceCount = sourcesQuery.data?.totalCount ?? 0;
  const publicSourceCount = publicSourcesQuery.data?.totalCount ?? 0;
  const validatedAnswerCount = validatedAnswersQuery.data?.totalCount ?? 0;
  const billingSummary = billing.summaryQuery.data;
  const questionLifecycle = getQuestionLifecycleData({
    archived: archivedQuestionCount,
    draft: draftQuestionCount,
    duplicate: duplicateQuestionCount,
    open: openQuestionCount,
  });
  const answerTrustFunnel = getAnswerTrustFunnelData({
    acceptedAnswerCount,
    answerCount,
    publishedAnswerCount,
    validatedAnswerCount,
  });
  const evidenceReadiness = getEvidenceReadinessData({
    publicSourceCount,
    sourceCount,
  });
  const spaceWorkload = getSpaceWorkloadData(spaces);
  const sourceUtilization = getSourceUtilizationData(sources);
  const activityMix = getActivityMixData(recentActivity);
  const activation = getActivationState({
    hasProfile: Boolean(profileQuery.data?.givenName),
    memberCount,
    questionCount,
    sourceCount,
    spaceCount: spacesQuery.data?.totalCount ?? 0,
    trustedAnswerCount: validatedAnswerCount,
  });
  const setupProgress = getSetupProgress(activation);
  const nextAction = getRoleAwareNextAction({
    billingSummary,
    memberCount,
    openQuestions,
    pendingQuestionCount,
    questionCount,
    sourceCount,
    spaces,
  });
  const kpis = getDashboardKpis({
    activity: recentActivity,
    answeredQuestionCount: acceptedAnswerCount,
    openQuestionCount,
    publicSourceCount,
    pendingQuestionCount,
    publishedAnswerCount,
    questionCount,
    spaces,
    sourceCount,
    validatedAnswerCount,
  });
  const businessReadout = getBusinessReadout({
    openQuestionCount,
    publicSourceCount,
    publishedAnswerCount,
    questionCount,
    questionLifecycle,
    sourceCount,
    spaces,
    validatedAnswerCount,
  });
  const queueQuestions = pendingQuestions.length
    ? pendingQuestions
    : openQuestions.length
      ? openQuestions
      : recentQuestions;
  const billingNeedsAttention = getBillingNeedsAttention(billingSummary);
  const activationMode = !activation.hasSpace;

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Home"
        description="A focused command center for setup, moderation throughput, source trust, and answer quality."
        actions={
          <Button asChild>
            <Link to={nextAction.to}>
              <ArrowRight className="size-4" />
              {translateText(nextAction.label)}
            </Link>
          </Button>
        }
      />

      {activationMode ? (
        <HomeHero nextAction={nextAction} setupProgress={setupProgress} />
      ) : null}

      {setupProgress < 100 ? (
        <ProgressChecklistCard
          eyebrow="Start here"
          title="Setup path to trusted answers"
          description="Each step comes from real workspace data where the API exposes it. When all steps are complete, the portal shifts toward daily operations."
          steps={[
            {
              id: "profile",
              label: "Confirm profile and time zone",
              description:
                "Your profile controls language, time zone, and teammate context.",
              complete: activation.hasProfile,
            },
            {
              id: "space",
              label: "Create first space",
              description:
                "Spaces define where questions, answers, tags, sources, and activity belong.",
              complete: activation.hasSpace,
            },
            {
              id: "source",
              label: "Add first source",
              description: "Sources make answers defensible and reusable.",
              complete: activation.hasSource,
            },
            {
              id: "member",
              label: "Invite teammate",
              description:
                "Bring another operator into the workspace before volume grows.",
              complete: activation.hasTeammate,
            },
            {
              id: "question",
              label: "Create first question",
              description:
                "Capture the first thread and move it through review.",
              complete: activation.hasQuestion,
            },
            {
              id: "answer",
              label: "Publish or validate first answer",
              description: "Validated answers become trusted knowledge.",
              complete: activation.hasTrustedAnswer,
            },
          ]}
          action={{
            label: nextAction.label,
            to: nextAction.to,
          }}
          secondaryAction={{
            label: "Open profile",
            to: "/app/settings/profile",
          }}
          hideWhenComplete
        />
      ) : null}

      {billingNeedsAttention ? (
        <BillingNotice
          status={billingSummary?.subscriptionStatus}
          invoiceStatus={billingSummary?.lastInvoice?.status}
        />
      ) : null}

      <Panel
        title="Business readout"
        description="The operating signals that matter most right now: attention, trust, evidence, concentration, and answer demand."
      >
        <BusinessReadout items={businessReadout} />
      </Panel>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)] lg:gap-7.5">
        <Panel
          title="Question lifecycle"
          description="Volume by workflow state so moderators can see pressure before queues become stale."
          action={
            <Badge variant="outline" appearance="outline">
              {translateText("{count} questions", { count: questionCount })}
            </Badge>
          }
        >
          <HorizontalValueChart
            data={questionLifecycle}
            emptyTitle="No question lifecycle yet"
            emptyDescription="Create the first question to see draft, active, duplicate, and archived flow."
            valueLabel="Questions"
          />
        </Panel>

        <Panel
          title="Trusted answer funnel"
          description="Progress from answer candidates to published, validated, and accepted knowledge."
          action={
            <Badge variant="outline" appearance="outline">
              {translateText("{count} answers", { count: answerCount })}
            </Badge>
          }
        >
          <HorizontalValueChart
            data={answerTrustFunnel}
            emptyTitle="No answer funnel yet"
            emptyDescription="Answer candidates, publication, validation, and accepted state will appear as the workflow matures."
            valueLabel="Answers"
          />
        </Panel>
      </div>

      <div className="grid gap-5 xl:grid-cols-[380px_minmax(0,1fr)] lg:gap-7.5">
        <Panel
          title="Evidence readiness"
          description="Source catalog quality: public material can support validated and official answers."
          action={
            <Badge variant="outline" appearance="outline">
              {translateText("{value}% ready", {
                value: kpis.sourceReadiness,
              })}
            </Badge>
          }
        >
          <CompactBreakdownChart
            centerLabel="Public sources"
            centerValue={`${kpis.sourceReadiness}%`}
            data={evidenceReadiness}
            emptyTitle="No evidence catalog yet"
            emptyDescription="Add reusable sources and make them public before scaling trusted answers."
          />
        </Panel>

        <Panel
          title="Question demand by space"
          description="Spaces with the highest thread concentration deserve review capacity, source coverage, and ownership."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to="/app/spaces">{translateText("Review spaces")}</Link>
            </Button>
          }
        >
          <HorizontalValueChart
            data={spaceWorkload}
            emptyTitle="No space demand yet"
            emptyDescription="Questions by space will appear after teams begin routing threads."
            valueLabel="Questions"
          />
        </Panel>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px] lg:gap-7.5">
        <Panel
          title="Source reuse"
          description="The most reused evidence is where governance, freshness, and canonical ownership matter most."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to="/app/sources">{translateText("Review sources")}</Link>
            </Button>
          }
        >
          <HorizontalValueChart
            data={sourceUtilization}
            emptyTitle="No source reuse yet"
            emptyDescription="Source reuse appears when spaces, questions, or answers cite existing evidence."
            valueLabel="References"
          />
        </Panel>

        <Panel
          title="Activity signal mix"
          description="Recent events grouped into workflow, answer lifecycle, public signal, and risk categories."
          action={
            <Badge variant="outline" appearance="outline">
              {translateText("{count} recent", {
                count: kpis.recentActivityCount,
              })}
            </Badge>
          }
        >
          <CompactBreakdownChart
            centerLabel="Recent events"
            centerValue={String(kpis.recentActivityCount)}
            data={activityMix}
            emptyTitle="No activity mix yet"
            emptyDescription="Workflow, answer, feedback, vote, and risk events will appear here."
          />
        </Panel>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px] lg:gap-7.5">
        <Panel
          title="Work queue"
          description="The highest-signal questions appear first: draft threads, then active threads, then recent activity."
          action={
            <Button asChild variant="outline" size="sm">
              <Link
                to={`/app/questions?status=${QuestionStatus.Draft}`}
              >
                {translateText("Open queue")}
              </Link>
            </Button>
          }
        >
          {queueQuestions.length ? (
            queueQuestions.map((question) => (
              <QuestionQueueRow
                key={question.id}
                question={question}
                timeZone={timeZone}
              />
            ))
          ) : (
            <EmptyState
              title="No questions need attention"
              description="When a space receives questions, review-ready and open threads will appear here."
            />
          )}
        </Panel>

        <Panel
          title="Quick actions"
          description="Role-aware paths for owners, moderators, contributors, and operations."
        >
          <div className="grid gap-3">
            {[
              {
                label: nextAction.label,
                description: nextAction.description,
                to: nextAction.to,
                icon: Sparkles,
              },
              {
                label: "Add source",
                description: "Attach reusable evidence before publishing.",
                to: "/app/sources/new",
                icon: FileCheck2,
              },
              {
                label: "Invite teammate",
                description: "Share moderation and authoring responsibility.",
                to: "/app/members",
                icon: Users,
              },
              {
                label: "Review activity",
                description: "Audit workflow changes and public signals.",
                to: "/app/activity",
                icon: Activity,
              },
            ].map((action) => {
              const Icon = action.icon;

              return (
                <Link
                  key={action.label}
                  to={action.to}
                  className="group flex items-start gap-3 rounded-xl border border-border/70 bg-background/75 p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
                >
                  <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary ring-1 ring-inset ring-primary/15">
                    <Icon className="size-4" />
                  </span>
                  <span className="min-w-0">
                    <span className="block font-medium text-mono group-hover:text-primary">
                      {translateText(action.label)}
                    </span>
                    <span className="mt-1 block text-sm leading-6 text-muted-foreground">
                      {translateText(action.description)}
                    </span>
                  </span>
                </Link>
              );
            })}
          </div>
        </Panel>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)] lg:gap-7.5">
        <Panel
          title="Answers awaiting validation"
          description="Published answers are useful. Validated answers become trusted reusable knowledge."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to={`/app/answers?status=${AnswerStatus.Published}`}>
                {translateText("Validate answers")}
              </Link>
            </Button>
          }
        >
          {publishedAnswers.length ? (
            publishedAnswers.map((answer) => (
              <Link
                key={answer.id}
                to={`/app/answers/${answer.id}`}
                className="block rounded-xl border border-border/70 bg-background/75 p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <AnswerStatusBadge status={answer.status} />
                  {answer.isAccepted ? (
                    <Badge variant="success" appearance="outline">
                      {translateText("Accepted")}
                    </Badge>
                  ) : null}
                  {answer.isOfficial ? (
                    <Badge variant="primary" appearance="outline">
                      {translateText("Official")}
                    </Badge>
                  ) : null}
                </div>
                <p className="mt-2 font-medium text-mono">{answer.headline}</p>
                <p className="mt-1 line-clamp-2 text-sm leading-6 text-muted-foreground">
                  {answer.body || translateText("No answer body recorded.")}
                </p>
              </Link>
            ))
          ) : (
            <EmptyState
              title="No validation backlog"
              description="Published answers that need validation will appear here."
            />
          )}
        </Panel>

        <Panel
          title="Recent activity"
          description="Workflow changes, moderation decisions, and customer signals."
          action={
            <Badge variant="outline" appearance="outline">
              {translateText("{count} recent", {
                count: kpis.recentActivityCount,
              })}
            </Badge>
          }
        >
          {recentActivityList.length ? (
            recentActivityList.map((entry) => (
              <ActivityRow key={entry.id} entry={entry} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No recent activity"
              description="Question, answer, feedback, vote, and report events will appear here."
            />
          )}
        </Panel>
      </div>
    </PageSurface>
  );
}
