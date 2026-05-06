import { LockKeyhole, MessageSquareText, MoveRight } from "lucide-react";
import { Navigate, useSearchParams } from "react-router-dom";
import { useAuth } from "@/platform/auth/use-auth";
import { RuntimeEnv } from "@/platform/runtime/env";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { LanguageSelectorControl } from "@/shared/ui/language-selector-control";
import { Alert, AlertDescription, Button } from "@/shared/ui";

export function LoginPage() {
  const { isConfigured, status, error, login } = useAuth();
  const { language, setLanguage, t } = usePortalI18n();
  const [searchParams] = useSearchParams();
  const nextPath = searchParams.get("next") ?? "/app/dashboard";
  const callbackUrl =
    RuntimeEnv.auth0RedirectUri ||
    `${window.location.origin}${RuntimeEnv.baseUrl}login`;
  const logoutUrl =
    RuntimeEnv.auth0LogoutUri ||
    `${window.location.origin}${RuntimeEnv.baseUrl}login`;

  if (status === "ready") {
    return <Navigate to={nextPath} replace />;
  }

  return (
    <div className="space-y-5">
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="inline-flex min-w-0 items-center gap-2.5">
            <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-[0_14px_30px_-14px_rgba(16,185,129,0.8)]">
              <MessageSquareText className="size-4" />
            </span>
            <span className="truncate text-sm font-semibold text-mono">
              Querify
            </span>
          </div>
          <LanguageSelectorControl
            language={language}
            onLanguageChange={setLanguage}
            ariaLabel={`${t("Language")}: ${language}`}
          />
        </div>

        <div className="space-y-2">
          <h2 className="text-3xl font-semibold text-mono">{t("Sign in")}</h2>
          <p className="text-sm leading-6 text-muted-foreground">
            {t("Manage your Querify QnA workspace")}
          </p>
        </div>
      </div>

      {!isConfigured ? (
        <Alert variant="destructive">
          <AlertDescription>
            {t(
              "Auth0 is not fully configured. Set VITE_AUTH0_CLIENT_ID to enable login.",
            )}
          </AlertDescription>
        </Alert>
      ) : null}

      {error ? (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}

      <Button
        className="h-12 w-full"
        disabled={!isConfigured || status === "booting"}
        onClick={() => void login(nextPath)}
      >
        {t("Continue with Auth0")}
        <MoveRight className="size-4" />
      </Button>

      <details className="group text-sm text-muted-foreground">
        <summary className="flex cursor-pointer list-none items-center gap-2 font-medium text-muted-foreground marker:hidden hover:text-foreground">
          <LockKeyhole className="size-4" />
          {t("Auth runtime")}
        </summary>
        <dl className="mt-3 space-y-2 rounded-lg border border-border/70 bg-muted/30 p-4">
          <div className="flex items-center justify-between gap-3">
            <dt>{t("Authority")}</dt>
            <dd className="truncate text-right">{RuntimeEnv.auth0Domain}</dd>
          </div>
          <div className="flex items-center justify-between gap-3">
            <dt>{t("Audience")}</dt>
            <dd className="truncate text-right">{RuntimeEnv.auth0Audience}</dd>
          </div>
          <div className="flex items-center justify-between gap-3">
            <dt>{t("Callback")}</dt>
            <dd className="truncate text-right">{callbackUrl}</dd>
          </div>
          <div className="flex items-center justify-between gap-3">
            <dt>{t("Logout")}</dt>
            <dd className="truncate text-right">{logoutUrl}</dd>
          </div>
        </dl>
      </details>
    </div>
  );
}
