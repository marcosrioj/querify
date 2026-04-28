import {
  CircleDollarSign,
  BarChart3,
  FolderKanban,
  MessagesSquare,
  Settings,
  Tags,
  Users,
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

const qnaContextKeys = new Set(["questions", "answers", "activity"]);

export const portalNavigation: NavigationItem[] = [
  {
    key: "qna",
    label: "QnA",
    description:
      "Dashboard, spaces, reusable tags, and reusable sources.",
    path: "/app/dashboard",
    icon: MessagesSquare,
    activePaths: [
      "/app/dashboard",
      "/app/spaces",
      "/app/questions",
      "/app/answers",
      "/app/sources",
      "/app/tags",
      "/app/activity",
    ],
    children: [
      {
        key: "dashboard",
        label: "Dashboard",
        description: "QnA overview and operational risks",
        path: "/app/dashboard",
        icon: BarChart3,
      },
      {
        key: "spaces",
        label: "Spaces",
        description: "Daily operational entry point",
        path: "/app/spaces",
        icon: FolderKanban,
      },
      {
        key: "tags",
        label: "Tags",
        description: "Reusable QnA taxonomy",
        path: "/app/tags",
        icon: Tags,
      },
      {
        key: "sources",
        label: "Sources",
        description: "Reusable evidence catalog",
        path: "/app/sources",
        icon: Waypoints,
      },
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

function getQnaNavigationItem() {
  return portalNavigation.find((item) => item.key === "qna");
}

function getQnaChildNavigationItem(key: string) {
  return getQnaNavigationItem()?.children?.find((item) => item.key === key);
}

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

  if (items === portalNavigation && qnaContextKeys.has(key)) {
    const qna = getQnaNavigationItem();
    const spaces = getQnaChildNavigationItem("spaces");

    if (!qna || !spaces) {
      return [];
    }

    if (key === "answers") {
      return [
        qna,
        spaces,
        {
          key: "questions-context",
          label: "Question",
          description: "Parent question context",
          path: "/app/spaces",
          icon: MessagesSquare,
        },
      ];
    }

    return [qna, spaces];
  }

  return [];
}
