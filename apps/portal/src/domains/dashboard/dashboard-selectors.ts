import type { ActivityDto } from "@/domains/activity/types";
import type { AnswerDto } from "@/domains/answers/types";
import type { QuestionDto } from "@/domains/questions/types";
import type { SpaceDto } from "@/domains/spaces/types";
import {
  AnswerStatus,
  QuestionStatus,
  TenantSubscriptionStatus,
} from "@/shared/constants/backend-enums";
import type { TenantBillingSummaryDto } from "@/domains/billing/types";

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
      description: "Create the first space so questions and answers have a home.",
      to: "/app/spaces/new",
    };
  }

  const questionSpace = spaces.find((space) => space.acceptsQuestions) ?? spaces[0];

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
      description: "Create the first question inside a Space so it inherits intake rules.",
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
      description: "Restore entitlement confidence before operational work expands.",
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
  answers,
  citableSourceCount,
  openQuestions,
  pendingQuestionCount,
  questionCount,
  spaces,
  sourceCount,
  validatedAnswerCount,
}: {
  activity: ActivityDto[];
  answers: AnswerDto[];
  citableSourceCount: number;
  openQuestions: QuestionDto[];
  pendingQuestionCount: number;
  questionCount: number;
  spaces: SpaceDto[];
  sourceCount: number;
  validatedAnswerCount: number;
}) {
  const activeSpaces = spaces.filter(
    (space) => space.acceptsAnswers || space.acceptsQuestions,
  ).length;
  const publishedAnswers = answers.filter(
    (answer) => answer.status === AnswerStatus.Published,
  ).length;
  const answeredCoverage =
    questionCount > 0
      ? Math.round(((questionCount - openQuestions.length) / questionCount) * 100)
      : 0;

  return {
    activeSpaces,
    pendingQuestionCount,
    openQuestionCount: openQuestions.length,
    validatedAnswerCount,
    citableSourceCount,
    recentActivityCount: activity.length,
    publishedAnswers,
    sourceCount,
    answeredCoverage,
  };
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
