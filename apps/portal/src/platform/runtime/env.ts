const readEnv = (value: string | undefined, fallback = '') =>
  value && value.trim().length > 0 ? value : fallback;

export const RuntimeEnv = {
  appName: 'BaseFAQ Portal',
  baseUrl: import.meta.env.BASE_URL ?? '/',
  faqPortalApiUrl: readEnv(
    import.meta.env.VITE_PORTAL_FAQ_API_URL,
    'http://localhost:5010',
  ),
  tenantPortalApiUrl: readEnv(
    import.meta.env.VITE_PORTAL_TENANT_API_URL,
    'http://localhost:5002',
  ),
  auth0Domain: readEnv(
    import.meta.env.VITE_AUTH0_DOMAIN,
    'basefaq.us.auth0.com',
  ),
  auth0Audience: readEnv(
    import.meta.env.VITE_AUTH0_AUDIENCE,
    'https://basefaq.com',
  ),
  auth0ClientId: readEnv(import.meta.env.VITE_AUTH0_CLIENT_ID),
  auth0RedirectUri: readEnv(import.meta.env.VITE_AUTH0_REDIRECT_URI),
  auth0LogoutUri: readEnv(import.meta.env.VITE_AUTH0_LOGOUT_URI),
} as const;

export const AuthRuntime = {
  isConfigured:
    Boolean(RuntimeEnv.auth0Domain) && Boolean(RuntimeEnv.auth0ClientId),
};
