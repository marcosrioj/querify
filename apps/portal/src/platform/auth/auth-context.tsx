import {
  PropsWithChildren,
  createContext,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { createAuth0Client, type Auth0Client } from '@auth0/auth0-spa-js';
import { AuthRuntime, RuntimeEnv } from '@/platform/runtime/env';
import {
  type AuthStatus,
  type PortalRole,
  type PortalSession,
  type PortalUser,
} from '@/platform/auth/types';
import { logger } from '@/platform/telemetry/logger';
import { translateText } from '@/shared/lib/i18n-core';

type AuthContextValue = {
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

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const resolveRedirectUri = () =>
  RuntimeEnv.auth0RedirectUri ||
  `${window.location.origin}${RuntimeEnv.baseUrl}login`;

const resolveLogoutUri = () =>
  RuntimeEnv.auth0LogoutUri ||
  `${window.location.origin}${RuntimeEnv.baseUrl}login`;

const base64UrlDecode = (value: string) => {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
  const padded = normalized.padEnd(
    normalized.length + ((4 - (normalized.length % 4 || 4)) % 4),
    '=',
  );
  return window.atob(padded);
};

const parseJwtPayload = (token: string) => {
  const [, payload] = token.split('.');
  if (!payload) {
    return {};
  }

  try {
    return JSON.parse(base64UrlDecode(payload)) as Record<string, unknown>;
  } catch {
    return {};
  }
};

const normalizeRole = (payload: Record<string, unknown>): PortalRole => {
  const rawRole = payload.role ?? payload.roles;
  if (Array.isArray(rawRole)) {
    return rawRole.some((entry) =>
      String(entry).toLowerCase().includes('admin'),
    )
      ? 'Admin'
      : 'Member';
  }

  return String(rawRole).toLowerCase().includes('admin') ? 'Admin' : 'Member';
};

const buildUser = (
  payload: Record<string, unknown>,
  auth0User?: Record<string, unknown>,
): PortalUser => {
  const email =
    (auth0User?.email as string | undefined) ||
    (payload['https://basefaq.com/email'] as string | undefined) ||
    (payload.email as string | undefined);
  const name =
    (auth0User?.name as string | undefined) ||
    (payload['https://basefaq.com/name'] as string | undefined) ||
    (payload.name as string | undefined);

  return {
    id: String(payload.sub ?? auth0User?.sub ?? 'unknown'),
    email,
    name,
    role: normalizeRole(payload),
  };
};

export function PortalAuthProvider({ children }: PropsWithChildren) {
  const [status, setStatus] = useState<AuthStatus>('booting');
  const [session, setSession] = useState<PortalSession>();
  const [user, setUser] = useState<PortalUser>();
  const [error, setError] = useState<string>();
  const clientRef = useRef<Auth0Client | null>(null);

  const syncSession = async (client: Auth0Client) => {
    const isAuthenticated = await client.isAuthenticated();

    if (!isAuthenticated) {
      setSession(undefined);
      setUser(undefined);
      setStatus('unauthenticated');
      return;
    }

    const accessToken = await client.getTokenSilently({
      authorizationParams: {
        audience: RuntimeEnv.auth0Audience,
      },
    });
    const payload = parseJwtPayload(accessToken);
    const auth0User = (await client.getUser()) as Record<string, unknown> | undefined;

    setSession({
      accessToken,
      expiresAt:
        typeof payload.exp === 'number' ? Number(payload.exp) * 1000 : undefined,
    });
    setUser(buildUser(payload, auth0User));
    setStatus('ready');
    setError(undefined);
  };

  useEffect(() => {
    if (!AuthRuntime.isConfigured) {
      setStatus('unauthenticated');
      setError(
        translateText(
          'Auth0 is not fully configured. Set VITE_AUTH0_CLIENT_ID to enable login.',
        ),
      );
      return;
    }

    let mounted = true;

    const boot = async () => {
      try {
        const client = await createAuth0Client({
          domain: RuntimeEnv.auth0Domain,
          clientId: RuntimeEnv.auth0ClientId,
          authorizationParams: {
            audience: RuntimeEnv.auth0Audience,
            redirect_uri: resolveRedirectUri(),
          },
          cacheLocation: 'localstorage',
          useRefreshTokens: true,
        });

        if (!mounted) {
          return;
        }

        clientRef.current = client;

        const searchParams = new URLSearchParams(window.location.search);
        if (searchParams.has('code') && searchParams.has('state')) {
          const redirectResult = await client.handleRedirectCallback();
          const nextPath = searchParams.get('next');
          window.history.replaceState(
            {},
            document.title,
            (redirectResult.appState?.nextPath as string | undefined) ||
              nextPath ||
              '/app/dashboard',
          );
        }

        await syncSession(client);
      } catch (bootError) {
        logger.error('Auth bootstrap failed', bootError);
        if (!mounted) {
          return;
        }

        setStatus('error');
        setError(
          bootError instanceof Error
            ? bootError.message
            : translateText('Unable to initialize Auth0.'),
        );
      }
    };

    void boot();

    return () => {
      mounted = false;
    };
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      isConfigured: AuthRuntime.isConfigured,
      status,
      session,
      user,
      error,
      async login(nextPath) {
        const client = clientRef.current;
        if (!client) {
          throw new Error(translateText('Auth0 client is not ready.'));
        }

        await client.loginWithRedirect({
          authorizationParams: {
            audience: RuntimeEnv.auth0Audience,
            redirect_uri: resolveRedirectUri(),
          },
          appState: {
            nextPath,
          },
        });
      },
      async logout() {
        const client = clientRef.current;
        if (!client) {
          setSession(undefined);
          setUser(undefined);
          setStatus('unauthenticated');
          return;
        }

        setSession(undefined);
        setUser(undefined);
        setStatus('unauthenticated');

        await client.logout({
          logoutParams: {
            returnTo: resolveLogoutUri(),
          },
        });
      },
      async refreshSession() {
        const client = clientRef.current;
        if (!client) {
          return;
        }

        await syncSession(client);
      },
      async getAccessToken() {
        const client = clientRef.current;
        if (!client) {
          return undefined;
        }

        try {
          const accessToken = await client.getTokenSilently({
            authorizationParams: {
              audience: RuntimeEnv.auth0Audience,
            },
          });

          const payload = parseJwtPayload(accessToken);
          setSession({
            accessToken,
            expiresAt:
              typeof payload.exp === 'number'
                ? Number(payload.exp) * 1000
                : undefined,
          });
          return accessToken;
        } catch (tokenError) {
          logger.warn('Access token refresh failed', tokenError);
          return session?.accessToken;
        }
      },
    }),
    [error, session, status, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within PortalAuthProvider.');
  }

  return context;
}
