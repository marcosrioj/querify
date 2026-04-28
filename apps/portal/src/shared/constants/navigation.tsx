import {
  CircleDollarSign,
  Gauge,
  MessagesSquare,
  Settings,
  Users,
  type LucideIcon,
} from "lucide-react";

export type NavigationItem = {
  key: string;
  label: string;
  description: string;
  path: string;
  icon: LucideIcon;
  activePaths?: string[];
  children?: NavigationItem[];
};

const qnaModuleKeys = new Set([
  "spaces",
  "questions",
  "answers",
  "sources",
  "tags",
  "activity",
]);

export const portalNavigation: NavigationItem[] = [
  {
    key: "dashboard",
    label: "Dashboard",
    description: "Workspace overview and usage signals",
    path: "/app/dashboard",
    icon: Gauge,
  },
  {
    key: "qna",
    label: "QnA",
    description:
      "Start with Spaces, then operate questions, answers, sources, tags, and activity.",
    path: "/app/spaces",
    icon: MessagesSquare,
    activePaths: [
      "/app/spaces",
      "/app/questions",
      "/app/answers",
      "/app/sources",
      "/app/tags",
      "/app/activity",
    ],
  },
  {
    key: "members",
    label: "Members",
    description: "People and workspace roles",
    path: "/app/members",
    icon: Users,
  },
  {
    key: "billing",
    label: "Billing",
    description: "Plan, contact, and invoices",
    path: "/app/billing",
    icon: CircleDollarSign,
  },
  {
    key: "settings",
    label: "Settings",
    description: "Appearance, profile, and workspace",
    path: "/app/settings/general",
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

  if (items === portalNavigation && qnaModuleKeys.has(key)) {
    return [portalNavigation[0]];
  }

  return [];
}
