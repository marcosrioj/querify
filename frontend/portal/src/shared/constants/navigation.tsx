import {
  Bot,
  CircleDollarSign,
  FileQuestion,
  Gauge,
  Settings,
  Users,
} from 'lucide-react';

export type NavigationItem = {
  key: string;
  label: string;
  description: string;
  path: string;
  icon: typeof Gauge;
};

export const portalNavigation: NavigationItem[] = [
  {
    key: 'dashboard',
    label: 'Dashboard',
    description: 'Workspace overview and usage signals',
    path: '/app/dashboard',
    icon: Gauge,
  },
  {
    key: 'faq',
    label: 'FAQs',
    description: 'Knowledge base, answers, and source material',
    path: '/app/faq',
    icon: FileQuestion,
  },
  {
    key: 'members',
    label: 'Members',
    description: 'Tenant access and roles',
    path: '/app/members',
    icon: Users,
  },
  {
    key: 'billing',
    label: 'Billing',
    description: 'Plan and usage visibility',
    path: '/app/billing',
    icon: CircleDollarSign,
  },
  {
    key: 'settings',
    label: 'Settings',
    description: 'Profile, security, and tenant controls',
    path: '/app/settings/general',
    icon: Settings,
  },
  {
    key: 'ai',
    label: 'AI',
    description: 'Generation controls and provider status',
    path: '/app/ai',
    icon: Bot,
  },
];
