import {
  ModuleEnum,
  TenantEdition,
  TenantUserRoleType,
} from '@/shared/constants/backend-enums';

export type TenantSummaryDto = {
  id: string;
  slug: string;
  name: string;
  edition: TenantEdition;
  module: ModuleEnum;
  isActive: boolean;
  currentUserRole: TenantUserRoleType;
};

export type TenantCreateOrUpdateRequestDto = {
  name: string;
  edition: TenantEdition;
};
