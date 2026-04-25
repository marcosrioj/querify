export enum PortalApp {
  Tenant = 0,
  QnA = 1,
  EngagementHub = 2,
  SupportCopilot = 3,
}

export enum TenantEdition {
  Free = 0,
  Starter = 1,
  Pro = 2,
  Business = 3,
  Enterprise = 4,
}

export enum TenantUserRoleType {
  Owner = 0,
  Member = 1,
}

export enum BillingProviderType {
  Unknown = 0,
  Stripe = 1,
}

export enum BillingIntervalType {
  Unknown = 0,
  Day = 1,
  Week = 2,
  Month = 3,
  Year = 4,
  OneTime = 5,
}

export enum BillingInvoiceStatus {
  Unknown = 0,
  Draft = 1,
  Open = 2,
  Paid = 3,
  Uncollectible = 4,
  Void = 5,
  Failed = 6,
}

export enum BillingPaymentStatus {
  Unknown = 0,
  Pending = 1,
  Succeeded = 2,
  Failed = 3,
  Refunded = 4,
}

export enum TenantSubscriptionStatus {
  Unknown = 0,
  Trialing = 1,
  Active = 2,
  PastDue = 3,
  Unpaid = 4,
  Canceled = 5,
  Incomplete = 6,
  IncompleteExpired = 7,
  Paused = 8,
}

export enum SpaceKind {
  ControlledPublication = 1,
  ModeratedCollaboration = 2,
  PublicValidation = 3,
}

export enum VisibilityScope {
  Internal = 1,
  Authenticated = 2,
  Public = 3,
  PublicIndexed = 4,
}

export enum SearchMarkupMode {
  CuratedList = 1,
  QuestionPage = 2,
  Hybrid = 3,
  Off = 4,
}

export enum QuestionStatus {
  Draft = 0,
  PendingReview = 1,
  Open = 2,
  Answered = 3,
  Validated = 4,
  Escalated = 5,
  Duplicate = 6,
  Archived = 7,
}

export enum ChannelKind {
  Manual = 1,
  Widget = 2,
  Api = 3,
  HelpCenter = 4,
  Import = 5,
  Other = 99,
}

export enum AnswerKind {
  Official = 1,
  Community = 2,
  Imported = 3,
}

export enum AnswerStatus {
  Draft = 0,
  PendingReview = 1,
  Published = 2,
  Validated = 3,
  Rejected = 4,
  Obsolete = 5,
  Archived = 6,
}

export enum SourceKind {
  Article = 1,
  WebPage = 2,
  Pdf = 3,
  Video = 4,
  Repository = 5,
  ProductNote = 6,
  InternalNote = 7,
  GovernanceRecord = 8,
  AuditRecord = 9,
  Other = 99,
}

export enum SourceRole {
  QuestionOrigin = 1,
  SupportingContext = 2,
  Evidence = 3,
  Citation = 4,
  CanonicalReference = 5,
  AuditProof = 6,
}

export enum ActivityKind {
  QuestionCreated = 1,
  QuestionUpdated = 2,
  QuestionSubmitted = 3,
  QuestionApproved = 4,
  QuestionRejected = 5,
  QuestionMarkedDuplicate = 6,
  QuestionEscalated = 7,
  AnswerCreated = 8,
  AnswerUpdated = 9,
  AnswerPublished = 10,
  AnswerAccepted = 11,
  AnswerValidated = 12,
  AnswerRejected = 13,
  FeedbackReceived = 14,
  VoteReceived = 15,
  AnswerRetired = 16,
  ReportReceived = 17,
}

export enum ActorKind {
  System = 1,
  Customer = 2,
  Contributor = 3,
  Moderator = 4,
  Integration = 5,
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

export const spaceKindLabels: Record<SpaceKind, string> = {
  [SpaceKind.ControlledPublication]: 'Controlled publication',
  [SpaceKind.ModeratedCollaboration]: 'Moderated collaboration',
  [SpaceKind.PublicValidation]: 'Public validation',
};

export const visibilityScopeLabels: Record<VisibilityScope, string> = {
  [VisibilityScope.Internal]: 'Internal',
  [VisibilityScope.Authenticated]: 'Authenticated',
  [VisibilityScope.Public]: 'Public',
  [VisibilityScope.PublicIndexed]: 'Public indexed',
};

export const searchMarkupModeLabels: Record<SearchMarkupMode, string> = {
  [SearchMarkupMode.CuratedList]: 'Curated list',
  [SearchMarkupMode.QuestionPage]: 'Question page',
  [SearchMarkupMode.Hybrid]: 'Hybrid',
  [SearchMarkupMode.Off]: 'Off',
};

export const questionStatusLabels: Record<QuestionStatus, string> = {
  [QuestionStatus.Draft]: 'Draft',
  [QuestionStatus.PendingReview]: 'Pending review',
  [QuestionStatus.Open]: 'Open',
  [QuestionStatus.Answered]: 'Answered',
  [QuestionStatus.Validated]: 'Validated',
  [QuestionStatus.Escalated]: 'Escalated',
  [QuestionStatus.Duplicate]: 'Duplicate',
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
  [AnswerStatus.PendingReview]: 'Pending review',
  [AnswerStatus.Published]: 'Published',
  [AnswerStatus.Validated]: 'Validated',
  [AnswerStatus.Rejected]: 'Rejected',
  [AnswerStatus.Obsolete]: 'Obsolete',
  [AnswerStatus.Archived]: 'Archived',
};

export const sourceKindLabels: Record<SourceKind, string> = {
  [SourceKind.Article]: 'Article',
  [SourceKind.WebPage]: 'Web page',
  [SourceKind.Pdf]: 'PDF',
  [SourceKind.Video]: 'Video',
  [SourceKind.Repository]: 'Repository',
  [SourceKind.ProductNote]: 'Product note',
  [SourceKind.InternalNote]: 'Internal note',
  [SourceKind.GovernanceRecord]: 'Governance record',
  [SourceKind.AuditRecord]: 'Audit record',
  [SourceKind.Other]: 'Other',
};

export const sourceRoleLabels: Record<SourceRole, string> = {
  [SourceRole.QuestionOrigin]: 'Question origin',
  [SourceRole.SupportingContext]: 'Supporting context',
  [SourceRole.Evidence]: 'Evidence',
  [SourceRole.Citation]: 'Citation',
  [SourceRole.CanonicalReference]: 'Canonical reference',
  [SourceRole.AuditProof]: 'Audit proof',
};

export const activityKindLabels: Record<ActivityKind, string> = {
  [ActivityKind.QuestionCreated]: 'Question created',
  [ActivityKind.QuestionUpdated]: 'Question updated',
  [ActivityKind.QuestionSubmitted]: 'Question submitted',
  [ActivityKind.QuestionApproved]: 'Question approved',
  [ActivityKind.QuestionRejected]: 'Question rejected',
  [ActivityKind.QuestionMarkedDuplicate]: 'Question marked duplicate',
  [ActivityKind.QuestionEscalated]: 'Question escalated',
  [ActivityKind.AnswerCreated]: 'Answer created',
  [ActivityKind.AnswerUpdated]: 'Answer updated',
  [ActivityKind.AnswerPublished]: 'Answer published',
  [ActivityKind.AnswerAccepted]: 'Answer accepted',
  [ActivityKind.AnswerValidated]: 'Answer validated',
  [ActivityKind.AnswerRejected]: 'Answer rejected',
  [ActivityKind.FeedbackReceived]: 'Feedback received',
  [ActivityKind.VoteReceived]: 'Vote received',
  [ActivityKind.AnswerRetired]: 'Answer retired',
  [ActivityKind.ReportReceived]: 'Report received',
};

export const actorKindLabels: Record<ActorKind, string> = {
  [ActorKind.System]: 'System',
  [ActorKind.Customer]: 'Customer',
  [ActorKind.Contributor]: 'Contributor',
  [ActorKind.Moderator]: 'Moderator',
  [ActorKind.Integration]: 'Integration',
};
