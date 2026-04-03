import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/platform/auth/auth-context';

export function RequirePortalAuth() {
  const { status } = useAuth();
  const location = useLocation();

  if (status === 'booting') {
    return (
      <div className="flex min-h-screen items-center justify-center bg-muted">
        <div className="rounded-2xl border border-border bg-background px-6 py-4 text-sm text-muted-foreground shadow-sm">
          Preparing your tenant workspace…
        </div>
      </div>
    );
  }

  if (status !== 'ready') {
    return (
      <Navigate
        to={`/login?next=${encodeURIComponent(location.pathname)}`}
        replace
      />
    );
  }

  return <Outlet />;
}
