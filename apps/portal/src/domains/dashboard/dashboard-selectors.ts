import type { QuestionDto } from "@/domains/questions/types";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  SpaceStatus,
  TenantSubscriptionStatus,
} from "@/shared/constants/backend-enums";
import type { TenantBillingSummaryDto } from "@/domains/billing/types";

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
  activeQuestionCount,
  draftQuestions,
  draftQuestionCount,
  billingSummary,
  memberCount,
}: {
  spaces: SpaceDto[];
  activeQuestionCount: number;
  draftQuestions: QuestionDto[];
  draftQuestionCount: number;
  billingSummary?: TenantBillingSummaryDto | null;
  memberCount: number;
}) {
  const subscriptionStatus = billingSummary?.subscriptionStatus;
  const activeSpaces = spaces.filter(
    (space) => space.status === SpaceStatus.Active,
  );

  if (activeSpaces.length === 0) {
    return {
      label: spaces.length > 0 ? "Open spaces" : "Start here",
      description:
        "Create or activate the first Space so questions and answers have a home.",
      to: spaces.length > 0 ? "/app/spaces" : "/app/spaces/new",
    };
  }

  const questionSpace =
    activeSpaces.find((space) => space.acceptsQuestions) ?? activeSpaces[0];
  const spaceWithoutQuestion = activeSpaces.find(
    (space) => space.questionCount === 0,
  );

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

  if (activeQuestionCount === 0) {
    return {
      label: "Open Space",
      description:
        "Create the first question inside a Space so it inherits intake rules.",
      to: `/app/spaces/${questionSpace.id}`,
    };
  }

  if (spaceWithoutQuestion) {
    return {
      label: "Open Space",
      description:
        "Add a Question to each Space before reviewing normal activity.",
      to: `/app/spaces/${spaceWithoutQuestion.id}`,
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

export type DashboardAdministrationItem = {
  key: string;
  label: string;
  value: string;
  detail: string;
  actionLabel: string;
  tone: DashboardSignalTone;
  to: string;
};

export type DashboardAdministration = {
  label: string;
  detail: string;
  tone: DashboardSignalTone;
  to: string;
  items: DashboardAdministrationItem[];
};

function getBillingTone(
  summary?: TenantBillingSummaryDto | null,
): DashboardSignalTone {
  if (!summary) {
    return "neutral";
  }

  if (
    summary.entitlement?.isActive === false ||
    summary.subscriptionStatus === TenantSubscriptionStatus.Unpaid ||
    summary.subscriptionStatus === TenantSubscriptionStatus.Canceled ||
    summary.subscriptionStatus === TenantSubscriptionStatus.IncompleteExpired
  ) {
    return "danger";
  }

  if (getBillingNeedsAttention(summary)) {
    return "warning";
  }

  return "success";
}

export function getAccountAdministration({
  billingSummary,
  hasCompleteProfile,
}: {
  billingSummary?: TenantBillingSummaryDto | null;
  hasCompleteProfile: boolean;
}): DashboardAdministration {
  const billingTone = getBillingTone(billingSummary);
  const billingNeedsAttention =
    billingTone === "danger" || billingTone === "warning";
  const profileTone: DashboardSignalTone = hasCompleteProfile
    ? "success"
    : "warning";

  const summary = billingNeedsAttention
    ? {
        label: "Billing needs review",
        detail:
          "Fix billing or entitlement issues before account administration expands.",
        tone: billingTone,
        to: "/app/billing",
      }
    : !hasCompleteProfile
      ? {
          label: "Complete your profile",
          detail:
            "Name, phone, language, and time zone keep account activity clear.",
          tone: profileTone,
          to: "/app/settings/profile",
        }
      : {
          label: "Administration is ready",
          detail: "Billing, profile, and settings are ready to review.",
          tone: "success" as DashboardSignalTone,
          to: "/app/settings/tenant",
        };

  return {
    ...summary,
    items: [
      {
        key: "billing",
        label: "Billing",
        value: billingNeedsAttention ? "Needs review" : "Current",
        detail: billingNeedsAttention
          ? "Subscription or entitlement needs attention."
          : "Subscription and entitlement are ready for normal workspace operation.",
        actionLabel: "Review billing",
        tone: billingTone,
        to: "/app/billing",
      },
      {
        key: "profile",
        label: "Profile",
        value: hasCompleteProfile ? "Ready" : "Incomplete",
        detail: hasCompleteProfile
          ? "Profile details support notifications, locale, and audit context."
          : "Complete name, phone, language, and time zone for clearer account activity.",
        actionLabel: hasCompleteProfile ? "Review profile" : "Complete profile",
        tone: profileTone,
        to: "/app/settings/profile",
      },
      {
        key: "settings",
        label: "Settings",
        value: "Open",
        detail:
          "Review tenant, general, and security settings before scaling access.",
        actionLabel: "Open settings",
        tone: "info",
        to: "/app/settings/tenant",
      },
    ],
  };
}

export function getBusinessReadout({
  activeQuestionCount,
  draftQuestionCount,
  activeAnswerCount,
  questionCount,
  spaceCount,
  spacesWithQuestionsCount,
  sourceCount,
  firstActiveQuestionId,
  firstActiveAnswerId,
  firstDraftQuestionId,
  firstSpaceWithoutQuestionId,
  firstSpaceId,
}: {
  activeQuestionCount: number;
  draftQuestionCount: number;
  activeAnswerCount: number;
  questionCount: number;
  spaceCount: number;
  spacesWithQuestionsCount: number;
  sourceCount: number;
  firstActiveQuestionId?: string;
  firstActiveAnswerId?: string;
  firstDraftQuestionId?: string;
  firstSpaceWithoutQuestionId?: string;
  firstSpaceId?: string;
}) {
  const reusableQuestionCoverage = Math.min(
    percentage(activeQuestionCount, questionCount),
    100,
  );
  const spacesWithoutQuestionsCount = Math.max(
    spaceCount - spacesWithQuestionsCount,
    0,
  );
  const hasSpacesWithoutQuestions = spacesWithoutQuestionsCount > 0;
  const unresolvedTargetCount =
    draftQuestionCount + spacesWithoutQuestionsCount;
  const fallbackSpaceTo = firstSpaceId
    ? `/app/spaces/${firstSpaceId}`
    : "/app/spaces";
  const draftReviewTo =
    draftQuestionCount > 0 && firstDraftQuestionId
      ? `/app/questions/${firstDraftQuestionId}`
      : hasSpacesWithoutQuestions && firstSpaceWithoutQuestionId
        ? `/app/spaces/${firstSpaceWithoutQuestionId}`
        : fallbackSpaceTo;
  const draftReviewDetail =
    draftQuestionCount > 0
      ? hasSpacesWithoutQuestions
        ? "Draft questions and Spaces without Questions need review."
        : "Draft questions need activation or archive review."
      : hasSpacesWithoutQuestions
        ? "Spaces without questions need a Question before activity review is clear."
        : "No targets need review before normal activity.";
  const draftReviewTone: DashboardSignalTone =
    draftQuestionCount > 0
      ? queueTone(draftQuestionCount)
      : hasSpacesWithoutQuestions
        ? "warning"
        : queueTone(draftQuestionCount);

  return [
    {
      label: "Targets to resolve",
      value: `${unresolvedTargetCount}`,
      benchmark: "Target 0 open targets",
      detail: draftReviewDetail,
      tone: draftReviewTone,
      to: draftReviewTo,
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
      label: "Sources",
      value: `${sourceCount}`,
      benchmark: "Optional context",
      detail:
        sourceCount > 0
          ? "Source records are available as reusable references."
          : "Sources are optional and can be added when answers need references.",
      tone: reusableAssetTone(sourceCount),
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
