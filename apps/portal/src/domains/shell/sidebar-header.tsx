import { MessageSquareText, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { Link } from "react-router-dom";
import { TenantSwitcher } from "@/domains/shell/tenant-switcher";
import { Button, Tooltip, TooltipContent, TooltipTrigger } from "@/shared/ui";
import { cn } from "@/lib/utils";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";

export function SidebarHeader({
  compact = false,
  onCompactChange,
  onNavigate,
}: {
  compact?: boolean;
  onCompactChange?: (compact: boolean) => void;
  onNavigate?: () => void;
}) {
  const { t } = usePortalI18n();
  const brandLink = (
    <Link
      to="/app/dashboard"
      onClick={onNavigate}
      className={cn(
        "inline-flex min-w-0 items-center gap-2.5",
        compact && "justify-center",
      )}
      aria-label={t("BaseFAQ QnA Portal")}
    >
      <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-[0_12px_24px_-14px_rgba(16,185,129,0.9)]">
        <MessageSquareText className="size-4" />
      </span>
      {!compact ? (
        <span className="default-logo text-lg font-semibold text-mono">
          BaseFAQ
        </span>
      ) : null}
    </Link>
  );

  return (
    <div className={cn("mb-4", compact && "mb-3")}>
      <div
        className={cn(
          "flex h-[76px] items-center gap-2.5 px-4",
          compact && "justify-center px-2",
        )}
      >
        {compact ? (
          <Tooltip>
            <TooltipTrigger asChild>{brandLink}</TooltipTrigger>
            <TooltipContent side="right">
              {t("BaseFAQ QnA Portal")}
            </TooltipContent>
          </Tooltip>
        ) : (
          brandLink
        )}

        {!compact ? (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                mode="icon"
                className="ms-auto size-9 rounded-lg hover:bg-background"
                aria-label={t("Collapse sidebar")}
                onClick={() => onCompactChange?.(true)}
              >
                <PanelLeftClose className="size-4" />
              </Button>
            </TooltipTrigger>
            <TooltipContent side="right">
              {t("Collapse sidebar")}
            </TooltipContent>
          </Tooltip>
        ) : null}
      </div>

      {compact ? (
        <div className="flex justify-center px-2 pb-1">
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                mode="icon"
                className="size-11 rounded-lg hover:bg-background"
                aria-label={t("Expand sidebar")}
                onClick={() => onCompactChange?.(false)}
              >
                <PanelLeftOpen className="size-4" />
              </Button>
            </TooltipTrigger>
            <TooltipContent side="right">{t("Expand sidebar")}</TooltipContent>
          </Tooltip>
        </div>
      ) : (
        <div className="px-3.5 pt-1">
          <TenantSwitcher />
        </div>
      )}
    </div>
  );
}
