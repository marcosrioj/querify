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

type MessagePattern = {
  key: string;
  regex: RegExp;
};

let currentPortalLanguage = DEFAULT_PORTAL_LANGUAGE;
let messagePatterns: MessagePattern[] | null = null;

function formatMessage(message: string, values?: TranslationValues) {
  if (!values) {
    return message;
  }

  return message.replace(/\{(\w+)\}/g, (_match, key) => {
    const value = values[key];
    return value === undefined || value === null ? "" : String(value);
  });
}

function escapeRegExp(input: string) {
  return input.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function getMessagePatterns() {
  if (messagePatterns) {
    return messagePatterns;
  }

  messagePatterns = Object.keys(defaultMessages)
    .filter((key) => /\{\w+\}/.test(key))
    .map((key) => ({
      key,
      regex: new RegExp(
        `^${escapeRegExp(key).replace(/\\\{(\w+)\\\}/g, (_, name) => `(?<${name}>.+?)`)}$`,
      ),
    }));

  return messagePatterns;
}

function findPatternMatch(input: string) {
  for (const pattern of getMessagePatterns()) {
    const match = pattern.regex.exec(input);
    if (!match) {
      continue;
    }

    return {
      key: pattern.key,
      values: match.groups as TranslationValues | undefined,
    };
  }

  return null;
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
  const exactMessage =
    dictionary[input as keyof typeof defaultMessages] ??
    defaultMessages[input as keyof typeof defaultMessages];

  if (exactMessage) {
    return formatMessage(exactMessage, values);
  }

  const patternMatch = findPatternMatch(input);
  if (patternMatch) {
    const template =
      dictionary[patternMatch.key as keyof typeof defaultMessages] ??
      defaultMessages[patternMatch.key as keyof typeof defaultMessages] ??
      patternMatch.key;

    return formatMessage(template, {
      ...patternMatch.values,
      ...values,
    });
  }

  return formatMessage(input, values);
}
