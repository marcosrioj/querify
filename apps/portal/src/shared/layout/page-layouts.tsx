import { Fragment, PropsWithChildren, ReactNode } from "react";
import { ArrowLeft, type LucideIcon } from "lucide-react";
import { Link, useMatches } from "react-router-dom";
import { AppRouteHandle } from "@/app/router/route-types";
import { Container } from "@/shared/layout/container";
import { translateMaybeString, usePortalI18n } from "@/shared/lib/i18n";
import { Button, Card, CardContent, ContextHint } from "@/shared/ui";
import { cn } from "@/lib/utils";

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
  if (backTo || typeof title !== "string" || !handle) {
    return true;
  }

  const normalizedTitle = title.trim().toLowerCase();
  const routeTitle = handle.title.trim().toLowerCase();
  const routeBreadcrumb = handle.breadcrumb?.trim().toLowerCase();

  return normalizedTitle !== routeTitle && normalizedTitle !== routeBreadcrumb;
}

export function PageHeader({
  title,
  description,
  descriptionMode = "inline",
  actions,
  backTo,
}: {
  title: ReactNode;
  description?: ReactNode;
  descriptionMode?: "inline" | "hint";
  actions?: ReactNode;
  backTo?: string;
}) {
  const { t } = usePortalI18n();
  const currentHandle = useCurrentRouteHandle();
  const renderTitle = shouldRenderPageTitle(title, currentHandle, backTo);
  const descriptionHint =
    description && descriptionMode === "hint" ? (
      <ContextHint
        content={translateMaybeString(description, t)}
        label={t("Page details")}
        className="mt-0.5"
      />
    ) : null;

  return (
    <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
      <div className="min-w-0 space-y-3">
        {renderTitle ? (
          <div className="flex flex-wrap items-start gap-3">
            {backTo ? (
              <Button asChild mode="icon" variant="outline" size="sm">
                <Link to={backTo}>
                  <ArrowLeft className="size-4" />
                </Link>
              </Button>
            ) : null}
            <div className="flex min-w-0 flex-wrap items-start gap-2">
              <h2 className="text-2xl font-semibold tracking-tight text-mono lg:text-3xl">
                {translateMaybeString(title, t)}
              </h2>
              {descriptionHint}
            </div>
          </div>
        ) : null}

        {description && descriptionMode === "inline" ? (
          <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
            {translateMaybeString(description, t)}
          </p>
        ) : !renderTitle ? (
          descriptionHint
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
  return <Container className={cn("pb-10", className)}>{children}</Container>;
}

export function ListLayout({
  header,
  filters,
  children,
}: PropsWithChildren<{
  header: ReactNode;
  filters?: ReactNode;
}>) {
  const { t } = usePortalI18n();

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      {header}
      {filters ? (
        <Card className="border-dashed bg-muted/20">
          <CardContent className="space-y-3 p-4 lg:p-5">
            <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
              {t("Refine view")}
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
        {sidebar ? (
          <div className="min-w-0 space-y-5 lg:space-y-7.5">{sidebar}</div>
        ) : null}
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
  const { t } = usePortalI18n();

  return (
    <Card>
      <CardContent className="p-2">
        <div className="grid gap-1">
          {items.map((item) => (
            <Button
              asChild
              key={item.key}
              variant={item.key === currentKey ? "secondary" : "ghost"}
              className="w-full justify-start"
            >
              <Link to={item.href}>{t(item.label)}</Link>
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
  valueClassName,
}: {
  items: Array<{
    key?: string;
    title: ReactNode;
    titleHint?: ReactNode;
    value: ReactNode;
    description?: ReactNode;
    icon?: LucideIcon;
    iconToneClassName?: string;
  }>;
  valueClassName?: string;
}) {
  const { t } = usePortalI18n();
  const toneClassNames = [
    "bg-blue-500/10 text-blue-600 dark:bg-blue-500/15 dark:text-blue-300",
    "bg-emerald-500/10 text-emerald-600 dark:bg-emerald-500/15 dark:text-emerald-300",
    "bg-cyan-500/10 text-cyan-600 dark:bg-cyan-500/15 dark:text-cyan-300",
    "bg-amber-500/10 text-amber-600 dark:bg-amber-500/15 dark:text-amber-300",
  ];

  return (
    <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
      {items.map((item, index) => (
        <Card
          key={
            item.key ??
            (typeof item.title === "string"
              ? item.title
              : `section-grid-${index}`)
          }
          className="bg-muted/10"
        >
          <CardContent className="relative min-w-0 p-5">
            <div className="min-w-0 space-y-2.5">
              <p className="flex items-center gap-1.5 text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
                <span>{translateMaybeString(item.title, t)}</span>
                {item.titleHint ? (
                  <ContextHint
                    content={translateMaybeString(item.titleHint, t)}
                    label={t("Metric details")}
                    className="size-4 text-[inherit]"
                  />
                ) : null}
              </p>
              <div
                className={cn(
                  "break-words text-2xl font-semibold tracking-tight text-mono sm:text-3xl",
                  valueClassName,
                )}
              >
                {translateMaybeString(item.value, t)}
              </div>
              {item.description ? (
                <p className="text-sm leading-6 text-muted-foreground">
                  {translateMaybeString(item.description, t)}
                </p>
              ) : null}
            </div>
            {item.icon ? (
              <div
                className={cn(
                  "pointer-events-none absolute right-5 top-5 flex size-5 items-center justify-center rounded-2xl",
                  item.iconToneClassName ?? toneClassNames[index % toneClassNames.length],
                )}
              >
                <item.icon className="size-5" />
              </div>
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
  const { t } = usePortalI18n();

  return (
    <dl className="overflow-hidden rounded-xl border border-border/70">
      {items.map((item) => (
        <Fragment key={item.label}>
          <div className="flex flex-col gap-1.5 border-b border-border/70 px-4 py-3 last:border-b-0 sm:flex-row sm:items-start sm:justify-between sm:gap-4">
            <dt className="text-sm text-muted-foreground">{t(item.label)}</dt>
            <dd className="break-words text-left text-sm font-medium text-foreground sm:text-right">
              {translateMaybeString(item.value, t)}
            </dd>
          </div>
        </Fragment>
      ))}
    </dl>
  );
}
