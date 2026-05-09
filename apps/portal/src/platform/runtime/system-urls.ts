import { RuntimeEnv } from '@/platform/runtime/env';

function getBaseUrlContext() {
  if (typeof window !== 'undefined') {
    return window.location.origin;
  }

  return 'http://localhost';
}

function appendPath(basePath: string, targetPath: string) {
  const normalizedBase = basePath.replace(/\/+$/, '');
  const normalizedTarget = targetPath.startsWith('/') ? targetPath : `/${targetPath}`;

  return `${normalizedBase}${normalizedTarget}` || '/';
}

function rewritePresignedUrl(url: string, baseUrl: string) {
  if (!baseUrl) {
    return url;
  }

  try {
    const sourceUrl = new URL(url);
    const systemUrl = new URL(baseUrl, getBaseUrlContext());

    systemUrl.pathname = appendPath(systemUrl.pathname, sourceUrl.pathname);
    systemUrl.search = sourceUrl.search;
    systemUrl.hash = sourceUrl.hash;

    return systemUrl.toString();
  } catch {
    return url;
  }
}

export function resolveObjectStorageUrl(url: string) {
  return rewritePresignedUrl(url, RuntimeEnv.systemUrls.s3Url);
}
