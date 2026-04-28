import { RouteObject } from 'react-router-dom';
import { AnswerDetailPage } from '@/domains/answers/answer-detail-page';
import { AnswerFormPage } from '@/domains/answers/answer-form-page';
import { AnswerListPage } from '@/domains/answers/answer-list-page';

export const AnswerRoutes: RouteObject[] = [
  {
    path: 'answers',
    element: <AnswerListPage />,
    handle: {
      title: 'Answers',
      breadcrumb: 'Answers',
      navKey: 'answers',
    },
  },
  {
    path: 'answers/new',
    element: <AnswerFormPage mode="create" />,
    handle: {
      title: 'New answer',
      breadcrumb: 'Create',
      navKey: 'answers',
    },
  },
  {
    path: 'answers/:id',
    element: <AnswerDetailPage />,
    handle: {
      title: 'Answer',
      breadcrumb: 'Answer',
      navKey: 'answers',
    },
  },
  {
    path: 'answers/:id/edit',
    element: <AnswerFormPage mode="edit" />,
    handle: {
      title: 'Edit answer',
      breadcrumb: 'Edit',
      navKey: 'answers',
    },
  },
];
