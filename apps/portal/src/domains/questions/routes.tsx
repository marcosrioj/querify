import { RouteObject } from 'react-router-dom';
import { QuestionDetailPage } from '@/domains/questions/question-detail-page';
import { QuestionFormPage } from '@/domains/questions/question-form-page';
import { QuestionListPage } from '@/domains/questions/question-list-page';

export const QuestionRoutes: RouteObject[] = [
  {
    path: 'questions',
    element: <QuestionListPage />,
    handle: {
      title: 'Questions',
      breadcrumb: 'Questions',
      navKey: 'questions',
    },
  },
  {
    path: 'questions/new',
    element: <QuestionFormPage mode="create" />,
    handle: {
      title: 'New question',
      breadcrumb: 'Create',
      navKey: 'questions',
    },
  },
  {
    path: 'questions/:id',
    element: <QuestionDetailPage />,
    handle: {
      title: 'Question',
      breadcrumb: 'Question',
      navKey: 'questions',
    },
  },
  {
    path: 'questions/:id/edit',
    element: <QuestionFormPage mode="edit" />,
    handle: {
      title: 'Edit question',
      breadcrumb: 'Edit',
      navKey: 'questions',
    },
  },
];
