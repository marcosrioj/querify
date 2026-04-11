export enum PortalApp {
  Tenant = 0,
  Faq = 1,
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

export enum AiCommandType {
  Generation = 10,
  Matching = 20,
}

export enum FaqStatus {
  Draft = 0,
  Published = 1,
  Archived = 2,
}

export enum ContentRefKind {
  Manual = 1,
  Web = 2,
  Pdf = 3,
  Document = 4,
  Video = 5,
  Repository = 6,
  Faq = 7,
  FaqItem = 8,
  Other = 99,
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

export const faqStatusLabels: Record<FaqStatus, string> = {
  [FaqStatus.Draft]: 'Draft',
  [FaqStatus.Published]: 'Published',
  [FaqStatus.Archived]: 'Archived',
};

export const contentRefKindLabels: Record<ContentRefKind, string> = {
  [ContentRefKind.Manual]: 'Manual',
  [ContentRefKind.Web]: 'Web',
  [ContentRefKind.Pdf]: 'PDF',
  [ContentRefKind.Document]: 'Document',
  [ContentRefKind.Video]: 'Video',
  [ContentRefKind.Repository]: 'Repository',
  [ContentRefKind.Faq]: 'FAQ',
  [ContentRefKind.FaqItem]: 'FAQ Item',
  [ContentRefKind.Other]: 'Other',
};
