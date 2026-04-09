import { RouteObject } from 'react-router-dom';
import { AuthLayout } from '@/domains/auth/auth-layout';
import { LoginPage } from '@/domains/auth/login-page';
import { LogoutPage } from '@/domains/auth/logout-page';
import { ResetPasswordPlaceholderPage } from '@/domains/auth/reset-password-placeholder-page';

export const AuthRoutes: RouteObject[] = [
  {
    element: <AuthLayout />,
    children: [
      {
        path: '/login',
        element: <LoginPage />,
        handle: {
          title: 'Sign in',
        },
      },
      {
        path: '/logout',
        element: <LogoutPage />,
        handle: {
          title: 'Sign out',
        },
      },
      {
        path: '/reset-password',
        element: <ResetPasswordPlaceholderPage />,
        handle: {
          title: 'Reset password',
        },
      },
    ],
  },
];
