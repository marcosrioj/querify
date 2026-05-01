import { Fragment, PropsWithChildren, ReactNode } from "react";
import { type LucideIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Container } from "@/shared/layout/container";
import {
  useRegisterPageChrome,
  type PageChromeBreadcrumb,
} from "@/shared/layout/page-chrome-context";
import { translateMaybeString } from "@/shared/lib/i18n-render";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import {
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ContextHint,
} from "@/shared/ui";
import { cn } from "@/lib/utils";

export function PageHeader({
  title,
  description,
  descriptionMode = "inline",
  actions,
  backTo,
  breadcrumbs,
}: {
  title: ReactNode;
  description?: ReactNode;
  descriptionMode?: "inline" | "hint";
  actions?: ReactNode;
  backTo?: string;
  breadcrumbs?: PageChromeBreadcrumb[];
}) {
  const { t } = usePortalI18n();
  const inlineDescription = description && descriptionMode === "inline";

  useRegisterPageChrome({
    title,
    description,
    descriptionMode,
    backTo,
    breadcrumbs,
  });

  if (!inlineDescription && !actions) {
    return null;
  }

  return (
    <div className="flex min-w-0 flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
      {inlineDescription ? (
        <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
          {translateMaybeString(description, t)}
        </p>
      ) : null}

      {actions ? (
        <div className="flex w-full min-w-0 flex-col gap-3 sm:w-auto sm:flex-row sm:flex-wrap sm:items-center lg:justify-end [&>*]:w-full sm:[&>*]:w-auto [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal">
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
  return (
    <Container className={cn("pb-10 pt-5 lg:pt-6", className)}>
      {children}
    </Container>
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
  const { t } = usePortalI18n();

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      {header}
      {filters ? (
        <Card className="border-dashed bg-muted/20 shadow-none">
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
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_340px] lg:gap-7.5">
        <div className="min-w-0 space-y-5 lg:space-y-7.5">{children}</div>
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
  variant = "default",
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
  variant?: "default" | "sidebar";
}) {
  const { t } = usePortalI18n();
  const isSidebar = variant === "sidebar";
  const toneClassNames = [
    "bg-blue-500/10 text-blue-600 dark:bg-blue-500/15 dark:text-blue-300",
    "bg-emerald-500/10 text-emerald-600 dark:bg-emerald-500/15 dark:text-emerald-300",
    "bg-cyan-500/10 text-cyan-600 dark:bg-cyan-500/15 dark:text-cyan-300",
    "bg-amber-500/10 text-amber-600 dark:bg-amber-500/15 dark:text-amber-300",
  ];

  return (
    <div
      className={cn(
        "grid gap-4",
        isSidebar ? "grid-cols-1" : "sm:grid-cols-2 xl:grid-cols-4 lg:gap-5",
      )}
    >
      {items.map((item, index) => (
        <Card
          key={
            item.key ??
            (typeof item.title === "string"
              ? item.title
              : `section-grid-${index}`)
          }
          className={cn(
            "group overflow-hidden bg-linear-to-b from-background to-muted/10 transition-transform duration-200 hover:-translate-y-0.5 hover:shadow-[var(--shadow-premium-elevated)]",
            isSidebar && "shadow-none",
          )}
        >
          <CardContent
            className={cn("relative min-w-0", isSidebar ? "p-4" : "p-5")}
          >
            <div
              className={cn(
                "flex items-start justify-between",
                isSidebar ? "gap-3" : "gap-4",
              )}
            >
              <div className="min-w-0 space-y-2.5">
                <p
                  className={cn(
                    "flex items-center gap-1.5 text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground",
                    isSidebar && "tracking-[0.14em]",
                  )}
                >
                  <span className="min-w-0 break-words">
                    {translateMaybeString(item.title, t)}
                  </span>
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
                    "break-words font-semibold leading-none text-mono",
                    isSidebar ? "text-xl" : "text-[1.7rem] sm:text-3xl",
                    valueClassName,
                  )}
                >
                  {translateMaybeString(item.value, t)}
                </div>
              </div>
              {item.icon ? (
                <div
                  className={cn(
                    "pointer-events-none flex shrink-0 items-center justify-center rounded-lg ring-1 ring-inset",
                    isSidebar ? "size-8" : "size-9",
                    item.iconToneClassName ??
                      toneClassNames[index % toneClassNames.length],
                  )}
                >
                  <item.icon className="size-4" />
                </div>
              ) : null}
            </div>
            <div className="mt-3 min-w-0">
              <div
                className={cn(
                  "h-px w-full bg-linear-to-r from-border via-border/60 to-transparent",
                )}
              />
              {item.description ? (
                <p className="mt-3 text-sm leading-6 text-muted-foreground">
                  {translateMaybeString(item.description, t)}
                </p>
              ) : null}
            </div>
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

export function DetailOverviewCard({
  title = "Overview",
  description,
  highlights,
  items,
}: {
  title?: string;
  description?: ReactNode;
  highlights: Array<{ label: string; value: ReactNode }>;
  items: Array<{ label: string; value: ReactNode }>;
}) {
  const { t } = usePortalI18n();

  return (
    <Card className="overflow-hidden">
      <CardHeader className="px-4 py-3">
        <CardHeading>
          <CardTitle className="flex items-center gap-2 text-[0.9375rem]">
            <span>{translateMaybeString(title, t)}</span>
            {description ? (
              <ContextHint
                content={translateMaybeString(description, t)}
                label={t("{title} details", { title: String(title) })}
              />
            ) : null}
          </CardTitle>
        </CardHeading>
      </CardHeader>
      <CardContent className="space-y-4 p-4">
        <div className="grid grid-cols-2 gap-2">
          {highlights.map((item) => (
            <div
              key={item.label}
              className="min-w-0 rounded-lg border border-border/70 bg-muted/15 p-3"
            >
              <p className="text-[0.6875rem] font-medium uppercase tracking-[0.14em] text-muted-foreground">
                {t(item.label)}
              </p>
              <div className="mt-2 min-w-0 break-words text-sm font-semibold leading-5 text-foreground">
                {translateMaybeString(item.value, t)}
              </div>
            </div>
          ))}
        </div>
        <KeyValueList items={items} />
      </CardContent>
    </Card>
  );
}

export function DetailFieldList({
  items,
}: {
  items: Array<{
    label: string;
    value: ReactNode;
    valueClassName?: string;
  }>;
}) {
  const { t } = usePortalI18n();

  return (
    <div className="overflow-hidden rounded-lg border border-border/70 bg-background">
      {items.map((item, index) => (
        <section
          key={item.label}
          className="grid gap-2.5 border-b border-border/60 px-4 py-4 last:border-b-0 lg:grid-cols-[160px_minmax(0,1fr)] lg:gap-5"
        >
          <div className="flex min-w-0 items-start gap-2">
            <span
              className={cn(
                "mt-2.5 size-1 shrink-0 rounded-full",
                index === 0 ? "bg-primary/70" : "bg-border",
              )}
            />
            <p className="min-w-0 text-xs font-medium uppercase leading-6 tracking-[0.12em] text-muted-foreground">
              {t(item.label)}
            </p>
          </div>
          <div
            className={cn(
              "min-w-0 text-sm leading-6 text-foreground [overflow-wrap:anywhere]",
              index === 0 && "font-medium",
              item.valueClassName,
            )}
          >
            {translateMaybeString(item.value, t)}
          </div>
        </section>
      ))}
    </div>
  );
}
