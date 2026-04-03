import { Fragment, PropsWithChildren, ReactNode } from 'react';
import { ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Container } from '@/shared/layout/container';
import { Button, Card, CardContent } from '@/shared/ui';
import { cn } from '@/lib/utils';

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
  return (
    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
      <div className="space-y-2">
        {eyebrow ? (
          <p className="text-xs font-medium uppercase tracking-[0.24em] text-primary">
            {eyebrow}
          </p>
        ) : null}
        <div className="flex flex-wrap items-center gap-3">
          {backTo ? (
            <Button asChild mode="icon" variant="outline">
              <Link to={backTo}>
                <ArrowLeft className="size-4" />
              </Link>
            </Button>
          ) : null}
          <h1 className="text-2xl font-semibold tracking-tight text-mono lg:text-3xl">
            {title}
          </h1>
        </div>
        {description ? (
          <p className="max-w-3xl text-sm leading-6 text-muted-foreground">
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
  return (
    <Container className={cn('pb-10 pt-1', className)}>{children}</Container>
  );
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
    <PageSurface className="space-y-6">
      {header}
      {filters ? <Card><CardContent className="p-4">{filters}</CardContent></Card> : null}
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
    <PageSurface className="space-y-6">
      {header}
      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_320px]">
        <div className="space-y-6">{children}</div>
        {sidebar ? <div className="space-y-6">{sidebar}</div> : null}
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
    <PageSurface className="space-y-6">
      {header}
      <div className="grid gap-6 xl:grid-cols-[260px_minmax(0,1fr)]">
        <SettingsNav items={items} currentKey={currentKey} />
        <div className="space-y-6">{children}</div>
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
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      {items.map((item) => (
        <Card key={item.title}>
          <CardContent className="space-y-2 p-5">
            <p className="text-sm text-muted-foreground">{item.title}</p>
            <p className="text-2xl font-semibold text-mono">{item.value}</p>
            {item.description ? (
              <p className="text-sm text-muted-foreground">{item.description}</p>
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
    <dl className="space-y-4">
      {items.map((item) => (
        <Fragment key={item.label}>
          <div className="flex items-start justify-between gap-3">
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
