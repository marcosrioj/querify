export type PortalRole = 'Member' | 'Admin';

export type PortalUser = {
  id: string;
  email?: string;
  name?: string;
  role: PortalRole;
};

export type PortalSession = {
  accessToken: string;
  expiresAt?: number;
};

export type AuthStatus =
  | 'booting'
  | 'ready'
  | 'unauthenticated'
  | 'error';
