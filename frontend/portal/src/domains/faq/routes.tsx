import { RouteObject } from 'react-router-dom';
import { FaqDetailPage } from '@/domains/faq/faq-detail-page';
import { FaqFormPage } from '@/domains/faq/faq-form-page';
import { FaqListPage } from '@/domains/faq/faq-list-page';

export const FaqRoutes: RouteObject[] = [
  {
    path: 'faq',
    element: <FaqListPage />,
    handle: {
      title: 'FAQs',
      breadcrumb: 'FAQs',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/new',
    element: <FaqFormPage mode="create" />,
    handle: {
      title: 'Create FAQ',
      breadcrumb: 'Create',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id',
    element: <FaqDetailPage />,
    handle: {
      title: 'FAQ detail',
      breadcrumb: 'Detail',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id/edit',
    element: <FaqFormPage mode="edit" />,
    handle: {
      title: 'Edit FAQ',
      breadcrumb: 'Edit',
      navKey: 'faq',
    },
  },
];
