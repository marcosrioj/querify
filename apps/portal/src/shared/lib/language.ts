export type LanguageDirection = "ltr" | "rtl";

export type LanguageOption = {
  code: string;
  label: string;
  direction: LanguageDirection;
};

export const DEFAULT_PORTAL_LANGUAGE = "en-US";

export const portalLanguageOptions: LanguageOption[] = [
  { code: "en-US", label: "English", direction: "ltr" },
  { code: "zh-CN", label: "Chinese (Simplified)", direction: "ltr" },
  { code: "hi-IN", label: "Hindi", direction: "ltr" },
  { code: "es-ES", label: "Spanish", direction: "ltr" },
  { code: "fr-FR", label: "French", direction: "ltr" },
  { code: "ar-SA", label: "Arabic", direction: "rtl" },
  { code: "bn-BD", label: "Bengali", direction: "ltr" },
  { code: "pt-BR", label: "Portuguese (Brazil)", direction: "ltr" },
  { code: "ru-RU", label: "Russian", direction: "ltr" },
  { code: "ur-PK", label: "Urdu", direction: "rtl" },
  { code: "id-ID", label: "Indonesian", direction: "ltr" },
  { code: "de-DE", label: "German", direction: "ltr" },
  { code: "ja-JP", label: "Japanese", direction: "ltr" },
  { code: "ko-KR", label: "Korean", direction: "ltr" },
  { code: "it-IT", label: "Italian", direction: "ltr" },
  { code: "tr-TR", label: "Turkish", direction: "ltr" },
  { code: "vi-VN", label: "Vietnamese", direction: "ltr" },
  { code: "th-TH", label: "Thai", direction: "ltr" },
  { code: "pl-PL", label: "Polish", direction: "ltr" },
  { code: "he-IL", label: "Hebrew", direction: "rtl" },
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
