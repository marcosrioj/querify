import {
  ArrowRight,
  Clock3,
  FileCheck2,
  ShieldCheck,
  Sparkles,
  Waypoints,
} from "lucide-react";
import type { ComponentType, ReactNode } from "react";
import { Link } from "react-router-dom";
import {
  getActivationState,
  getBillingNeedsAttention,
  getBusinessReadout,
  getRoleAwareNextAction,
  getSetupProgress,
  getSpaceWorkloadData,
} from "@/domains/dashboard/dashboard-selectors";
import { useAnswerList } from "@/domains/answers/hooks";
import type { AnswerDto } from "@/domains/answers/types";
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
  AnswerStatusBadge,
  QuestionStatusBadge,
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
  Skeleton,
} from "@/shared/ui";
import { ErrorState } from "@/shared/ui/placeholder-state";

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
      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <Skeleton key={index} className="h-36 rounded-xl" />
        ))}
      </div>
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px]">
        <Skeleton className="h-80 rounded-2xl" />
        <Skeleton className="h-80 rounded-2xl" />
      </div>
    </PageSurface>
  );
}

function SetupFocusCard({
  nextAction,
  setupProgress,
}: {
  nextAction: { label: string; description: string; to: string };
  setupProgress: number;
}) {
  return (
    <Card className="border-primary/15 bg-primary/[0.03]">
      <CardContent className="grid gap-5 p-5 lg:grid-cols-[minmax(0,1fr)_280px] lg:items-center">
        <div className="min-w-0 space-y-3">
          <div className="inline-flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.16em] text-primary">
            <Sparkles className="size-3.5" />
            {translateText("Business setup")}
          </div>
          <div className="max-w-3xl space-y-2">
            <h2 className="text-2xl font-semibold tracking-normal text-mono">
              {translateText("Get to trusted answers faster")}
            </h2>
            <p className="text-sm leading-6 text-muted-foreground">
              {translateText(nextAction.description)}
            </p>
          </div>
        </div>

        <div className="space-y-4 rounded-xl border border-border/70 bg-background/80 p-4">
          <div className="flex items-end justify-between gap-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                {translateText("Setup progress")}
              </p>
              <p className="mt-2 text-3xl font-semibold leading-none text-mono">
                {setupProgress}%
              </p>
            </div>
            <Button asChild size="sm">
              <Link to={nextAction.to}>
                {translateText(nextAction.label)}
                <ArrowRight className="size-4" />
              </Link>
            </Button>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-muted">
            <div
              className="h-full rounded-full bg-primary transition-[width] duration-500"
              style={{ width: `${setupProgress}%` }}
            />
          </div>
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

const businessReadoutIcons: Record<
  string,
  ComponentType<{ className?: string }>
> = {
  "Demand to resolve": Clock3,
  "Active answers": FileCheck2,
  "Evidence readiness": Waypoints,
  "Trusted coverage": ShieldCheck,
};

function BusinessReadout({
  items,
}: {
  items: Array<{ label: string; value: string; detail: string; to: string }>;
}) {
  return (
    <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
      {items.map((item) => {
        const Icon = businessReadoutIcons[item.label] ?? ShieldCheck;
        const content = (
          <>
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
          </>
        );

        return (
          <Link
            key={item.label}
            to={item.to}
            className="rounded-xl border border-border/70 bg-background/75 p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
          >
            {content}
          </Link>
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
            {question.acceptedAnswerId ? (
              <Badge variant="success" appearance="outline">
                {translateText("Accepted answer")}
              </Badge>
            ) : null}
          </div>
          <p className="font-medium text-mono group-hover:text-primary">
            {question.title}
          </p>
        </div>
        {question.lastActivityAtUtc ? (
          <div className="shrink-0 text-sm text-muted-foreground">
            {formatNumericDateTimeInTimeZone(
              question.lastActivityAtUtc,
              timeZone,
            )}
          </div>
        ) : null}
      </div>
    </Link>
  );
}

function AnswerQueueRow({
  answer,
  timeZone,
}: {
  answer: AnswerDto;
  timeZone: string;
}) {
  return (
    <Link
      to={`/app/answers/${answer.id}`}
      className="group block rounded-xl border border-border/70 bg-background/75 p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
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
          <p className="font-medium text-mono group-hover:text-primary">
            {answer.headline}
          </p>
        </div>
        {answer.activatedAtUtc ? (
          <div className="shrink-0 text-sm text-muted-foreground">
            {formatNumericDateTimeInTimeZone(answer.activatedAtUtc, timeZone)}
          </div>
        ) : null}
      </div>
    </Link>
  );
}

function QueueSectionHeader({
  count,
  title,
  to,
}: {
  count: number;
  title: string;
  to: string;
}) {
  return (
    <div className="flex min-w-0 items-center justify-between gap-3">
      <div className="min-w-0">
        <p className="truncate text-sm font-semibold text-mono">
          {translateText(title)}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
        <Badge variant="outline" appearance="outline">
          {formatChartNumber(count)}
        </Badge>
        <Button asChild variant="ghost" size="sm">
          <Link to={to}>
            {translateText("Open")}
            <ArrowRight className="size-4" />
          </Link>
        </Button>
      </div>
    </div>
  );
}

function InlineEmptyState({
  description,
  title,
}: {
  description: string;
  title: string;
}) {
  return (
    <div className="rounded-xl border border-dashed border-border/70 bg-muted/10 p-5">
      <p className="font-medium text-mono">{translateText(title)}</p>
      <p className="mt-2 text-sm leading-6 text-muted-foreground">
        {translateText(description)}
      </p>
    </div>
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
  const draftQuestionsQuery = useQuestionList({
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
  const activeAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 5,
    sorting: "ActivatedAtUtc DESC",
    status: AnswerStatus.Active,
  });
  const acceptedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: "ActivatedAtUtc DESC",
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

  const isInitialDashboardLoading =
    spacesQuery.isLoading ||
    questionsSummaryQuery.isLoading ||
    draftQuestionsQuery.isLoading ||
    openQuestionsQuery.isLoading ||
    activeAnswersQuery.isLoading ||
    acceptedAnswersQuery.isLoading ||
    sourcesQuery.isLoading ||
    publicSourcesQuery.isLoading ||
    membersQuery.isLoading ||
    profileQuery.isLoading ||
    billing.summaryQuery.isLoading;

  const hasCriticalError =
    spacesQuery.isError ||
    questionsSummaryQuery.isError ||
    draftQuestionsQuery.isError ||
    openQuestionsQuery.isError ||
    activeAnswersQuery.isError ||
    acceptedAnswersQuery.isError ||
    sourcesQuery.isError ||
    publicSourcesQuery.isError;

  if (isInitialDashboardLoading) {
    return <DashboardLoadingState />;
  }

  if (hasCriticalError) {
    const error =
      spacesQuery.error ??
      questionsSummaryQuery.error ??
      draftQuestionsQuery.error ??
      openQuestionsQuery.error ??
      activeAnswersQuery.error ??
      acceptedAnswersQuery.error ??
      sourcesQuery.error ??
      publicSourcesQuery.error;

    return (
      <PageSurface>
        <PageHeader
          title="Business dashboard"
          description="Priorities, risks, and demand for the QnA operation."
        />
        <ErrorState
          title="Unable to load dashboard"
          error={error}
          retry={() => {
            void spacesQuery.refetch();
            void questionsSummaryQuery.refetch();
            void draftQuestionsQuery.refetch();
            void openQuestionsQuery.refetch();
            void activeAnswersQuery.refetch();
            void acceptedAnswersQuery.refetch();
            void sourcesQuery.refetch();
            void publicSourcesQuery.refetch();
          }}
        />
      </PageSurface>
    );
  }

  const spaces = spacesQuery.data?.items ?? [];
  const draftQuestions = draftQuestionsQuery.data?.items ?? [];
  const openQuestions = openQuestionsQuery.data?.items ?? [];
  const activeAnswers = activeAnswersQuery.data?.items ?? [];
  const memberCount = membersQuery.data?.length ?? 0;
  const questionCount = questionsSummaryQuery.data?.totalCount ?? 0;
  const draftQuestionCount = draftQuestionsQuery.data?.totalCount ?? 0;
  const openQuestionCount = openQuestionsQuery.data?.totalCount ?? 0;
  const activeAnswerCount = activeAnswersQuery.data?.totalCount ?? 0;
  const acceptedAnswerCount = acceptedAnswersQuery.data?.totalCount ?? 0;
  const sourceCount = sourcesQuery.data?.totalCount ?? 0;
  const publicSourceCount = publicSourcesQuery.data?.totalCount ?? 0;
  const billingSummary = billing.summaryQuery.data;
  const spaceWorkload = getSpaceWorkloadData(spaces);
  const activation = getActivationState({
    hasProfile: Boolean(profileQuery.data?.givenName),
    memberCount,
    questionCount,
    sourceCount,
    spaceCount: spacesQuery.data?.totalCount ?? 0,
    activeAnswerCount,
  });
  const setupProgress = getSetupProgress(activation);
  const nextAction = getRoleAwareNextAction({
    billingSummary,
    memberCount,
    openQuestions,
    draftQuestionCount,
    questionCount,
    sourceCount,
    spaces,
  });
  const businessReadout = getBusinessReadout({
    acceptedAnswerCount,
    openQuestionCount,
    draftQuestionCount,
    publicSourceCount,
    activeAnswerCount,
    questionCount,
    sourceCount,
  });
  const queueQuestions = [...draftQuestions, ...openQuestions].slice(0, 5);
  const billingNeedsAttention = getBillingNeedsAttention(billingSummary);

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Business dashboard"
        description="Priorities, risks, and demand for the QnA operation."
        actions={
          <Button asChild>
            <Link to={nextAction.to}>
              <ArrowRight className="size-4" />
              {translateText(nextAction.label)}
            </Link>
          </Button>
        }
      />

      {setupProgress < 100 ? (
        <SetupFocusCard
          nextAction={nextAction}
          setupProgress={setupProgress}
        />
      ) : null}

      {billingNeedsAttention ? (
        <BillingNotice
          status={billingSummary?.subscriptionStatus}
          invoiceStatus={billingSummary?.lastInvoice?.status}
        />
      ) : null}

      <Panel
        title="Business priorities"
        description="The few signals that decide where the team should spend time: unresolved demand, active answers, trusted coverage, and evidence readiness."
      >
        <BusinessReadout items={businessReadout} />
      </Panel>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px] lg:gap-7.5">
        <Panel
          title="Priority queue"
          description="Only work that changes customer resolution or trusted knowledge appears here."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to={nextAction.to}>
                {translateText("Open queue")}
              </Link>
            </Button>
          }
        >
          <div className="grid gap-5 lg:grid-cols-2">
            <div className="space-y-3">
              <QueueSectionHeader
                count={draftQuestionCount + openQuestionCount}
                title="Questions to resolve"
                to={
                  draftQuestionCount > 0
                    ? `/app/questions?status=${QuestionStatus.Draft}`
                    : `/app/questions?status=${QuestionStatus.Active}`
                }
              />
              {queueQuestions.length ? (
                queueQuestions.map((question) => (
                  <QuestionQueueRow
                    key={question.id}
                    question={question}
                    timeZone={timeZone}
                  />
                ))
              ) : (
                <InlineEmptyState
                  title="No questions need attention"
                  description="Draft and active questions will appear here."
                />
              )}
            </div>

            <div className="space-y-3">
              <QueueSectionHeader
                count={activeAnswerCount}
                title="Active answers"
                to={`/app/answers?status=${AnswerStatus.Active}`}
              />
              {activeAnswers.length ? (
                activeAnswers.map((answer) => (
                  <AnswerQueueRow
                    key={answer.id}
                    answer={answer}
                    timeZone={timeZone}
                  />
                ))
              ) : (
                <InlineEmptyState
                  title="No active answers yet"
                  description="Active answers ready for reuse will appear here."
                />
              )}
            </div>
          </div>
        </Panel>

        <Panel
          title="Demand by space"
          description="Shows where customer questions are concentrated so ownership and evidence can follow demand."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to="/app/spaces">{translateText("Review spaces")}</Link>
            </Button>
          }
        >
          <HorizontalValueChart
            data={spaceWorkload}
            emptyTitle="No space demand yet"
            emptyDescription="Questions by space will appear after teams begin routing questions."
            valueLabel="Questions"
          />
        </Panel>
      </div>
    </PageSurface>
  );
}
