import { portalRequest, requireAccessToken } from '@/platform/api/http-client';

export type UserProfileDto = {
  givenName: string;
  surName?: string | null;
  email: string;
  phoneNumber: string;
  language?: string | null;
  timeZone?: string | null;
};

export type UserProfileUpdateRequestDto = {
  givenName: string;
  surName?: string | null;
  phoneNumber?: string | null;
  language?: string | null;
  timeZone?: string | null;
};

export function getUserProfile(accessToken?: string) {
  return portalRequest<UserProfileDto>({
    service: 'tenant',
    path: '/api/user/user-profile',
    accessToken: requireAccessToken(accessToken),
  });
}

export function updateUserProfile(
  accessToken: string | undefined,
  body: UserProfileUpdateRequestDto,
) {
  return portalRequest<boolean>({
    service: 'tenant',
    path: '/api/user/user-profile',
    method: 'PUT',
    accessToken: requireAccessToken(accessToken),
    body,
  });
}
