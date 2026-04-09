import { createBrowserRouter } from 'react-router-dom';
import type { RouteObject } from 'react-router-dom';
import { PortalLayout } from '@/app/layouts/portal-layout';
import {
  RootRedirectPage,
  RouterNotFoundPage,
} from '@/app/router/router-pages';
import { RequirePortalAuth } from '@/domains/auth/require-portal-auth';
import { AuthRoutes } from '@/domains/auth/routes';
import { AiRoutes } from '@/domains/ai/routes';
import { BillingRoutes } from '@/domains/billing/routes';
import { ContentRefRoutes } from '@/domains/content-refs/routes';
import { DashboardRoutes } from '@/domains/dashboard/routes';
import { FaqItemRoutes } from '@/domains/faq-items/routes';
import { FaqRoutes } from '@/domains/faq/routes';
import { MembersRoutes } from '@/domains/members/routes';
import { SettingsRoutes } from '@/domains/settings/routes';
import { RuntimeEnv } from '@/platform/runtime/env';

const protectedChildren: RouteObject[] = [
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
      Component: RootRedirectPage,
    },
    ...AuthRoutes,
    {
      path: '/app',
      Component: RequirePortalAuth,
      children: [
        {
          Component: PortalLayout,
          children: protectedChildren,
        },
      ],
    },
    {
      path: '*',
      Component: RouterNotFoundPage,
    },
  ],
  {
    basename: RuntimeEnv.baseUrl,
  },
);
