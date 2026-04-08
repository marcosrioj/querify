import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { getUserProfile, updateUserProfile, type UserProfileUpdateRequestDto } from '@/domains/settings/settings-api';
import { translateText } from '@/shared/lib/i18n-core';
import { DEFAULT_PORTAL_TIME_ZONE } from '@/shared/lib/time-zone';
import { useAuth } from '@/platform/auth/auth-context';

const settingsKeys = {
  profile: ['portal', 'settings', 'profile'] as const,
};

export function useUserProfile() {
  const { session, status } = useAuth();

  return useQuery({
    queryKey: settingsKeys.profile,
    queryFn: () => getUserProfile(session?.accessToken),
    enabled: status === 'ready',
  });
}

export function useUpdateUserProfile() {
  const { session } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ['portal', 'settings', 'update-profile'],
    mutationFn: (body: UserProfileUpdateRequestDto) =>
      updateUserProfile(session?.accessToken, body),
    onSuccess: async () => {
      toast.success(translateText('Profile settings saved.'));
      await queryClient.invalidateQueries({ queryKey: settingsKeys.profile });
    },
  });
}

export function usePortalTimeZone() {
  const profileQuery = useUserProfile();

  return profileQuery.data?.timeZone?.trim() || DEFAULT_PORTAL_TIME_ZONE;
}
