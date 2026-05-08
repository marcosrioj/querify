import { RuntimeEnv } from "@/platform/runtime/env";

const HUB_PATH = "/api/qna/hubs/portal-notifications";

export function buildPortalNotificationsHubUrl(tenantId: string) {
  const url = new URL(HUB_PATH, normalizeUrlBase(RuntimeEnv.qnaPortalApiUrl));
  url.searchParams.set("tenantId", tenantId);
  return url.toString();
}

function normalizeUrlBase(baseUrl: string) {
  const trimmed = baseUrl.trim();

  if (trimmed.startsWith("//")) {
    return `${globalThis.location?.protocol ?? "http:"}${trimmed}`;
  }

  if (trimmed.startsWith("/")) {
    return new URL(trimmed, globalThis.location?.origin ?? "http://localhost").toString();
  }

  return trimmed;
}
