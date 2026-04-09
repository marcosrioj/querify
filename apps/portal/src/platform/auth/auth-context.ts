import { createContext } from 'react';
import type {
  AuthStatus,
  PortalSession,
  PortalUser,
} from '@/platform/auth/types';

export type AuthContextValue = {
  isConfigured: boolean;
  status: AuthStatus;
  session?: PortalSession;
  user?: PortalUser;
  error?: string;
  login: (nextPath?: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshSession: () => Promise<void>;
  getAccessToken: () => Promise<string | undefined>;
};

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
