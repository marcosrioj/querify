import { HelmetProvider } from 'react-helmet-async';
import { RouterProvider } from 'react-router-dom';
import { AppRouter } from '@/app/router/router';
import { AuthProvider } from '@/providers/auth-provider';
import { I18nProvider } from '@/providers/i18n-provider';
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
              <I18nProvider>
                <TenantProvider>
                  <ToastProvider />
                  <RouterProvider router={AppRouter} />
                </TenantProvider>
              </I18nProvider>
            </AuthProvider>
          </QueryProvider>
        </ThemeProvider>
      </HelmetProvider>
    </AppErrorBoundary>
  );
}
