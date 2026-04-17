import { useEffect, useState } from 'react';
import { Menu } from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import { toAbsoluteUrl } from '@/lib/helpers';
import { Button, Sheet, SheetBody, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/shared/ui';
import { Container } from '@/shared/layout/container';
import { PortalSidebar } from '@/domains/shell/portal-sidebar';

export function MobileHeader() {
  const { pathname } = useLocation();
  const [isSheetOpen, setIsSheetOpen] = useState(false);

  useEffect(() => {
    setIsSheetOpen(false);
  }, [pathname]);

  return (
    <header className="fixed inset-x-0 top-0 z-10 flex h-[var(--header-height)] items-center bg-muted lg:hidden">
      <Container className="flex items-center justify-between gap-3">
        <Link to="/app/dashboard">
          <img
            src={toAbsoluteUrl('/media/app/mini-logo-gray.svg')}
            className="min-h-[30px] dark:hidden"
            alt="BaseFAQ QnA Portal"
          />
          <img
            src={toAbsoluteUrl('/media/app/mini-logo-gray-dark.svg')}
            className="hidden min-h-[30px] dark:block"
            alt="BaseFAQ QnA Portal"
          />
        </Link>

        <Sheet open={isSheetOpen} onOpenChange={setIsSheetOpen}>
          <SheetTrigger asChild>
            <Button variant="ghost" mode="icon">
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
