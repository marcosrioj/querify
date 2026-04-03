import { RouteObject } from 'react-router-dom';
import { ContentRefDetailPage } from '@/domains/content-refs/content-ref-detail-page';
import { ContentRefFormPage } from '@/domains/content-refs/content-ref-form-page';
import { FaqDetailPage } from '@/domains/faq/faq-detail-page';
import { FaqFormPage } from '@/domains/faq/faq-form-page';
import { FaqListPage } from '@/domains/faq/faq-list-page';
import { FaqItemDetailPage } from '@/domains/faq-items/faq-item-detail-page';
import { FaqItemFormPage } from '@/domains/faq-items/faq-item-form-page';

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
  {
    path: 'faq/:id/items/new',
    element: <FaqItemFormPage mode="create" />,
    handle: {
      title: 'Create FAQ item',
      breadcrumb: 'New answer',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id/items/:itemId',
    element: <FaqItemDetailPage />,
    handle: {
      title: 'FAQ item detail',
      breadcrumb: 'Answer',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id/items/:itemId/edit',
    element: <FaqItemFormPage mode="edit" />,
    handle: {
      title: 'Edit FAQ item',
      breadcrumb: 'Edit answer',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id/content-refs/new',
    element: <ContentRefFormPage mode="create" />,
    handle: {
      title: 'Create content ref',
      breadcrumb: 'New source',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id/content-refs/:contentRefId',
    element: <ContentRefDetailPage />,
    handle: {
      title: 'Content ref detail',
      breadcrumb: 'Source',
      navKey: 'faq',
    },
  },
  {
    path: 'faq/:id/content-refs/:contentRefId/edit',
    element: <ContentRefFormPage mode="edit" />,
    handle: {
      title: 'Edit content ref',
      breadcrumb: 'Edit source',
      navKey: 'faq',
    },
  },
];
