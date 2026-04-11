import {
  BillingIntervalType,
  BillingInvoiceStatus,
  BillingPaymentStatus,
  BillingProviderType,
  TenantSubscriptionStatus,
} from '@/shared/constants/backend-enums';

export type BillingInvoiceDto = {
  id: string;
  tenantId: string;
  tenantSubscriptionId: string | null;
  provider: BillingProviderType;
  externalInvoiceId: string;
  amountMinor: number;
  currency: string;
  dueDateUtc: string | null;
  paidAtUtc: string | null;
  status: BillingInvoiceStatus;
  hostedUrl: string | null;
  pdfUrl: string | null;
  createdDateUtc: string | null;
  updatedDateUtc: string | null;
};

export type BillingPaymentDto = {
  id: string;
  tenantId: string;
  billingInvoiceId: string | null;
  provider: BillingProviderType;
  externalPaymentId: string | null;
  method: string | null;
  amountMinor: number;
  currency: string;
  status: BillingPaymentStatus;
  failureCode: string | null;
  failureMessage: string | null;
  paidAtUtc: string | null;
  createdDateUtc: string | null;
  updatedDateUtc: string | null;
};

export type BillingProviderSubscriptionDto = {
  id: string;
  tenantId: string;
  tenantSubscriptionId: string;
  provider: BillingProviderType;
  externalSubscriptionId: string;
  externalPriceId: string | null;
  externalProductId: string | null;
  status: TenantSubscriptionStatus;
  currentPeriodStartUtc: string | null;
  currentPeriodEndUtc: string | null;
  trialEndsAtUtc: string | null;
  cancelAtPeriodEnd: boolean;
  cancelledAtUtc: string | null;
  lastEventCreatedAtUtc: string | null;
  createdDateUtc: string | null;
  updatedDateUtc: string | null;
};

export type TenantEntitlementSnapshotDto = {
  id: string;
  tenantId: string;
  planCode: string | null;
  subscriptionStatus: TenantSubscriptionStatus;
  isActive: boolean;
  isInGracePeriod: boolean;
  effectiveUntilUtc: string | null;
  featureJson: string | null;
  updatedAtUtc: string | null;
};

export type TenantSubscriptionDetailDto = {
  id: string | null;
  tenantId: string;
  planCode: string | null;
  billingInterval: BillingIntervalType;
  status: TenantSubscriptionStatus;
  currency: string | null;
  countryCode: string | null;
  trialEndsAtUtc: string | null;
  currentPeriodStartUtc: string | null;
  currentPeriodEndUtc: string | null;
  graceUntilUtc: string | null;
  defaultProvider: BillingProviderType;
  cancelAtPeriodEnd: boolean;
  cancelledAtUtc: string | null;
  lastEventCreatedAtUtc: string | null;
  providerSubscriptions: BillingProviderSubscriptionDto[];
};

export type TenantBillingSummaryDto = {
  tenantId: string;
  currentPlanCode: string | null;
  defaultProvider: BillingProviderType;
  subscriptionStatus: TenantSubscriptionStatus;
  trialEndsAtUtc: string | null;
  currentPeriodStartUtc: string | null;
  currentPeriodEndUtc: string | null;
  graceUntilUtc: string | null;
  lastInvoice: BillingInvoiceDto | null;
  lastPayment: BillingPaymentDto | null;
  entitlement: TenantEntitlementSnapshotDto | null;
};

export type BillingListRequest = {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
};
