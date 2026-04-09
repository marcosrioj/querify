import { createContext } from 'react';
import type { TenantSummaryDto } from '@/domains/tenants/types';

export type TenantContextValue = {
  tenants: TenantSummaryDto[];
  currentTenantId?: string;
  currentTenant?: TenantSummaryDto;
  isLoading: boolean;
  setCurrentTenantId: (tenantId: string) => void;
  refreshTenants: () => Promise<void>;
};

export const TenantContext = createContext<TenantContextValue | undefined>(
  undefined,
);
