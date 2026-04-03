import { Menu } from 'lucide-react';
import { Outlet, useMatches } from 'react-router-dom';
import { useState } from 'react';
import { Helmet } from 'react-helmet-async';
import { AppRouteHandle } from '@/app/router/route-types';
import { NotificationsMenu } from '@/domains/shell/notifications-menu';
import { PortalCommandDialog } from '@/domains/shell/portal-command-dialog';
import { PortalSidebar } from '@/domains/shell/portal-sidebar';
import { TenantSwitcher } from '@/domains/shell/tenant-switcher';
import { UserMenu } from '@/domains/shell/user-menu';
import { Container } from '@/shared/layout/container';
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
  Button,
  Sheet,
  SheetBody,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/shared/ui';

function useRouteHandles() {
  return useMatches()
    .map((match) => match.handle as AppRouteHandle | undefined)
    .filter((handle): handle is AppRouteHandle => Boolean(handle?.title));
}

export function PortalLayout() {
  const handles = useRouteHandles();
  const title = handles.at(-1)?.title ?? 'BaseFAQ Portal';
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <>
      <Helmet>
        <title>{title} | BaseFAQ Portal</title>
      </Helmet>

      <div className="min-h-screen bg-muted">
        <aside className="fixed inset-y-0 left-0 z-30 hidden w-[270px] border-r border-border bg-background/95 backdrop-blur lg:flex">
          <PortalSidebar />
        </aside>

        <div className="flex min-h-screen flex-col lg:pl-[270px]">
          <header className="sticky top-0 z-20 border-b border-border bg-background/90 backdrop-blur">
            <Container className="flex h-[68px] items-center justify-between gap-3">
              <div className="flex min-w-0 items-center gap-3">
                <Sheet open={mobileNavOpen} onOpenChange={setMobileNavOpen}>
                  <SheetTrigger asChild className="lg:hidden">
                    <Button mode="icon" variant="outline">
                      <Menu className="size-4" />
                    </Button>
                  </SheetTrigger>
                  <SheetContent side="left" className="w-[290px] p-0">
                    <SheetHeader className="sr-only">
                      <SheetTitle>Portal navigation</SheetTitle>
                    </SheetHeader>
                    <SheetBody className="p-0">
                      <PortalSidebar onNavigate={() => setMobileNavOpen(false)} />
                    </SheetBody>
                  </SheetContent>
                </Sheet>

                <div className="min-w-0">
                  <div className="truncate text-sm font-medium text-mono">
                    {title}
                  </div>
                  <Breadcrumb className="hidden md:block">
                    <BreadcrumbList>
                      {handles.map((handle, index) => {
                        const isLast = index === handles.length - 1;
                        return (
                          <div key={`${handle.title}-${index}`} className="flex items-center">
                            <BreadcrumbItem>
                              {isLast ? (
                                <BreadcrumbPage>{handle.breadcrumb ?? handle.title}</BreadcrumbPage>
                              ) : (
                                <BreadcrumbLink>
                                  {handle.breadcrumb ?? handle.title}
                                </BreadcrumbLink>
                              )}
                            </BreadcrumbItem>
                            {!isLast ? <BreadcrumbSeparator /> : null}
                          </div>
                        );
                      })}
                    </BreadcrumbList>
                  </Breadcrumb>
                </div>
              </div>

              <div className="flex items-center gap-2 md:gap-3">
                <TenantSwitcher />
                <PortalCommandDialog />
                <NotificationsMenu />
                <UserMenu />
              </div>
            </Container>
          </header>

          <main className="flex-1 py-6">
            <Outlet />
          </main>
        </div>
      </div>
    </>
  );
}
