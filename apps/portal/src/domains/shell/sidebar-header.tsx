import { MessageSquareText } from "lucide-react";
import { Link } from "react-router-dom";
import { TenantSwitcher } from "@/domains/shell/tenant-switcher";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";

export function SidebarHeader({ onNavigate }: { onNavigate?: () => void }) {
  const { t } = usePortalI18n();

  return (
    <div className="mb-4">
      <div className="flex h-[76px] items-center gap-2.5 px-4">
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

      <div className="px-3.5 pt-1">
        <TenantSwitcher />
      </div>
    </div>
  );
}
