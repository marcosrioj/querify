import {
  ArrowUpRight,
  BookOpen,
  Bot,
  BrainCircuit,
  CircleAlert,
  Files,
  Gauge,
  MessageSquare,
  ShieldCheck,
  Sparkles,
  WandSparkles,
  type LucideIcon,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, XAxis, YAxis } from 'recharts';
import {
  useCurrentWorkspace,
  useTenantWorkspace,
} from '@/domains/tenants/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { type ContentRefDto } from '@/domains/content-refs/types';
import { useFaqItemList } from '@/domains/faq-items/hooks';
import { type FaqItemDto } from '@/domains/faq-items/types';
import { useFaqList } from '@/domains/faq/hooks';
import { type FaqDto } from '@/domains/faq/types';
import { useAuth } from '@/platform/auth/auth-context';
import { ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart';
import { Progress, ProgressRadial } from '@/components/ui/progress';
import { PageHeader, PageSurface } from '@/shared/layout/page-layouts';
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/shared/ui';
import {
  AiCommandType,
  FaqStatus,
  tenantEditionLabels,
} from '@/shared/constants/backend-enums';
import {
  ContentRefKindBadge,
  FaqStatusBadge,
} from '@/shared/ui/status-badges';

function toPercent(value: number, total: number) {
  if (total <= 0) {
    return 0;
  }

  return Math.round((value / total) * 100);
}

function formatNumber(value: number) {
  return new Intl.NumberFormat('en-US').format(value);
}

function commandLabel(value: AiCommandType) {
  return value === AiCommandType.Generation ? 'Generation' : 'Matching';
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
      <CardContent className="flex items-start justify-between gap-4 p-5">
        <div className="space-y-2">
          <p className="text-sm text-muted-foreground">{title}</p>
          <p className="text-3xl font-semibold tracking-tight text-mono">{value}</p>
          <p className="text-sm text-muted-foreground">{description}</p>
        </div>
        <div
          className={`flex size-11 shrink-0 items-center justify-center rounded-2xl ${toneClassName}`}
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
        <span className="text-sm font-medium text-foreground">{label}</span>
        <span className="text-sm font-semibold text-mono">{value}%</span>
      </div>
      <Progress
        value={value}
        className="h-2"
        indicatorClassName={indicatorClassName}
      />
      <p className="text-xs leading-5 text-muted-foreground">{helper}</p>
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
      <p className="font-medium text-mono">{title}</p>
      <p className="mt-1 text-sm text-muted-foreground">{description}</p>
    </div>
  );
}

function AnswerRow({ item }: { item: FaqItemDto }) {
  return (
    <div className="flex items-start justify-between gap-4 rounded-2xl border border-border/70 px-4 py-3">
      <div className="min-w-0 space-y-2">
        <Link
          to={`/app/faq/${item.faqId}/items/${item.id}`}
          className="line-clamp-2 text-sm font-medium text-mono hover:text-primary"
        >
          {item.question}
        </Link>
        <div className="flex flex-wrap items-center gap-2">
          <Badge variant={item.isActive ? 'success' : 'warning'} appearance="light">
            {item.isActive ? 'Active' : 'Inactive'}
          </Badge>
          <Badge variant={item.contentRefId ? 'primary' : 'outline'} appearance="light">
            {item.contentRefId ? 'Source linked' : 'No source'}
          </Badge>
        </div>
      </div>
      <div className="space-y-1 text-right text-xs text-muted-foreground">
        <div className="font-semibold text-mono">Vote {item.voteScore}</div>
        <div className="font-semibold text-mono">
          AI {item.aiConfidenceScore}
        </div>
      </div>
    </div>
  );
}

function FaqRow({ faq }: { faq: FaqDto }) {
  return (
    <div className="flex items-start justify-between gap-4 rounded-2xl border border-border/70 px-4 py-3">
      <div className="min-w-0 space-y-1.5">
        <Link
          to={`/app/faq/${faq.id}`}
          className="truncate text-sm font-medium text-mono hover:text-primary"
        >
          {faq.name}
        </Link>
        <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
          {faq.language}
        </p>
      </div>
      <FaqStatusBadge status={faq.status} />
    </div>
  );
}

function SourceRow({ contentRef }: { contentRef: ContentRefDto }) {
  return (
    <div className="rounded-2xl border border-border/70 px-4 py-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <p className="truncate text-sm font-medium text-mono">
            {contentRef.label || 'Untitled source'}
          </p>
          <p className="truncate text-sm text-muted-foreground">
            {contentRef.locator}
          </p>
        </div>
        <ContentRefKindBadge kind={contentRef.kind} />
      </div>
      <p className="mt-2 text-xs text-muted-foreground">
        {contentRef.scope || 'No scope assigned'}
      </p>
    </div>
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const currentWorkspace = useCurrentWorkspace();
  const { aiProvidersQuery, clientKeyQuery } = useTenantWorkspace();
  const faqOverviewQuery = useFaqList({
    page: 1,
    pageSize: 4,
    sorting: 'UpdatedDate DESC',
  });
  const faqPublishedQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: 'UpdatedDate DESC',
    status: FaqStatus.Published,
  });
  const faqDraftQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: 'UpdatedDate DESC',
    status: FaqStatus.Draft,
  });
  const faqArchivedQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: 'UpdatedDate DESC',
    status: FaqStatus.Archived,
  });
  const faqItemTopQuery = useFaqItemList({
    page: 1,
    pageSize: 5,
    sorting: 'VoteScore DESC',
  });
  const faqItemActiveQuery = useFaqItemList({
    page: 1,
    pageSize: 1,
    sorting: 'UpdatedDate DESC',
    isActive: true,
  });
  const contentRefOverviewQuery = useContentRefList({
    page: 1,
    pageSize: 5,
    sorting: 'UpdatedDate DESC',
  });

  const totalFaqs = faqOverviewQuery.data?.totalCount ?? 0;
  const publishedFaqs = faqPublishedQuery.data?.totalCount ?? 0;
  const draftFaqs = faqDraftQuery.data?.totalCount ?? 0;
  const archivedFaqs = faqArchivedQuery.data?.totalCount ?? 0;
  const totalFaqItems = faqItemTopQuery.data?.totalCount ?? 0;
  const activeFaqItems = faqItemActiveQuery.data?.totalCount ?? 0;
  const inactiveFaqItems = Math.max(totalFaqItems - activeFaqItems, 0);
  const totalContentRefs = contentRefOverviewQuery.data?.totalCount ?? 0;
  const aiProviders = aiProvidersQuery.data ?? [];
  const configuredAiProviders = aiProviders.filter(
    (provider) => provider.isAiProviderKeyConfigured,
  ).length;
  const clientKeyReady = Boolean(clientKeyQuery.data);
  const publishedFaqPercent = toPercent(publishedFaqs, totalFaqs);
  const activeAnswerPercent = toPercent(activeFaqItems, totalFaqItems);
  const providerCoveragePercent = toPercent(
    configuredAiProviders,
    aiProviders.length,
  );
  const readinessScore = Math.round(
    (
      publishedFaqPercent +
      activeAnswerPercent +
      providerCoveragePercent +
      (clientKeyReady ? 100 : 0)
    ) / 4,
  );

  const assetMixData = [
    {
      name: 'FAQs',
      total: totalFaqs,
      fill: 'var(--chart-1)',
    },
    {
      name: 'Answers',
      total: totalFaqItems,
      fill: 'var(--chart-2)',
    },
    {
      name: 'Sources',
      total: totalContentRefs,
      fill: 'var(--chart-3)',
    },
  ];
  const faqLifecycleData = [
    {
      name: 'Published',
      total: publishedFaqs,
      fill: 'var(--chart-2)',
    },
    {
      name: 'Draft',
      total: draftFaqs,
      fill: 'var(--chart-3)',
    },
    {
      name: 'Archived',
      total: archivedFaqs,
      fill: 'var(--chart-4)',
    },
  ];
  const heroHighlights = [
    {
      label: 'Published portfolio',
      value: `${publishedFaqPercent}%`,
      description: `${publishedFaqs} of ${totalFaqs} FAQs are live`,
    },
    {
      label: 'Inactive answers',
      value: formatNumber(inactiveFaqItems),
      description: 'Answers still blocked from end users',
    },
    {
      label: 'Provider keys secured',
      value: `${providerCoveragePercent}%`,
      description: `${configuredAiProviders} of ${aiProviders.length} providers ready`,
    },
  ];

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        eyebrow="Dashboard"
        title={`Knowledge operations${user?.name ? ` for ${user.name}` : ''}`}
        description="A tighter operational view of tenant FAQs, answer health, source coverage, and AI setup using only the current Portal contracts."
        actions={
          <>
            <Button asChild>
              <Link to="/app/faq/new">
                <WandSparkles className="size-4" />
                Create FAQ
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/app/settings/tenant">Tenant settings</Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.5fr)_380px] lg:gap-7.5">
        <Card
          className="relative overflow-hidden border-none text-white shadow-xl shadow-slate-950/10"
          style={{
            backgroundImage:
              'linear-gradient(135deg, hsl(205 92% 47%) 0%, hsl(221 83% 29%) 55%, hsl(224 64% 18%) 100%)',
          }}
        >
          <div className="pointer-events-none absolute inset-0 overflow-hidden">
            <div className="absolute -top-12 right-0 h-48 w-48 rounded-full bg-white/10 blur-3xl" />
            <div className="absolute bottom-0 left-12 h-40 w-40 rounded-full bg-cyan-200/15 blur-3xl" />
          </div>
          <CardContent className="relative space-y-8 p-6 lg:p-7.5">
            <div className="flex flex-wrap items-center gap-2 text-xs font-medium uppercase tracking-[0.24em] text-white/70">
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-[0.6875rem] tracking-[0.2em] text-white">
                {currentWorkspace?.slug ?? 'workspace-pending'}
              </span>
              {currentWorkspace ? (
                <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-[0.6875rem] tracking-[0.2em] text-white">
                  {currentWorkspace.isActive ? 'Active workspace' : 'Inactive workspace'}
                </span>
              ) : null}
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-[0.6875rem] tracking-[0.2em] text-white">
                {user?.role ?? 'Member'} access
              </span>
            </div>

            <div className="max-w-3xl space-y-3">
              <div className="flex flex-wrap items-center gap-3">
                <h2 className="text-2xl font-semibold tracking-tight lg:text-3xl">
                  {currentWorkspace?.name ?? 'Set up your tenant workspace'}
                </h2>
                {currentWorkspace ? (
                  <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-xs font-medium text-white">
                    {tenantEditionLabels[currentWorkspace.edition]}
                  </span>
                ) : null}
              </div>
              <p className="max-w-2xl text-sm leading-6 text-white/78">
                Focus the Portal on what matters for BaseFAQ operations: publishable
                FAQ coverage, answer activation, connected source material, and
                AI providers that are ready for tenant traffic.
              </p>
            </div>

            <div className="flex flex-wrap gap-3">
              <Button
                asChild
                className="border-white/20 bg-white text-slate-950 shadow-none hover:bg-white/90"
              >
                <Link to="/app/faq">
                  Open FAQ workspace
                  <ArrowUpRight className="size-4" />
                </Link>
              </Button>
              <Button
                asChild
                variant="outline"
                className="border-white/20 bg-white/10 text-white hover:bg-white/15"
              >
                <Link to="/app/settings/tenant">Review AI configuration</Link>
              </Button>
            </div>

            <div className="grid gap-3 md:grid-cols-3">
              {heroHighlights.map((highlight) => (
                <div
                  key={highlight.label}
                  className="rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-xs"
                >
                  <p className="text-xs uppercase tracking-[0.22em] text-white/65">
                    {highlight.label}
                  </p>
                  <p className="mt-2 text-2xl font-semibold tracking-tight">
                    {highlight.value}
                  </p>
                  <p className="mt-1 text-sm text-white/70">
                    {highlight.description}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Launch readiness</CardTitle>
            <CardDescription>
              Readiness is weighted across published FAQs, active answers, AI
              provider keys, and public client key availability.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="flex items-center gap-5">
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
                    Ready
                  </div>
                </div>
              </ProgressRadial>

              <div className="space-y-3">
                <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                  <Gauge className="size-4 text-primary" />
                  Tenant knowledge is operationally visible.
                </div>
                <p className="text-sm leading-6 text-muted-foreground">
                  This score stays grounded in current Portal APIs instead of
                  placeholder growth metrics.
                </p>
                <div className="flex flex-wrap gap-2">
                  <Badge variant={clientKeyReady ? 'success' : 'warning'} appearance="light">
                    {clientKeyReady ? 'Client key live' : 'Client key missing'}
                  </Badge>
                  <Badge
                    variant={configuredAiProviders > 0 ? 'primary' : 'outline'}
                    appearance="light"
                  >
                    {configuredAiProviders} secured providers
                  </Badge>
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <ReadinessRow
                label="Published FAQ coverage"
                value={publishedFaqPercent}
                helper={`${publishedFaqs} published, ${draftFaqs} draft, ${archivedFaqs} archived`}
                indicatorClassName="bg-emerald-500"
              />
              <ReadinessRow
                label="Active answer coverage"
                value={activeAnswerPercent}
                helper={`${activeFaqItems} active answers, ${inactiveFaqItems} inactive`}
                indicatorClassName="bg-blue-500"
              />
              <ReadinessRow
                label="AI provider key coverage"
                value={providerCoveragePercent}
                helper={`${configuredAiProviders} providers secured for tenant use`}
                indicatorClassName="bg-cyan-500"
              />
              <ReadinessRow
                label="Client app key"
                value={clientKeyReady ? 100 : 0}
                helper={
                  clientKeyReady
                    ? 'Public client key is available for Portal integrations'
                    : 'Generate a client key before embedding tenant AI features'
                }
                indicatorClassName="bg-amber-500"
              />
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
        <MetricCard
          icon={BookOpen}
          title="FAQ spaces"
          value={formatNumber(totalFaqs)}
          description={`${formatNumber(publishedFaqs)} currently published`}
          toneClassName="bg-blue-500/10 text-blue-600 dark:bg-blue-500/15 dark:text-blue-300"
        />
        <MetricCard
          icon={Sparkles}
          title="Published FAQs"
          value={formatNumber(publishedFaqs)}
          description={`${formatNumber(draftFaqs)} still waiting in draft`}
          toneClassName="bg-emerald-500/10 text-emerald-600 dark:bg-emerald-500/15 dark:text-emerald-300"
        />
        <MetricCard
          icon={MessageSquare}
          title="Live answers"
          value={formatNumber(activeFaqItems)}
          description={`${formatNumber(inactiveFaqItems)} inactive answers need attention`}
          toneClassName="bg-cyan-500/10 text-cyan-600 dark:bg-cyan-500/15 dark:text-cyan-300"
        />
        <MetricCard
          icon={Files}
          title="Source refs"
          value={formatNumber(totalContentRefs)}
          description={`${contentRefOverviewQuery.data?.items.length ?? 0} recent sources loaded on this page`}
          toneClassName="bg-amber-500/10 text-amber-600 dark:bg-amber-500/15 dark:text-amber-300"
        />
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.35fr)_minmax(0,1fr)] lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardTitle>Knowledge asset mix</CardTitle>
            <CardDescription>
              The current tenant footprint across FAQ spaces, answer rows, and
              reusable source material.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <ChartContainer
              config={{
                total: {
                  label: 'Records',
                  color: 'var(--chart-1)',
                },
              }}
              className="min-h-[290px] w-full"
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
                <YAxis allowDecimals={false} axisLine={false} tickLine={false} />
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
                    {asset.name}
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
            <CardTitle>FAQ lifecycle</CardTitle>
            <CardDescription>
              A compact view of what is ready for users versus what is still in
              editorial flow.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <ChartContainer
              config={{
                total: {
                  label: 'FAQs',
                  color: 'var(--chart-2)',
                },
              }}
              className="mx-auto min-h-[280px] w-full max-w-[320px]"
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
                      {entry.name}
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
          <CardHeader className="flex-row items-start justify-between gap-3">
            <div className="space-y-1.5">
              <CardTitle>Answer leaderboard</CardTitle>
              <CardDescription>
                Highest vote scores in the current tenant answer catalog.
              </CardDescription>
            </div>
            <Button asChild variant="ghost" mode="link">
              <Link to="/app/faq">Open FAQ workspace</Link>
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {faqItemTopQuery.data?.items.length ? (
              faqItemTopQuery.data.items.map((item) => (
                <AnswerRow key={item.id} item={item} />
              ))
            ) : (
              <EmptyMiniState
                title="No answers ranked yet"
                description="Create FAQ items to start measuring answer quality and coverage."
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex-row items-start justify-between gap-3">
            <div className="space-y-1.5">
              <CardTitle>Recent FAQs</CardTitle>
              <CardDescription>
                The latest FAQ spaces loaded from the Portal API.
              </CardDescription>
            </div>
            <Button asChild variant="ghost" mode="link">
              <Link to="/app/faq">View all</Link>
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {faqOverviewQuery.data?.items.length ? (
              faqOverviewQuery.data.items.map((faq) => (
                <FaqRow key={faq.id} faq={faq} />
              ))
            ) : (
              <EmptyMiniState
                title="No FAQs created"
                description="Start a knowledge collection to organize answers per topic or product."
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Source intake</CardTitle>
            <CardDescription>
              Recent content references available to support generation quality.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {contentRefOverviewQuery.data?.items.length ? (
              contentRefOverviewQuery.data.items.map((contentRef) => (
                <SourceRow key={contentRef.id} contentRef={contentRef} />
              ))
            ) : (
              <EmptyMiniState
                title="No source material yet"
                description="Attach web pages, PDFs, documents, or repositories to improve answer grounding."
              />
            )}
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_420px] lg:gap-7.5">
        <Card>
          <CardHeader>
            <CardTitle>AI provider stack</CardTitle>
            <CardDescription>
              Provider credentials currently configured for this workspace.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {aiProviders.length ? (
              aiProviders.map((provider) => (
                <div
                  key={provider.id}
                  className="flex items-start justify-between gap-4 rounded-2xl border border-border/70 px-4 py-3"
                >
                  <div className="min-w-0 space-y-1.5">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="text-sm font-medium text-mono">
                        {provider.provider}
                      </p>
                      <Badge variant="outline" appearance="light">
                        {commandLabel(provider.command)}
                      </Badge>
                    </div>
                    <p className="truncate text-sm text-muted-foreground">
                      {provider.model}
                    </p>
                  </div>
                  <Badge
                    variant={
                      provider.isAiProviderKeyConfigured ? 'success' : 'warning'
                    }
                    appearance="light"
                  >
                    {provider.isAiProviderKeyConfigured ? 'Secured' : 'Needs key'}
                  </Badge>
                </div>
              ))
            ) : (
              <EmptyMiniState
                title="No AI providers configured"
                description="Connect at least one provider in tenant settings before enabling AI workflows."
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Known gaps</CardTitle>
            <CardDescription>
              Current Portal limits still visible in the backend surface.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex gap-3 rounded-2xl border border-border/70 px-4 py-3">
              <CircleAlert className="mt-0.5 size-4 shrink-0 text-amber-500" />
              <div>
                <p className="text-sm font-medium text-mono">Members API is still temporary</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Portal member management remains backed by the temporary adapter.
                </p>
              </div>
            </div>
            <div className="flex gap-3 rounded-2xl border border-border/70 px-4 py-3">
              <ShieldCheck className="mt-0.5 size-4 shrink-0 text-blue-500" />
              <div>
                <p className="text-sm font-medium text-mono">Billing is not exposed yet</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Edition visibility exists, but billing and invoicing endpoints do not.
                </p>
              </div>
            </div>
            <div className="flex gap-3 rounded-2xl border border-border/70 px-4 py-3">
              <BrainCircuit className="mt-0.5 size-4 shrink-0 text-cyan-500" />
              <div>
                <p className="text-sm font-medium text-mono">AI jobs have no history endpoint</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Generation requests can be triggered, but job listings and status dashboards are not exposed.
                </p>
              </div>
            </div>
            <div className="rounded-2xl border border-dashed border-border px-4 py-3">
              <div className="flex items-center gap-2 text-sm font-medium text-mono">
                <Bot className="size-4 text-primary" />
                Highest signal setup path
              </div>
              <p className="mt-2 text-sm text-muted-foreground">
                Publish key FAQs, activate answers, attach source refs, then secure tenant AI providers.
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </PageSurface>
  );
}
