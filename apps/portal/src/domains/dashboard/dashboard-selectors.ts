import type { ActivityDto } from "@/domains/activity/types";
import type { QuestionDto } from "@/domains/questions/types";
import type { SourceDto } from "@/domains/sources/types";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  ActivityKind,
  QuestionStatus,
  TenantSubscriptionStatus,
} from "@/shared/constants/backend-enums";
import type { TenantBillingSummaryDto } from "@/domains/billing/types";

const chartFills = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
  "var(--color-info)",
  "var(--color-warning)",
  "var(--color-destructive-accent)",
];

function percentage(value: number, total: number) {
  return total > 0 ? Math.round((value / total) * 100) : 0;
}

function pickChartFill(index: number) {
  return chartFills[index % chartFills.length];
}

function hasQuestionWorkflowPressure(status: QuestionStatus) {
  return (
    status === QuestionStatus.Draft ||
    status === QuestionStatus.PendingReview ||
    status === QuestionStatus.Open ||
    status === QuestionStatus.Escalated
  );
}

export type PortalActivationState = {
  hasProfile: boolean;
  hasSpace: boolean;
  hasSource: boolean;
  hasTeammate: boolean;
  hasQuestion: boolean;
  hasTrustedAnswer: boolean;
};

export function getSetupProgress(state: PortalActivationState) {
  const values = Object.values(state);
  const completeCount = values.filter(Boolean).length;

  return Math.round((completeCount / values.length) * 100);
}

export function getActivationState({
  hasProfile,
  memberCount,
  sourceCount,
  spaceCount,
  questionCount,
  trustedAnswerCount,
}: {
  hasProfile: boolean;
  memberCount: number;
  sourceCount: number;
  spaceCount: number;
  questionCount: number;
  trustedAnswerCount: number;
}): PortalActivationState {
  return {
    hasProfile,
    hasSpace: spaceCount > 0,
    hasSource: sourceCount > 0,
    hasTeammate: memberCount > 1,
    hasQuestion: questionCount > 0,
    hasTrustedAnswer: trustedAnswerCount > 0,
  };
}

export function getRoleAwareNextAction({
  spaces,
  sourceCount,
  questionCount,
  pendingQuestionCount,
  openQuestions,
  billingSummary,
  memberCount,
}: {
  spaces: SpaceDto[];
  sourceCount: number;
  questionCount: number;
  pendingQuestionCount: number;
  openQuestions: QuestionDto[];
  billingSummary?: TenantBillingSummaryDto | null;
  memberCount: number;
}) {
  const subscriptionStatus = billingSummary?.subscriptionStatus;

  if (spaces.length === 0) {
    return {
      label: "Start here",
      description:
        "Create the first space so questions and answers have a home.",
      to: "/app/spaces/new",
    };
  }

  const questionSpace =
    spaces.find((space) => space.acceptsQuestions) ?? spaces[0];

  if (sourceCount === 0) {
    return {
      label: "Add first source",
      description: "Add trusted evidence before answers start circulating.",
      to: "/app/sources/new",
    };
  }

  if (questionCount === 0) {
    return {
      label: "Open Space",
      description:
        "Create the first question inside a Space so it inherits intake rules.",
      to: `/app/spaces/${questionSpace.id}`,
    };
  }

  if (pendingQuestionCount > 0) {
    return {
      label: "Review pending questions",
      description: "Clear moderation decisions before the queue grows.",
      to: `/app/questions?status=${QuestionStatus.PendingReview}`,
    };
  }

  if (openQuestions.length > 0) {
    return {
      label: "Add answer",
      description: "Open questions are waiting for a trusted response.",
      to: `/app/questions/${openQuestions[0].id}`,
    };
  }

  if (
    subscriptionStatus === TenantSubscriptionStatus.PastDue ||
    subscriptionStatus === TenantSubscriptionStatus.Unpaid
  ) {
    return {
      label: "Review billing",
      description:
        "Restore entitlement confidence before operational work expands.",
      to: "/app/billing",
    };
  }

  if (memberCount <= 1) {
    return {
      label: "Invite teammate",
      description: "Bring one more teammate into the workflow.",
      to: "/app/members",
    };
  }

  return {
    label: "Review activity",
    description: "Use recent signals to keep trusted answers fresh.",
    to: "/app/activity",
  };
}

export function getDashboardKpis({
  activity,
  answeredQuestionCount,
  openQuestionCount,
  publishedAnswerCount,
  citableSourceCount,
  pendingQuestionCount,
  questionCount,
  spaces,
  sourceCount,
  validatedAnswerCount,
}: {
  activity: ActivityDto[];
  answeredQuestionCount: number;
  openQuestionCount: number;
  publishedAnswerCount: number;
  citableSourceCount: number;
  pendingQuestionCount: number;
  questionCount: number;
  spaces: SpaceDto[];
  sourceCount: number;
  validatedAnswerCount: number;
}) {
  const activeSpaces = spaces.filter(
    (space) => space.acceptsAnswers || space.acceptsQuestions,
  ).length;

  return {
    activeSpaces,
    pendingQuestionCount,
    openQuestionCount,
    validatedAnswerCount,
    citableSourceCount,
    recentActivityCount: activity.length,
    publishedAnswers: publishedAnswerCount,
    sourceCount,
    answeredCoverage: percentage(answeredQuestionCount, questionCount),
    sourceReadiness: percentage(citableSourceCount, sourceCount),
  };
}

export function getQuestionLifecycleData({
  draft,
  pending,
  open,
  answered,
  validated,
  escalated,
  duplicate,
  archived,
}: {
  draft: number;
  pending: number;
  open: number;
  answered: number;
  validated: number;
  escalated: number;
  duplicate: number;
  archived: number;
}) {
  const rows = [
    {
      key: "pending",
      label: "Pending review",
      status: QuestionStatus.PendingReview,
      value: pending,
      fill: "var(--color-info)",
    },
    {
      key: "open",
      label: "Open",
      status: QuestionStatus.Open,
      value: open,
      fill: "var(--chart-2)",
    },
    {
      key: "answered",
      label: "Answered",
      status: QuestionStatus.Answered,
      value: answered,
      fill: "var(--chart-1)",
    },
    {
      key: "validated",
      label: "Validated",
      status: QuestionStatus.Validated,
      value: validated,
      fill: "var(--chart-5)",
    },
    {
      key: "escalated",
      label: "Escalated",
      status: QuestionStatus.Escalated,
      value: escalated,
      fill: "var(--color-destructive-accent)",
    },
    {
      key: "draft",
      label: "Draft",
      status: QuestionStatus.Draft,
      value: draft,
      fill: "var(--color-warning)",
    },
    {
      key: "duplicate",
      label: "Duplicate",
      status: QuestionStatus.Duplicate,
      value: duplicate,
      fill: "var(--muted-foreground)",
    },
    {
      key: "archived",
      label: "Archived",
      status: QuestionStatus.Archived,
      value: archived,
      fill: "var(--border)",
    },
  ];

  return rows.filter((row) => row.value > 0);
}

export function getAnswerTrustFunnelData({
  answerCount,
  publishedAnswerCount,
  validatedAnswerCount,
  acceptedAnswerCount,
}: {
  answerCount: number;
  publishedAnswerCount: number;
  validatedAnswerCount: number;
  acceptedAnswerCount: number;
}) {
  return [
    {
      key: "answers",
      label: "Answer candidates",
      value: answerCount,
      fill: "var(--chart-2)",
    },
    {
      key: "published",
      label: "Published",
      value: publishedAnswerCount,
      fill: "var(--chart-1)",
    },
    {
      key: "validated",
      label: "Validated",
      value: validatedAnswerCount,
      fill: "var(--chart-5)",
    },
    {
      key: "accepted",
      label: "Accepted",
      value: acceptedAnswerCount,
      fill: "var(--color-success)",
    },
  ];
}

export function getEvidenceReadinessData({
  citableSourceCount,
  sourceCount,
}: {
  citableSourceCount: number;
  sourceCount: number;
}) {
  const nonCitableSourceCount = Math.max(sourceCount - citableSourceCount, 0);

  return [
    {
      key: "citable",
      label: "Citable",
      value: citableSourceCount,
      fill: "var(--chart-1)",
    },
    {
      key: "non-citable",
      label: "Needs citation approval",
      value: nonCitableSourceCount,
      fill: "var(--chart-3)",
    },
  ].filter((row) => row.value > 0);
}

export function getSpaceWorkloadData(spaces: SpaceDto[]) {
  return [...spaces]
    .sort((left, right) => right.questionCount - left.questionCount)
    .filter((space) => space.questionCount > 0)
    .slice(0, 6)
    .map((space, index) => ({
      key: space.id,
      label: space.name,
      value: space.questionCount,
      fill: pickChartFill(index),
      to: `/app/spaces/${space.id}`,
    }));
}

export function getSourceUtilizationData(sources: SourceDto[]) {
  return [...sources]
    .map((source) => ({
      key: source.id,
      label: source.label || source.locator,
      value:
        source.spaceUsageCount +
        source.questionUsageCount +
        source.answerUsageCount,
      fill: source.allowsCitation ? "var(--chart-1)" : "var(--chart-3)",
      to: `/app/sources/${source.id}`,
    }))
    .filter((source) => source.value > 0)
    .sort((left, right) => right.value - left.value)
    .slice(0, 6);
}

export function getActivityMixData(activity: ActivityDto[]) {
  const groups = [
    {
      key: "question",
      label: "Question workflow",
      value: 0,
      fill: "var(--chart-2)",
    },
    {
      key: "answer",
      label: "Answer lifecycle",
      value: 0,
      fill: "var(--chart-1)",
    },
    {
      key: "signal",
      label: "Public signals",
      value: 0,
      fill: "var(--chart-3)",
    },
    {
      key: "risk",
      label: "Risk events",
      value: 0,
      fill: "var(--color-destructive-accent)",
    },
  ];

  for (const entry of activity) {
    if (
      entry.kind === ActivityKind.QuestionEscalated ||
      entry.kind === ActivityKind.ReportReceived ||
      entry.kind === ActivityKind.AnswerRejected
    ) {
      groups[3].value += 1;
    } else if (
      entry.kind === ActivityKind.FeedbackReceived ||
      entry.kind === ActivityKind.VoteReceived
    ) {
      groups[2].value += 1;
    } else if (
      entry.kind >= ActivityKind.AnswerCreated &&
      entry.kind <= ActivityKind.AnswerRetired
    ) {
      groups[1].value += 1;
    } else {
      groups[0].value += 1;
    }
  }

  return groups.filter((row) => row.value > 0);
}

export function getBusinessReadout({
  citableSourceCount,
  openQuestionCount,
  publishedAnswerCount,
  questionLifecycle,
  questionCount,
  sourceCount,
  spaces,
  validatedAnswerCount,
}: {
  citableSourceCount: number;
  openQuestionCount: number;
  publishedAnswerCount: number;
  questionLifecycle: Array<{ status: QuestionStatus; value: number }>;
  questionCount: number;
  sourceCount: number;
  spaces: SpaceDto[];
  validatedAnswerCount: number;
}) {
  const workflowPressure = questionLifecycle
    .filter((row) => hasQuestionWorkflowPressure(row.status))
    .reduce((total, row) => total + row.value, 0);
  const topSpace = [...spaces].sort(
    (left, right) => right.questionCount - left.questionCount,
  )[0];
  const topSpaceShare = topSpace
    ? percentage(topSpace.questionCount, Math.max(questionCount, 1))
    : 0;

  return [
    {
      label: "Moderation pressure",
      value: `${workflowPressure}`,
      detail:
        workflowPressure > 0
          ? "Draft, pending, open, or escalated threads need operator action."
          : "No visible workflow pressure in the current workspace.",
    },
    {
      label: "Validation backlog",
      value: `${publishedAnswerCount}`,
      detail:
        publishedAnswerCount > 0
          ? "Published answers should be validated before they become trusted knowledge."
          : "No published answers are waiting for validation.",
    },
    {
      label: "Evidence readiness",
      value: `${percentage(citableSourceCount, sourceCount)}%`,
      detail:
        sourceCount > 0
          ? "Share of sources already approved for citation."
          : "Add sources before scaling answer publication.",
    },
    {
      label: "Knowledge concentration",
      value: topSpace ? `${topSpaceShare}%` : "0%",
      detail: topSpace
        ? "Largest space carries the highest question share."
        : "Create spaces to understand demand concentration.",
    },
    {
      label: "Trusted answer base",
      value: `${validatedAnswerCount}`,
      detail:
        validatedAnswerCount > 0
          ? "Validated answers are ready for reuse across customer journeys."
          : "No trusted answers have been validated yet.",
    },
    {
      label: "Open answer demand",
      value: `${openQuestionCount}`,
      detail:
        openQuestionCount > 0
          ? "Open questions still need accepted or validated answers."
          : "No answer demand in the current queue.",
    },
  ];
}

export function getBillingNeedsAttention(
  summary?: TenantBillingSummaryDto | null,
) {
  if (!summary) {
    return false;
  }

  return (
    summary.subscriptionStatus === TenantSubscriptionStatus.PastDue ||
    summary.subscriptionStatus === TenantSubscriptionStatus.Unpaid ||
    summary.entitlement?.isActive === false
  );
}
