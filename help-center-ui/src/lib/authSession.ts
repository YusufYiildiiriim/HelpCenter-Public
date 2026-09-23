/**
 * The access token intentionally lives only in this module's memory. Refresh
 * credentials are HttpOnly cookies managed by the API and are never readable
 * from client-side JavaScript.
 */
let accessToken: string | null = null;
let accessTokenExpiresAt: string | null = null;

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
