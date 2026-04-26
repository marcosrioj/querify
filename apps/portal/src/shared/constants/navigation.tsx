import {
  Activity,
  CircleDollarSign,
  Gauge,
  MessageSquareText,
  MessagesSquare,
  PanelsTopLeft,
  Settings,
  Tags,
  Users,
  Waypoints,
} from 'lucide-react';

export type NavigationItem = {
  key: string;
  label: string;
  description: string;
  path: string;
  icon: typeof Gauge;
  children?: NavigationItem[];
};

const qnaNavigationItems: NavigationItem[] = [
  {
    key: 'spaces',
    label: 'Spaces',
    description: 'QnA surfaces, modes, and governed knowledge',
    path: '/app/spaces',
    icon: PanelsTopLeft,
  },
  {
    key: 'questions',
    label: 'Questions',
    description: 'Threads, workflow, duplicates, and accepted answers',
    path: '/app/questions',
    icon: MessagesSquare,
  },
  {
    key: 'answers',
    label: 'Answers',
    description: 'Publication, validation, ranking, and retirement',
    path: '/app/answers',
    icon: MessageSquareText,
  },
  {
    key: 'sources',
    label: 'Sources',
    description: 'Reusable evidence, citations, and curated references',
    path: '/app/sources',
    icon: Waypoints,
  },
  {
    key: 'tags',
    label: 'Tags',
    description: 'Reusable taxonomy for spaces and questions',
    path: '/app/tags',
    icon: Tags,
  },
  {
    key: 'activity',
    label: 'Activity',
    description: 'Operational audit trail and public signals',
    path: '/app/activity',
    icon: Activity,
  },
];

export const portalNavigation: NavigationItem[] = [
  {
    key: 'dashboard',
    label: 'Dashboard',
    description: 'Workspace overview and usage signals',
    path: '/app/dashboard',
    icon: Gauge,
  },
  {
    key: 'qna',
    label: 'QnA',
    description: 'Operate QnA end to end',
    path: '/app/spaces',
    icon: MessagesSquare,
    children: qnaNavigationItems,
  },
  {
    key: 'members',
    label: 'Members',
    description: 'People and workspace roles',
    path: '/app/members',
    icon: Users,
  },
  {
    key: 'billing',
    label: 'Billing',
    description: 'Plan, contact, and invoices',
    path: '/app/billing',
    icon: CircleDollarSign,
  },
  {
    key: 'settings',
    label: 'Settings',
    description: 'Appearance, profile, and workspace',
    path: '/app/settings/general',
    icon: Settings,
  },
];

export function findPortalNavigationPath(
  key: string,
  items: NavigationItem[] = portalNavigation,
): NavigationItem[] {
  for (const item of items) {
    if (item.key === key) {
      return [item];
    }

    if (item.children) {
      const childPath = findPortalNavigationPath(key, item.children);

      if (childPath.length > 0) {
        return [item, ...childPath];
      }
    }
  }

  return [];
}
