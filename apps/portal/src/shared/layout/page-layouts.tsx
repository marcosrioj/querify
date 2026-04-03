import { Fragment, PropsWithChildren, ReactNode } from 'react';
import { ArrowLeft } from 'lucide-react';
import { Link, useMatches } from 'react-router-dom';
import { AppRouteHandle } from '@/app/router/route-types';
import { Container } from '@/shared/layout/container';
import { Button, Card, CardContent } from '@/shared/ui';
import { cn } from '@/lib/utils';

function useCurrentRouteHandle() {
  return useMatches()
    .map((match) => match.handle as AppRouteHandle | undefined)
    .filter((handle): handle is AppRouteHandle => Boolean(handle?.title))
    .at(-1);
}

function shouldRenderPageTitle(
  title: ReactNode,
  handle: AppRouteHandle | undefined,
  backTo: string | undefined,
) {
  if (backTo || typeof title !== 'string' || !handle) {
    return true;
  }

  const normalizedTitle = title.trim().toLowerCase();
  const routeTitle = handle.title.trim().toLowerCase();
  const routeBreadcrumb = handle.breadcrumb?.trim().toLowerCase();

  return normalizedTitle !== routeTitle && normalizedTitle !== routeBreadcrumb;
}

export function PageHeader({
  eyebrow,
  title,
  description,
  actions,
  backTo,
}: {
  eyebrow?: string;
  title: ReactNode;
  description?: ReactNode;
  actions?: ReactNode;
  backTo?: string;
}) {
  const currentHandle = useCurrentRouteHandle();
  const renderTitle = shouldRenderPageTitle(title, currentHandle, backTo);

  return (
    <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
      <div className="min-w-0 space-y-3">
        {eyebrow ? (
          <p className="inline-flex w-fit rounded-full border border-primary/15 bg-primary/8 px-3 py-1 text-[0.6875rem] font-medium uppercase tracking-[0.2em] text-primary">
            {eyebrow}
          </p>
        ) : null}

        {renderTitle ? (
          <div className="flex flex-wrap items-start gap-3">
            {backTo ? (
              <Button asChild mode="icon" variant="outline" size="sm">
                <Link to={backTo}>
                  <ArrowLeft className="size-4" />
                </Link>
              </Button>
            ) : null}
            <h2 className="text-2xl font-semibold tracking-tight text-mono lg:text-3xl">
              {title}
            </h2>
          </div>
        ) : null}

        {description ? (
          <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
            {description}
          </p>
        ) : null}
      </div>

      {actions ? (
        <div className="flex w-full flex-col gap-3 sm:w-auto sm:flex-row sm:flex-wrap sm:items-center lg:justify-end [&>*]:w-full sm:[&>*]:w-auto">
          {actions}
        </div>
      ) : null}
    </div>
  );
}

export function PageSurface({
  children,
  className,
}: PropsWithChildren<{ className?: string }>) {
  return <Container className={cn('pb-10', className)}>{children}</Container>;
}

export function ListLayout({
  header,
  filters,
  children,
}: PropsWithChildren<{
  header: ReactNode;
  filters?: ReactNode;
}>) {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      {header}
      {filters ? (
        <Card className="border-dashed bg-muted/20">
          <CardContent className="space-y-3 p-4 lg:p-5">
            <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
              Refine view
            </p>
            {filters}
          </CardContent>
        </Card>
      ) : null}
      {children}
    </PageSurface>
  );
}

export function DetailLayout({
  header,
  sidebar,
  children,
}: PropsWithChildren<{
  header: ReactNode;
  sidebar?: ReactNode;
}>) {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      {header}
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_320px] lg:gap-7.5">
        <div className="space-y-5 lg:space-y-7.5">{children}</div>
        {sidebar ? <div className="min-w-0 space-y-5 lg:space-y-7.5">{sidebar}</div> : null}
      </div>
    </PageSurface>
  );
}

export function SettingsNav({
  items,
  currentKey,
}: {
  items: Array<{ key: string; label: string; href: string }>;
  currentKey: string;
}) {
  return (
    <Card>
      <CardContent className="p-2">
        <div className="grid gap-1">
          {items.map((item) => (
            <Button
              asChild
              key={item.key}
              variant={item.key === currentKey ? 'secondary' : 'ghost'}
              className="w-full justify-start"
            >
              <Link to={item.href}>{item.label}</Link>
            </Button>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

export function SettingsLayout({
  header,
  currentKey,
  items,
  children,
}: PropsWithChildren<{
  header: ReactNode;
  currentKey: string;
  items: Array<{ key: string; label: string; href: string }>;
}>) {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      {header}
      <div className="grid gap-5 xl:grid-cols-[260px_minmax(0,1fr)] lg:gap-7.5">
        <SettingsNav items={items} currentKey={currentKey} />
        <div className="min-w-0 space-y-5 lg:space-y-7.5">{children}</div>
      </div>
    </PageSurface>
  );
}

export function SectionGrid({
  items,
}: {
  items: Array<{ title: string; value: ReactNode; description?: ReactNode }>;
}) {
  return (
    <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
      {items.map((item) => (
        <Card key={item.title} className="bg-muted/10">
          <CardContent className="min-w-0 space-y-2.5 p-5">
            <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
              {item.title}
            </p>
            <div className="break-words text-xl font-semibold tracking-tight text-mono sm:text-2xl">
              {item.value}
            </div>
            {item.description ? (
              <p className="text-sm leading-5 text-muted-foreground">{item.description}</p>
            ) : null}
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export function KeyValueList({
  items,
}: {
  items: Array<{ label: string; value: ReactNode }>;
}) {
  return (
    <dl className="overflow-hidden rounded-xl border border-border/70">
      {items.map((item) => (
        <Fragment key={item.label}>
          <div className="flex flex-col gap-1.5 border-b border-border/70 px-4 py-3 last:border-b-0 sm:flex-row sm:items-start sm:justify-between sm:gap-4">
            <dt className="text-sm text-muted-foreground">{item.label}</dt>
            <dd className="break-words text-left text-sm font-medium text-foreground sm:text-right">
              {item.value}
            </dd>
          </div>
        </Fragment>
      ))}
    </dl>
  );
}
