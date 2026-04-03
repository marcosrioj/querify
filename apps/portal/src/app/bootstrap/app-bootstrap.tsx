import { HelmetProvider } from 'react-helmet-async';
import { RouterProvider } from 'react-router-dom';
import { AppRouter } from '@/app/router/router';
import { AuthProvider } from '@/providers/auth-provider';
import { QueryProvider } from '@/providers/query-provider';
import { TenantProvider } from '@/providers/tenant-provider';
import { ThemeProvider } from '@/providers/theme-provider';
import { ToastProvider } from '@/providers/toast-provider';
import { AppErrorBoundary } from '@/platform/telemetry/error-boundary';

export function AppBootstrap() {
  return (
    <AppErrorBoundary>
      <HelmetProvider>
        <ThemeProvider>
          <QueryProvider>
            <AuthProvider>
              <TenantProvider>
                <ToastProvider />
                <RouterProvider router={AppRouter} />
              </TenantProvider>
            </AuthProvider>
          </QueryProvider>
        </ThemeProvider>
      </HelmetProvider>
    </AppErrorBoundary>
  );
}
