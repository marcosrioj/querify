import { ApiError, isAbortError, toApiError } from '@/platform/api/api-error';
import { RuntimeEnv } from '@/platform/runtime/env';
import { translateText } from '@/shared/lib/i18n-core';

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

function getServiceLabel(service: PortalService) {
  return translateText(service === 'tenant' ? 'Tenant API' : 'FAQ API');
}

function buildHttpErrorFallback(service: PortalService, status: number) {
  switch (status) {
    case 400:
      return translateText('The request is invalid.');
    case 401:
      return translateText('Your session expired. Sign in again.');
    case 403:
      return translateText('You do not have access to this workspace.');
    case 404:
      return translateText('The requested record was not found.');
    case 409:
      return translateText('This change conflicts with the current data.');
    case 422:
      return translateText('The submitted data is invalid.');
    case 429:
      return translateText('{serviceLabel} is throttling requests right now.', {
        serviceLabel: getServiceLabel(service),
      });
    default:
      return status >= 500
        ? translateText('{serviceLabel} is unavailable right now.', {
            serviceLabel: getServiceLabel(service),
          })
        : translateText('{serviceLabel} request failed.', {
            serviceLabel: getServiceLabel(service),
          });
  }
}

function buildNetworkErrorMessage(service: PortalService) {
  if (typeof navigator !== 'undefined' && navigator.onLine === false) {
    return translateText('You are offline.');
  }

  return translateText('Cannot reach the {serviceLabel}.', {
    serviceLabel: getServiceLabel(service),
  });
}

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

  if (tenantId) {
    headers.set('X-Tenant-Id', tenantId);
  }

  if (body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  let response: Response;

  try {
    response = await fetch(url, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal,
    });
  } catch (error) {
    if (isAbortError(error)) {
      throw error;
    }

    throw new ApiError(
      buildNetworkErrorMessage(service),
      0,
      undefined,
      undefined,
      { service, path, method, url },
    );
  }

  if (!response.ok) {
    throw await toApiError(
      response,
      buildHttpErrorFallback(service, response.status),
      { service, path, method, url },
    );
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
    throw new ApiError(translateText('Sign in again to continue.'), 401);
  }

  return token;
}

export function requireTenantId(tenantId?: string) {
  if (!tenantId) {
    throw new ApiError(translateText('Select a workspace to continue.'), 400);
  }

  return tenantId;
}
