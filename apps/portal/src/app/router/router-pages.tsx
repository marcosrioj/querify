import { useEffect } from 'react';
import { isRouteErrorResponse, Navigate, useRouteError } from 'react-router-dom';
import { captureException } from '@/platform/telemetry/logger';
import { ErrorState, NotFoundPage } from '@/shared/ui/placeholder-state';

function getRouteErrorDescription(error: unknown) {
  if (isRouteErrorResponse(error)) {
    return error.status === 404
      ? 'This route is outside the Portal surface or has not been mapped yet.'
      : 'The requested page failed before it could finish loading.';
  }

  return 'An unexpected error interrupted this page.';
}

export function RootRedirectPage() {
  return <Navigate to="/app/dashboard" replace />;
}

export function RouterErrorPage() {
  const error = useRouteError();

  useEffect(() => {
    captureException(error, { source: 'react-router' });
  }, [error]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted px-4">
      <div className="w-full max-w-xl">
        <ErrorState
          title="Unable to load this page"
          description={getRouteErrorDescription(error)}
          retry={() => window.location.reload()}
        />
      </div>
    </div>
  );
}

export function RouterNotFoundPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted px-4">
      <NotFoundPage />
    </div>
  );
}
