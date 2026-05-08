import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Building2, Check, ChevronsUpDown, LoaderCircle } from "lucide-react";
import { flushSync } from "react-dom";
import { useLocation, useMatches, useNavigate } from "react-router-dom";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { useRefreshAllowedTenantCache } from "@/domains/tenants/hooks";
import { useTenant } from "@/platform/tenant/use-tenant";
import { cn } from "@/lib/utils";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { tenantUserRoleTypeLabels } from "@/shared/constants/backend-enums";
import {
  Button,
  Popover,
  PopoverContent,
  PopoverTrigger,
  Skeleton,
} from "@/shared/ui";
import {
  TenantEditionBadge,
  TenantUserRoleBadge,
} from "@/shared/ui/status-badges";

const tenantScopedQueryRoots = new Set([
  "qna",
  "members",
  "billing",
  "settings",
  "tenant-domain",
]);

function isTenantScopedPortalQueryKey(queryKey: readonly unknown[]) {
  return (
    queryKey[0] === "portal" &&
    typeof queryKey[1] === "string" &&
    tenantScopedQueryRoots.has(queryKey[1])
  );
}

function hasIdSearchParam(search: string) {
  const searchParams = new URLSearchParams(search);

  for (const [key, value] of searchParams) {
    const normalizedKey = key.trim();
    const isIdParam =
      normalizedKey === "id" ||
      normalizedKey.endsWith("Id") ||
      normalizedKey.endsWith("ID");

    if (isIdParam && value.trim()) {
      return true;
    }
  }

  return false;
}

export function TenantSwitcher() {
  const { t } = usePortalI18n();
  const [open, setOpen] = useState(false);
  const location = useLocation();
  const matches = useMatches();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { tenants, currentTenantId, setCurrentTenantId, isLoading } =
    useTenant();
  const refreshAllowedTenantCache = useRefreshAllowedTenantCache();
  const currentTenant =
    tenants.find((tenant) => tenant.id === currentTenantId) ?? tenants[0];
  const selectedTenantId = currentTenantId ?? currentTenant?.id;
  const currentUrlHasId =
    matches.some((match) => Boolean(match.params.id)) ||
    hasIdSearchParam(location.search);

  async function handleTenantChange(tenantId: string) {
    if (tenantId === selectedTenantId || refreshAllowedTenantCache.isPending) {
      return;
    }

    try {
      const cacheUpdated = await refreshAllowedTenantCache.mutateAsync();
      if (!cacheUpdated) {
        return;
      }

      if (currentUrlHasId) {
        await queryClient.cancelQueries({
          predicate: (query) => isTenantScopedPortalQueryKey(query.queryKey),
        });
        await Promise.resolve(
          navigate("/app/dashboard", {
            replace: true,
            state: null,
            flushSync: true,
          }),
        );
        setCurrentTenantId(tenantId);
        return;
      }

      flushSync(() => {
        setCurrentTenantId(tenantId);
      });

      await queryClient.invalidateQueries({
        predicate: (query) => isTenantScopedPortalQueryKey(query.queryKey),
        refetchType: "active",
      });
    } catch {
      // Mutation errors are surfaced by the shared query provider.
    }
  }

  if (!tenants.length) {
    if (isLoading) {
      return <Skeleton className="h-[132px] w-full rounded-2xl" />;
    }

    return (
      <div className="rounded-2xl border border-dashed border-border bg-background/60 px-3.5 py-3.5">
        <div className="flex items-start gap-3">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
            <Building2 className="size-4" />
          </div>
          <div className="min-w-0">
            <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-muted-foreground/80">
              {t("Workspace")}
            </p>
            <p className="mt-1 text-sm font-medium text-mono">
              {t("No workspaces available")}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              {t(
                "Add or request access to a workspace to enable Portal features.",
              )}
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          size="lg"
          autoHeight
          className="h-auto w-full justify-start rounded-xl border-border/80 bg-background px-3 py-2.5 text-left shadow-xs shadow-black/5 hover:border-primary/20 hover:bg-primary/[0.03]"
          aria-label={t("Switch workspace")}
          disabled={refreshAllowedTenantCache.isPending}
        >
          <div className="flex w-full items-center gap-3">
            <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary ring-1 ring-inset ring-primary/15">
              <Building2 className="size-4" />
            </div>

            <div className="min-w-0 flex-1">
              <p className="text-[10px] font-semibold uppercase tracking-[0.16em] text-muted-foreground/75">
                {t("Workspace")}
              </p>
              <p className="mt-0.5 truncate text-sm font-semibold text-mono">
                {currentTenant?.name ?? t("Select workspace")}
              </p>
              <p className="truncate text-xs text-muted-foreground">
                {currentTenant?.slug
                  ? `@${currentTenant.slug}`
                  : t("Select a workspace to activate Portal features.")}
              </p>
            </div>

            <div className="pt-1 text-muted-foreground">
              {refreshAllowedTenantCache.isPending ? (
                <LoaderCircle className="size-4 animate-spin" />
              ) : (
                <ChevronsUpDown className="size-4" />
              )}
            </div>
          </div>
        </Button>
      </PopoverTrigger>

      <PopoverContent
        align="start"
        sideOffset={8}
        className="w-[min(24rem,calc(100vw-1.5rem))] overflow-hidden rounded-2xl border-border/80 bg-background p-0 shadow-[var(--shadow-premium-elevated)] ring-1 ring-black/5 dark:ring-white/[0.035]"
      >
        <Command className="[&_[cmdk-input-wrapper]]:mx-3 [&_[cmdk-input-wrapper]]:mb-3 [&_[cmdk-input-wrapper]]:mt-3 [&_[cmdk-input-wrapper]]:rounded-xl [&_[cmdk-input-wrapper]]:border [&_[cmdk-input-wrapper]]:border-border/70 [&_[cmdk-input-wrapper]]:bg-muted/30 [&_[cmdk-input-wrapper]]:px-3 [&_[cmdk-input-wrapper]_svg]:size-4 [&_[cmdk-input-wrapper]_svg]:text-muted-foreground [&_[cmdk-input]]:h-10 [&_[cmdk-input]]:py-0">
          <div className="border-b border-border/70 bg-muted/20 px-4 py-3.5">
            <div className="min-w-0">
              <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between gap-3">
                  <p className="text-sm font-semibold text-mono">
                    {t("Switch workspace")}
                  </p>
                  <span className="inline-flex items-center rounded-full border border-border/70 bg-background px-2 py-0.5 text-[11px] font-medium text-muted-foreground">
                    {t("{count} available", { count: tenants.length })}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <CommandInput placeholder={t("Search workspaces...")} autoFocus />

          <CommandList className="max-h-[360px] px-3 pb-3">
            <CommandEmpty className="py-8 text-sm text-muted-foreground">
              {t("No workspaces found.")}
            </CommandEmpty>
            <CommandGroup
              heading={t("Available workspaces")}
              className="p-0 [&_[cmdk-group-heading]]:px-1 [&_[cmdk-group-heading]]:pb-2 [&_[cmdk-group-heading]]:pt-0"
            >
              {tenants.map((tenant) => {
                const isSelected = tenant.id === selectedTenantId;
                const roleLabel = t(
                  tenantUserRoleTypeLabels[tenant.currentUserRole],
                );

                return (
                  <CommandItem
                    key={tenant.id}
                    value={`${tenant.name} ${tenant.slug} ${roleLabel}`}
                    keywords={[tenant.slug, roleLabel]}
                    disabled={refreshAllowedTenantCache.isPending}
                    onSelect={() => {
                      setOpen(false);
                      void handleTenantChange(tenant.id);
                    }}
                    className={cn(
                      "group mb-1.5 cursor-pointer items-start gap-3 rounded-xl border px-3 py-2.5 transition-all last:mb-0 hover:-translate-y-px hover:border-border/70 hover:bg-muted/45 hover:shadow-xs",
                      isSelected
                        ? "border-primary/20 bg-primary/[0.05]"
                        : "border-transparent bg-transparent",
                      "data-[selected=true]:border-border/70 data-[selected=true]:bg-muted/40",
                    )}
                  >
                    <span
                      className={cn(
                        "mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-sm border transition-colors group-hover:border-primary/40",
                        isSelected
                          ? "border-primary bg-primary text-primary-foreground"
                          : "border-input text-transparent",
                      )}
                    >
                      <Check className="size-3" />
                    </span>

                    <span className="min-w-0 flex-1">
                      <span className="flex items-start justify-between gap-3">
                        <span className="truncate font-medium text-foreground">
                          {tenant.name}
                        </span>
                        <span className="shrink-0">
                          <TenantEditionBadge edition={tenant.edition} />
                        </span>
                      </span>
                      <span className="mt-1 flex flex-wrap items-center gap-1.5">
                        <span className="truncate text-xs text-muted-foreground">
                          @{tenant.slug}
                        </span>
                        <TenantUserRoleBadge role={tenant.currentUserRole} />
                      </span>
                    </span>
                  </CommandItem>
                );
              })}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
