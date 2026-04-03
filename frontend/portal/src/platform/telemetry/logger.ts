type LogContext = Record<string, unknown> | unknown;

export const logger = {
  info(message: string, context?: LogContext) {
    console.info(`[BaseFAQ Portal] ${message}`, context);
  },
  warn(message: string, context?: LogContext) {
    console.warn(`[BaseFAQ Portal] ${message}`, context);
  },
  error(message: string, context?: LogContext) {
    console.error(`[BaseFAQ Portal] ${message}`, context);
  },
};

export function captureException(error: unknown, context?: LogContext) {
  logger.error('Unhandled exception', { error, context });
  // TODO: wire this to Sentry or the tenant-approved telemetry pipeline.
}
