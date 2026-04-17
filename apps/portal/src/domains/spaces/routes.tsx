import { RouteObject } from 'react-router-dom';
import { SpaceDetailPage } from '@/domains/spaces/space-detail-page';
import { SpaceFormPage } from '@/domains/spaces/space-form-page';
import { SpaceListPage } from '@/domains/spaces/space-list-page';

export const SpaceRoutes: RouteObject[] = [
  {
    path: 'spaces',
    element: <SpaceListPage />,
    handle: {
      title: 'Spaces',
      breadcrumb: 'Spaces',
      navKey: 'spaces',
    },
  },
  {
    path: 'spaces/new',
    element: <SpaceFormPage mode="create" />,
    handle: {
      title: 'New space',
      breadcrumb: 'Create',
      navKey: 'spaces',
    },
  },
  {
    path: 'spaces/:id',
    element: <SpaceDetailPage />,
    handle: {
      title: 'Space',
      breadcrumb: 'Detail',
      navKey: 'spaces',
    },
  },
  {
    path: 'spaces/:id/edit',
    element: <SpaceFormPage mode="edit" />,
    handle: {
      title: 'Edit space',
      breadcrumb: 'Edit',
      navKey: 'spaces',
    },
  },
];
