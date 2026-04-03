import { Navigate, RouteObject } from 'react-router-dom';

export const FaqItemRoutes: RouteObject[] = [
  {
    path: 'faq-items/*',
    element: <Navigate to="/app/faq" replace />,
  },
];
