import { RouteObject } from 'react-router-dom';
import { MembersPage } from '@/domains/members/members-page';

export const MembersRoutes: RouteObject[] = [
  {
    path: 'members',
    element: <MembersPage />,
    handle: {
      title: 'Members',
      breadcrumb: 'Members',
      navKey: 'members',
    },
  },
];
