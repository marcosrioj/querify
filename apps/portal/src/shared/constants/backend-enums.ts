export enum ModuleEnum {
  Tenant = 1,
  QnA = 6,
  Direct = 11,
  Broadcast = 16,
  Trust = 21,
}

export enum TenantEdition {
  Free = 1,
  Starter = 6,
  Pro = 11,
  Business = 16,
  Enterprise = 21,
}

export enum TenantUserRoleType {
  Owner = 1,
  Member = 6,
}

export enum BillingProviderType {
  Unknown = 1,
  Stripe = 6,
}

export enum BillingIntervalType {
  Unknown = 1,
  Day = 6,
  Week = 11,
  Month = 16,
  Year = 21,
  OneTime = 26,
}

export enum BillingInvoiceStatus {
  Unknown = 1,
  Draft = 6,
  Open = 11,
  Paid = 16,
  Uncollectible = 21,
  Void = 26,
  Failed = 31,
}

export enum BillingPaymentStatus {
  Unknown = 1,
  Pending = 6,
  Succeeded = 11,
  Failed = 16,
  Refunded = 21,
}

export enum TenantSubscriptionStatus {
  Unknown = 1,
  Trialing = 6,
  Active = 11,
  PastDue = 16,
  Unpaid = 21,
  Canceled = 26,
  Incomplete = 31,
  IncompleteExpired = 36,
  Paused = 41,
}

export enum SpaceStatus {
  Draft = 1,
  Active = 6,
  Archived = 11,
}

export enum VisibilityScope {
  Internal = 1,
  Authenticated = 6,
  Public = 11,
}

export enum QuestionStatus {
  Draft = 1,
  Active = 6,
  Archived = 11,
}

export enum ChannelKind {
  Manual = 1,
  Widget = 6,
  Api = 11,
  HelpCenter = 16,
  Import = 21,
  Other = 26,
}

export enum AnswerKind {
  Official = 1,
  Community = 6,
  Imported = 11,
}

export enum AnswerStatus {
  Draft = 1,
  Active = 6,
  Archived = 11,
}

export enum SourceRole {
  Origin = 1,
  Context = 6,
  Evidence = 11,
  Reference = 16,
}

export enum SourceUploadStatus {
  None = 1,
  Pending = 6,
  Uploaded = 11,
  Verified = 16,
  Quarantined = 21,
  Failed = 26,
  Expired = 31,
}

export enum ActivityKind {
  QuestionCreated = 1,
  QuestionUpdated = 6,
  QuestionDraft = 11,
  QuestionActive = 16,
  QuestionArchived = 21,
  AnswerCreated = 26,
  AnswerUpdated = 31,
  AnswerDraft = 36,
  AnswerActive = 41,
  AnswerArchived = 46,
  FeedbackReceived = 51,
  VoteReceived = 56,
}

export enum ActorKind {
  System = 1,
  Customer = 6,
  Contributor = 11,
  Moderator = 16,
  Integration = 21,
}

export const tenantEditionLabels: Record<TenantEdition, string> = {
  [TenantEdition.Free]: 'Free',
  [TenantEdition.Starter]: 'Starter',
  [TenantEdition.Pro]: 'Pro',
  [TenantEdition.Business]: 'Business',
  [TenantEdition.Enterprise]: 'Enterprise',
};

export const tenantUserRoleTypeLabels: Record<TenantUserRoleType, string> = {
  [TenantUserRoleType.Owner]: 'Owner',
  [TenantUserRoleType.Member]: 'Member',
};

export const billingProviderLabels: Record<BillingProviderType, string> = {
  [BillingProviderType.Unknown]: 'Unknown',
  [BillingProviderType.Stripe]: 'Stripe',
};

export const billingIntervalLabels: Record<BillingIntervalType, string> = {
  [BillingIntervalType.Unknown]: 'Unknown',
  [BillingIntervalType.Day]: 'Day',
  [BillingIntervalType.Week]: 'Week',
  [BillingIntervalType.Month]: 'Month',
  [BillingIntervalType.Year]: 'Year',
  [BillingIntervalType.OneTime]: 'One time',
};

export const billingInvoiceStatusLabels: Record<BillingInvoiceStatus, string> = {
  [BillingInvoiceStatus.Unknown]: 'Unknown',
  [BillingInvoiceStatus.Draft]: 'Draft',
  [BillingInvoiceStatus.Open]: 'Open',
  [BillingInvoiceStatus.Paid]: 'Paid',
  [BillingInvoiceStatus.Uncollectible]: 'Uncollectible',
  [BillingInvoiceStatus.Void]: 'Void',
  [BillingInvoiceStatus.Failed]: 'Failed',
};

export const billingPaymentStatusLabels: Record<BillingPaymentStatus, string> = {
  [BillingPaymentStatus.Unknown]: 'Unknown',
  [BillingPaymentStatus.Pending]: 'Pending',
  [BillingPaymentStatus.Succeeded]: 'Succeeded',
  [BillingPaymentStatus.Failed]: 'Failed',
  [BillingPaymentStatus.Refunded]: 'Refunded',
};

export const tenantSubscriptionStatusLabels: Record<TenantSubscriptionStatus, string> =
  {
    [TenantSubscriptionStatus.Unknown]: 'Unknown',
    [TenantSubscriptionStatus.Trialing]: 'Trialing',
    [TenantSubscriptionStatus.Active]: 'Active',
    [TenantSubscriptionStatus.PastDue]: 'Past due',
    [TenantSubscriptionStatus.Unpaid]: 'Unpaid',
    [TenantSubscriptionStatus.Canceled]: 'Canceled',
    [TenantSubscriptionStatus.Incomplete]: 'Incomplete',
    [TenantSubscriptionStatus.IncompleteExpired]: 'Incomplete expired',
    [TenantSubscriptionStatus.Paused]: 'Paused',
  };

export const spaceStatusLabels: Record<SpaceStatus, string> = {
  [SpaceStatus.Draft]: 'Draft',
  [SpaceStatus.Active]: 'Active',
  [SpaceStatus.Archived]: 'Archived',
};

export const visibilityScopeLabels: Record<VisibilityScope, string> = {
  [VisibilityScope.Internal]: 'Internal',
  [VisibilityScope.Authenticated]: 'Authenticated',
  [VisibilityScope.Public]: 'Public',
};

export const questionStatusLabels: Record<QuestionStatus, string> = {
  [QuestionStatus.Draft]: 'Draft',
  [QuestionStatus.Active]: 'Active',
  [QuestionStatus.Archived]: 'Archived',
};

export const channelKindLabels: Record<ChannelKind, string> = {
  [ChannelKind.Manual]: 'Manual',
  [ChannelKind.Widget]: 'Widget',
  [ChannelKind.Api]: 'API',
  [ChannelKind.HelpCenter]: 'Help center',
  [ChannelKind.Import]: 'Import',
  [ChannelKind.Other]: 'Other',
};

export const answerKindLabels: Record<AnswerKind, string> = {
  [AnswerKind.Official]: 'Official',
  [AnswerKind.Community]: 'Community',
  [AnswerKind.Imported]: 'Imported',
};

export const answerStatusLabels: Record<AnswerStatus, string> = {
  [AnswerStatus.Draft]: 'Draft',
  [AnswerStatus.Active]: 'Active',
  [AnswerStatus.Archived]: 'Archived',
};

export const sourceRoleLabels: Record<SourceRole, string> = {
  [SourceRole.Origin]: 'Origin',
  [SourceRole.Context]: 'Context',
  [SourceRole.Evidence]: 'Evidence',
  [SourceRole.Reference]: 'Reference',
};

export const sourceUploadStatusLabels: Record<SourceUploadStatus, string> = {
  [SourceUploadStatus.None]: 'None',
  [SourceUploadStatus.Pending]: 'Pending',
  [SourceUploadStatus.Uploaded]: 'Uploaded',
  [SourceUploadStatus.Verified]: 'Verified',
  [SourceUploadStatus.Quarantined]: 'Quarantined',
  [SourceUploadStatus.Failed]: 'Failed',
  [SourceUploadStatus.Expired]: 'Expired',
};

export const activityKindLabels: Record<ActivityKind, string> = {
  [ActivityKind.QuestionCreated]: 'Question created',
  [ActivityKind.QuestionUpdated]: 'Question updated',
  [ActivityKind.QuestionDraft]: 'Question draft',
  [ActivityKind.QuestionActive]: 'Question active',
  [ActivityKind.QuestionArchived]: 'Question archived',
  [ActivityKind.AnswerCreated]: 'Answer created',
  [ActivityKind.AnswerUpdated]: 'Answer updated',
  [ActivityKind.AnswerDraft]: 'Answer draft',
  [ActivityKind.AnswerActive]: 'Answer active',
  [ActivityKind.AnswerArchived]: 'Answer archived',
  [ActivityKind.FeedbackReceived]: 'Feedback received',
  [ActivityKind.VoteReceived]: 'Vote received',
};

export const actorKindLabels: Record<ActorKind, string> = {
  [ActorKind.System]: 'System',
  [ActorKind.Customer]: 'Customer',
  [ActorKind.Contributor]: 'Contributor',
  [ActorKind.Moderator]: 'Moderator',
  [ActorKind.Integration]: 'Integration',
};

export type BackendEnumSelectOption = {
  value: string;
  label: string;
};

export function backendEnumSelectOptions(
  labels: Record<number, string>,
): BackendEnumSelectOption[] {
  return Object.entries(labels).map(([value, label]) => ({
    value,
    label,
  }));
}
