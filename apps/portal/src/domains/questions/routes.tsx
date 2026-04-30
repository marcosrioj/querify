import { RouteObject } from "react-router-dom";
import { QuestionDetailPage } from "@/domains/questions/question-detail-page";
import { QuestionFormPage } from "@/domains/questions/question-form-page";

export const QuestionRoutes: RouteObject[] = [
  {
    path: "questions/new",
    element: <QuestionFormPage mode="create" />,
    handle: {
      title: "New question",
      breadcrumb: "Create",
      navKey: "spaces",
    },
  },
  {
    path: "questions/:id",
    element: <QuestionDetailPage />,
    handle: {
      title: "Question",
      breadcrumb: "Question",
      navKey: "spaces",
    },
  },
  {
    path: "questions/:id/edit",
    element: <QuestionFormPage mode="edit" />,
    handle: {
      title: "Edit question",
      breadcrumb: "Edit",
      navKey: "spaces",
    },
  },
];
