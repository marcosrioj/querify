import { RouteObject } from "react-router-dom";
import { ActivityDetailPage } from "@/domains/activity/activity-detail-page";

export const ActivityRoutes: RouteObject[] = [
  {
    path: "activity/:id",
    element: <ActivityDetailPage />,
    handle: {
      title: "Activity event",
      breadcrumb: "Activity",
      navKey: "spaces",
    },
  },
];
