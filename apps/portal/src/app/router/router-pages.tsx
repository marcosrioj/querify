import { Navigate } from 'react-router-dom';
import { NotFoundPage } from '@/shared/ui/placeholder-state';

export function RootRedirectPage() {
  return <Navigate to="/app/dashboard" replace />;
}

export function RouterNotFoundPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted px-4">
      <NotFoundPage />
    </div>
  );
}
