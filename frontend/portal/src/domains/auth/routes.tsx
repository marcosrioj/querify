import { RouteObject } from 'react-router-dom';
import { AuthLayout } from '@/domains/auth/auth-layout';
import { ForgotPasswordPage } from '@/domains/auth/forgot-password-page';
import { LoginPage } from '@/domains/auth/login-page';
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
        path: '/forgot-password',
        element: <ForgotPasswordPage />,
        handle: {
          title: 'Forgot password',
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
