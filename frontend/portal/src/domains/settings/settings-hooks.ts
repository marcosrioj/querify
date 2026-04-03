import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { getUserProfile, updateUserProfile, type UserProfileUpdateRequestDto } from '@/domains/settings/settings-api';
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
      toast.success('Profile settings saved.');
      await queryClient.invalidateQueries({ queryKey: settingsKeys.profile });
    },
  });
}
