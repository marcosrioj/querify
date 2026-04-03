import { RouteObject } from 'react-router-dom';
import { FaqItemDetailPage } from '@/domains/faq-items/faq-item-detail-page';
import { FaqItemFormPage } from '@/domains/faq-items/faq-item-form-page';
import { FaqItemListPage } from '@/domains/faq-items/faq-item-list-page';

export const FaqItemRoutes: RouteObject[] = [
  {
    path: 'faq-items',
    element: <FaqItemListPage />,
    handle: {
      title: 'FAQ Items',
      breadcrumb: 'FAQ Items',
      navKey: 'faq-items',
    },
  },
  {
    path: 'faq-items/new',
    element: <FaqItemFormPage mode="create" />,
    handle: {
      title: 'Create FAQ item',
      breadcrumb: 'Create',
      navKey: 'faq-items',
    },
  },
  {
    path: 'faq-items/:id',
    element: <FaqItemDetailPage />,
    handle: {
      title: 'FAQ item detail',
      breadcrumb: 'Detail',
      navKey: 'faq-items',
    },
  },
  {
    path: 'faq-items/:id/edit',
    element: <FaqItemFormPage mode="edit" />,
    handle: {
      title: 'Edit FAQ item',
      breadcrumb: 'Edit',
      navKey: 'faq-items',
    },
  },
];
