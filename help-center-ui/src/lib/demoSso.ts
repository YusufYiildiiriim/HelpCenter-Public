const identifierPattern = /^(?:[1-9]\d*|[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12})$/i;
const jwtPattern = /^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$/;

export function isDemoSsoEnabled(): boolean {
  return process.env.NEXT_PUBLIC_DEMO_SSO_ENABLED === "true";
}

export function hasValidDemoSsoParams(token: string | null, companyId: string | null, userId: string | null): boolean {
  return Boolean(token && companyId && userId && jwtPattern.test(token) && identifierPattern.test(companyId) && identifierPattern.test(userId));
}
