import { useContext } from 'react';
import { PortalI18nContext } from '@/shared/lib/portal-i18n-context';

export function usePortalI18n() {
  const context = useContext(PortalI18nContext);
  if (!context) {
    throw new Error('usePortalI18n must be used within PortalI18nProvider.');
  }

  return context;
}
