import type { RouteObject } from 'react-router-dom';
import { BillingPage } from '@/domains/billing/billing-page';

export const BillingRoutes: RouteObject[] = [
  {
    path: 'billing',
    Component: BillingPage,
    handle: {
      title: 'Billing',
      breadcrumb: 'Billing',
      navKey: 'billing',
    },
  },
];
