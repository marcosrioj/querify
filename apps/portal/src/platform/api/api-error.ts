type ApiErrorPayload = {
  errorCode?: number;
  messageError?: string;
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[] | string>;
  data?: unknown;
};

type ApiErrorContext = {
  service?: string;
  path?: string;
  method?: string;
  url?: string;
};

export class ApiError extends Error {
  status: number;
  errorCode?: number;
  data?: unknown;
  context?: ApiErrorContext;

  constructor(
    message: string,
    status = 500,
    errorCode?: number,
    data?: unknown,
    context?: ApiErrorContext,
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errorCode = errorCode;
    this.data = data;
    this.context = context;
  }
}

const isApiErrorPayload = (value: unknown): value is ApiErrorPayload => {
  return (
    typeof value === 'object' &&
    value !== null &&
    ('messageError' in value ||
      'errorCode' in value ||
      'message' in value ||
      'title' in value ||
      'detail' in value ||
      'errors' in value)
  );
};

const GENERIC_MESSAGES = new Set([
  'Api error',
  'BaseFAQ request failed.',
  'Request failed.',
  'Something went wrong while communicating with BaseFAQ.',
]);

function normalizeText(value: unknown) {
  return typeof value === 'string' && value.trim() ? value.trim() : undefined;
}

function parsePayload(rawBody: string, contentType?: string | null): unknown {
  const trimmedBody = rawBody.trim();
  if (!trimmedBody) {
    return undefined;
  }

  const looksLikeJson =
    contentType?.includes('application/json') ||
    trimmedBody.startsWith('{') ||
    trimmedBody.startsWith('[');

  if (!looksLikeJson) {
    return trimmedBody;
  }

  try {
    return JSON.parse(trimmedBody);
  } catch {
    return trimmedBody;
  }
}

function normalizeErrorData(data: unknown) {
  const normalizedText = normalizeText(data);
  if (!normalizedText) {
    return data;
  }

  if (!normalizedText.startsWith('{') && !normalizedText.startsWith('[')) {
    return normalizedText;
  }

  try {
    return JSON.parse(normalizedText);
  } catch {
    return normalizedText;
  }
}

function extractValidationMessage(
  errors: Record<string, string[] | string> | undefined,
) {
  if (!errors) {
    return undefined;
  }

  for (const [fieldName, rawValue] of Object.entries(errors)) {
    const messages = Array.isArray(rawValue) ? rawValue : [rawValue];
    const firstMessage = messages.find((message) => normalizeText(message));

    if (!firstMessage) {
      continue;
    }

    return fieldName ? `${fieldName}: ${firstMessage}` : firstMessage;
  }

  return undefined;
}

function extractPayloadMessage(payload: unknown) {
  if (typeof payload === 'string') {
    return normalizeText(payload);
  }

  if (!isApiErrorPayload(payload)) {
    return undefined;
  }

  return (
    normalizeText(payload.messageError) ||
    normalizeText(payload.message) ||
    normalizeText(payload.detail) ||
    extractValidationMessage(payload.errors) ||
    normalizeText(payload.title)
  );
}

function extractPayloadErrorCode(payload: unknown) {
  return isApiErrorPayload(payload) && typeof payload.errorCode === 'number'
    ? payload.errorCode
    : undefined;
}

function extractPayloadData(payload: unknown) {
  return isApiErrorPayload(payload) ? normalizeErrorData(payload.data) : undefined;
}

export function isAbortError(error: unknown) {
  if (error instanceof DOMException && error.name === 'AbortError') {
    return true;
  }

  return (
    error instanceof Error &&
    (error.name === 'AbortError' ||
      error.name === 'CanceledError' ||
      error.message === 'canceled')
  );
}

export function toErrorMessage(
  error: unknown,
  fallbackMessage = translateText('Request failed.'),
) {
  if (error instanceof ApiError) {
    const message = normalizeText(error.message);
    if (message && !GENERIC_MESSAGES.has(message)) {
      return message;
    }

    return fallbackMessage;
  }

  if (error instanceof Error) {
    const message = normalizeText(error.message);
    if (message && message !== 'Failed to fetch' && message !== 'Load failed') {
      return message;
    }
  }

  return fallbackMessage;
}

export async function toApiError(
  response: Response,
  fallbackMessage: string,
  context?: ApiErrorContext,
): Promise<ApiError> {
  const rawBody = await response.text();
  const payload = parsePayload(rawBody, response.headers.get('content-type'));
  const message = extractPayloadMessage(payload) || fallbackMessage;

  return new ApiError(
    message,
    response.status,
    extractPayloadErrorCode(payload),
    extractPayloadData(payload),
    context,
  );
}
import { translateText } from '@/shared/lib/i18n-core';
