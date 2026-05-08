import {
  ActivityKind,
  ActorKind,
  AnswerKind,
  AnswerStatus,
  BillingIntervalType,
  BillingInvoiceStatus,
  BillingPaymentStatus,
  BillingProviderType,
  ChannelKind,
  ModuleEnum,
  QuestionStatus,
  SourceRole,
  SourceUploadStatus,
  SpaceStatus,
  TenantEdition,
  TenantSubscriptionStatus,
  TenantUserRoleType,
  VisibilityScope,
  activityKindLabels,
  actorKindLabels,
  answerKindLabels,
  answerStatusLabels,
  billingIntervalLabels,
  billingInvoiceStatusLabels,
  billingPaymentStatusLabels,
  billingProviderLabels,
  channelKindLabels,
  questionStatusLabels,
  sourceRoleLabels,
  sourceUploadStatusLabels,
  spaceStatusLabels,
  tenantEditionLabels,
  tenantSubscriptionStatusLabels,
  tenantUserRoleTypeLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";

export type BadgeVariant =
  | "default"
  | "primary"
  | "secondary"
  | "outline"
  | "destructive"
  | "warning"
  | "success"
  | "info";

export type EnumPresentation = {
  label: string;
  description: string;
  badgeVariant: BadgeVariant;
  sortGroup: number;
};

export const modulePresentation: Record<ModuleEnum, EnumPresentation> = {
  [ModuleEnum.Tenant]: {
    label: "Tenant",
    description: "Workspace administration and access.",
    badgeVariant: "outline",
    sortGroup: 1,
  },
  [ModuleEnum.QnA]: {
    label: "QnA",
    description: "Questions, answers, sources, tags, and activity.",
    badgeVariant: "primary",
    sortGroup: 2,
  },
  [ModuleEnum.Broadcast]: {
    label: "Broadcast",
    description: "Broadcast communications module.",
    badgeVariant: "info",
    sortGroup: 3,
  },
  [ModuleEnum.Direct]: {
    label: "Direct",
    description: "Direct customer communication module.",
    badgeVariant: "secondary",
    sortGroup: 4,
  },
  [ModuleEnum.Trust]: {
    label: "Trust",
    description: "Trust and governance module.",
    badgeVariant: "success",
    sortGroup: 5,
  },
};

export const tenantEditionPresentation = Object.fromEntries(
  Object.entries(tenantEditionLabels).map(([value, label]) => [
    value,
    {
      label,
      description: `${label} workspace edition.`,
      badgeVariant:
        Number(value) >= TenantEdition.Business ? "primary" : "outline",
      sortGroup: Number(value),
    },
  ]),
) as Record<TenantEdition, EnumPresentation>;

export const tenantUserRolePresentation: Record<
  TenantUserRoleType,
  EnumPresentation
> = {
  [TenantUserRoleType.Owner]: {
    label: tenantUserRoleTypeLabels[TenantUserRoleType.Owner],
    description: "Can administer billing, members, settings, and workspace keys.",
    badgeVariant: "primary",
    sortGroup: 1,
  },
  [TenantUserRoleType.Member]: {
    label: tenantUserRoleTypeLabels[TenantUserRoleType.Member],
    description: "Can operate assigned workspace content.",
    badgeVariant: "secondary",
    sortGroup: 2,
  },
};

export const billingProviderPresentation = Object.fromEntries(
  Object.entries(billingProviderLabels).map(([value, label]) => [
    value,
    {
      label,
      description: "Billing processor.",
      badgeVariant: Number(value) === BillingProviderType.Stripe ? "info" : "outline",
      sortGroup: Number(value),
    },
  ]),
) as Record<BillingProviderType, EnumPresentation>;

export const billingIntervalPresentation = Object.fromEntries(
  Object.entries(billingIntervalLabels).map(([value, label]) => [
    value,
    {
      label,
      description: "Billing cadence.",
      badgeVariant: Number(value) === BillingIntervalType.Unknown ? "outline" : "secondary",
      sortGroup: Number(value),
    },
  ]),
) as Record<BillingIntervalType, EnumPresentation>;

export const billingInvoiceStatusPresentation: Record<
  BillingInvoiceStatus,
  EnumPresentation
> = {
  [BillingInvoiceStatus.Unknown]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Unknown],
    description: "Invoice state is not available.",
    badgeVariant: "outline",
    sortGroup: 9,
  },
  [BillingInvoiceStatus.Draft]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Draft],
    description: "Invoice has not been finalized.",
    badgeVariant: "warning",
    sortGroup: 2,
  },
  [BillingInvoiceStatus.Open]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Open],
    description: "Invoice is payable.",
    badgeVariant: "warning",
    sortGroup: 1,
  },
  [BillingInvoiceStatus.Paid]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Paid],
    description: "Invoice is paid.",
    badgeVariant: "success",
    sortGroup: 4,
  },
  [BillingInvoiceStatus.Uncollectible]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Uncollectible],
    description: "Invoice could not be collected.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [BillingInvoiceStatus.Void]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Void],
    description: "Invoice was voided.",
    badgeVariant: "outline",
    sortGroup: 8,
  },
  [BillingInvoiceStatus.Failed]: {
    label: billingInvoiceStatusLabels[BillingInvoiceStatus.Failed],
    description: "Invoice processing failed.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
};

export const billingPaymentStatusPresentation: Record<
  BillingPaymentStatus,
  EnumPresentation
> = {
  [BillingPaymentStatus.Unknown]: {
    label: billingPaymentStatusLabels[BillingPaymentStatus.Unknown],
    description: "Payment state is not available.",
    badgeVariant: "outline",
    sortGroup: 9,
  },
  [BillingPaymentStatus.Pending]: {
    label: billingPaymentStatusLabels[BillingPaymentStatus.Pending],
    description: "Payment is still processing.",
    badgeVariant: "warning",
    sortGroup: 1,
  },
  [BillingPaymentStatus.Succeeded]: {
    label: billingPaymentStatusLabels[BillingPaymentStatus.Succeeded],
    description: "Payment succeeded.",
    badgeVariant: "success",
    sortGroup: 4,
  },
  [BillingPaymentStatus.Failed]: {
    label: billingPaymentStatusLabels[BillingPaymentStatus.Failed],
    description: "Payment failed.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [BillingPaymentStatus.Refunded]: {
    label: billingPaymentStatusLabels[BillingPaymentStatus.Refunded],
    description: "Payment was refunded.",
    badgeVariant: "outline",
    sortGroup: 8,
  },
};

export const tenantSubscriptionStatusPresentation: Record<
  TenantSubscriptionStatus,
  EnumPresentation
> = {
  [TenantSubscriptionStatus.Unknown]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Unknown],
    description: "Subscription state is not available.",
    badgeVariant: "outline",
    sortGroup: 9,
  },
  [TenantSubscriptionStatus.Trialing]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Trialing],
    description: "Trial access is active.",
    badgeVariant: "info",
    sortGroup: 2,
  },
  [TenantSubscriptionStatus.Active]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Active],
    description: "Subscription access is active.",
    badgeVariant: "success",
    sortGroup: 3,
  },
  [TenantSubscriptionStatus.PastDue]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.PastDue],
    description: "Payment needs attention.",
    badgeVariant: "warning",
    sortGroup: 1,
  },
  [TenantSubscriptionStatus.Unpaid]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Unpaid],
    description: "Subscription is unpaid.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [TenantSubscriptionStatus.Canceled]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Canceled],
    description: "Subscription has been canceled.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [TenantSubscriptionStatus.Incomplete]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Incomplete],
    description: "Subscription setup is incomplete.",
    badgeVariant: "warning",
    sortGroup: 1,
  },
  [TenantSubscriptionStatus.IncompleteExpired]: {
    label:
      tenantSubscriptionStatusLabels[TenantSubscriptionStatus.IncompleteExpired],
    description: "Incomplete setup expired.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [TenantSubscriptionStatus.Paused]: {
    label: tenantSubscriptionStatusLabels[TenantSubscriptionStatus.Paused],
    description: "Subscription is paused.",
    badgeVariant: "warning",
    sortGroup: 1,
  },
};

export const spaceStatusPresentation: Record<SpaceStatus, EnumPresentation> = {
  [SpaceStatus.Draft]: {
    label: spaceStatusLabels[SpaceStatus.Draft],
    description: "Not ready for public use.",
    badgeVariant: "warning",
    sortGroup: 2,
  },
  [SpaceStatus.Active]: {
    label: spaceStatusLabels[SpaceStatus.Active],
    description: "Available for QnA use.",
    badgeVariant: "success",
    sortGroup: 1,
  },
  [SpaceStatus.Archived]: {
    label: spaceStatusLabels[SpaceStatus.Archived],
    description: "No longer active.",
    badgeVariant: "outline",
    sortGroup: 3,
  },
};

export const visibilityPresentation: Record<VisibilityScope, EnumPresentation> = {
  [VisibilityScope.Internal]: {
    label: visibilityScopeLabels[VisibilityScope.Internal],
    description: "Visible only inside the portal.",
    badgeVariant: "outline",
    sortGroup: 0,
  },
  [VisibilityScope.Authenticated]: {
    label: visibilityScopeLabels[VisibilityScope.Authenticated],
    description: "Visible outside the portal to authenticated users.",
    badgeVariant: "info",
    sortGroup: 1,
  },
  [VisibilityScope.Public]: {
    label: visibilityScopeLabels[VisibilityScope.Public],
    description: "Visible outside the portal to any visitor.",
    badgeVariant: "success",
    sortGroup: 2,
  },
};

export const questionStatusPresentation: Record<
  QuestionStatus,
  EnumPresentation
> = {
  [QuestionStatus.Draft]: {
    label: questionStatusLabels[QuestionStatus.Draft],
    description: "Not yet active.",
    badgeVariant: "warning",
    sortGroup: 2,
  },
  [QuestionStatus.Active]: {
    label: questionStatusLabels[QuestionStatus.Active],
    description: "Available as reusable question knowledge.",
    badgeVariant: "success",
    sortGroup: 1,
  },
  [QuestionStatus.Archived]: {
    label: questionStatusLabels[QuestionStatus.Archived],
    description: "No longer active.",
    badgeVariant: "outline",
    sortGroup: 3,
  },
};

export const channelKindPresentation = Object.fromEntries(
  Object.entries(channelKindLabels).map(([value, label]) => [
    value,
    {
      label,
      description: "Question intake channel.",
      badgeVariant: Number(value) === ChannelKind.Manual ? "outline" : "info",
      sortGroup: Number(value),
    },
  ]),
) as Record<ChannelKind, EnumPresentation>;

export const answerKindPresentation: Record<AnswerKind, EnumPresentation> = {
  [AnswerKind.Official]: {
    label: answerKindLabels[AnswerKind.Official],
    description: "Official workspace answer.",
    badgeVariant: "primary",
    sortGroup: 1,
  },
  [AnswerKind.Community]: {
    label: answerKindLabels[AnswerKind.Community],
    description: "Community contribution.",
    badgeVariant: "secondary",
    sortGroup: 2,
  },
  [AnswerKind.Imported]: {
    label: answerKindLabels[AnswerKind.Imported],
    description: "Imported answer.",
    badgeVariant: "outline",
    sortGroup: 3,
  },
};

export const answerStatusPresentation: Record<AnswerStatus, EnumPresentation> = {
  [AnswerStatus.Draft]: {
    label: answerStatusLabels[AnswerStatus.Draft],
    description: "Not active yet.",
    badgeVariant: "warning",
    sortGroup: 2,
  },
  [AnswerStatus.Active]: {
    label: answerStatusLabels[AnswerStatus.Active],
    description: "Available as reusable answer knowledge.",
    badgeVariant: "success",
    sortGroup: 1,
  },
  [AnswerStatus.Archived]: {
    label: answerStatusLabels[AnswerStatus.Archived],
    description: "No longer active.",
    badgeVariant: "outline",
    sortGroup: 5,
  },
};

export const sourceRolePresentation = Object.fromEntries(
  Object.entries(sourceRoleLabels).map(([value, label]) => [
    value,
    {
      label,
      description: "How this source supports the record.",
      badgeVariant: Number(value) === SourceRole.Reference ? "primary" : "outline",
      sortGroup: Number(value),
    },
  ]),
) as Record<SourceRole, EnumPresentation>;

export const sourceUploadStatusPresentation: Record<
  SourceUploadStatus,
  EnumPresentation
> = {
  [SourceUploadStatus.None]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.None],
    description: "External source without an upload workflow.",
    badgeVariant: "outline",
    sortGroup: 7,
  },
  [SourceUploadStatus.Pending]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.Pending],
    description: "Upload intent created; file has not completed yet.",
    badgeVariant: "warning",
    sortGroup: 2,
  },
  [SourceUploadStatus.Uploaded]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.Uploaded],
    description: "File uploaded and waiting for worker verification.",
    badgeVariant: "info",
    sortGroup: 3,
  },
  [SourceUploadStatus.Verified]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.Verified],
    description: "File verified and ready for private download.",
    badgeVariant: "success",
    sortGroup: 1,
  },
  [SourceUploadStatus.Quarantined]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.Quarantined],
    description: "File was moved to quarantine.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [SourceUploadStatus.Failed]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.Failed],
    description: "Upload verification failed.",
    badgeVariant: "destructive",
    sortGroup: 0,
  },
  [SourceUploadStatus.Expired]: {
    label: sourceUploadStatusLabels[SourceUploadStatus.Expired],
    description: "Upload intent expired before completion.",
    badgeVariant: "outline",
    sortGroup: 6,
  },
};

export const activityKindPresentation = Object.fromEntries(
  Object.entries(activityKindLabels).map(([value, label]) => [
    value,
    {
      label,
      description: "Workflow and signal event.",
      badgeVariant:
        Number(value) === ActivityKind.QuestionActive ||
        Number(value) === ActivityKind.AnswerActive
          ? "success"
          : Number(value) === ActivityKind.QuestionArchived ||
              Number(value) === ActivityKind.AnswerArchived
            ? "secondary"
            : "outline",
      sortGroup: Number(value),
    },
  ]),
) as Record<ActivityKind, EnumPresentation>;

export const actorKindPresentation: Record<ActorKind, EnumPresentation> = {
  [ActorKind.System]: {
    label: actorKindLabels[ActorKind.System],
    description: "Automated system event.",
    badgeVariant: "outline",
    sortGroup: 5,
  },
  [ActorKind.Customer]: {
    label: actorKindLabels[ActorKind.Customer],
    description: "Customer or public user event.",
    badgeVariant: "secondary",
    sortGroup: 4,
  },
  [ActorKind.Contributor]: {
    label: actorKindLabels[ActorKind.Contributor],
    description: "Contributor event.",
    badgeVariant: "info",
    sortGroup: 3,
  },
  [ActorKind.Moderator]: {
    label: actorKindLabels[ActorKind.Moderator],
    description: "Moderator event.",
    badgeVariant: "primary",
    sortGroup: 1,
  },
  [ActorKind.Integration]: {
    label: actorKindLabels[ActorKind.Integration],
    description: "Integration event.",
    badgeVariant: "outline",
    sortGroup: 6,
  },
};
