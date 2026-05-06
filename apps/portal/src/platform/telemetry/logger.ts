type LogContext = Record<string, unknown> | unknown;

export const logger = {
  info(message: string, context?: LogContext) {
    console.info(`[Querify Portal] ${message}`, context);
  },
  warn(message: string, context?: LogContext) {
    console.warn(`[Querify Portal] ${message}`, context);
  },
  error(message: string, context?: LogContext) {
    console.error(`[Querify Portal] ${message}`, context);
  },
};

export function captureException(error: unknown, context?: LogContext) {
  logger.error('Unhandled exception', { error, context });
  // TODO: wire this to Sentry or the tenant-approved telemetry pipeline.
}
