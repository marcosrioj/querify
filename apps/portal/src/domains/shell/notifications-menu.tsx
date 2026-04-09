import { Bell } from 'lucide-react';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import {
  Button,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/ui';

export function NotificationsMenu({
  triggerVariant = 'outline',
}: {
  triggerVariant?: 'outline' | 'ghost';
}) {
  const { t } = usePortalI18n();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button mode="icon" variant={triggerVariant} aria-label={t('Notifications')}>
          <Bell className="size-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-80">
        <DropdownMenuLabel>{t('Notifications')}</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem className="items-start">
          {t('Live notification feeds are not in the current Portal API surface.')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
