import { RouteObject } from "react-router-dom";
import { AnswerDetailPage } from "@/domains/answers/answer-detail-page";
import { AnswerFormPage } from "@/domains/answers/answer-form-page";

export const AnswerRoutes: RouteObject[] = [
  {
    path: "answers/new",
    element: <AnswerFormPage mode="create" />,
    handle: {
      title: "New answer",
      breadcrumb: "Create",
      navKey: "spaces",
    },
  },
  {
    path: "answers/:id",
    element: <AnswerDetailPage />,
    handle: {
      title: "Answer",
      breadcrumb: "Answer",
      navKey: "spaces",
    },
  },
  {
    path: "answers/:id/edit",
    element: <AnswerFormPage mode="edit" />,
    handle: {
      title: "Edit answer",
      breadcrumb: "Edit",
      navKey: "spaces",
    },
  },
];
