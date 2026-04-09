import { useEffect, useRef, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/platform/auth/auth-context';
import { usePortalI18n } from '@/shared/lib/i18n';

export function LogoutPage() {
  const { logout, status } = useAuth();
  const { t } = usePortalI18n();
  const [logoutStarted, setLogoutStarted] = useState(false);
  const logoutRequestedRef = useRef(false);

  useEffect(() => {
    if (status !== 'ready' || logoutRequestedRef.current) {
      return;
    }

    logoutRequestedRef.current = true;
    setLogoutStarted(true);
    void logout().catch(() => {
      logoutRequestedRef.current = false;
      setLogoutStarted(false);
    });
  }, [logout, status]);

  if (status === 'booting' || logoutStarted) {
    return (
      <div className="rounded-2xl border border-border bg-background px-6 py-4 text-sm text-muted-foreground shadow-sm">
        {t('Initializing session')}
      </div>
    );
  }

  if (status !== 'ready') {
    return <Navigate to="/login" replace />;
  }

  return null;
}
