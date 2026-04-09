import { Fragment, useEffect, useMemo, useState } from 'react';
import type { PropsWithChildren } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getUserProfile } from '@/domains/settings/settings-api';
import { useAuth } from '@/platform/auth/use-auth';
import {
  getCurrentPortalLanguage,
  setCurrentPortalLanguage,
  translateText,
} from '@/shared/lib/i18n-core';
import {
  getBrowserPortalLanguage,
  getLanguageDirection,
  getStoredPortalLanguage,
  isRtlLanguage,
  normalizePortalLanguage,
  setStoredPortalLanguage,
} from '@/shared/lib/language';
import {
  PortalI18nContext,
  type PortalI18nContextValue,
} from '@/shared/lib/portal-i18n-context';

export function PortalI18nProvider({ children }: PropsWithChildren) {
  const { session, status } = useAuth();
  const browserLanguage = useMemo(() => getBrowserPortalLanguage(), []);
  const [localLanguage, setLocalLanguage] = useState(() =>
    getStoredPortalLanguage() ?? browserLanguage,
  );
  const profileQuery = useQuery({
    queryKey: ['portal', 'settings', 'profile'],
    queryFn: () => getUserProfile(session?.accessToken),
    enabled: status === 'ready' && Boolean(session?.accessToken),
    staleTime: 60_000,
  });
  const profileLanguage = profileQuery.data?.language?.trim()
    ? normalizePortalLanguage(profileQuery.data.language)
    : null;
  const language = profileLanguage ?? localLanguage ?? getCurrentPortalLanguage();

  const direction = getLanguageDirection(language);
  const contextValue = useMemo<PortalI18nContextValue>(
    () => ({
      language,
      direction,
      isRtl: isRtlLanguage(language),
      setLanguage: (nextLanguage) => {
        const normalizedLanguage = normalizePortalLanguage(nextLanguage);
        setLocalLanguage(normalizedLanguage);
        setStoredPortalLanguage(normalizedLanguage);
        setCurrentPortalLanguage(normalizedLanguage);
      },
      t: (input, values, requestedLanguage) =>
        translateText(input, values, requestedLanguage ?? language),
    }),
    [direction, language],
  );

  useEffect(() => {
    if (!profileLanguage) {
      return;
    }

    setStoredPortalLanguage(profileLanguage);
    if (profileLanguage !== localLanguage) {
      setLocalLanguage(profileLanguage);
    }
  }, [localLanguage, profileLanguage]);

  useEffect(() => {
    setCurrentPortalLanguage(language);
    document.documentElement.lang = language;
    document.documentElement.dir = direction;
    document.body.dir = direction;
  }, [direction, language]);

  if (status === 'ready' && profileQuery.isPending) {
    return <div className="min-h-screen bg-muted" aria-busy="true" />;
  }

  return (
    <PortalI18nContext.Provider value={contextValue}>
      <Fragment key={language}>{children}</Fragment>
    </PortalI18nContext.Provider>
  );
}
