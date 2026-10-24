/**
 * The access token intentionally lives only in this module's memory. Refresh
 * credentials are HttpOnly cookies managed by the API and are never readable
 * from client-side JavaScript.
 */
let accessToken: string | null = null;
let accessTokenExpiresAt: string | null = null;
export const LOGOUT_EVENT_KEY = "helpcenter:auth-logout";

export function setAccessToken(token: string, expiresAt?: string): void {
  accessToken = token;
  accessTokenExpiresAt = expiresAt ?? null;
}

export function getAccessToken(): string | null {
  return accessToken;
}

export function getAccessTokenExpiresAt(): string | null {
  return accessTokenExpiresAt;
}

export function clearAccessToken(): void {
  accessToken = null;
  accessTokenExpiresAt = null;
}

export function broadcastLogout(): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(LOGOUT_EVENT_KEY, String(Date.now()));
}

export function subscribeToLogout(handler: () => void): () => void {
  if (typeof window === "undefined") return () => undefined;
  const listener = (event: StorageEvent) => {
    if (event.key === LOGOUT_EVENT_KEY && event.newValue) handler();
  };
  window.addEventListener("storage", listener);
  return () => window.removeEventListener("storage", listener);
}
