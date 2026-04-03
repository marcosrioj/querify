import { Navigate, RouteObject } from 'react-router-dom';

export const ContentRefRoutes: RouteObject[] = [
  {
    path: 'content-refs/*',
    element: <Navigate to="/app/faq" replace />,
  },
];
