import { createContext } from 'react';
import { translateText } from '@/shared/lib/i18n-core';

export type PortalI18nContextValue = {
  language: string;
  direction: 'ltr' | 'rtl';
  isRtl: boolean;
  t: typeof translateText;
};

export const PortalI18nContext = createContext<
  PortalI18nContextValue | undefined
>(undefined);
