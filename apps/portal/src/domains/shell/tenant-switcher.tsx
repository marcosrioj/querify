import { Building2 } from 'lucide-react';
import { useTenant } from '@/platform/tenant/tenant-context';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui';

export function TenantSwitcher() {
  const { tenants, currentTenantId, setCurrentTenantId, isLoading } = useTenant();

  if (!tenants.length) {
    return (
      <div className="hidden items-center gap-2 rounded-xl border border-dashed border-border px-3 py-2 text-sm text-muted-foreground md:flex">
        <Building2 className="size-4" />
        {isLoading ? 'Loading workspaces' : 'No workspaces'}
      </div>
    );
  }

  return (
    <Select value={currentTenantId} onValueChange={setCurrentTenantId}>
      <SelectTrigger className="w-[220px]">
        <SelectValue placeholder="Select workspace" />
      </SelectTrigger>
      <SelectContent>
        {tenants.map((tenant) => (
          <SelectItem key={tenant.id} value={tenant.id}>
            {tenant.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
