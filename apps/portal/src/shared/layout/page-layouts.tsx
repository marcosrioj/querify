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
      <div className="space-y-3">
        {eyebrow ? (
          <p className="inline-flex w-fit rounded-full border border-primary/15 bg-primary/8 px-3 py-1 text-[0.6875rem] font-medium uppercase tracking-[0.2em] text-primary">
            {eyebrow}
          </p>
        ) : null}

        {renderTitle ? (
          <div className="flex flex-wrap items-center gap-3">
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
        <div className="flex flex-wrap items-center gap-3">{actions}</div>
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
        {sidebar ? <div className="space-y-5 lg:space-y-7.5">{sidebar}</div> : null}
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
              className="justify-start"
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
        <div className="space-y-5 lg:space-y-7.5">{children}</div>
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
    <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
      {items.map((item) => (
        <Card key={item.title} className="bg-muted/10">
          <CardContent className="space-y-2.5 p-5">
            <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
              {item.title}
            </p>
            <p className="text-2xl font-semibold tracking-tight text-mono">{item.value}</p>
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
          <div className="flex items-start justify-between gap-4 border-b border-border/70 px-4 py-3 last:border-b-0">
            <dt className="text-sm text-muted-foreground">{item.label}</dt>
            <dd className="text-right text-sm font-medium text-foreground">
              {item.value}
            </dd>
          </div>
        </Fragment>
      ))}
    </dl>
  );
}
