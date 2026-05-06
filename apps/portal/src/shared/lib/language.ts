export type LanguageDirection = "ltr" | "rtl";

export type LanguageOption = {
  code: string;
  label: string;
  direction: LanguageDirection;
};

export const DEFAULT_PORTAL_LANGUAGE = "en-US";
export const PORTAL_LANGUAGE_STORAGE_KEY = "querify.portal.language";

export const portalLanguageOptions: LanguageOption[] = [
  { code: "en-US", label: "English", direction: "ltr" },
  { code: "zh-CN", label: "中文（简体）", direction: "ltr" },
  { code: "hi-IN", label: "हिन्दी", direction: "ltr" },
  { code: "es-ES", label: "Español", direction: "ltr" },
  { code: "fr-FR", label: "Français", direction: "ltr" },
  { code: "ar-SA", label: "العربية", direction: "rtl" },
  { code: "bn-BD", label: "বাংলা", direction: "ltr" },
  { code: "pt-BR", label: "Português (Brasil)", direction: "ltr" },
  { code: "ru-RU", label: "Русский", direction: "ltr" },
  { code: "ur-PK", label: "اردو", direction: "rtl" },
  { code: "id-ID", label: "Bahasa Indonesia", direction: "ltr" },
  { code: "de-DE", label: "Deutsch", direction: "ltr" },
  { code: "ja-JP", label: "日本語", direction: "ltr" },
  { code: "ko-KR", label: "한국어", direction: "ltr" },
  { code: "it-IT", label: "Italiano", direction: "ltr" },
  { code: "tr-TR", label: "Türkçe", direction: "ltr" },
  { code: "vi-VN", label: "Tiếng Việt", direction: "ltr" },
  { code: "th-TH", label: "ไทย", direction: "ltr" },
  { code: "pl-PL", label: "Polski", direction: "ltr" },
  { code: "he-IL", label: "עברית", direction: "rtl" },
];

const languageByCode = new Map(
  portalLanguageOptions.map((option) => [option.code.toLowerCase(), option]),
);

const languageCodeByBase = new Map(
  portalLanguageOptions.map((option) => [
    option.code.split("-")[0].toLowerCase(),
    option.code,
  ]),
);

export function getLanguageOption(language?: string | null) {
  if (!language) {
    return portalLanguageOptions[0];
  }

  const normalizedLanguage = language.trim().toLowerCase();

  return (
    languageByCode.get(normalizedLanguage) ??
    languageByCode.get(languageCodeByBase.get(normalizedLanguage.split("-")[0]) ?? "") ??
    portalLanguageOptions[0]
  );
}

export function normalizePortalLanguage(language?: string | null) {
  return getLanguageOption(language).code;
}

export function getLanguageDirection(language?: string | null) {
  return getLanguageOption(language).direction;
}

export function isRtlLanguage(language?: string | null) {
  return getLanguageDirection(language) === "rtl";
}

export function getBrowserPortalLanguage() {
  if (typeof navigator === "undefined") {
    return DEFAULT_PORTAL_LANGUAGE;
  }

  const preferredLanguage = navigator.languages?.[0] || navigator.language;
  return normalizePortalLanguage(preferredLanguage);
}

export function getStoredPortalLanguage() {
  if (typeof window === "undefined") {
    return null;
  }

  try {
    const storedLanguage = window.localStorage.getItem(PORTAL_LANGUAGE_STORAGE_KEY);
    return storedLanguage ? normalizePortalLanguage(storedLanguage) : null;
  } catch {
    return null;
  }
}

export function setStoredPortalLanguage(language?: string | null) {
  if (typeof window === "undefined") {
    return;
  }

  try {
    if (!language?.trim()) {
      window.localStorage.removeItem(PORTAL_LANGUAGE_STORAGE_KEY);
      return;
    }

    window.localStorage.setItem(
      PORTAL_LANGUAGE_STORAGE_KEY,
      normalizePortalLanguage(language),
    );
  } catch {
    // Ignore storage failures and keep the in-memory language instead.
  }
}
