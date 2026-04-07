import {
  AiCommandType,
  PortalApp,
  TenantEdition,
  TenantUserRoleType,
} from '@/shared/constants/backend-enums';

export type TenantSummaryDto = {
  id: string;
  slug: string;
  name: string;
  edition: TenantEdition;
  app: PortalApp;
  isActive: boolean;
  currentUserRole: TenantUserRoleType;
};

export type TenantAiProviderDto = {
  id: string;
  tenantId: string;
  aiProviderId: string;
  provider: string;
  model: string;
  command: AiCommandType;
  isAiProviderKeyConfigured: boolean;
};

export type TenantCreateOrUpdateRequestDto = {
  name: string;
  edition: TenantEdition;
};

export type TenantSetAiProviderCredentialsRequestDto = {
  aiProviderId: string;
  aiProviderKey: string;
};
