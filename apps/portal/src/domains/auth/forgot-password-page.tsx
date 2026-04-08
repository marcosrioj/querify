import { ArrowUpRight, KeyRound } from 'lucide-react';
import { Link } from 'react-router-dom';
import { RuntimeEnv } from '@/platform/runtime/env';
import { usePortalI18n } from '@/shared/lib/i18n';
import { Alert, AlertDescription, Button } from '@/shared/ui';

export function ForgotPasswordPage() {
  const hasResetUrl = Boolean(RuntimeEnv.auth0PasswordResetUrl);
  const { t } = usePortalI18n();

  return (
    <div className="space-y-5">
      <div className="space-y-4">
        <div className="inline-flex size-12 items-center justify-center rounded-2xl bg-muted text-mono">
          <KeyRound className="size-5" />
        </div>

        <div className="space-y-2">
          <h2 className="text-3xl font-semibold text-mono">{t('Password reset')}</h2>
          <p className="text-sm leading-6 text-muted-foreground">
            {t(
              'There is no Portal-owned password reset endpoint in the repo. This flow should stay with the external identity provider.',
            )}
          </p>
        </div>
      </div>

      {hasResetUrl ? (
        <Button asChild className="w-full">
          <a
            href={RuntimeEnv.auth0PasswordResetUrl}
            target="_blank"
            rel="noreferrer"
          >
            {t('Open Auth0 reset flow')}
            <ArrowUpRight className="size-4" />
          </a>
        </Button>
      ) : (
        <Alert>
          <AlertDescription>
            Set `VITE_AUTH0_PASSWORD_RESET_URL` when your Auth0 reset screen is
            provisioned for the Portal SPA.
          </AlertDescription>
        </Alert>
      )}

      <div className="text-sm text-muted-foreground">
        <Link className="hover:text-foreground" to="/login">
          {t('Back to sign in')}
        </Link>
      </div>
    </div>
  );
}
