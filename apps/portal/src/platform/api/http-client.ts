import { ApiError, toApiError } from '@/platform/api/api-error';
import { RuntimeEnv } from '@/platform/runtime/env';

export type PortalService = 'tenant' | 'faq';

export type RequestOptions = {
  service: PortalService;
  path: string;
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  accessToken?: string;
  tenantId?: string;
  query?: Record<string, unknown>;
  body?: unknown;
  signal?: AbortSignal;
};

const serviceBaseUrl: Record<PortalService, string> = {
  tenant: RuntimeEnv.tenantPortalApiUrl,
  faq: RuntimeEnv.faqPortalApiUrl,
};

function buildQueryString(query?: Record<string, unknown>) {
  if (!query) {
    return '';
  }

  const params = new URLSearchParams();

  for (const [key, rawValue] of Object.entries(query)) {
    if (
      rawValue === undefined ||
      rawValue === null ||
      rawValue === '' ||
      (Array.isArray(rawValue) && rawValue.length === 0)
    ) {
      continue;
    }

    if (Array.isArray(rawValue)) {
      rawValue.forEach((value) => params.append(key, String(value)));
      continue;
    }

    params.set(key, String(rawValue));
  }

  const queryString = params.toString();
  return queryString ? `?${queryString}` : '';
}

export async function portalRequest<T>({
  service,
  path,
  method = 'GET',
  accessToken,
  tenantId,
  query,
  body,
  signal,
}: RequestOptions): Promise<T> {
  const url = `${serviceBaseUrl[service]}${path}${buildQueryString(query)}`;
  const headers = new Headers({
    Accept: 'application/json',
  });

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  if (tenantId && service === 'faq') {
    headers.set('X-Tenant-Id', tenantId);
  }

  if (body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(url, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal,
  });

  if (!response.ok) {
    throw await toApiError(response, 'BaseFAQ request failed.');
  }

  if (response.status === 204) {
    return undefined as T;
  }

  if (response.status === 202) {
    return (await response.json()) as T;
  }

  const contentType = response.headers.get('content-type');
  if (contentType?.includes('application/json')) {
    return (await response.json()) as T;
  }

  const text = await response.text();
  return text as T;
}

export function requireAccessToken(token?: string) {
  if (!token) {
    throw new ApiError('Authentication is required to call this endpoint.', 401);
  }

  return token;
}

export function requireTenantId(tenantId?: string) {
  if (!tenantId) {
    throw new ApiError(
      'Select a tenant workspace before using tenant-scoped features.',
      400,
    );
  }

  return tenantId;
}
