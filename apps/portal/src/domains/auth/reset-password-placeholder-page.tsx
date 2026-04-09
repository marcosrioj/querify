import { Link } from 'react-router-dom';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';

export function ResetPasswordPlaceholderPage() {
  const { t } = usePortalI18n();

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <h2 className="text-3xl font-semibold text-mono">{t('Reset password')}</h2>
        <p className="text-sm leading-6 text-muted-foreground">
          {t(
            'Portal delegates password recovery to Auth0. Keep this route as a UI placeholder unless the identity provider requires a custom callback screen here.',
          )}
        </p>
      </div>

      <div className="text-sm text-muted-foreground">
        <Link className="hover:text-foreground" to="/login">
          {t('Back to sign in')}
        </Link>
      </div>
    </div>
  );
}
