import { translateText } from '@/shared/lib/i18n-core';

type ApiErrorPayload = {
  errorCode?: number;
  ErrorCode?: number;
  messageError?: string;
  MessageError?: string;
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[] | string>;
  data?: unknown;
  Data?: unknown;
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
      'MessageError' in value ||
      'errorCode' in value ||
      'ErrorCode' in value ||
      'message' in value ||
      'title' in value ||
      'detail' in value ||
      'errors' in value)
  );
};

const GENERIC_MESSAGES = new Set([
  'Api error',
  'BaseFAQ request failed.',
  'One or more validation errors occurred.',
  'Request failed.',
  'Something went wrong while communicating with BaseFAQ.',
]);

const API_ERROR_MESSAGE_ALIASES: Array<{
  pattern: RegExp;
  message: string;
}> = [
  {
    pattern: /^.+ '.+' was not found\.$/,
    message: 'The requested record was not found.',
  },
  {
    pattern: /^Current user profile was not found\.$/,
    message: 'The requested record was not found.',
  },
  {
    pattern: /^Accepted answer '.+' belongs to a different question\.$/,
    message: 'The accepted answer belongs to a different question.',
  },
  {
    pattern: /^User '.+' already belongs to tenant '.+'\.$/,
    message: 'This email is already a member of the workspace.',
  },
  {
    pattern: /^Tenant '.+' is not allowed for the current user\.$/,
    message: 'You do not have access to this workspace.',
  },
  {
    pattern: /^Tenant context is missing from the current request\.$/,
    message: 'Select a workspace to continue.',
  },
  {
    pattern: /^External user ID is missing from the current session\.$/,
    message: 'Your session expired. Sign in again.',
  },
  {
    pattern: /^HttpContext is missing from the current request\.$/,
    message: 'Your session expired. Sign in again.',
  },
  {
    pattern: /^Client key .+\.$/,
    message: 'The request is invalid.',
  },
  {
    pattern: /^Missing required header '.+'\.$/,
    message: 'The request is invalid.',
  },
  {
    pattern: /^Header '.+' (is required|must be a valid GUID)\.$/,
    message: 'The request is invalid.',
  },
  {
    pattern: /^Tenant ID '.+' required\.$/,
    message: 'The request is invalid.',
  },
  {
    pattern: /^Unsupported cipher format\.$/,
    message: 'The request is invalid.',
  },
  {
    pattern: /^Stripe .+\.$/,
    message: 'The request is invalid.',
  },
  {
    pattern:
      /^(Billing webhook ingress is not ready|Cors Options Not Found|Missing connection string|Redis .+ is missing|Tenant '.+' has an invalid connection string|Current tenant connection for .+ was not found)\.?/,
    message: 'The service is unavailable right now.',
  },
];

function normalizeText(value: unknown) {
  return typeof value === 'string' && value.trim() ? value.trim() : undefined;
}

function resolveApiErrorMessage(message: string) {
  const normalizedMessage = normalizeText(message);
  if (!normalizedMessage) {
    return undefined;
  }

  if (normalizedMessage === 'One or more validation errors occurred.') {
    return 'The submitted data is invalid.';
  }

  const alias = API_ERROR_MESSAGE_ALIASES.find(({ pattern }) =>
    pattern.test(normalizedMessage),
  );

  return alias?.message ?? normalizedMessage;
}

function translateApiErrorMessage(message: string) {
  return translateText(resolveApiErrorMessage(message) ?? message);
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

  for (const rawValue of Object.values(errors)) {
    const messages = Array.isArray(rawValue) ? rawValue : [rawValue];
    const firstMessage = messages.find((message) => normalizeText(message));

    if (!firstMessage) {
      continue;
    }

    return 'The submitted data is invalid.';
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
    normalizeText(payload.MessageError) ||
    normalizeText(payload.message) ||
    normalizeText(payload.detail) ||
    extractValidationMessage(payload.errors) ||
    normalizeText(payload.title)
  );
}

function extractPayloadErrorCode(payload: unknown) {
  if (!isApiErrorPayload(payload)) {
    return undefined;
  }

  return typeof payload.errorCode === 'number'
    ? payload.errorCode
    : typeof payload.ErrorCode === 'number'
      ? payload.ErrorCode
      : undefined;
}

function extractPayloadData(payload: unknown) {
  if (!isApiErrorPayload(payload)) {
    return undefined;
  }

  return normalizeErrorData(payload.data ?? payload.Data);
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
      return translateApiErrorMessage(message);
    }

    return fallbackMessage;
  }

  if (error instanceof Error) {
    const message = normalizeText(error.message);
    if (message && message !== 'Failed to fetch' && message !== 'Load failed') {
      return translateApiErrorMessage(message);
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
  const payloadMessage = extractPayloadMessage(payload);
  const message = payloadMessage
    ? (resolveApiErrorMessage(payloadMessage) ?? fallbackMessage)
    : fallbackMessage;

  return new ApiError(
    message,
    response.status,
    extractPayloadErrorCode(payload),
    extractPayloadData(payload),
    context,
  );
}
