import { RouteObject } from 'react-router-dom';
import { TagFormPage } from '@/domains/tags/tag-form-page';
import { TagListPage } from '@/domains/tags/tag-list-page';

export const TagRoutes: RouteObject[] = [
  {
    path: 'tags',
    element: <TagListPage />,
    handle: {
      title: 'Tags',
      breadcrumb: 'Tags',
      navKey: 'tags',
    },
  },
  {
    path: 'tags/new',
    element: <TagFormPage mode="create" />,
    handle: {
      title: 'New tag',
      breadcrumb: 'Create',
      navKey: 'tags',
    },
  },
  {
    path: 'tags/:id/edit',
    element: <TagFormPage mode="edit" />,
    handle: {
      title: 'Edit tag',
      breadcrumb: 'Edit',
      navKey: 'tags',
    },
  },
];
