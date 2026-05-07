import type { QuestionDto } from "@/domains/questions/types";
import type { SpaceDto } from "@/domains/spaces/types";
import { TenantSubscriptionStatus } from "@/shared/constants/backend-enums";
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

export type DashboardSignalTone =
  | "danger"
  | "info"
  | "neutral"
  | "success"
  | "warning";

function demandTone(value: number): DashboardSignalTone {
  if (value === 0) {
    return "success";
  }

  if (value >= 10) {
    return "danger";
  }

  return "warning";
}

function ratioTone(value: number, total: number): DashboardSignalTone {
  if (total === 0) {
    return "neutral";
  }

  if (value >= 80) {
    return "success";
  }

  if (value >= 50) {
    return "warning";
  }

  return "danger";
}

function activeAnswersTone(value: number): DashboardSignalTone {
  if (value > 0) {
    return "success";
  }

  return "neutral";
}

function pickChartFill(index: number) {
  return chartFills[index % chartFills.length];
}

export type PortalActivationState = {
  hasProfile: boolean;
  hasSpace: boolean;
  hasTeammate: boolean;
  hasQuestion: boolean;
  hasActiveAnswer: boolean;
};

export function getSetupProgress(state: PortalActivationState) {
  const values = Object.values(state);
  const completeCount = values.filter(Boolean).length;

  return Math.round((completeCount / values.length) * 100);
}

export function getActivationState({
  hasProfile,
  memberCount,
  spaceCount,
  questionCount,
  activeAnswerCount,
}: {
  hasProfile: boolean;
  memberCount: number;
  spaceCount: number;
  questionCount: number;
  activeAnswerCount: number;
}): PortalActivationState {
  return {
    hasProfile,
    hasSpace: spaceCount > 0,
    hasTeammate: memberCount > 1,
    hasQuestion: questionCount > 0,
    hasActiveAnswer: activeAnswerCount > 0,
  };
}

export function getRoleAwareNextAction({
  spaces,
  questionCount,
  draftQuestions,
  draftQuestionCount,
  openQuestions,
  billingSummary,
  memberCount,
}: {
  spaces: SpaceDto[];
  questionCount: number;
  draftQuestions: QuestionDto[];
  draftQuestionCount: number;
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

  if (questionCount === 0) {
    return {
      label: "Open Space",
      description:
        "Create the first question inside a Space so it inherits intake rules.",
      to: `/app/spaces/${questionSpace.id}`,
    };
  }

  if (draftQuestionCount > 0) {
    return {
      label: "Review draft questions",
      description:
        "Activate or archive draft questions before the queue grows.",
      to: draftQuestions[0]
        ? `/app/questions/${draftQuestions[0].id}`
        : `/app/spaces/${questionSpace.id}`,
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
    to: `/app/spaces/${questionSpace.id}`,
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
  publicSourceCount,
  openQuestionCount,
  draftQuestionCount,
  activeAnswerCount,
  questionCount,
  sourceCount,
  firstActiveAnswerId,
  firstDemandQuestionId,
  firstSpaceId,
}: {
  publicSourceCount: number;
  openQuestionCount: number;
  draftQuestionCount: number;
  activeAnswerCount: number;
  questionCount: number;
  sourceCount: number;
  firstActiveAnswerId?: string;
  firstDemandQuestionId?: string;
  firstSpaceId?: string;
}) {
  const demandToResolve = draftQuestionCount + openQuestionCount;
  const answerCoverage = Math.min(
    percentage(activeAnswerCount, questionCount),
    100,
  );
  const sourceVisibility = percentage(publicSourceCount, sourceCount);
  const fallbackSpaceTo = firstSpaceId
    ? `/app/spaces/${firstSpaceId}`
    : "/app/spaces";

  return [
    {
      label: "Demand to resolve",
      value: `${demandToResolve}`,
      benchmark: "Target 0 waiting",
      detail:
        demandToResolve > 0
          ? "Draft and active questions are waiting to be activated or answered."
          : "No open customer demand in the current queue.",
      tone: demandTone(demandToResolve),
      to:
        demandToResolve > 0 && firstDemandQuestionId
          ? `/app/questions/${firstDemandQuestionId}`
          : fallbackSpaceTo,
    },
    {
      label: "Active answers",
      value: `${activeAnswerCount}`,
      benchmark: "Reusable knowledge",
      detail:
        activeAnswerCount > 0
          ? "Active answers are ready for reuse across customer journeys."
          : "No active answers are ready for reuse yet.",
      tone: activeAnswersTone(activeAnswerCount),
      to:
        activeAnswerCount > 0 && firstActiveAnswerId
          ? `/app/answers/${firstActiveAnswerId}`
          : fallbackSpaceTo,
    },
    {
      label: "Answer coverage",
      value: `${answerCoverage}%`,
      benchmark: "Active answers",
      detail:
        questionCount > 0
          ? "Active reusable answers compared with the current question count."
          : "Create questions before measuring answer coverage.",
      progress: answerCoverage,
      tone: ratioTone(answerCoverage, questionCount),
      to: fallbackSpaceTo,
    },
    {
      label: "Source visibility",
      value: `${sourceVisibility}%`,
      benchmark: "Optional context",
      detail:
        sourceCount > 0
          ? "Share of optional source records visible publicly."
          : "Sources are optional and can be added when answers need references.",
      progress: sourceVisibility,
      tone: ratioTone(sourceVisibility, sourceCount),
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
