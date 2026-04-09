import { LockKeyhole, MoveRight } from 'lucide-react';
import { Navigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '@/platform/auth/use-auth';
import { RuntimeEnv } from '@/platform/runtime/env';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import { Alert, AlertDescription, Badge, Button } from '@/shared/ui';

export function LoginPage() {
  const { isConfigured, status, error, login } = useAuth();
  const { t } = usePortalI18n();
  const [searchParams] = useSearchParams();
  const nextPath = searchParams.get('next') ?? '/app/dashboard';
  const callbackUrl =
    RuntimeEnv.auth0RedirectUri || `${window.location.origin}${RuntimeEnv.baseUrl}login`;
  const logoutUrl =
    RuntimeEnv.auth0LogoutUri || `${window.location.origin}${RuntimeEnv.baseUrl}login`;

  if (status === 'ready') {
    return <Navigate to={nextPath} replace />;
  }

  return (
    <div className="space-y-5">
      <div className="space-y-4">
        <Badge variant="outline" className="w-fit">
          {t('BaseFAQ Portal')}
        </Badge>

        <div className="space-y-2">
          <h2 className="text-3xl font-semibold text-mono">
            {t('Sign in to your tenant workspace')}
          </h2>
          <p className="text-sm leading-6 text-muted-foreground">
            {t(
              'The Portal app authenticates against Auth0 and then calls only the Portal-side Tenant and FAQ APIs already present in this repository.',
            )}
          </p>
        </div>
      </div>

      {!isConfigured ? (
        <Alert variant="destructive">
          <AlertDescription>
            `VITE_AUTH0_CLIENT_ID` is not set. Auth0 domain and audience were
            derived from the backend config, but the Portal SPA client still
            needs to be configured in the frontend environment.
          </AlertDescription>
        </Alert>
      ) : null}

      {error ? (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}

      <div className="rounded-xl border border-border bg-muted/50 p-4 text-sm text-muted-foreground">
        <div className="flex items-center gap-2 font-medium text-mono">
          <LockKeyhole className="size-4" />
          {t('Auth runtime')}
        </div>
        <dl className="mt-3 space-y-2">
          <div className="flex items-center justify-between gap-3">
            <dt>{t('Authority')}</dt>
            <dd className="truncate text-right">{RuntimeEnv.auth0Domain}</dd>
          </div>
          <div className="flex items-center justify-between gap-3">
            <dt>{t('Audience')}</dt>
            <dd className="truncate text-right">{RuntimeEnv.auth0Audience}</dd>
          </div>
          <div className="flex items-center justify-between gap-3">
            <dt>{t('Callback')}</dt>
            <dd className="truncate text-right">{callbackUrl}</dd>
          </div>
          <div className="flex items-center justify-between gap-3">
            <dt>{t('Logout')}</dt>
            <dd className="truncate text-right">{logoutUrl}</dd>
          </div>
        </dl>
      </div>

      <Button
        className="h-12 w-full"
        disabled={!isConfigured || status === 'booting'}
        onClick={() => void login(nextPath)}
      >
        {t('Continue with Auth0')}
        <MoveRight className="size-4" />
      </Button>

      <div className="text-right text-sm text-muted-foreground">
        <span>{status === 'booting' ? t('Initializing session') : t('Portal login')}</span>
      </div>
    </div>
  );
}
