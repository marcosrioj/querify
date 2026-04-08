import {
  DEFAULT_PORTAL_LANGUAGE,
  normalizePortalLanguage,
} from "@/shared/lib/language";
import {
  defaultMessages,
  messagesByLanguage,
} from "@/shared/lib/i18n/messages";

type TranslationValues = Record<
  string,
  string | number | boolean | null | undefined
>;

let currentPortalLanguage = DEFAULT_PORTAL_LANGUAGE;

function formatMessage(message: string, values?: TranslationValues) {
  if (!values) {
    return message;
  }

  return message.replace(/\{(\w+)\}/g, (_match, key) => {
    const value = values[key];
    return value === undefined || value === null ? "" : String(value);
  });
}

export function setCurrentPortalLanguage(language?: string | null) {
  currentPortalLanguage = normalizePortalLanguage(language);
}

export function getCurrentPortalLanguage() {
  return currentPortalLanguage;
}

export function translateText(
  input: string,
  values?: TranslationValues,
  language = currentPortalLanguage,
) {
  const normalizedLanguage = normalizePortalLanguage(language);
  const dictionary = messagesByLanguage[normalizedLanguage] ?? defaultMessages;
  const message = dictionary[input as keyof typeof defaultMessages] ?? defaultMessages[input as keyof typeof defaultMessages] ?? input;

  return formatMessage(message, values);
}
