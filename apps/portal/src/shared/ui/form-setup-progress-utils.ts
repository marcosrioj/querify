export function hasSetupText(value: unknown, minLength = 1) {
  return typeof value === "string" && value.trim().length >= minLength;
}

export function hasSetupValue(value: unknown) {
  return value !== undefined && value !== null && value !== "";
}
