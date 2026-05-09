export const DEFAULT_PORTAL_TIME_ZONE = "UTC";

export function isSupportedTimeZone(timeZone: string) {
  try {
    new Intl.DateTimeFormat("en-US", { timeZone }).format(new Date(0));
    return true;
  } catch {
    return false;
  }
}

export function resolvePortalTimeZone(timeZone?: string | null) {
  const trimmedTimeZone = timeZone?.trim();
  if (!trimmedTimeZone) {
    return DEFAULT_PORTAL_TIME_ZONE;
  }

  return isSupportedTimeZone(trimmedTimeZone)
    ? trimmedTimeZone
    : DEFAULT_PORTAL_TIME_ZONE;
}

export function getBrowserTimeZone() {
  const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
  return timeZone && isSupportedTimeZone(timeZone) ? timeZone : null;
}

export function formatNumericDateTimeInTimeZone(
  input: Date | string | number,
  timeZone: string,
) {
  const resolvedTimeZone = resolvePortalTimeZone(timeZone);

  // Portal date rendering assumes every timestamp persisted in the database is UTC.
  return new Intl.DateTimeFormat("en-CA", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: resolvedTimeZone,
  }).format(new Date(input));
}

export function formatOptionalDateTimeInTimeZone(
  input: Date | string | number | null | undefined,
  timeZone: string,
  fallback: string,
) {
  if (!input) {
    return fallback;
  }

  return formatNumericDateTimeInTimeZone(input, timeZone);
}

export function getTimeZoneOptions() {
  const supportedTimeZones = new Set(
    [
      DEFAULT_PORTAL_TIME_ZONE,
      getBrowserTimeZone(),
      ...(typeof Intl.supportedValuesOf === "function"
        ? Intl.supportedValuesOf("timeZone")
        : []),
    ].filter((timeZone): timeZone is string => Boolean(timeZone)),
  );

  const orderedTimeZones = Array.from(supportedTimeZones);

  return orderedTimeZones.map((timeZone) => ({
    value: timeZone,
    label: timeZone.replaceAll("_", " "),
    description: timeZone,
    keywords: timeZone.split("/"),
  }));
}
