import {
  CheckCircle2,
  Clock3,
  MessageSquareText,
  ShieldCheck,
  Waypoints,
} from "lucide-react";
import { Link, Outlet } from "react-router-dom";
import { Helmet } from "react-helmet-async";
import { RuntimeEnv } from "@/platform/runtime/env";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { Card, CardContent } from "@/shared/ui";

export function AuthLayout() {
  const { t } = usePortalI18n();

  return (
    <>
      <Helmet>
        <title>
          {t("Sign in")} | {RuntimeEnv.appName}
        </title>
      </Helmet>

      <div className="portal-body-background grid grow lg:grid-cols-[minmax(0,0.88fr)_minmax(0,1.12fr)]">
        <div className="order-2 flex min-w-0 items-center justify-center p-4 sm:p-8 lg:order-1 lg:p-10">
          <Card className="w-full max-w-[440px] min-w-0">
            <CardContent className="p-6 lg:p-8">
              <Outlet />
            </CardContent>
          </Card>
        </div>

        <div className="order-1 m-5 hidden overflow-hidden rounded-lg border border-border/70 bg-background shadow-[0_28px_80px_-48px_rgba(24,24,27,0.55)] lg:flex">
          <div className="flex w-full flex-col justify-between gap-8 bg-linear-to-b from-background to-muted/40 p-8 lg:p-12">
            <Link
              to="/login"
              className="inline-flex w-fit items-center gap-2.5"
              aria-label={t("BaseFAQ QnA Portal")}
            >
              <span className="flex size-10 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-[0_14px_30px_-14px_rgba(16,185,129,0.8)]">
                <MessageSquareText className="size-5" />
              </span>
              <span className="text-xl font-semibold text-mono">BaseFAQ</span>
            </Link>

            <div className="max-w-xl space-y-5">
              <div className="space-y-3">
                <h1 className="text-2xl font-semibold text-mono">
                  {t("Manage your BaseFAQ QnA workspace")}
                </h1>
                <p className="text-base font-medium text-secondary-foreground">
                  {t(
                    "Sign in to manage spaces, questions, answers, sources, activity, billing, and workspace settings.",
                  )}
                </p>
              </div>

              <div className="rounded-lg border border-border/70 bg-background/90 p-4 shadow-[0_22px_70px_-40px_rgba(24,24,27,0.7)]">
                <div className="flex items-center justify-between gap-4 border-b border-border/60 pb-4">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">
                      {t("Workspace readiness")}
                    </p>
                    <p className="mt-1 text-lg font-semibold text-mono">
                      {t("QnA dashboard")}
                    </p>
                  </div>
                  <span className="rounded-full border border-primary/20 bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">
                    {t("Live")}
                  </span>
                </div>

                <div className="grid gap-3 py-4 sm:grid-cols-2">
                  {[
                    {
                      label: "Questions in review",
                      value: "12",
                      icon: Clock3,
                      tone: "text-amber-500 bg-amber-500/10",
                    },
                    {
                      label: "Active answers",
                      value: "48",
                      icon: CheckCircle2,
                      tone: "text-primary bg-primary/10",
                    },
                    {
                      label: "Sources",
                      value: "31",
                      icon: Waypoints,
                      tone: "text-sky-500 bg-sky-500/10",
                    },
                    {
                      label: "Accepted answers",
                      value: "26",
                      icon: ShieldCheck,
                      tone: "text-violet-500 bg-violet-500/10",
                    },
                  ].map((item) => (
                    <div
                      key={item.label}
                      className="rounded-lg border border-border/70 bg-muted/20 p-3"
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="text-xs text-muted-foreground">
                            {t(item.label)}
                          </p>
                          <p className="mt-1 text-2xl font-semibold text-mono">
                            {item.value}
                          </p>
                        </div>
                        <span
                          className={`flex size-8 items-center justify-center rounded-md ${item.tone}`}
                        >
                          <item.icon className="size-4" />
                        </span>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="space-y-2 border-t border-border/60 pt-4">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium text-foreground">
                      {t("Next: {label}", { label: t("Public client key") })}
                    </span>
                    <span className="font-semibold text-primary">75%</span>
                  </div>
                  <div className="h-2 rounded-full bg-primary/10">
                    <div className="h-full w-3/4 rounded-full bg-primary" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
