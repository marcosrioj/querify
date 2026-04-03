import { RouteObject } from "react-router-dom";
import { ContentRefDetailPage } from "@/domains/content-refs/content-ref-detail-page";
import { ContentRefFormPage } from "@/domains/content-refs/content-ref-form-page";
import { FaqDetailPage } from "@/domains/faq/faq-detail-page";
import { FaqFormPage } from "@/domains/faq/faq-form-page";
import { FaqListPage } from "@/domains/faq/faq-list-page";
import { FaqItemDetailPage } from "@/domains/faq-items/faq-item-detail-page";
import { FaqItemFormPage } from "@/domains/faq-items/faq-item-form-page";

export const FaqRoutes: RouteObject[] = [
  {
    path: "faq",
    element: <FaqListPage />,
    handle: {
      title: "FAQs",
      breadcrumb: "FAQs",
      navKey: "faq",
    },
  },
  {
    path: "faq/new",
    element: <FaqFormPage mode="create" />,
    handle: {
      title: "New FAQ",
      breadcrumb: "Create",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id",
    element: <FaqDetailPage />,
    handle: {
      title: "FAQ",
      breadcrumb: "Detail",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/edit",
    element: <FaqFormPage mode="edit" />,
    handle: {
      title: "Edit FAQ",
      breadcrumb: "Edit",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/items/new",
    element: <FaqItemFormPage mode="create" />,
    handle: {
      title: "New Q&A item",
      breadcrumb: "New Q&A item",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/items/:itemId",
    element: <FaqItemDetailPage />,
    handle: {
      title: "Q&A item",
      breadcrumb: "Q&A item",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/items/:itemId/edit",
    element: <FaqItemFormPage mode="edit" />,
    handle: {
      title: "Edit Q&A item",
      breadcrumb: "Edit Q&A item",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/content-refs/new",
    element: <ContentRefFormPage mode="create" />,
    handle: {
      title: "New source",
      breadcrumb: "New source",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/content-refs/:contentRefId",
    element: <ContentRefDetailPage />,
    handle: {
      title: "Source",
      breadcrumb: "Source",
      navKey: "faq",
    },
  },
  {
    path: "faq/:id/content-refs/:contentRefId/edit",
    element: <ContentRefFormPage mode="edit" />,
    handle: {
      title: "Edit source",
      breadcrumb: "Edit source",
      navKey: "faq",
    },
  },
];
