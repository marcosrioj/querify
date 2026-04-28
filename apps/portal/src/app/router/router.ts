import { createElement } from 'react';
import { createBrowserRouter } from 'react-router-dom';
import type { RouteObject } from 'react-router-dom';
import { PortalLayout } from '@/app/layouts/portal-layout';
import {
  RootRedirectPage,
  RouterErrorPage,
  RouterNotFoundPage,
} from '@/app/router/router-pages';
import { ActivityRoutes } from '@/domains/activity/routes';
import { AnswerRoutes } from '@/domains/answers/routes';
import { RequirePortalAuth } from '@/domains/auth/require-portal-auth';
import { AuthRoutes } from '@/domains/auth/routes';
import { BillingRoutes } from '@/domains/billing/routes';
import { DashboardRoutes } from '@/domains/dashboard/routes';
import { MembersRoutes } from '@/domains/members/routes';
import { QuestionRoutes } from '@/domains/questions/routes';
import { SettingsRoutes } from '@/domains/settings/routes';
import { SourceRoutes } from '@/domains/sources/routes';
import { SpaceRoutes } from '@/domains/spaces/routes';
import { TagRoutes } from '@/domains/tags/routes';
import { RuntimeEnv } from '@/platform/runtime/env';

const protectedChildren: RouteObject[] = [
  ...DashboardRoutes,
  ...SpaceRoutes,
  ...QuestionRoutes,
  ...AnswerRoutes,
  ...SourceRoutes,
  ...TagRoutes,
  ...ActivityRoutes,
  ...MembersRoutes,
  ...BillingRoutes,
  ...SettingsRoutes,
];

const routerErrorElement = createElement(RouterErrorPage);

export const AppRouter = createBrowserRouter(
  [
    {
      path: '/',
      Component: RootRedirectPage,
      errorElement: routerErrorElement,
    },
    ...AuthRoutes,
    {
      path: '/app',
      Component: RequirePortalAuth,
      errorElement: routerErrorElement,
      children: [
        {
          Component: PortalLayout,
          errorElement: routerErrorElement,
          children: protectedChildren,
        },
      ],
    },
    {
      path: '*',
      Component: RouterNotFoundPage,
      errorElement: routerErrorElement,
    },
  ],
  {
    basename: RuntimeEnv.baseUrl,
  },
);
