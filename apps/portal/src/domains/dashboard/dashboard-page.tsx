import {
  Activity,
  ArrowRight,
  CheckCircle2,
  Clock3,
  ExternalLink,
  FolderKanban,
  MessageSquareText,
  MessagesSquare,
  Plus,
  ShieldCheck,
  Waypoints,
} from 'lucide-react';
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { useActivityList } from '@/domains/activity/hooks';
import type { ActivityDto } from '@/domains/activity/types';
import { useAnswerList } from '@/domains/answers/hooks';
import type { AnswerDto } from '@/domains/answers/types';
import { usePortalTimeZone } from '@/domains/settings/settings-hooks';
import { useQuestionList } from '@/domains/questions/hooks';
import type { QuestionDto } from '@/domains/questions/types';
import { useSourceList } from '@/domains/sources/hooks';
import type { SourceDto } from '@/domains/sources/types';
import { useSpaceList } from '@/domains/spaces/hooks';
import type { SpaceDto } from '@/domains/spaces/types';
import { useCurrentWorkspace, useTenantWorkspace } from '@/domains/tenants/hooks';
import { Button, Card, CardContent, CardDescription, CardHeader, CardHeading, CardTitle } from '@/shared/ui';
import { PageHeader, PageSurface, SectionGrid } from '@/shared/layout/page-layouts';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { formatNumericDateTimeInTimeZone } from '@/shared/lib/time-zone';
import { translateText } from '@/shared/lib/i18n-core';
import {
  ActivityKindBadge,
  ActorKindBadge,
  AnswerStatusBadge,
  QuestionStatusBadge,
  SourceKindBadge,
  SpaceKindBadge,
  VisibilityBadge,
} from '@/shared/ui/status-badges';
import {
  AnswerStatus,
  QuestionStatus,
  VisibilityScope,
} from '@/shared/constants/backend-enums';

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
    <Card className="min-h-full">
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

function SpaceRow({ space, timeZone }: { space: SpaceDto; timeZone: string }) {
  return (
    <div className="rounded-2xl border border-border/70 px-4 py-3">
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
            <p className="line-clamp-2 text-sm text-muted-foreground">{space.summary}</p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <SpaceKindBadge kind={space.kind} />
          <VisibilityBadge visibility={space.visibility} />
        </div>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">
        {translateText('Questions {count} • Last validated {value}', {
          count: space.questionCount,
          value: formatNumericDateTimeInTimeZone(space.lastValidatedAtUtc, timeZone),
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
    <div className="rounded-2xl border border-border/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/questions/${question.id}`}
            className="block text-sm font-medium text-mono hover:text-primary"
          >
            {question.title}
          </Link>
          <p className="text-sm text-muted-foreground">
            {question.spaceKey}
          </p>
          {question.summary ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">{question.summary}</p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <QuestionStatusBadge status={question.status} />
          <VisibilityBadge visibility={question.visibility} />
        </div>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        <span>
          {translateText('Feedback {value}', { value: question.feedbackScore })}
        </span>
        <span>
          {translateText('Confidence {value}', { value: question.aiConfidenceScore })}
        </span>
        {question.acceptedAnswerId ? (
          <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-emerald-700">
            {translateText('Accepted')}
          </span>
        ) : null}
        {question.duplicateOfQuestionId ? (
          <span className="rounded-full border border-border bg-muted px-2 py-0.5">
            {translateText('Duplicate')}
          </span>
        ) : null}
        <span>
          {translateText('Last activity {value}', {
            value: formatNumericDateTimeInTimeZone(question.lastActivityAtUtc, timeZone),
          })}
        </span>
      </div>
    </div>
  );
}

function AnswerRow({ answer, timeZone }: { answer: AnswerDto; timeZone: string }) {
  return (
    <div className="rounded-2xl border border-border/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/answers/${answer.id}`}
            className="block text-sm font-medium text-mono hover:text-primary"
          >
            {answer.headline}
          </Link>
          {answer.body ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">{answer.body}</p>
          ) : null}
        </div>
        <div className="space-y-2 text-right">
          <AnswerStatusBadge status={answer.status} />
          <VisibilityBadge visibility={answer.visibility} />
        </div>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        <span>{translateText('Score {value}', { value: answer.score })}</span>
        <span>{translateText('Sort {value}', { value: answer.sort })}</span>
        <span>{translateText('Vote score {value}', { value: answer.voteScore })}</span>
        <span>{translateText('Confidence {value}', { value: answer.aiConfidenceScore })}</span>
        {answer.isAccepted ? (
          <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-emerald-700">
            {translateText('Accepted')}
          </span>
        ) : null}
        <span>
          {translateText('Published {value}', {
            value: formatNumericDateTimeInTimeZone(answer.publishedAtUtc, timeZone),
          })}
        </span>
      </div>
    </div>
  );
}

function SourceRow({ source, timeZone }: { source: SourceDto; timeZone: string }) {
  return (
    <div className="rounded-2xl border border-border/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/sources/${source.id}`}
            className="block truncate text-sm font-medium text-mono hover:text-primary"
          >
            {source.label || source.locator}
          </Link>
          <p className="truncate text-sm text-muted-foreground">{source.locator}</p>
        </div>
        <div className="space-y-2 text-right">
          <SourceKindBadge kind={source.kind} />
          <VisibilityBadge visibility={source.visibility} />
        </div>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        {source.isAuthoritative ? (
          <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-emerald-700">
            {translateText('Authoritative')}
          </span>
        ) : null}
        {source.allowsPublicCitation ? (
          <span className="rounded-full border border-border bg-muted px-2 py-0.5">
            {translateText('Public citation')}
          </span>
        ) : null}
        <span>
          {translateText('Verified {value}', {
            value: formatNumericDateTimeInTimeZone(source.lastVerifiedAtUtc, timeZone),
          })}
        </span>
      </div>
    </div>
  );
}

function ActivityRow({ entry, timeZone }: { entry: ActivityDto; timeZone: string }) {
  return (
    <div className="rounded-2xl border border-border/70 px-4 py-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 space-y-1.5">
          <Link
            to={`/app/activity/${entry.id}`}
            className="block text-sm font-medium text-mono hover:text-primary"
          >
            {entry.userPrint}
          </Link>
          <p className="text-sm text-muted-foreground">
            {entry.actorLabel || translateText('Unlabeled actor')}
          </p>
          {entry.notes ? (
            <p className="line-clamp-2 text-sm text-muted-foreground">{entry.notes}</p>
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
    sorting: 'PublishedAtUtc DESC',
  });
  const publicSpacesQuery = useSpaceList({
    page: 1,
    pageSize: 1,
    visibility: VisibilityScope.Public,
    sorting: 'PublishedAtUtc DESC',
  });
  const questionsQuery = useQuestionList({
    page: 1,
    pageSize: 5,
    sorting: 'LastActivityAtUtc DESC',
    includeAnswers: true,
  });
  const pendingQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: 'LastActivityAtUtc DESC',
    status: QuestionStatus.PendingReview,
  });
  const openQuestionsQuery = useQuestionList({
    page: 1,
    pageSize: 1,
    sorting: 'LastActivityAtUtc DESC',
    status: QuestionStatus.Open,
  });
  const answersQuery = useAnswerList({
    page: 1,
    pageSize: 5,
    sorting: 'PublishedAtUtc DESC',
  });
  const publishedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: 'PublishedAtUtc DESC',
    status: AnswerStatus.Published,
  });
  const validatedAnswersQuery = useAnswerList({
    page: 1,
    pageSize: 1,
    sorting: 'ValidatedAtUtc DESC',
    status: AnswerStatus.Validated,
  });
  const sourcesQuery = useSourceList({
    page: 1,
    pageSize: 5,
    sorting: 'LastVerifiedAtUtc DESC',
  });
  const activityQuery = useActivityList({
    page: 1,
    pageSize: 6,
    sorting: 'OccurredAtUtc DESC',
  });

  const hasCriticalError =
    spacesQuery.isError ||
    questionsQuery.isError ||
    answersQuery.isError ||
    sourcesQuery.isError ||
    activityQuery.isError;

  if (hasCriticalError) {
    const error =
      spacesQuery.error ??
      questionsQuery.error ??
      answersQuery.error ??
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
            void answersQuery.refetch();
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
                {translateText('New space')}
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/app/questions/new">
                <Plus className="size-4" />
                {translateText('New question')}
              </Link>
            </Button>
            <Button asChild>
              <Link to="/app/answers/new">
                <Plus className="size-4" />
                {translateText('New answer')}
              </Link>
            </Button>
          </>
        }
      />

      <SectionGrid
        items={[
          {
            title: 'Spaces',
            value: spacesQuery.data?.totalCount ?? 0,
            description: 'Configured knowledge spaces for this workspace',
            icon: FolderKanban,
          },
          {
            title: 'Questions in review',
            value: pendingQuestionsQuery.data?.totalCount ?? 0,
            description: 'Threads waiting for moderation or approval',
            icon: Clock3,
          },
          {
            title: 'Published answers',
            value: publishedAnswersQuery.data?.totalCount ?? 0,
            description: 'Answers already live in the operational flow',
            icon: MessageSquareText,
          },
          {
            title: 'Activity events',
            value: activityQuery.data?.totalCount ?? 0,
            description: 'Audit trail and public interaction signals',
            icon: Activity,
          },
        ]}
      />

      <SectionGrid
        items={[
          {
            title: 'Public spaces',
            value: publicSpacesQuery.data?.totalCount ?? 0,
            description: 'Spaces exposed beyond internal operations',
            icon: ExternalLink,
          },
          {
            title: 'Open questions',
            value: openQuestionsQuery.data?.totalCount ?? 0,
            description: 'Threads still awaiting a resolved answer',
            icon: MessagesSquare,
          },
          {
            title: 'Validated answers',
            value: validatedAnswersQuery.data?.totalCount ?? 0,
            description: 'Answers that passed validation workflow',
            icon: CheckCircle2,
          },
          {
            title: 'Sources',
            value: sourcesQuery.data?.totalCount ?? 0,
            description: 'Reusable evidence, citations, and references',
            icon: Waypoints,
          },
        ]}
      />

      <div className="grid gap-5 xl:grid-cols-2 lg:gap-7.5">
        <DashboardSection
          title="Workspace readiness"
          description="Keep the operational prerequisites aligned before exposing QnA publicly."
          action={{ label: 'Open workspace settings', to: '/app/settings/tenant' }}
        >
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="rounded-2xl border border-border/70 px-4 py-3">
              <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
                {translateText('Workspace')}
              </p>
              <p className="mt-2 text-lg font-semibold text-mono">
                {workspace?.name ?? translateText('No workspace selected')}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {workspace?.slug
                  ? `@${workspace.slug}`
                  : translateText('Pick a workspace to load tenant-scoped QnA data.')}
              </p>
            </div>
            <div className="rounded-2xl border border-border/70 px-4 py-3">
              <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
                {translateText('Public client key')}
              </p>
              <p className="mt-2 text-lg font-semibold text-mono">
                {clientKeyQuery.data ? translateText('Live') : translateText('Missing')}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {clientKeyQuery.data
                  ? translateText('Public QnA previews and widgets can authenticate.')
                  : translateText('Generate a client key before depending on public feedback or vote flows.')}
              </p>
            </div>
            <div className="rounded-2xl border border-border/70 px-4 py-3">
              <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
                {translateText('Review guardrails')}
              </p>
              <p className="mt-2 text-lg font-semibold text-mono">
                {translateText('{count} waiting', {
                  count: pendingQuestionsQuery.data?.totalCount ?? 0,
                })}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {translateText('Pending questions indicate where moderation pressure currently sits.')}
              </p>
            </div>
            <div className="rounded-2xl border border-border/70 px-4 py-3">
              <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
                {translateText('Validation')}
              </p>
              <p className="mt-2 text-lg font-semibold text-mono">
                {translateText('{count} validated', {
                  count: validatedAnswersQuery.data?.totalCount ?? 0,
                })}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {translateText('Validated answers are the strongest candidates for trusted public visibility.')}
              </p>
            </div>
          </div>
        </DashboardSection>

        <DashboardSection
          title="Recent spaces"
          description="Spaces drive operating mode, visibility, and curated source policy."
          action={{ label: 'Open spaces', to: '/app/spaces' }}
        >
          {recentSpaces.length ? (
            recentSpaces.map((space) => (
              <SpaceRow key={space.id} space={space} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No spaces yet"
              description="Create a space to define operating mode, exposure, and curated source rules."
              action={{ label: 'Create first space', to: '/app/spaces/new' }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Question workflow"
          description="Watch pending review, duplicates, accepted answers, and customer feedback signals."
          action={{ label: 'Open questions', to: '/app/questions' }}
        >
          {recentQuestions.length ? (
            recentQuestions.map((question) => (
              <QuestionRow key={question.id} question={question} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No questions yet"
              description="Create the first question thread so answers, votes, and activity can accumulate around it."
              action={{ label: 'Create first question', to: '/app/questions/new' }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Answer publication"
          description="Track ranking, confidence, accepted answers, and answer lifecycle."
          action={{ label: 'Open answers', to: '/app/answers' }}
        >
          {recentAnswers.length ? (
            recentAnswers.map((answer) => (
              <AnswerRow key={answer.id} answer={answer} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No answers yet"
              description="Add an answer candidate so the question workflow can move toward publication and validation."
              action={{ label: 'Create first answer', to: '/app/answers/new' }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Curated sources"
          description="Sources feed evidence, citations, and public trust signals across spaces, questions, and answers."
          action={{ label: 'Open sources', to: '/app/sources' }}
        >
          {recentSources.length ? (
            recentSources.map((source) => (
              <SourceRow key={source.id} source={source} timeZone={timeZone} />
            ))
          ) : (
            <EmptyState
              title="No sources yet"
              description="Register a reusable source before attaching evidence or citations to QnA records."
              action={{ label: 'Create first source', to: '/app/sources/new' }}
            />
          )}
        </DashboardSection>

        <DashboardSection
          title="Latest activity"
          description="This feed reflects workflow operations, public feedback, votes, and audit events."
          action={{ label: 'Open activity', to: '/app/activity' }}
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
              {translateText('Operate QnA end to end')}
            </p>
            <p className="text-sm text-muted-foreground">
              {translateText(
                'Spaces define operating boundaries, questions drive workflow, answers hold publication state, sources ground evidence, and activity captures moderation plus public signals.',
              )}
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <Button asChild variant="outline">
              <Link to="/app/tags">
                <ShieldCheck className="size-4" />
                {translateText('Manage tags')}
              </Link>
            </Button>
            <Button asChild>
              <Link to="/app/activity">
                <Activity className="size-4" />
                {translateText('Review activity')}
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </PageSurface>
  );
}
