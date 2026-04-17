import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import { toPagedQuery } from '@/shared/lib/pagination';
import type { PagedResultDto } from '@/shared/types/api';
import type { ActivityDto } from '@/domains/activity/types';

export function listActivity(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    questionId,
    answerId,
    kind,
    actorKind,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    questionId?: string;
    answerId?: string;
    kind?: number;
    actorKind?: number;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<ActivityDto>>({
    service: 'qna',
    path: '/api/qna/activity',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      QuestionId: questionId,
      AnswerId: answerId,
      Kind: kind,
      ActorKind: actorKind,
    }),
    signal,
  });
}

export function getActivity(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<ActivityDto>({
    service: 'qna',
    path: `/api/qna/activity/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
