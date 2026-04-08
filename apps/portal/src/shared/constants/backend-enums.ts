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
