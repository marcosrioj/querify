import { useEffect, useRef } from "react";
import {
  ArrowRight,
  ArrowUpRight,
  BookOpen,
  CircleAlert,
  Files,
  Gauge,
  MessageSquare,
  Plus,
  ShieldCheck,
  SlidersHorizontal,
  Sparkles,
  type LucideIcon,
} from "lucide-react";
import { Link } from "react-router-dom";
import {
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
  useCurrentWorkspace,
  useTenantWorkspace,
} from "@/domains/tenants/hooks";
import { useContentRefList } from "@/domains/content-refs/hooks";
import { type ContentRefDto } from "@/domains/content-refs/types";
import { useFaqItemList } from "@/domains/faq-items/hooks";
import { type FaqItemDto } from "@/domains/faq-items/types";
import { useFaqList } from "@/domains/faq/hooks";
import { type FaqDto } from "@/domains/faq/types";
import { useAuth } from "@/platform/auth/use-auth";
import { useTenant } from "@/platform/tenant/use-tenant";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { Progress, ProgressRadial } from "@/components/ui/progress";
import { PageHeader, PageSurface } from "@/shared/layout/page-layouts";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  CardToolbar,
  ContextHint,
  ProgressChecklistCard,
} from "@/shared/ui";
import {
  FaqStatus,
  tenantUserRoleTypeLabels,
  tenantEditionLabels,
} from "@/shared/constants/backend-enums";
import { ContentRefKindBadge, FaqStatusBadge } from "@/shared/ui/status-badges";
import { getCurrentPortalLanguage, translateText } from "@/shared/lib/i18n-core";

function toPercent(value: number, total: number) {
  if (total <= 0) {
    return 0;
  }

  return Math.round((value / total) * 100);
}

function formatNumber(value: number) {
  return new Intl.NumberFormat(getCurrentPortalLanguage()).format(value);
}

function MetricCard({
  icon: Icon,
  title,
  value,
  description,
  toneClassName,
}: {
  icon: LucideIcon;
  title: string;
  value: string;
  description: string;
  toneClassName: string;
}) {
  return (
    <Card>
      <CardContent className="relative p-5">
        <div className="min-w-0 space-y-2">
          <p className="text-sm text-muted-foreground">{translateText(title)}</p>
          <p className="break-words text-3xl font-semibold tracking-tight text-mono">
            {value}
          </p>
          <p className="text-sm text-muted-foreground">
            {translateText(description)}
          </p>
        </div>
        <div
          className={`pointer-events-none absolute right-5 top-5 flex size-5 items-center justify-center rounded-2xl ${toneClassName}`}
        >
          <Icon className="size-5" />
        </div>
      </CardContent>
    </Card>
  );
}

function ReadinessRow({
  label,
  value,
  helper,
  indicatorClassName,
}: {
  label: string;
  value: number;
  helper: string;
  indicatorClassName: string;
}) {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between gap-3">
        <span className="text-sm font-medium text-foreground">
          {translateText(label)}
        </span>
        <span className="text-sm font-semibold text-mono">{value}%</span>
      </div>
      <Progress
        value={value}
        className="h-2"
        indicatorClassName={indicatorClassName}
      />
      <p className="text-xs leading-5 text-muted-foreground">
        {translateText(helper)}
      </p>
    </div>
  );
}

function EmptyMiniState({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="rounded-2xl border border-dashed border-border px-4 py-5">
      <p className="font-medium text-mono">{translateText(title)}</p>
      <p className="mt-1 text-sm text-muted-foreground">
        {translateText(description)}
      </p>
    </div>
  );
}

function AnswerRow({ item }: { item: FaqItemDto }) {
  return (
    <div className="flex flex-col gap-4 rounded-2xl border border-border/70 px-4 py-3 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0 space-y-2">
        <Link
          to={`/app/faq/${item.faqId}/items/${item.id}`}
          className="line-clamp-2 text-sm font-medium text-mono hover:text-primary"
        >
          {item.question}
        </Link>
        <div className="flex flex-wrap items-center gap-2">
          <Badge
            variant={item.isActive ? "success" : "warning"}
            appearance="light"
          >
            {translateText(item.isActive ? "Active" : "Inactive")}
          </Badge>
          <Badge
            variant={item.contentRefId ? "primary" : "outline"}
            appearance="light"
          >
            {translateText(item.contentRefId ? "Source linked" : "No source")}
          </Badge>
        </div>
      </div>
      <div className="space-y-1 text-left text-xs text-muted-foreground sm:text-right">
        <div className="font-semibold text-mono">
          {translateText("Feedback {value}", { value: item.feedbackScore })}
        </div>
        <div className="font-semibold text-mono">
          {translateText("Confidence {value}", { value: item.confidenceScore })}
        </div>
      </div>
    </div>
  );
}

function FaqRow({ faq }: { faq: FaqDto }) {
  return (
    <div className="flex flex-col gap-3 rounded-2xl border border-border/70 px-4 py-3 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0 space-y-1.5">
        <Link
          to={`/app/faq/${faq.id}`}
          className="block truncate text-sm font-medium text-mono hover:text-primary"
        >
          {faq.name}
        </Link>
        <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
          {faq.language}
        </p>
      </div>
      <div className="shrink-0">
        <FaqStatusBadge status={faq.status} />
      </div>
    </div>
  );
}

function SourceRow({ contentRef }: { contentRef: ContentRefDto }) {
  return (
    <div className="rounded-2xl border border-border/70 px-4 py-3">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-1.5">
          <p className="truncate text-sm font-medium text-mono">
            {contentRef.label || translateText("Untitled source")}
          </p>
          <p className="break-all text-sm text-muted-foreground sm:break-words lg:truncate">
            {contentRef.locator}
          </p>
        </div>
        <ContentRefKindBadge kind={contentRef.kind} />
      </div>
      <p className="mt-2 text-xs text-muted-foreground">
        {contentRef.scope || translateText("No scope assigned")}
      </p>
    </div>
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const { currentTenantId } = useTenant();
  const currentWorkspace = useCurrentWorkspace();
  const refreshedTenantIdRef = useRef<string | undefined>(undefined);
  const { clientKeyQuery } = useTenantWorkspace();
  const faqOverviewQuery = useFaqList({
    page: 1,
    pageSize: 4,
    sorting: "UpdatedDate DESC",
  });
  const faqPublishedQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: "UpdatedDate DESC",
    status: FaqStatus.Published,
  });
  const faqDraftQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: "UpdatedDate DESC",
    status: FaqStatus.Draft,
  });
  const faqArchivedQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: "UpdatedDate DESC",
    status: FaqStatus.Archived,
  });
  const faqItemTopQuery = useFaqItemList({
    page: 1,
    pageSize: 5,
    sorting: "FeedbackScore DESC",
  });
  const faqItemActiveQuery = useFaqItemList({
    page: 1,
    pageSize: 1,
    sorting: "UpdatedDate DESC",
    isActive: true,
  });
  const contentRefOverviewQuery = useContentRefList({
    page: 1,
    pageSize: 5,
    sorting: "UpdatedDate DESC",
  });

  useEffect(() => {
    if (!currentTenantId) {
      refreshedTenantIdRef.current = undefined;
      return;
    }

    if (refreshedTenantIdRef.current === currentTenantId) {
      return;
    }

    refreshedTenantIdRef.current = currentTenantId;

    const tenantScopedDashboardQueries = [
      faqOverviewQuery,
      faqPublishedQuery,
      faqDraftQuery,
      faqArchivedQuery,
      faqItemTopQuery,
      faqItemActiveQuery,
      contentRefOverviewQuery,
    ];

    void Promise.all(
      tenantScopedDashboardQueries
        .filter((query) => query.fetchStatus === "idle")
        .map((query) => query.refetch()),
    );
  }, [
    contentRefOverviewQuery,
    currentTenantId,
    faqArchivedQuery,
    faqDraftQuery,
    faqItemActiveQuery,
    faqItemTopQuery,
    faqOverviewQuery,
    faqPublishedQuery,
  ]);

  const totalFaqs = faqOverviewQuery.data?.totalCount ?? 0;
  const publishedFaqs = faqPublishedQuery.data?.totalCount ?? 0;
  const draftFaqs = faqDraftQuery.data?.totalCount ?? 0;
  const archivedFaqs = faqArchivedQuery.data?.totalCount ?? 0;
  const totalFaqItems = faqItemTopQuery.data?.totalCount ?? 0;
  const activeFaqItems = faqItemActiveQuery.data?.totalCount ?? 0;
  const inactiveFaqItems = Math.max(totalFaqItems - activeFaqItems, 0);
  const totalContentRefs = contentRefOverviewQuery.data?.totalCount ?? 0;
  const clientKeyReady = Boolean(clientKeyQuery.data);
  const primaryFaqId = faqOverviewQuery.data?.items[0]?.id;
  const publishedFaqPercent = toPercent(publishedFaqs, totalFaqs);
  const activeAnswerPercent = toPercent(activeFaqItems, totalFaqItems);
  const readinessScore = Math.round(
    (publishedFaqPercent +
      activeAnswerPercent +
      (clientKeyReady ? 100 : 0)) /
      3,
  );

  const assetMixData = [
    {
      name: translateText("FAQs"),
      total: totalFaqs,
      fill: "var(--chart-1)",
    },
    {
      name: translateText("Q&A items"),
      total: totalFaqItems,
      fill: "var(--chart-2)",
    },
    {
      name: translateText("Sources"),
      total: totalContentRefs,
      fill: "var(--chart-3)",
    },
  ];
  const faqLifecycleData = [
    {
      name: translateText("Published"),
      total: publishedFaqs,
      fill: "var(--chart-2)",
    },
    {
      name: translateText("Draft"),
      total: draftFaqs,
      fill: "var(--chart-3)",
    },
    {
      name: translateText("Archived"),
      total: archivedFaqs,
      fill: "var(--chart-4)",
    },
  ];
  const heroHighlights = [
    {
      label: "Published",
      value: `${publishedFaqPercent}%`,
      description: translateText("{published} of {total} FAQs are live", {
        published: publishedFaqs,
        total: totalFaqs,
      }),
    },
    {
      label: "Inactive",
      value: formatNumber(inactiveFaqItems),
      description: "Q&A items still inactive",
    },
    {
      label: "Client key",
      value: clientKeyReady ? "Ready" : "Missing",
      description: clientKeyReady
        ? "Public previews can use the current key"
        : "Generate a public key before exposing tenant previews",
    },
  ];
  const onboardingSteps = [
    {
      id: "faq",
      label: "Create your first FAQ",
      description: totalFaqs
        ? translateText("{count} FAQ records already exist in this workspace.", {
            count: formatNumber(totalFaqs),
          })
        : "Define the first FAQ so the workspace has a customer-facing destination.",
      complete: totalFaqs > 0,
    },
    {
      id: "item",
      label: "Add a Q&A item",
      description: totalFaqItems
        ? translateText(
            "{count} Q&A items are already filling out the knowledge base.",
            { count: formatNumber(totalFaqItems) },
          )
        : "Write the first answer so visitors can actually resolve a question.",
      complete: totalFaqItems > 0,
    },
    {
      id: "source",
      label: "Connect a source",
      description: totalContentRefs
        ? translateText("{count} reusable sources are already linked.", {
            count: formatNumber(totalContentRefs),
          })
        : "Attach source material so answers stay traceable and reusable.",
      complete: totalContentRefs > 0,
    },
    {
      id: "publish",
      label: "Publish a FAQ",
      description: publishedFaqs
        ? translateText("{count} FAQs are already live for customers.", {
            count: formatNumber(publishedFaqs),
          })
        : "Move one FAQ out of draft so the workflow reaches a visible outcome.",
      complete: publishedFaqs > 0,
    },
  ];
  const nextOnboardingAction =
    totalFaqs === 0
      ? { label: "Create first FAQ", to: "/app/faq/new" }
      : totalFaqItems === 0 && primaryFaqId
        ? {
            label: "Add first Q&A item",
            to: `/app/faq/${primaryFaqId}/items/new`,
          }
        : totalContentRefs === 0 && primaryFaqId
          ? {
              label: "Add first source",
              to: `/app/faq/${primaryFaqId}/content-refs/new`,
            }
          : publishedFaqs === 0 && primaryFaqId
            ? {
                label: "Review publish settings",
                to: `/app/faq/${primaryFaqId}/edit`,
              }
            : {
                label: "Open FAQs",
                to: "/app/faq",
              };
  const heroPrimaryAction = onboardingSteps.every((step) => step.complete)
    ? { label: "Open FAQs", to: "/app/faq" }
    : { label: "Start here", to: nextOnboardingAction.to };
  const heroSecondaryAction = {
    label: "Workspace settings",
    to: "/app/settings/tenant",
  };

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="Dashboard"
        description="Quick view of FAQs, Q&A items, sources, and workspace readiness."
        descriptionMode="inline"
        actions={
          <>
            <Button asChild>
              <Link to="/app/faq/new">
                <Plus className="size-4" />
                {translateText("New FAQ")}
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/app/settings/tenant">
                <SlidersHorizontal className="size-4" />
                {translateText("Settings")}
              </Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.5fr)_380px] lg:gap-7.5">
        <Card
          className="relative overflow-hidden border-none text-white shadow-xl shadow-slate-950/10"
          style={{
            backgroundImage:
              "linear-gradient(135deg, hsl(205 92% 47%) 0%, hsl(221 83% 29%) 55%, hsl(224 64% 18%) 100%)",
          }}
        >
          <div className="pointer-events-none absolute inset-0 overflow-hidden">
            <div className="absolute -top-12 right-0 h-48 w-48 rounded-full bg-white/10 blur-3xl" />
            <div className="absolute bottom-0 left-12 h-40 w-40 rounded-full bg-cyan-200/15 blur-3xl" />
          </div>
          <CardContent className="relative space-y-8 p-6 lg:p-7.5">
            <div className="flex flex-wrap items-center gap-2 text-xs font-medium uppercase tracking-[0.24em] text-white/70">
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-[0.6875rem] tracking-[0.2em] text-white">
                {currentWorkspace?.slug ?? translateText("workspace-pending")}
              </span>
              {currentWorkspace ? (
                <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-[0.6875rem] tracking-[0.2em] text-white">
                  {currentWorkspace.isActive
                    ? translateText("Active workspace")
                    : translateText("Inactive workspace")}
                </span>
              ) : null}
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-[0.6875rem] tracking-[0.2em] text-white">
                {translateText("{role} access", {
                  role: translateText(
                    currentWorkspace
                      ? tenantUserRoleTypeLabels[currentWorkspace.currentUserRole]
                      : user?.role ?? "Member",
                  ),
                })}
              </span>
            </div>

            <div className="max-w-3xl space-y-3">
              <div className="flex flex-wrap items-center gap-3">
                <h2 className="text-2xl font-semibold tracking-tight lg:text-3xl">
                  {currentWorkspace?.name ??
                    translateText("Set up your tenant workspace")}
                </h2>
                {currentWorkspace ? (
                  <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-xs font-medium text-white">
                    {translateText(tenantEditionLabels[currentWorkspace.edition])}
                  </span>
                ) : null}
              </div>
              <p className="max-w-2xl text-sm leading-6 text-white/78">
                {translateText(
                  "Track FAQ coverage, Q&A item health, source links, and public readiness for the current workspace.",
                )}
              </p>
            </div>

            <div className="flex flex-wrap gap-3">
              <Button
                asChild
                className="border-white/20 bg-white text-slate-950 shadow-none hover:bg-white/90"
              >
                <Link to={heroPrimaryAction.to}>
                  {translateText(heroPrimaryAction.label)}
                  <ArrowUpRight className="size-4" />
                </Link>
              </Button>
              <Button
                asChild
                variant="outline"
                className="border-white/20 bg-white/10 text-white hover:bg-white/15"
              >
                <Link to={heroSecondaryAction.to}>
                  <SlidersHorizontal className="size-4" />
                  {translateText(heroSecondaryAction.label)}
                </Link>
              </Button>
            </div>

            <div className="rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-xs">
              <p className="text-xs font-medium uppercase tracking-[0.22em] text-white/70">
                {translateText("Next recommended step")}
              </p>
              <p className="mt-2 text-lg font-semibold tracking-tight text-white">
                {translateText(nextOnboardingAction.label)}
              </p>
              <p className="mt-1 text-sm text-white/72">
                {translateText(
                  "Keep the workspace moving toward a published FAQ with traceable answers.",
                )}
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
              {heroHighlights.map((highlight) => (
                <div
                  key={highlight.label}
                  className="rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-xs"
                >
                  <p className="text-xs uppercase tracking-[0.22em] text-white/65">
                    {translateText(highlight.label)}
                  </p>
                  <p className="mt-2 text-2xl font-semibold tracking-tight">
                    {highlight.value}
                  </p>
                  <p className="mt-1 text-sm text-white/70">
                    {translateText(highlight.description)}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Readiness")}</span>
                <ContextHint
                  content="Readiness is weighted across published FAQs, active Q&A items, and public client key availability."
                  label="Readiness details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="flex flex-col gap-5 sm:flex-row sm:items-center">
              <ProgressRadial
                value={readinessScore}
                size={148}
                strokeWidth={10}
                indicatorClassName="text-primary"
                trackClassName="text-secondary"
                showLabel
              >
                <div className="space-y-1 text-center">
                  <div className="text-3xl font-semibold tracking-tight text-mono">
                    {readinessScore}%
                  </div>
                  <div className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                    {translateText("Ready")}
                  </div>
                </div>
              </ProgressRadial>

              <div className="space-y-3">
                <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                  <Gauge className="size-11 text-primary" />
                  {translateText("Workspace health at a glance.")}
                </div>
                <p className="text-sm leading-6 text-muted-foreground">
                  {translateText(
                    "This score uses live Portal data instead of placeholder growth metrics.",
                  )}
                </p>
                <Badge
                  variant={clientKeyReady ? "success" : "warning"}
                  appearance="light"
                >
                  {translateText(
                    clientKeyReady ? "Client key live" : "Client key missing",
                  )}
                </Badge>
              </div>
            </div>

            <div className="space-y-4">
              <ReadinessRow
                label="Published"
                value={publishedFaqPercent}
                helper={translateText(
                  "{published} published, {draft} draft, {archived} archived",
                  {
                    published: publishedFaqs,
                    draft: draftFaqs,
                    archived: archivedFaqs,
                  },
                )}
                indicatorClassName="bg-emerald-500"
              />
              <ReadinessRow
                label="Active Q&A items"
                value={activeAnswerPercent}
                helper={translateText("{active} active, {inactive} inactive", {
                  active: activeFaqItems,
                  inactive: inactiveFaqItems,
                })}
                indicatorClassName="bg-blue-500"
              />
              <ReadinessRow
                label="Client key"
                value={clientKeyReady ? 100 : 0}
                helper={
                  clientKeyReady
                    ? "Public client key is available for Portal integrations"
                    : "Generate a client key before exposing public previews"
                }
                indicatorClassName="bg-cyan-500"
              />
            </div>
          </CardContent>
        </Card>
      </div>

      <ProgressChecklistCard
        title="Launch a usable FAQ in four steps"
        description="This replaces a lecture-style setup flow with a simple sequence that shows what to do next and how much is already complete."
        steps={onboardingSteps}
        action={nextOnboardingAction}
        secondaryAction={{ label: "Open dashboard settings", to: "/app/settings/tenant" }}
      />

      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
        <MetricCard
          icon={BookOpen}
          title="FAQs"
          value={formatNumber(totalFaqs)}
          description={translateText("{count} currently published", {
            count: formatNumber(publishedFaqs),
          })}
          toneClassName="bg-blue-500/10 text-blue-600 dark:bg-blue-500/15 dark:text-blue-300"
        />
        <MetricCard
          icon={Sparkles}
          title="Published"
          value={formatNumber(publishedFaqs)}
          description={translateText("{count} still waiting in draft", {
            count: formatNumber(draftFaqs),
          })}
          toneClassName="bg-emerald-500/10 text-emerald-600 dark:bg-emerald-500/15 dark:text-emerald-300"
        />
        <MetricCard
          icon={MessageSquare}
          title="Q&A items"
          value={formatNumber(activeFaqItems)}
          description={translateText("{count} inactive items need attention", {
            count: formatNumber(inactiveFaqItems),
          })}
          toneClassName="bg-cyan-500/10 text-cyan-600 dark:bg-cyan-500/15 dark:text-cyan-300"
        />
        <MetricCard
          icon={Files}
          title="Sources"
          value={formatNumber(totalContentRefs)}
          description={translateText("{count} recent sources loaded on this page", {
            count: contentRefOverviewQuery.data?.items.length ?? 0,
          })}
          toneClassName="bg-amber-500/10 text-amber-600 dark:bg-amber-500/15 dark:text-amber-300"
        />
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.35fr)_minmax(0,1fr)] lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Assets")}</span>
                <ContextHint
                  content="The current tenant footprint across FAQs, Q&A items, and reusable sources."
                  label="Asset details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-4">
            <ChartContainer
              config={{
                total: {
                  label: translateText("Records"),
                  color: "var(--chart-1)",
                },
              }}
              className="h-[220px] w-full min-w-0 aspect-auto sm:h-[290px]"
            >
              <BarChart
                data={assetMixData}
                margin={{ top: 10, right: 12, left: -10, bottom: 0 }}
              >
                <CartesianGrid vertical={false} />
                <XAxis
                  dataKey="name"
                  axisLine={false}
                  tickLine={false}
                  tickMargin={10}
                />
                <YAxis
                  allowDecimals={false}
                  axisLine={false}
                  tickLine={false}
                />
                <ChartTooltip
                  cursor={false}
                  content={<ChartTooltipContent indicator="dot" />}
                />
                <Bar dataKey="total" radius={[12, 12, 0, 0]}>
                  {assetMixData.map((entry) => (
                    <Cell key={entry.name} fill={entry.fill} />
                  ))}
                </Bar>
              </BarChart>
            </ChartContainer>

            <div className="grid gap-3 sm:grid-cols-3">
              {assetMixData.map((asset) => (
                <div
                  key={asset.name}
                  className="rounded-2xl border border-border/70 px-4 py-3"
                >
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <span
                    className="size-2 rounded-full"
                    style={{ backgroundColor: asset.fill }}
                  />
                    {translateText(asset.name)}
                  </div>
                  <p className="mt-2 text-2xl font-semibold tracking-tight text-mono">
                    {formatNumber(asset.total)}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("FAQ status")}</span>
                <ContextHint
                  content="A compact view of what is ready for users versus what is still in editorial flow."
                  label="FAQ status details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-6">
            <ChartContainer
              config={{
                total: {
                  label: translateText("FAQs"),
                  color: "var(--chart-2)",
                },
              }}
              className="mx-auto h-[220px] w-full min-w-0 max-w-[320px] aspect-auto sm:h-[280px]"
            >
              <PieChart>
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      hideLabel
                      indicator="dot"
                      nameKey="name"
                    />
                  }
                />
                <Pie
                  data={faqLifecycleData}
                  dataKey="total"
                  nameKey="name"
                  innerRadius={68}
                  outerRadius={102}
                  paddingAngle={3}
                  strokeWidth={0}
                >
                  {faqLifecycleData.map((entry) => (
                    <Cell key={entry.name} fill={entry.fill} />
                  ))}
                </Pie>
              </PieChart>
            </ChartContainer>

            <div className="space-y-3">
              {faqLifecycleData.map((entry) => (
                <div
                  key={entry.name}
                  className="flex items-center justify-between gap-3 rounded-2xl border border-border/70 px-4 py-3"
                >
                  <div className="flex items-center gap-2">
                    <span
                      className="size-2.5 rounded-full"
                      style={{ backgroundColor: entry.fill }}
                    />
                    <span className="text-sm font-medium text-foreground">
                      {translateText(entry.name)}
                    </span>
                  </div>
                  <span className="text-sm font-semibold text-mono">
                    {formatNumber(entry.total)}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.35fr)_minmax(0,1fr)_minmax(0,1fr)] lg:gap-7.5">
        <Card>
          <CardHeader className="gap-3 sm:flex-row sm:items-start sm:justify-between">
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Top Q&A items")}</span>
                <ContextHint
                  content="Highest feedback scores in the current Q&A list."
                  label="Top Q&A item details"
                />
              </CardTitle>
            </CardHeading>
            <CardToolbar>
              <Button asChild variant="ghost" mode="link">
                <Link to="/app/faq">
                  <ArrowRight className="size-4" />
                  {translateText("Open FAQs")}
                </Link>
              </Button>
            </CardToolbar>
          </CardHeader>
          <CardContent className="space-y-3">
            {faqItemTopQuery.data?.items.length ? (
              faqItemTopQuery.data.items.map((item) => (
                <AnswerRow key={item.id} item={item} />
              ))
            ) : (
              <EmptyMiniState
                title="No Q&A items yet"
                description="Create a Q&A item to start tracking quality and coverage."
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="gap-3 sm:flex-row sm:items-start sm:justify-between">
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Recent FAQs")}</span>
                <ContextHint
                  content="The latest FAQs loaded from the Portal API."
                  label="Recent FAQ details"
                />
              </CardTitle>
            </CardHeading>
            <CardToolbar>
              <Button asChild variant="ghost" mode="link">
                <Link to="/app/faq">
                  <ArrowRight className="size-4" />
                  {translateText("View all")}
                </Link>
              </Button>
            </CardToolbar>
          </CardHeader>
          <CardContent className="space-y-3">
            {faqOverviewQuery.data?.items.length ? (
              faqOverviewQuery.data.items.map((faq) => (
                <FaqRow key={faq.id} faq={faq} />
              ))
            ) : (
              <EmptyMiniState
                title="No FAQs yet"
                description="Create a FAQ to group Q&A items by tag or product."
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Recent sources")}</span>
                <ContextHint
                  content="Recent sources available to support answer quality."
                  label="Recent source details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-3">
            {contentRefOverviewQuery.data?.items.length ? (
              contentRefOverviewQuery.data.items.map((contentRef) => (
                <SourceRow key={contentRef.id} contentRef={contentRef} />
              ))
            ) : (
              <EmptyMiniState
                title="No sources yet"
                description="Attach web pages, PDFs, docs, or repos to support your Q&A items."
              />
            )}
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_420px] lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Workspace access")}</span>
                <ContextHint
                  content="Public access and workspace metadata available from the current tenant configuration."
                  label="Workspace access details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex flex-col gap-4 rounded-2xl border border-border/70 px-4 py-3 sm:flex-row sm:items-start sm:justify-between">
              <div className="min-w-0 space-y-1.5">
                <p className="text-sm font-medium text-mono">
                  {translateText("Public client key")}
                </p>
                <p className="break-words text-sm text-muted-foreground">
                  {clientKeyQuery.data ?? translateText("No client key has been generated yet.")}
                </p>
              </div>
              <Badge
                variant={clientKeyReady ? "success" : "warning"}
                appearance="light"
              >
                {translateText(clientKeyReady ? "Ready" : "Missing")}
              </Badge>
            </div>
            {currentWorkspace ? (
              <div className="rounded-2xl border border-border/70 px-4 py-3">
                <p className="text-sm font-medium text-mono">
                  {translateText("Workspace summary")}
                </p>
                <p className="mt-1 text-sm text-muted-foreground">
                  {translateText("{name} · {edition}", {
                    name: currentWorkspace.name,
                    edition: translateText(
                      tenantEditionLabels[currentWorkspace.edition],
                    ),
                  })}
                </p>
              </div>
            ) : (
              <EmptyMiniState
                title="No active workspace"
                description="Select a workspace to review public access configuration."
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Gaps")}</span>
                <ContextHint
                  content="Current Portal limits still visible in the backend surface."
                  label="Gap details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex gap-3 rounded-2xl border border-border/70 px-4 py-3">
              <CircleAlert className="mt-0.5 size-4 shrink-0 text-amber-500" />
                <div>
                  <p className="text-sm font-medium text-mono">
                    {translateText("Member adds require existing users")}
                  </p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {translateText(
                      "Workspace memberships are live, but adding someone still requires a user account that already exists in BaseFAQ.",
                    )}
                  </p>
                </div>
              </div>
              <div className="flex gap-3 rounded-2xl border border-border/70 px-4 py-3">
                <ShieldCheck className="mt-0.5 size-4 shrink-0 text-blue-500" />
                <div>
                  <p className="text-sm font-medium text-mono">
                    {translateText("Billing is not exposed yet")}
                  </p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {translateText(
                      "Edition visibility exists, but billing and invoicing endpoints do not.",
                    )}
                  </p>
                </div>
              </div>
              <div className="rounded-2xl border border-dashed border-border px-4 py-3">
                <div className="flex items-center gap-2 text-sm font-medium text-mono">
                  <Gauge className="size-4 text-primary" />
                  {translateText("Next steps")}
                </div>
                <p className="mt-2 text-sm text-muted-foreground">
                  {translateText(
                    "Publish key FAQs, activate answers, attach source refs, then review workspace access settings.",
                  )}
                </p>
              </div>
          </CardContent>
        </Card>
      </div>
    </PageSurface>
  );
}
