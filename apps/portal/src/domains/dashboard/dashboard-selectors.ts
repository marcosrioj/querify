import type { QuestionDto } from "@/domains/questions/types";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  AnswerStatus,
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
      label: "Review draft questions",
      description: "Activate or archive draft threads before the queue grows.",
      to: `/app/questions?status=${QuestionStatus.Draft}`,
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

export function getBusinessReadout({
  acceptedAnswerCount,
  publicSourceCount,
  openQuestionCount,
  pendingQuestionCount,
  publishedAnswerCount,
  questionCount,
  sourceCount,
}: {
  acceptedAnswerCount: number;
  publicSourceCount: number;
  openQuestionCount: number;
  pendingQuestionCount: number;
  publishedAnswerCount: number;
  questionCount: number;
  sourceCount: number;
}) {
  const demandToResolve = pendingQuestionCount + openQuestionCount;
  const questionStatus =
    pendingQuestionCount > 0 ? QuestionStatus.Draft : QuestionStatus.Active;

  return [
    {
      label: "Demand to resolve",
      value: `${demandToResolve}`,
      detail:
        demandToResolve > 0
          ? "Draft and active questions are waiting for review or an answer."
          : "No open customer demand in the current queue.",
      to:
        demandToResolve > 0
          ? `/app/questions?status=${questionStatus}`
          : "/app/questions",
    },
    {
      label: "Validation backlog",
      value: `${publishedAnswerCount}`,
      detail:
        publishedAnswerCount > 0
          ? "Published answers should be validated before they become trusted knowledge."
          : "No published answers are waiting for validation.",
      to: `/app/answers?status=${AnswerStatus.Published}`,
    },
    {
      label: "Trusted coverage",
      value: `${percentage(acceptedAnswerCount, questionCount)}%`,
      detail:
        questionCount > 0
          ? "Share of questions with an accepted answer."
          : "Create questions before measuring trusted coverage.",
      to: "/app/answers",
    },
    {
      label: "Evidence readiness",
      value: `${percentage(publicSourceCount, sourceCount)}%`,
      detail:
        sourceCount > 0
          ? "Share of sources visible publicly for reusable answers."
          : "Add sources before scaling answer publication.",
      to: "/app/sources",
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
