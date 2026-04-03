type ApiErrorPayload = {
  errorCode?: number;
  messageError?: string;
  data?: unknown;
};

export class ApiError extends Error {
  status: number;
  errorCode?: number;
  data?: unknown;

  constructor(message: string, status = 500, errorCode?: number, data?: unknown) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errorCode = errorCode;
    this.data = data;
  }
}

const isApiErrorPayload = (value: unknown): value is ApiErrorPayload => {
  return (
    typeof value === 'object' &&
    value !== null &&
    ('messageError' in value || 'errorCode' in value)
  );
};

export async function toApiError(
  response: Response,
  fallbackMessage: string,
): Promise<ApiError> {
  let payload: unknown;

  try {
    payload = await response.json();
  } catch {
    payload = undefined;
  }

  if (isApiErrorPayload(payload)) {
    return new ApiError(
      payload.messageError || fallbackMessage,
      response.status,
      payload.errorCode,
      payload.data,
    );
  }

  return new ApiError(fallbackMessage, response.status);
}
