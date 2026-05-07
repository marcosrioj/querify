import {
  AlertTriangle,
  ArrowRight,
  CheckCircle2,
  CircleGauge,
  Clock3,
  CreditCard,
  FileCheck2,
  ShieldCheck,
  Sparkles,
  Settings2,
  UserRound,
  Waypoints,
} from "lucide-react";
import type { ComponentType, ReactNode } from "react";
import { Link } from "react-router-dom";
import {
  type DashboardAdministration,
  type DashboardSignalTone,
  getActivationState,
  getAccountAdministration,
  getBillingNeedsAttention,
  getBusinessReadout,
  getRoleAwareNextAction,
  getSetupProgress,
} from "@/domains/dashboard/dashboard-selectors";
import { useAnswerList } from "@/domains/answers/hooks";
import type { AnswerDto } from "@/domains/answers/types";
import { useBillingSummary } from "@/domains/billing/hooks";
import { useTenantMembers } from "@/domains/members/hooks";
import { useQuestionList } from "@/domains/questions/hooks";
import type { QuestionDto } from "@/domains/questions/types";
import { useUserProfile } from "@/domains/settings/settings-hooks";
import { useSourceList } from "@/domains/sources/hooks";
import { useSpaceList } from "@/domains/spaces/hooks";
import {
  AnswerStatus,
  QuestionStatus,
  SpaceStatus,
  TenantSubscriptionStatus,
  VisibilityScope,
} from "@/shared/constants/backend-enums";
import {
  billingInvoiceStatusPresentation,
  tenantSubscriptionStatusPresentation,
} from "@/shared/constants/enum-ui";
import { PageHeader, PageSurface } from "@/shared/layout/page-layouts";
import {
  DEFAULT_PORTAL_TIME_ZONE,
  formatNumericDateTimeInTimeZone,
} from "@/shared/lib/time-zone";
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
import { cn } from "@/lib/utils";

const DASHBOARD_QUERY_STALE_TIME = 5 * 60 * 1000;
const DASHBOARD_QUERY_GC_TIME = 15 * 60 * 1000;
const DASHBOARD_QUEUE_LIMIT = 4;
const DASHBOARD_SPACE_PREVIEW_LIMIT = 4;

function DashboardLoadingState() {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.35fr)_minmax(340px,0.65fr)] lg:gap-7.5">
        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-[var(--shadow-premium-card)]">
          <div className="space-y-5">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-14 w-48" />
            <Skeleton className="h-4 w-full max-w-2xl" />
            <Skeleton className="h-10 w-44" />
            <div className="grid gap-3 sm:grid-cols-2">
              {Array.from({ length: 3 }).map((_, index) => (
                <Skeleton key={index} className="h-20 rounded-xl" />
              ))}
            </div>
          </div>
        </div>
        <div className="grid gap-3 sm:grid-cols-3 xl:grid-cols-1">
          {Array.from({ length: 3 }).map((_, index) => (
            <Skeleton key={index} className="h-36 rounded-xl" />
          ))}
        </div>
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

type BusinessReadoutItem = {
  label: string;
  value: string;
  benchmark: string;
  detail: string;
  progress?: number;
  tone: DashboardSignalTone;
  to: string;
};

const signalToneClassNames: Record<
  DashboardSignalTone,
  {
    badge: string;
    border: string;
    icon: string;
    meter: string;
    text: string;
  }
> = {
  danger: {
    badge:
      "border-[var(--color-destructive-alpha)] bg-[var(--color-destructive-soft)] text-[var(--color-destructive-accent)]",
    border: "border-[var(--color-destructive-alpha)]",
    icon: "bg-[var(--color-destructive-soft)] text-[var(--color-destructive-accent)] ring-[var(--color-destructive-alpha)]",
    meter: "bg-[var(--color-destructive-accent)]",
    text: "text-[var(--color-destructive-accent)]",
  },
  info: {
    badge:
      "border-[var(--color-info-alpha)] bg-[var(--color-info-soft)] text-[var(--color-info-accent)]",
    border: "border-[var(--color-info-alpha)]",
    icon: "bg-[var(--color-info-soft)] text-[var(--color-info-accent)] ring-[var(--color-info-alpha)]",
    meter: "bg-[var(--color-info-accent)]",
    text: "text-[var(--color-info-accent)]",
  },
  neutral: {
    badge: "border-border/70 bg-muted/35 text-muted-foreground",
    border: "border-border/70",
    icon: "bg-muted/50 text-muted-foreground ring-border/70",
    meter: "bg-muted-foreground/55",
    text: "text-muted-foreground",
  },
  success: {
    badge:
      "border-[var(--color-success-alpha)] bg-[var(--color-success-soft)] text-[var(--color-success-accent)]",
    border: "border-[var(--color-success-alpha)]",
    icon: "bg-[var(--color-success-soft)] text-[var(--color-success-accent)] ring-[var(--color-success-alpha)]",
    meter: "bg-[var(--color-success-accent)]",
    text: "text-[var(--color-success-accent)]",
  },
  warning: {
    badge:
      "border-[var(--color-warning-alpha)] bg-[var(--color-warning-soft)] text-[var(--color-warning-accent)]",
    border: "border-[var(--color-warning-alpha)]",
    icon: "bg-[var(--color-warning-soft)] text-[var(--color-warning-accent)] ring-[var(--color-warning-alpha)]",
    meter: "bg-[var(--color-warning-accent)]",
    text: "text-[var(--color-warning-accent)]",
  },
};


const businessReadoutIcons: Record<
  string,
  ComponentType<{ className?: string }>
> = {
  "Targets to resolve": Clock3,
  "Reusable questions": ShieldCheck,
  "Reusable answers": FileCheck2,
  "Source visibility": Waypoints,
};

function signalLabel(tone: DashboardSignalTone) {
  switch (tone) {
    case "danger":
      return "At risk";
    case "info":
      return "Watching";
    case "success":
      return "On track";
    case "warning":
      return "Needs focus";
    default:
      return "Not started";
  }
}

function ProgressMeter({
  label,
  tone,
  value,
}: {
  label: string;
  tone: DashboardSignalTone;
  value: number;
}) {
  const toneClassNames = signalToneClassNames[tone];

  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between gap-3 text-xs">
        <span className="text-muted-foreground">{translateText(label)}</span>
        <span className="font-semibold tabular-nums text-mono">{value}%</span>
      </div>
      <div
        className="h-1.5 overflow-hidden rounded-full bg-muted"
        aria-label={translateText(label)}
        aria-valuemax={100}
        aria-valuemin={0}
        aria-valuenow={value}
        role="progressbar"
      >
        <div
          className={cn(
            "h-full rounded-full transition-[width] duration-500",
            toneClassNames.meter,
          )}
          style={{ width: `${Math.min(Math.max(value, 0), 100)}%` }}
        />
      </div>
    </div>
  );
}

function ExecutiveSummaryCard({
  activeQuestionCount,
  draftQuestionCount,
  nextAction,
  primary,
  spaceCount,
  spacesWithQuestionsValue,
}: {
  activeQuestionCount: number;
  draftQuestionCount: number;
  nextAction: { label: string; description: string; to: string };
  primary: BusinessReadoutItem;
  spaceCount: number;
  spacesWithQuestionsValue: string;
}) {
  const toneClassNames = signalToneClassNames[primary.tone];
  const headlineIcon =
    primary.tone === "success"
      ? CheckCircle2
      : primary.tone === "neutral"
        ? CircleGauge
        : AlertTriangle;
  const HeadlineIcon = headlineIcon;
  const summaryMetrics = [
    {
      label: "Draft questions",
      value: draftQuestionCount,
      detail: translateText("Needs review"),
    },
    {
      label: "Reusable questions",
      value: activeQuestionCount,
      detail: translateText("Ready for use"),
    },
    {
      label: "Active Spaces",
      value: spaceCount,
      detail: `${spacesWithQuestionsValue} ${translateText(
        "Active Spaces with questions",
      )}`,
    },
  ] satisfies Array<{
    label: string;
    value: number | string;
    detail: string;
  }>;

  return (
    <Card
      className={cn(
        "overflow-hidden bg-linear-to-br from-card via-card to-muted/25",
        toneClassNames.border,
      )}
    >
      <CardContent className="p-5 sm:p-6">
        <div className="flex min-w-0 flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div className="min-w-0 space-y-4">
            <Badge
              variant="outline"
              appearance="outline"
              className={cn("gap-1.5", toneClassNames.badge)}
            >
              <HeadlineIcon className="size-3.5" />
              {translateText(signalLabel(primary.tone))}
            </Badge>

            <div className="space-y-2">
              <p className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                {translateText(primary.label)}
              </p>
              <div className="flex flex-wrap items-end gap-x-3 gap-y-2">
                <p className="text-5xl font-semibold leading-none text-mono sm:text-6xl">
                  {primary.value}
                </p>
                <p
                  className={cn(
                    "pb-1 text-sm font-semibold",
                    toneClassNames.text,
                  )}
                >
                  {translateText(primary.benchmark)}
                </p>
              </div>
              <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
                {translateText(primary.detail)}
              </p>
            </div>
          </div>

          <Button asChild className="w-full sm:w-auto lg:shrink-0">
            <Link to={nextAction.to}>
              {translateText(nextAction.label)}
              <ArrowRight className="size-4" />
            </Link>
          </Button>
        </div>

        <div className="mt-6 grid gap-3 sm:grid-cols-2">
          {summaryMetrics.map((item) => (
            <div
              key={item.label}
              className="min-w-0 rounded-xl border border-border/70 bg-background/70 p-3"
            >
              <p className="text-xs font-medium uppercase tracking-[0.14em] text-muted-foreground">
                {translateText(item.label)}
              </p>
              <p className="mt-2 text-2xl font-semibold leading-none text-mono">
                {item.value}
              </p>
              <p className="mt-2 text-xs text-muted-foreground">
                {item.detail}
              </p>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function ReadoutMetricCard({ item }: { item: BusinessReadoutItem }) {
  const Icon = businessReadoutIcons[item.label] ?? ShieldCheck;
  const toneClassNames = signalToneClassNames[item.tone];

  return (
    <Link
      to={item.to}
      className={cn(
        "group flex min-h-36 min-w-0 flex-col justify-between rounded-xl border bg-card p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]",
        toneClassNames.border,
      )}
    >
      <div className="flex min-w-0 items-start justify-between gap-3">
        <p className="min-w-0 text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground">
          {translateText(item.label)}
        </p>
        <span
          className={cn(
            "flex size-9 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
            toneClassNames.icon,
          )}
        >
          <Icon className="size-4" />
        </span>
      </div>

      <div className="mt-4 min-w-0 space-y-2">
        <div className="flex flex-wrap items-end gap-x-2 gap-y-1">
          <p className="text-3xl font-semibold leading-none text-mono">
            {item.value}
          </p>
          <span className={cn("text-xs font-semibold", toneClassNames.text)}>
            {translateText(item.benchmark)}
          </span>
        </div>
        {item.progress !== undefined ? (
          <ProgressMeter
            label={item.label}
            tone={item.tone}
            value={item.progress}
          />
        ) : null}
        <p className="text-sm leading-6 text-muted-foreground">
          {translateText(item.detail)}
        </p>
      </div>
    </Link>
  );
}

function BusinessReadout({ items }: { items: BusinessReadoutItem[] }) {
  return (
    <div className="grid gap-3 sm:grid-cols-3 xl:grid-cols-1">
      {items.map((item) => (
        <ReadoutMetricCard key={item.label} item={item} />
      ))}
    </div>
  );
}

const administrationIcons: Record<
  string,
  ComponentType<{ className?: string }>
> = {
  Billing: CreditCard,
  Profile: UserRound,
  Settings: Settings2,
};

function AccountAdministrationPanel({
  administration,
}: {
  administration: DashboardAdministration;
}) {
  const toneClassNames = signalToneClassNames[administration.tone];
  const SummaryIcon =
    administration.tone === "success"
      ? CheckCircle2
      : administration.tone === "neutral"
        ? CircleGauge
        : AlertTriangle;

  return (
    <div className="space-y-3">
      <Link
        to={administration.to}
        className={cn(
          "block rounded-xl border bg-background/70 p-3 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]",
          toneClassNames.border,
        )}
      >
        <div className="flex min-w-0 items-start gap-2.5">
          <span
            className={cn(
              "flex size-8 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
              toneClassNames.icon,
            )}
          >
            <SummaryIcon className="size-4" />
          </span>
          <div className="min-w-0">
            <div className="flex min-w-0 items-start justify-between gap-2">
              <p className="min-w-0 text-sm font-semibold leading-5 text-mono">
                {translateText(administration.label)}
              </p>
              <Badge
                variant="outline"
                appearance="outline"
                className={cn(
                  "shrink-0 text-[0.6875rem]",
                  toneClassNames.badge,
                )}
              >
                {translateText(signalLabel(administration.tone))}
              </Badge>
            </div>
            <p className="mt-1 text-xs leading-5 text-muted-foreground">
              {translateText(administration.detail)}
            </p>
          </div>
        </div>
      </Link>

      <div className="space-y-2">
        {administration.items.map((item) => {
          const itemToneClassNames = signalToneClassNames[item.tone];
          const Icon = administrationIcons[item.label] ?? Settings2;

          return (
            <Link
              key={item.key}
              to={item.to}
              className="group block rounded-xl border border-border/70 bg-background/70 p-2.5 transition-colors hover:border-primary/25 hover:bg-primary/[0.025]"
            >
              <div className="flex min-w-0 items-start gap-2.5">
                <span
                  className={cn(
                    "flex size-8 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
                    itemToneClassNames.icon,
                  )}
                >
                  <Icon className="size-3.5" />
                </span>
                <div className="min-w-0 flex-1">
                  <div className="flex min-w-0 items-start justify-between gap-3">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold text-mono group-hover:text-primary">
                        {translateText(item.label)}
                      </p>
                      <p className="mt-0.5 text-xs leading-5 text-muted-foreground">
                        {translateText(item.detail)}
                      </p>
                    </div>
                    <p className="shrink-0 text-right text-sm font-semibold tabular-nums text-mono">
                      {translateText(item.value)}
                    </p>
                  </div>
                  <div className="mt-1.5 flex min-w-0 items-center justify-between gap-3">
                    <Badge
                      variant="outline"
                      appearance="outline"
                      className={cn("text-[0.6875rem]", itemToneClassNames.badge)}
                    >
                      {translateText(signalLabel(item.tone))}
                    </Badge>
                    <span className="inline-flex min-w-0 shrink items-center justify-end gap-1 text-xs font-semibold text-primary">
                      <span className="truncate">
                        {translateText(item.actionLabel)}
                      </span>
                      <ArrowRight className="size-3.5" />
                    </span>
                  </div>
                </div>
              </div>
            </Link>
          );
        })}
      </div>
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

function AnswerQueueRow({ answer }: { answer: AnswerDto }) {
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
      </div>
    </Link>
  );
}

function QueueSectionHeader({ title, to }: { title: string; to: string }) {
  return (
    <div className="flex min-w-0 items-center justify-between gap-3">
      <div className="min-w-0">
        <p className="truncate text-sm font-semibold text-mono">
          {translateText(title)}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
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
  const profileQuery = useUserProfile();
  const timeZone =
    profileQuery.data?.timeZone?.trim() || DEFAULT_PORTAL_TIME_ZONE;
  const membersQuery = useTenantMembers({
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const billingSummaryQuery = useBillingSummary({
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });

  const spacesQuery = useSpaceList({
    page: 1,
    pageSize: DASHBOARD_SPACE_PREVIEW_LIMIT,
    sorting: "QuestionCount DESC",
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const knownSpaceCount = spacesQuery.data?.totalCount ?? 0;
  const loadedPreviewSpaceCount = spacesQuery.data?.items.length ?? 0;
  const spacesWithQuestionsQuery = useSpaceList({
    page: 1,
    pageSize: Math.max(knownSpaceCount, 1),
    sorting: "QuestionCount DESC",
    enabled: knownSpaceCount > loadedPreviewSpaceCount,
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const questionsSummaryQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Active,
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const draftQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: DASHBOARD_QUEUE_LIMIT,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Draft,
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const activeQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: DASHBOARD_QUEUE_LIMIT,
    sorting: "LastActivityAtUtc DESC",
    status: QuestionStatus.Active,
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const activeAnswersQuery = useAnswerList({
    page: 1,
    pageSize: DASHBOARD_QUEUE_LIMIT,
    sorting: "LastUpdatedAtUtc DESC",
    status: AnswerStatus.Active,
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const sourcesQuery = useSourceList({
    page: 1,
    pageSize: 1,
    sorting: "LastVerifiedAtUtc DESC",
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });
  const publicSourcesQuery = useSourceList({
    page: 1,
    pageSize: 1,
    sorting: "LastVerifiedAtUtc DESC",
    visibility: VisibilityScope.Public,
    gcTime: DASHBOARD_QUERY_GC_TIME,
    staleTime: DASHBOARD_QUERY_STALE_TIME,
  });

  const isInitialDashboardLoading =
    spacesQuery.isLoading ||
    spacesWithQuestionsQuery.isLoading ||
    questionsSummaryQuery.isLoading ||
    draftQuestionsQuery.isLoading ||
    activeQuestionsQuery.isLoading ||
    activeAnswersQuery.isLoading ||
    sourcesQuery.isLoading ||
    publicSourcesQuery.isLoading ||
    membersQuery.isLoading ||
    profileQuery.isLoading ||
    billingSummaryQuery.isLoading;

  const hasCriticalError =
    spacesQuery.isError ||
    spacesWithQuestionsQuery.isError ||
    questionsSummaryQuery.isError ||
    draftQuestionsQuery.isError ||
    activeQuestionsQuery.isError ||
    activeAnswersQuery.isError ||
    sourcesQuery.isError ||
    publicSourcesQuery.isError;

  if (isInitialDashboardLoading) {
    return <DashboardLoadingState />;
  }

  if (hasCriticalError) {
    const error =
      spacesQuery.error ??
      spacesWithQuestionsQuery.error ??
      questionsSummaryQuery.error ??
      draftQuestionsQuery.error ??
      activeQuestionsQuery.error ??
      activeAnswersQuery.error ??
      sourcesQuery.error ??
      publicSourcesQuery.error;

    return (
      <PageSurface>
        <PageHeader
          title="Business dashboard"
          description="Priorities, reusable knowledge, and operational context for QnA."
          descriptionMode="hint"
        />
        <ErrorState
          title="Unable to load dashboard"
          error={error}
          retry={() => {
            void spacesQuery.refetch();
            void spacesWithQuestionsQuery.refetch();
            void questionsSummaryQuery.refetch();
            void draftQuestionsQuery.refetch();
            void activeQuestionsQuery.refetch();
            void activeAnswersQuery.refetch();
            void sourcesQuery.refetch();
            void publicSourcesQuery.refetch();
          }}
        />
      </PageSurface>
    );
  }

  const spaces = spacesQuery.data?.items ?? [];
  const dashboardSpaces = spacesWithQuestionsQuery.data?.items ?? spaces;
  const activeDashboardSpaces = dashboardSpaces.filter(
    (space) => space.status === SpaceStatus.Active,
  );
  const draftQuestions = draftQuestionsQuery.data?.items ?? [];
  const activeQuestions = activeQuestionsQuery.data?.items ?? [];
  const activeAnswers = activeAnswersQuery.data?.items ?? [];
  const memberCount = membersQuery.data?.length ?? 0;
  const questionCount = questionsSummaryQuery.data?.totalCount ?? 0;
  const draftQuestionCount = draftQuestionsQuery.data?.totalCount ?? 0;
  const activeQuestionCount = activeQuestionsQuery.data?.totalCount ?? 0;
  const activeAnswerCount = activeAnswersQuery.data?.totalCount ?? 0;
  const sourceCount = sourcesQuery.data?.totalCount ?? 0;
  const publicSourceCount = publicSourcesQuery.data?.totalCount ?? 0;
  const spaceCount = activeDashboardSpaces.length;
  const spacesWithQuestionsCount = activeDashboardSpaces.filter(
    (space) => space.questionCount > 0,
  ).length;
  const firstSpaceWithoutQuestionId = activeDashboardSpaces.find(
    (space) => space.questionCount === 0,
  )?.id;
  const spacesWithQuestionsValue = `${spacesWithQuestionsCount}`;
  const billingSummary = billingSummaryQuery.data;
  const hasCompleteProfile = Boolean(
    profileQuery.data?.givenName?.trim() &&
      profileQuery.data.phoneNumber?.trim() &&
      profileQuery.data.language?.trim() &&
      profileQuery.data.timeZone?.trim(),
  );
  const activation = getActivationState({
    hasProfile: Boolean(profileQuery.data?.givenName),
    memberCount,
    questionCount: activeQuestionCount,
    spaceCount,
    activeAnswerCount,
  });
  const setupProgress = getSetupProgress(activation);
  const nextAction = getRoleAwareNextAction({
    billingSummary,
    memberCount,
    draftQuestions,
    draftQuestionCount,
    activeQuestionCount,
    spaces: dashboardSpaces,
  });
  const queueQuestions = draftQuestions.slice(0, DASHBOARD_QUEUE_LIMIT);
  const businessReadout = getBusinessReadout({
    activeQuestionCount,
    draftQuestionCount,
    publicSourceCount,
    activeAnswerCount,
    questionCount,
    spaceCount,
    spacesWithQuestionsCount,
    sourceCount,
    firstActiveQuestionId: activeQuestions[0]?.id,
    firstActiveAnswerId: activeAnswers[0]?.id,
    firstDraftQuestionId: queueQuestions[0]?.id,
    firstSpaceWithoutQuestionId,
    firstSpaceId: activeDashboardSpaces[0]?.id,
  });
  const primaryReadout = businessReadout[0] as BusinessReadoutItem;
  const secondaryReadout = businessReadout.slice(1) as BusinessReadoutItem[];
  const accountAdministration = getAccountAdministration({
    billingSummary,
    hasCompleteProfile,
  });
  const billingNeedsAttention = getBillingNeedsAttention(billingSummary);

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Business dashboard"
        description="Priorities, reusable knowledge, and operational context for QnA."
        descriptionMode="hint"
      />

      {setupProgress < 100 ? (
        <SetupFocusCard nextAction={nextAction} setupProgress={setupProgress} />
      ) : null}

      {billingNeedsAttention ? (
        <BillingNotice
          status={billingSummary?.subscriptionStatus}
          invoiceStatus={billingSummary?.lastInvoice?.status}
        />
      ) : null}

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.35fr)_minmax(340px,0.65fr)] lg:gap-7.5">
        <ExecutiveSummaryCard
          activeQuestionCount={activeQuestionCount}
          draftQuestionCount={draftQuestionCount}
          nextAction={nextAction}
          primary={primaryReadout}
          spaceCount={spaceCount}
          spacesWithQuestionsValue={spacesWithQuestionsValue}
        />
        <BusinessReadout items={secondaryReadout} />
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px] lg:gap-7.5">
        <Panel
          title="Workflow queue"
          description="Draft questions appear here; active records are treated as reusable knowledge."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to={nextAction.to}>{translateText("Open queue")}</Link>
            </Button>
          }
        >
          <div className="grid gap-5 lg:grid-cols-2">
            <div className="space-y-3">
              <QueueSectionHeader
                title="Draft questions to review"
                to={
                  queueQuestions[0]
                    ? `/app/questions/${queueQuestions[0].id}`
                    : activeDashboardSpaces[0]
                      ? `/app/spaces/${activeDashboardSpaces[0].id}`
                      : "/app/spaces"
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
                  title="No draft questions need review"
                  description="Active questions are usable and stay out of the draft queue."
                />
              )}
            </div>

            <div className="space-y-3">
              <QueueSectionHeader
                title="Answers ready for reuse"
                to={
                  activeAnswers[0]
                    ? `/app/answers/${activeAnswers[0].id}`
                    : activeDashboardSpaces[0]
                      ? `/app/spaces/${activeDashboardSpaces[0].id}`
                      : "/app/spaces"
                }
              />
              {activeAnswers.length ? (
                activeAnswers.map((answer) => (
                  <AnswerQueueRow key={answer.id} answer={answer} />
                ))
              ) : (
                <InlineEmptyState
                  title="No active answers yet"
                  description="Reusable answers will appear here."
                />
              )}
            </div>
          </div>
        </Panel>

        <Panel
          title="Account administration"
          description="Shows account tasks for billing, profile, and workspace settings without extra dashboard API calls."
          action={
            <Button asChild variant="outline" size="sm">
              <Link to={accountAdministration.to}>
                {translateText("Open")}
              </Link>
            </Button>
          }
        >
          <AccountAdministrationPanel administration={accountAdministration} />
        </Panel>
      </div>
    </PageSurface>
  );
}
