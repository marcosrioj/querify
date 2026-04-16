import { RouteObject } from 'react-router-dom';
import { ActivityDetailPage } from '@/domains/activity/activity-detail-page';
import { ActivityListPage } from '@/domains/activity/activity-list-page';

export const ActivityRoutes: RouteObject[] = [
  {
    path: 'activity',
    element: <ActivityListPage />,
    handle: {
      title: 'Activity',
      breadcrumb: 'Activity',
      navKey: 'activity',
    },
  },
  {
    path: 'activity/:id',
    element: <ActivityDetailPage />,
    handle: {
      title: 'Activity event',
      breadcrumb: 'Detail',
      navKey: 'activity',
    },
  },
];
