import { MessageSquareText } from "lucide-react";
import { Link } from "react-router-dom";
import { PortalCommandDialog } from "@/domains/shell/portal-command-dialog";
import { TenantSwitcher } from "@/domains/shell/tenant-switcher";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";

export function SidebarHeader({ onNavigate }: { onNavigate?: () => void }) {
  const { t } = usePortalI18n();

  return (
    <div className="mb-3.5">
      <div className="flex h-[70px] items-center gap-2.5 px-3.5">
        <Link
          to="/app/dashboard"
          onClick={onNavigate}
          className="inline-flex min-w-0 items-center gap-2.5"
          aria-label={t("BaseFAQ QnA Portal")}
        >
          <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-[0_12px_24px_-14px_rgba(16,185,129,0.9)]">
            <MessageSquareText className="size-4" />
          </span>
          <span className="default-logo text-lg font-semibold text-mono">
            BaseFAQ
          </span>
        </Link>
      </div>

      <div className="px-3.5 pt-2.5">
        <TenantSwitcher />
      </div>

      <div className="mb-1 px-3.5 pt-2.5">
        <PortalCommandDialog variant="sidebar" />
      </div>
    </div>
  );
}
