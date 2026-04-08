import arSaMessages from "@/shared/lib/i18n/locales/ar-SA.json";
import bnBdMessages from "@/shared/lib/i18n/locales/bn-BD.json";
import deDeMessages from "@/shared/lib/i18n/locales/de-DE.json";
import enUsMessages from "@/shared/lib/i18n/locales/en-US.json";
import esEsMessages from "@/shared/lib/i18n/locales/es-ES.json";
import frFrMessages from "@/shared/lib/i18n/locales/fr-FR.json";
import heIlMessages from "@/shared/lib/i18n/locales/he-IL.json";
import hiInMessages from "@/shared/lib/i18n/locales/hi-IN.json";
import idIdMessages from "@/shared/lib/i18n/locales/id-ID.json";
import itItMessages from "@/shared/lib/i18n/locales/it-IT.json";
import jaJpMessages from "@/shared/lib/i18n/locales/ja-JP.json";
import koKrMessages from "@/shared/lib/i18n/locales/ko-KR.json";
import plPlMessages from "@/shared/lib/i18n/locales/pl-PL.json";
import ptBrMessages from "@/shared/lib/i18n/locales/pt-BR.json";
import ruRuMessages from "@/shared/lib/i18n/locales/ru-RU.json";
import thThMessages from "@/shared/lib/i18n/locales/th-TH.json";
import trTrMessages from "@/shared/lib/i18n/locales/tr-TR.json";
import urPkMessages from "@/shared/lib/i18n/locales/ur-PK.json";
import viVnMessages from "@/shared/lib/i18n/locales/vi-VN.json";
import zhCnMessages from "@/shared/lib/i18n/locales/zh-CN.json";

export type TranslationDictionary = typeof enUsMessages;

export const defaultMessages = enUsMessages;

export const messagesByLanguage: Record<string, TranslationDictionary> = {
  "ar-SA": arSaMessages,
  "bn-BD": bnBdMessages,
  "de-DE": deDeMessages,
  "en-US": enUsMessages,
  "es-ES": esEsMessages,
  "fr-FR": frFrMessages,
  "he-IL": heIlMessages,
  "hi-IN": hiInMessages,
  "id-ID": idIdMessages,
  "it-IT": itItMessages,
  "ja-JP": jaJpMessages,
  "ko-KR": koKrMessages,
  "pl-PL": plPlMessages,
  "pt-BR": ptBrMessages,
  "ru-RU": ruRuMessages,
  "th-TH": thThMessages,
  "tr-TR": trTrMessages,
  "ur-PK": urPkMessages,
  "vi-VN": viVnMessages,
  "zh-CN": zhCnMessages,
};
