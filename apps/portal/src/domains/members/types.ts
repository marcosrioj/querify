import { TenantUserRoleType } from '@/shared/constants/backend-enums';

export type TenantUserDto = {
  id: string;
  tenantId: string;
  userId: string;
  givenName: string;
  surName?: string | null;
  email: string;
  role: TenantUserRoleType;
  isCurrentUser: boolean;
};

export type TenantUserCreateRequestDto = {
  email: string;
  role: TenantUserRoleType;
};

export type TenantUserUpdateRequestDto = {
  role: TenantUserRoleType;
};
