import {
  Fragment,
  PropsWithChildren,
  ReactNode,
  createContext,
  useContext,
  useEffect,
  useMemo,
} from "react";
import { useQuery } from "@tanstack/react-query";
import { getUserProfile } from "@/domains/settings/settings-api";
import { useAuth } from "@/platform/auth/auth-context";
import {
  getCurrentPortalLanguage,
  setCurrentPortalLanguage,
  translateText,
} from "@/shared/lib/i18n-core";
import {
  getBrowserPortalLanguage,
  getLanguageDirection,
  isRtlLanguage,
  normalizePortalLanguage,
} from "@/shared/lib/language";
import { translateRenderableNode } from "@/shared/lib/translate-renderable-node";

type PortalI18nContextValue = {
  language: string;
  direction: "ltr" | "rtl";
  isRtl: boolean;
  t: typeof translateText;
};

const PortalI18nContext = createContext<PortalI18nContextValue | undefined>(
  undefined,
);

function resolvePortalLanguageFromProfile(
  profileLanguage?: string | null,
  browserLanguage?: string,
) {
  return normalizePortalLanguage(
    profileLanguage || browserLanguage || getCurrentPortalLanguage(),
  );
}

export function PortalI18nProvider({ children }: PropsWithChildren) {
  const { session, status } = useAuth();
  const browserLanguage = useMemo(() => getBrowserPortalLanguage(), []);
  const profileQuery = useQuery({
    queryKey: ["portal", "settings", "profile"],
    queryFn: () => getUserProfile(session?.accessToken),
    enabled: status === "ready",
    staleTime: 60_000,
  });

  const language = resolvePortalLanguageFromProfile(
    profileQuery.data?.language,
    browserLanguage,
  );
  const direction = getLanguageDirection(language);
  const contextValue = useMemo<PortalI18nContextValue>(
    () => ({
      language,
      direction,
      isRtl: isRtlLanguage(language),
      t: (input, values, requestedLanguage) =>
        translateText(input, values, requestedLanguage ?? language),
    }),
    [direction, language],
  );

  useEffect(() => {
    setCurrentPortalLanguage(language);
    document.documentElement.lang = language;
    document.documentElement.dir = direction;
    document.body.dir = direction;
  }, [direction, language]);

  if (status === "ready" && profileQuery.isPending) {
    return <div className="min-h-screen bg-muted" aria-busy="true" />;
  }

  return (
    <PortalI18nContext.Provider value={contextValue}>
      <Fragment key={language}>{children}</Fragment>
    </PortalI18nContext.Provider>
  );
}

export function usePortalI18n() {
  const context = useContext(PortalI18nContext);
  if (!context) {
    throw new Error("usePortalI18n must be used within PortalI18nProvider.");
  }

  return context;
}

export function translateNode(node: ReactNode) {
  return translateRenderableNode(node);
}

export function translateMaybeString(
  value: ReactNode,
  t: typeof translateText = translateText,
) {
  if (typeof value === "string") {
    return t(value);
  }

  return translateRenderableNode(value);
}
