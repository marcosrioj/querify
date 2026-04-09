import { useCallback } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  AccordionMenu,
  AccordionMenuClassNames,
  AccordionMenuGroup,
  AccordionMenuItem,
  AccordionMenuLabel,
} from '@/components/ui/accordion-menu';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import { portalNavigation } from '@/shared/constants/navigation';

export function SidebarMenuPrimary({
  onNavigate,
}: {
  onNavigate?: () => void;
}) {
  const { t } = usePortalI18n();
  const { pathname } = useLocation();
  const navigate = useNavigate();

  const matchPath = useCallback(
    (path: string): boolean =>
      path === pathname || (path.length > 1 && pathname.startsWith(path)),
    [pathname],
  );

  const classNames: AccordionMenuClassNames = {
    root: 'space-y-2.5 px-3.5',
    group: 'gap-px',
    label:
      'pt-2.25 pb-px text-xs font-medium uppercase text-muted-foreground/70',
    item: 'h-9 border border-transparent bg-transparent text-accent-foreground hover:bg-transparent hover:text-mono data-[selected=true]:border-border data-[selected=true]:bg-background data-[selected=true]:font-medium data-[selected=true]:text-mono',
  };

  return (
    <AccordionMenu
      type="single"
      selectedValue={pathname}
      matchPath={matchPath}
      collapsible
      classNames={classNames}
      onItemClick={(value) => {
        navigate(value);
        onNavigate?.();
      }}
    >
      <AccordionMenuLabel>{t('Portal')}</AccordionMenuLabel>
      <AccordionMenuGroup>
        {portalNavigation.map((item) => {
          const Icon = item.icon;

          return (
            <AccordionMenuItem
              key={item.key}
              value={item.path}
              className="text-sm font-medium"
            >
              <Icon data-slot="accordion-menu-icon" />
              <span data-slot="accordion-menu-title">{t(item.label)}</span>
            </AccordionMenuItem>
          );
        })}
      </AccordionMenuGroup>
    </AccordionMenu>
  );
}
