import { Link } from "react-router-dom";
import { toAbsoluteUrl } from "@/lib/helpers";
import { PortalCommandDialog } from "@/domains/shell/portal-command-dialog";
import { TenantSwitcher } from "@/domains/shell/tenant-switcher";

export function SidebarHeader({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <div className="mb-3.5">
      <div className="flex h-[70px] items-center gap-2.5 px-3.5">
        <Link to="/app/dashboard" onClick={onNavigate}>
          <img
            src={toAbsoluteUrl("/media/app/default-logo.svg")}
            className="default-logo h-[28px] dark:hidden"
            alt="BaseFAQ Portal"
          />
          <img
            src={toAbsoluteUrl("/media/app/default-logo-dark.svg")}
            className="default-logo hidden h-[28px] dark:inline-block"
            alt="BaseFAQ Portal"
          />
          <img
            src={toAbsoluteUrl("/media/app/mini-logo-circle-primary.svg")}
            className="small-logo hidden h-[32px] dark:hidden"
            alt="BaseFAQ Portal"
          />
          <img
            src={toAbsoluteUrl("/media/app/mini-logo-circle-primary-dark.svg")}
            className="small-logo hidden h-[32px] dark:inline-block"
            alt="BaseFAQ Portal"
          />
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
