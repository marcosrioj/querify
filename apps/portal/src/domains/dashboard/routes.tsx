import { RouteObject } from 'react-router-dom';
import { DashboardPage } from '@/domains/dashboard/dashboard-page';

export const DashboardRoutes: RouteObject[] = [
  {
    path: 'dashboard',
    element: <DashboardPage />,
    handle: {
      title: 'Home',
      breadcrumb: 'Home',
      navKey: 'dashboard',
    },
  },
];
