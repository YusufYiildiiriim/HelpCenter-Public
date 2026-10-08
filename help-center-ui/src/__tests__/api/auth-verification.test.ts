import { afterEach, describe, expect, it, vi } from 'vitest';
import api from '@/lib/axios';
import { clearAccessToken, setAccessToken } from '@/lib/authSession';
import { AuthService, type VerifyResponse } from '@/services/common/AuthService';
import { getApiRetryAfterSeconds } from '@/lib/api/errors';

const identity: VerifyResponse = {
  authenticated: true, role: 'Admin', username: 'admin', userId: 1, isPasswordChangeRequired: false,
};
afterEach(() => { clearAccessToken(); vi.restoreAllMocks(); });

describe('in-flight auth verification', () => {
  it('shares concurrent requests but verifies again after completion', async () => {
    let resolve!: (value: { data: VerifyResponse }) => void;
    const pending = new Promise<{ data: VerifyResponse }>((res) => { resolve = res; });
    const get = vi.spyOn(api, 'get').mockImplementation(() => pending);
    const first = AuthService.verify();
    const second = AuthService.verify();
    const initialRequests = get.mock.calls.length;
    resolve({ data: identity });
    expect(await first).toEqual(identity);
    expect(await second).toEqual(identity);
    expect(initialRequests).toBe(1);
    get.mockResolvedValueOnce({ data: identity });
    await AuthService.verify();
    expect(get).toHaveBeenCalledTimes(2);
  });

  it('clears a rejected request so retry can reach the backend', async () => {
    const error = new Error('429');
    const get = vi.spyOn(api, 'get').mockRejectedValue(error);
    const first = AuthService.verify();
    const second = AuthService.verify();
    await Promise.all([expect(first).rejects.toBe(error), expect(second).rejects.toBe(error)]);
    get.mockResolvedValueOnce({ data: identity });
    await expect(AuthService.verify()).resolves.toEqual(identity);
    expect(get).toHaveBeenCalledTimes(2);
  });

  it('does not share verification across different access tokens', async () => {
    let resolve!: (value: { data: VerifyResponse }) => void;
    const get = vi.spyOn(api, 'get').mockImplementationOnce(() => new Promise((res) => { resolve = res; }))
      .mockResolvedValueOnce({ data: { ...identity, userId: 2 } });
    setAccessToken('session-a');
    const first = AuthService.verify();
    setAccessToken('session-b');
    await expect(AuthService.verify()).resolves.toMatchObject({ userId: 2 });
    resolve({ data: identity });
    await first;
    expect(get).toHaveBeenCalledTimes(2);
  });
});

describe('Retry-After parsing', () => {
  const error = (value?: string) => ({ isAxiosError: true, response: { headers: { 'retry-after': value } } });
  it('reads delay seconds and zero', () => {
    expect(getApiRetryAfterSeconds(error('12'))).toBe(12);
    expect(getApiRetryAfterSeconds(error('0'))).toBe(0);
  });
  it('reads HTTP dates and treats expired dates as ready', () => {
    const now = Date.UTC(2026, 9, 8, 12);
    expect(getApiRetryAfterSeconds(error(new Date(now + 3000).toUTCString()), now)).toBe(3);
    expect(getApiRetryAfterSeconds(error(new Date(now - 3000).toUTCString()), now)).toBe(0);
  });
  it.each([undefined, '', 'invalid'])('falls back to one minute for %s', (value) => {
    expect(getApiRetryAfterSeconds(error(value))).toBe(60);
  });
});
