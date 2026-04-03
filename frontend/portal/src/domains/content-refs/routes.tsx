import { RouteObject } from 'react-router-dom';
import { ContentRefDetailPage } from '@/domains/content-refs/content-ref-detail-page';
import { ContentRefFormPage } from '@/domains/content-refs/content-ref-form-page';
import { ContentRefListPage } from '@/domains/content-refs/content-ref-list-page';

export const ContentRefRoutes: RouteObject[] = [
  {
    path: 'content-refs',
    element: <ContentRefListPage />,
    handle: {
      title: 'Content refs',
      breadcrumb: 'Content Refs',
      navKey: 'content-refs',
    },
  },
  {
    path: 'content-refs/new',
    element: <ContentRefFormPage mode="create" />,
    handle: {
      title: 'Create content ref',
      breadcrumb: 'Create',
      navKey: 'content-refs',
    },
  },
  {
    path: 'content-refs/:id',
    element: <ContentRefDetailPage />,
    handle: {
      title: 'Content ref detail',
      breadcrumb: 'Detail',
      navKey: 'content-refs',
    },
  },
  {
    path: 'content-refs/:id/edit',
    element: <ContentRefFormPage mode="edit" />,
    handle: {
      title: 'Edit content ref',
      breadcrumb: 'Edit',
      navKey: 'content-refs',
    },
  },
];
