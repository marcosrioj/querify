import {
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
    description: 'FAQs, Q&A items, and sources',
    path: '/app/faq',
    icon: FileQuestion,
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
