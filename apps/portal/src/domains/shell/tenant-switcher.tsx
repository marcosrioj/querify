import { useQueryClient } from '@tanstack/react-query';
import { Building2 } from 'lucide-react';
import { flushSync } from 'react-dom';
import { useLocation, useNavigate } from 'react-router-dom';
import { useRefreshAllowedTenantCache } from '@/domains/tenants/hooks';
import { useTenant } from '@/platform/tenant/tenant-context';
import { tenantUserRoleTypeLabels } from '@/shared/constants/backend-enums';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui';

const GUID_PATH_SEGMENT_PATTERN =
  /(?:^|\/)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(?=\/|$)/i;
const tenantScopedQueryRoots = new Set([
  'faq',
  'faq-items',
  'content-refs',
]);

function hasGuidPathSegment(pathname: string) {
  return GUID_PATH_SEGMENT_PATTERN.test(pathname);
}

export function TenantSwitcher() {
  const location = useLocation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { tenants, currentTenantId, setCurrentTenantId, isLoading } = useTenant();
  const refreshAllowedTenantCache = useRefreshAllowedTenantCache();

  async function handleTenantChange(tenantId: string) {
    if (tenantId === currentTenantId || refreshAllowedTenantCache.isPending) {
      return;
    }

    try {
      const cacheUpdated = await refreshAllowedTenantCache.mutateAsync(tenantId);
      if (!cacheUpdated) {
        return;
      }

      flushSync(() => {
        setCurrentTenantId(tenantId);
      });

      if (hasGuidPathSegment(location.pathname)) {
        navigate('/app/dashboard', { replace: true });
        return;
      }

      if (location.pathname.startsWith('/app/dashboard')) {
        return;
      }

      await queryClient.invalidateQueries({
        predicate: (query) =>
          Array.isArray(query.queryKey) &&
          query.queryKey[0] === 'portal' &&
          typeof query.queryKey[1] === 'string' &&
          tenantScopedQueryRoots.has(query.queryKey[1]),
        refetchType: 'active',
      });
    } catch {
      // Mutation errors are surfaced by the shared query provider.
    }
  }

  if (!tenants.length) {
    return (
      <div className="hidden items-center gap-2 rounded-xl border border-dashed border-border px-3 py-2 text-sm text-muted-foreground md:flex">
        <Building2 className="size-4" />
        {isLoading ? 'Loading workspaces' : 'No workspaces'}
      </div>
    );
  }

  return (
    <Select
      value={currentTenantId}
      onValueChange={(tenantId) => void handleTenantChange(tenantId)}
      disabled={isLoading || refreshAllowedTenantCache.isPending}
    >
      <SelectTrigger className="w-full min-w-0 sm:w-[220px]">
        <SelectValue placeholder="Select workspace" />
      </SelectTrigger>
      <SelectContent>
        {tenants.map((tenant) => (
          <SelectItem key={tenant.id} value={tenant.id}>
            {tenant.name} · {tenantUserRoleTypeLabels[tenant.currentUserRole]}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
