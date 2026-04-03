import { Navigate, RouteObject } from 'react-router-dom';
import { GeneralSettingsPage } from '@/domains/settings/general-settings-page';
import { ProfileSettingsPage } from '@/domains/settings/profile-settings-page';
import { SecuritySettingsPage } from '@/domains/settings/security-settings-page';
import { TenantSettingsPage } from '@/domains/tenants/tenant-settings-page';

export const SettingsRoutes: RouteObject[] = [
  {
    path: 'settings',
    children: [
      {
        index: true,
        element: <Navigate to="/app/settings/general" replace />,
      },
      {
        path: 'general',
        element: <GeneralSettingsPage />,
        handle: {
          title: 'General settings',
          breadcrumb: 'General',
          navKey: 'settings',
        },
      },
      {
        path: 'profile',
        element: <ProfileSettingsPage />,
        handle: {
          title: 'Profile settings',
          breadcrumb: 'Profile',
          navKey: 'settings',
        },
      },
      {
        path: 'security',
        element: <SecuritySettingsPage />,
        handle: {
          title: 'Security settings',
          breadcrumb: 'Security',
          navKey: 'settings',
        },
      },
      {
        path: 'tenant',
        element: <TenantSettingsPage />,
        handle: {
          title: 'Tenant settings',
          breadcrumb: 'Tenant',
          navKey: 'settings',
        },
      },
    ],
  },
];
