import {
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

export type TenantCreateOrUpdateRequestDto = {
  name: string;
  edition: TenantEdition;
};
