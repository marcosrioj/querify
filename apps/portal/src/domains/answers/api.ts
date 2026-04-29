import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import { toPagedQuery } from '@/shared/lib/pagination';
import type { PagedResultDto } from '@/shared/types/api';
import type {
  AnswerCreateRequestDto,
  AnswerDto,
  AnswerSourceLinkCreateRequestDto,
  AnswerUpdateRequestDto,
} from '@/domains/answers/types';

export function listAnswers(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    questionId,
    status,
    visibility,
    isAccepted,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    questionId?: string;
    status?: number;
    visibility?: number;
    isAccepted?: boolean;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<AnswerDto>>({
    service: 'qna',
    path: '/api/qna/answer',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      QuestionId: questionId,
      Status: status,
      Visibility: visibility,
      IsAccepted: isAccepted,
    }),
    signal,
  });
}

export function getAnswer(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<AnswerDto>({
    service: 'qna',
    path: `/api/qna/answer/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createAnswer(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: AnswerCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: '/api/qna/answer',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateAnswer(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: AnswerUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/answer/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function activateAnswer(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/answer/${id}/activate`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function retireAnswer(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/answer/${id}/retire`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function deleteAnswer(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/answer/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function addAnswerSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: AnswerSourceLinkCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/answer/${id}/source`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function removeAnswerSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  sourceLinkId: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/answer/${id}/source/${sourceLinkId}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
