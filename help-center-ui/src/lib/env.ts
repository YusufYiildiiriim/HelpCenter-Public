import { z } from 'zod';

const schema = z.object({
  NEXT_PUBLIC_API_URL: z.string().url(),
  NEXT_PUBLIC_APP_URL: z.string().url(),
});

export const env = schema.parse({
  NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
  NEXT_PUBLIC_APP_URL: process.env.NEXT_PUBLIC_APP_URL,
});

export const apiBaseUrl = env.NEXT_PUBLIC_API_URL.replace(/\/$/, '');

export function toApiUrl(path: string): string {
  return `${apiBaseUrl}/${path.replace(/^\/+/, '')}`;
}
