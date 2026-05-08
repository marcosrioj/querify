import { RuntimeEnv } from "@/platform/runtime/env";

const HUB_PATH = "/api/qna/hubs/portal-notifications";

export function buildPortalNotificationsHubUrl(tenantId: string) {
  const url = new URL(HUB_PATH, RuntimeEnv.qnaPortalApiUrl);
  url.searchParams.set("tenantId", tenantId);
  return url.toString();
}
