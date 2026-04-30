import {
  Activity,
  CircleDollarSign,
  FolderKanban,
  Home,
  MessagesSquare,
  SearchCheck,
  Settings,
  Tags,
  Users,
  UserRound,
  Waypoints,
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

export type NavigationGroup = {
  key: string;
  label: string;
  items: NavigationItem[];
};

export const portalNavigationGroups: NavigationGroup[] = [
  {
    key: "workspace",
    label: "Workspace",
    items: [
      {
        key: "dashboard",
        label: "Home",
        description: "Attention queue and value proof",
        path: "/app/dashboard",
        icon: Home,
      },
      {
        key: "spaces",
        label: "Spaces",
        description: "Operating boundaries",
        path: "/app/spaces",
        icon: FolderKanban,
      },
      {
        key: "questions",
        label: "Questions",
        description: "Moderation and resolution queue",
        path: "/app/questions",
        icon: MessagesSquare,
      },
      {
        key: "answers",
        label: "Answers",
        description: "Answer candidates in this workspace",
        path: "/app/answers",
        icon: SearchCheck,
      },
      {
        key: "sources",
        label: "Sources",
        description: "Evidence and trust catalog",
        path: "/app/sources",
        icon: Waypoints,
      },
      {
        key: "tags",
        label: "Tags",
        description: "Reusable taxonomy",
        path: "/app/tags",
        icon: Tags,
      },
      {
        key: "activity",
        label: "Activity",
        description: "Audit trail and signals",
        path: "/app/activity",
        icon: Activity,
      },
    ],
  },
  {
    key: "administration",
    label: "Administration",
    items: [
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
        description: "Plan, invoices, and payments",
        path: "/app/billing",
        icon: CircleDollarSign,
      },
      {
        key: "settings",
        label: "Settings",
        description: "Workspace and client key",
        path: "/app/settings/tenant",
        icon: Settings,
        activePaths: ["/app/settings/tenant", "/app/settings/general", "/app/settings/security"],
      },
    ],
  },
  {
    key: "account",
    label: "Account",
    items: [
      {
        key: "profile",
        label: "Profile",
        description: "Language, time zone, and contact info",
        path: "/app/settings/profile",
        icon: UserRound,
        activePaths: ["/app/settings/profile"],
      },
    ],
  },
];

export const portalNavigation = portalNavigationGroups.flatMap(
  (group) => group.items,
);

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
