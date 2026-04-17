import { portalRequest, requireAccessToken, requireTenantId } from '@/platform/api/http-client';
import { toPagedQuery } from '@/shared/lib/pagination';
import type { PagedResultDto } from '@/shared/types/api';
import type {
  QuestionCreateRequestDto,
  QuestionDetailDto,
  QuestionDto,
  QuestionSourceLinkCreateRequestDto,
  QuestionTagCreateRequestDto,
  QuestionUpdateRequestDto,
} from '@/domains/questions/types';

export function listQuestions(
  accessToken: string | undefined,
  tenantId: string | undefined,
  {
    page,
    pageSize,
    sorting,
    searchText,
    spaceId,
    acceptedAnswerId,
    duplicateOfQuestionId,
    status,
    visibility,
    kind,
    spaceKey,
    contextKey,
    language,
    includeAnswers,
    includeTags,
    includeSources,
    includeActivity,
  }: {
    page: number;
    pageSize: number;
    sorting?: string;
    searchText?: string;
    spaceId?: string;
    acceptedAnswerId?: string;
    duplicateOfQuestionId?: string;
    status?: number;
    visibility?: number;
    kind?: number;
    spaceKey?: string;
    contextKey?: string;
    language?: string;
    includeAnswers?: boolean;
    includeTags?: boolean;
    includeSources?: boolean;
    includeActivity?: boolean;
  },
  signal?: AbortSignal,
) {
  return portalRequest<PagedResultDto<QuestionDto>>({
    service: 'qna',
    path: '/api/qna/question',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    query: toPagedQuery(page, pageSize, sorting, {
      SearchText: searchText,
      SpaceId: spaceId,
      AcceptedAnswerId: acceptedAnswerId,
      DuplicateOfQuestionId: duplicateOfQuestionId,
      Status: status,
      Visibility: visibility,
      Kind: kind,
      SpaceKey: spaceKey,
      ContextKey: contextKey,
      Language: language,
      IncludeAnswers: includeAnswers,
      IncludeTags: includeTags,
      IncludeSources: includeSources,
      IncludeActivity: includeActivity,
    }),
    signal,
  });
}

export function getQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<QuestionDetailDto>({
    service: 'qna',
    path: `/api/qna/question/${id}`,
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function createQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  body: QuestionCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: '/api/qna/question',
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function updateQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: QuestionUpdateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}`,
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function submitQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}/submit`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function approveQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}/approve`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function rejectQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  notes?: string,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}/reject`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body: notes ?? '',
  });
}

export function escalateQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  notes?: string,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}/escalate`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body: notes ?? '',
  });
}

export function deleteQuestion(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/question/${id}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function addQuestionTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: QuestionTagCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}/tag`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function removeQuestionTag(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  tagId: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/question/${id}/tag/${tagId}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}

export function addQuestionSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  body: QuestionSourceLinkCreateRequestDto,
) {
  return portalRequest<string>({
    service: 'qna',
    path: `/api/qna/question/${id}/source`,
    method: 'POST',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
    body,
  });
}

export function removeQuestionSource(
  accessToken: string | undefined,
  tenantId: string | undefined,
  id: string,
  sourceLinkId: string,
) {
  return portalRequest<void>({
    service: 'qna',
    path: `/api/qna/question/${id}/source/${sourceLinkId}`,
    method: 'DELETE',
    accessToken: requireAccessToken(accessToken),
    tenantId: requireTenantId(tenantId),
  });
}
