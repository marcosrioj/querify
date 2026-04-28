import { useEffect, useState } from "react";
import { Menu, MessageSquareText } from "lucide-react";
import { Link, useLocation } from "react-router-dom";
import {
  Button,
  Sheet,
  SheetBody,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/shared/ui";
import { Container } from "@/shared/layout/container";
import { PortalSidebar } from "@/domains/shell/portal-sidebar";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";

export function MobileHeader() {
  const { pathname } = useLocation();
  const { t } = usePortalI18n();
  const [isSheetOpen, setIsSheetOpen] = useState(false);

  useEffect(() => {
    setIsSheetOpen(false);
  }, [pathname]);

  return (
    <header className="fixed inset-x-0 top-0 z-10 flex h-[var(--header-height)] items-center bg-muted xl:hidden">
      <Container className="flex items-center justify-between gap-3">
        <Link
          to="/app/dashboard"
          className="inline-flex items-center gap-2"
          aria-label="BaseFAQ QnA Portal"
        >
          <span className="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <MessageSquareText className="size-4" />
          </span>
          <span className="text-base font-semibold text-mono">BaseFAQ</span>
        </Link>

        <Sheet open={isSheetOpen} onOpenChange={setIsSheetOpen}>
          <SheetTrigger asChild>
            <Button variant="ghost" mode="icon" aria-label={t("Open navigation")}>
              <Menu />
            </Button>
          </SheetTrigger>
          <SheetContent
            side="left"
            close={false}
            className="w-[min(275px,85vw)] gap-0 p-0"
          >
            <SheetHeader className="sr-only">
              <SheetTitle>Portal navigation</SheetTitle>
            </SheetHeader>
            <SheetBody className="flex grow flex-col p-0">
              <PortalSidebar mobile onNavigate={() => setIsSheetOpen(false)} />
            </SheetBody>
          </SheetContent>
        </Sheet>
      </Container>
    </header>
  );
}
