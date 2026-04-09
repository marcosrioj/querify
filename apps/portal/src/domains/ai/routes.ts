import type { RouteObject } from 'react-router-dom';
import { AiWorkspacePage } from '@/domains/ai/ai-workspace-page';

export const AiRoutes: RouteObject[] = [
  {
    path: 'ai',
    Component: AiWorkspacePage,
    handle: {
      title: 'AI',
      breadcrumb: 'AI',
      navKey: 'ai',
    },
  },
];
