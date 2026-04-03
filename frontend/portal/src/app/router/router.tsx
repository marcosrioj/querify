import { createBrowserRouter, Navigate, Outlet } from 'react-router-dom';
import { AuthRoutes } from '@/domains/auth/routes';
import { AiRoutes } from '@/domains/ai/routes';
import { BillingRoutes } from '@/domains/billing/routes';
import { ContentRefRoutes } from '@/domains/content-refs/routes';
import { DashboardRoutes } from '@/domains/dashboard/routes';
import { FaqItemRoutes } from '@/domains/faq-items/routes';
import { FaqRoutes } from '@/domains/faq/routes';
import { MembersRoutes } from '@/domains/members/routes';
import { SettingsRoutes } from '@/domains/settings/routes';
import { RequirePortalAuth } from '@/domains/auth/require-portal-auth';
import { PortalLayout } from '@/app/layouts/portal-layout';
import { RuntimeEnv } from '@/platform/runtime/env';
import { NotFoundPage } from '@/shared/ui/placeholder-state';

function RootRedirect() {
  return <Navigate to="/app/dashboard" replace />;
}

const protectedChildren = [
  ...DashboardRoutes,
  ...FaqRoutes,
  ...FaqItemRoutes,
  ...ContentRefRoutes,
  ...MembersRoutes,
  ...BillingRoutes,
  ...SettingsRoutes,
  ...AiRoutes,
];

export const AppRouter = createBrowserRouter(
  [
    {
      path: '/',
      element: <RootRedirect />,
    },
    ...AuthRoutes,
    {
      path: '/app',
      element: <RequirePortalAuth />,
      children: [
        {
          element: <PortalLayout />,
          children: protectedChildren,
        },
      ],
    },
    {
      path: '*',
      element: (
        <div className="flex min-h-screen items-center justify-center bg-muted px-4">
          <NotFoundPage />
        </div>
      ),
    },
  ],
  {
    basename: RuntimeEnv.baseUrl,
  },
);

export function RoutedOutlet() {
  return <Outlet />;
}
