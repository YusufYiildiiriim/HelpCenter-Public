const PUBLIC_ID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** Public route identifiers are GUIDs; reject malformed paths before REST or SignalR calls. */
export function isPublicId(value: string | undefined): value is string {
  return typeof value === "string" && PUBLIC_ID_PATTERN.test(value);
}
