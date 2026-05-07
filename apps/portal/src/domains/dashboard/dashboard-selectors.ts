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

function queueTone(value: number): DashboardSignalTone {
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

function reusableAssetTone(value: number): DashboardSignalTone {
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
  billingSummary,
  memberCount,
}: {
  spaces: SpaceDto[];
  questionCount: number;
  draftQuestions: QuestionDto[];
  draftQuestionCount: number;
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
  activeQuestionCount,
  publicSourceCount,
  draftQuestionCount,
  activeAnswerCount,
  questionCount,
  sourceCount,
  firstActiveQuestionId,
  firstActiveAnswerId,
  firstDraftQuestionId,
  firstSpaceId,
}: {
  activeQuestionCount: number;
  publicSourceCount: number;
  draftQuestionCount: number;
  activeAnswerCount: number;
  questionCount: number;
  sourceCount: number;
  firstActiveQuestionId?: string;
  firstActiveAnswerId?: string;
  firstDraftQuestionId?: string;
  firstSpaceId?: string;
}) {
  const reusableQuestionCoverage = Math.min(
    percentage(activeQuestionCount, questionCount),
    100,
  );
  const sourceVisibility = percentage(publicSourceCount, sourceCount);
  const fallbackSpaceTo = firstSpaceId
    ? `/app/spaces/${firstSpaceId}`
    : "/app/spaces";

  return [
    {
      label: "Draft review",
      value: `${draftQuestionCount}`,
      benchmark: "Target 0 drafts",
      detail:
        draftQuestionCount > 0
          ? "Draft questions need activation or archive review."
          : "No draft questions need lifecycle review.",
      tone: queueTone(draftQuestionCount),
      to:
        draftQuestionCount > 0 && firstDraftQuestionId
          ? `/app/questions/${firstDraftQuestionId}`
          : fallbackSpaceTo,
    },
    {
      label: "Reusable questions",
      value: `${activeQuestionCount}`,
      benchmark: "Active knowledge",
      detail:
        activeQuestionCount > 0
          ? "Active questions are available for operators and automation."
          : "No active questions are ready for reuse yet.",
      progress: reusableQuestionCoverage,
      tone: ratioTone(reusableQuestionCoverage, questionCount),
      to:
        activeQuestionCount > 0 && firstActiveQuestionId
          ? `/app/questions/${firstActiveQuestionId}`
          : fallbackSpaceTo,
    },
    {
      label: "Reusable answers",
      value: `${activeAnswerCount}`,
      benchmark: "Active knowledge",
      detail:
        activeAnswerCount > 0
          ? "Active answers are available for operators and automation."
          : "No active answers are ready for reuse yet.",
      tone: reusableAssetTone(activeAnswerCount),
      to:
        activeAnswerCount > 0 && firstActiveAnswerId
          ? `/app/answers/${firstActiveAnswerId}`
          : fallbackSpaceTo,
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
