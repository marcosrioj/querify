import { RouteObject } from 'react-router-dom';
import { SourceDetailPage } from '@/domains/sources/source-detail-page';
import { SourceFormPage } from '@/domains/sources/source-form-page';
import { SourceListPage } from '@/domains/sources/source-list-page';

export const SourceRoutes: RouteObject[] = [
  {
    path: 'sources',
    element: <SourceListPage />,
    handle: {
      title: 'Sources',
      breadcrumb: 'Sources',
      navKey: 'sources',
    },
  },
  {
    path: 'sources/new',
    element: <SourceFormPage mode="create" />,
    handle: {
      title: 'New source',
      breadcrumb: 'Create',
      navKey: 'sources',
    },
  },
  {
    path: 'sources/:id',
    element: <SourceDetailPage />,
    handle: {
      title: 'Source',
      breadcrumb: 'Detail',
      navKey: 'sources',
    },
  },
  {
    path: 'sources/:id/edit',
    element: <SourceFormPage mode="edit" />,
    handle: {
      title: 'Edit source',
      breadcrumb: 'Edit',
      navKey: 'sources',
    },
  },
];
