export function getBrowserTimeZone() {
  return Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
}

export function formatNumericDateTimeInTimeZone(
  input: Date | string | number,
  timeZone: string,
) {
  return new Intl.DateTimeFormat("en-CA", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone,
  }).format(new Date(input));
}

export function getTimeZoneOptions() {
  const supportedTimeZones =
    typeof Intl.supportedValuesOf === "function"
      ? Intl.supportedValuesOf("timeZone")
      : ["UTC"];

  return supportedTimeZones.map((timeZone) => ({
    value: timeZone,
    label: timeZone.replaceAll("_", " "),
    description: timeZone,
    keywords: timeZone.split("/"),
  }));
}
