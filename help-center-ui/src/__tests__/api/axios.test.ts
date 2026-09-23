import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import MockAdapter from 'axios-mock-adapter';
import type { InternalAxiosRequestConfig } from 'axios';
import api from '@/lib/axios';
import { buildQueryParams } from '@/lib/api/queryParams';
import { clearAccessToken, getAccessToken, setAccessToken } from '@/lib/authSession';

interface InterceptorHandler<V> {
  fulfilled?: (value: V) => V | Promise<V>;
}
interface InterceptorManagerWithHandlers<V> {
  handlers: Array<InterceptorHandler<V> | null>;
}

function getRequestInterceptor() {
  const manager = api.interceptors.request as unknown as InterceptorManagerWithHandlers<InternalAxiosRequestConfig>;
  const handler = manager.handlers[0];
  if (!handler?.fulfilled) throw new Error('Request interceptor was not registered.');
  return handler.fulfilled;
}

describe('axios instance', () => {
  let mock: MockAdapter;
  const originalLocation = window.location;

  beforeEach(() => {
    mock = new MockAdapter(api);
    clearAccessToken();
    sessionStorage.clear();
    localStorage.clear();
    Object.defineProperty(window, 'location', {
      configurable: true,
      writable: true,
      value: { ...originalLocation, href: '', pathname: '/admin/dashboard' },
    });
  });

  afterEach(() => {
    mock.restore();
    Object.defineProperty(window, 'location', {
      configurable: true,
      writable: true,
      value: originalLocation,
    });
  });

  describe('request interceptor', () => {
    it('adds the in-memory access token to the Authorization header', async () => {
      setAccessToken('memory-token-123');
      mock.onGet('/api/ping').reply((config) => {
        expect(config.headers?.Authorization).toBe('Bearer memory-token-123');
        return [200, { success: true }];
      });

      await api.get('/api/ping');
    });

    it('never reads an access token from browser storage', async () => {
      sessionStorage.setItem('auth_token', 'stale-session-token');
      localStorage.setItem('auth_token', 'stale-local-token');
      mock.onGet('/api/ping').reply((config) => {
        expect(config.headers?.Authorization).toBeUndefined();
        return [200, { success: true }];
      });

      await api.get('/api/ping');
    });

    it('removes Content-Type for FormData bodies', () => {
      const interceptor = getRequestInterceptor();
      const formData = new FormData();
      formData.append('file', 'dummy-content');
      const config = {
        headers: { 'Content-Type': 'application/json' },
        data: formData,
      } as InternalAxiosRequestConfig;

      const result = interceptor(config) as InternalAxiosRequestConfig;
      expect(result.headers['Content-Type']).toBeUndefined();
    });

    it('preserves Content-Type for JSON bodies', async () => {
      mock.onPost('/api/entities').reply((config) => {
        expect(config.headers?.['Content-Type']).toBe('application/json');
        return [200, { success: true }];
      });

      await api.post('/api/entities', { name: 'test' });
    });
  });

  describe('paramsSerializer', () => {
    it('serializes array values as repeated unbracketed keys', () => {
      const params = buildQueryParams({ statusIds: [1, 2, 3] });
      const uri = api.getUri({ url: '/api/requests', params });
      expect(uri).toContain('statusIds=1&statusIds=2&statusIds=3');
      expect(uri).not.toContain('statusIds[]');
    });

    it('serializes scalar and array values together', () => {
      const params = buildQueryParams({
        onlyActive: true,
        search: 'billing',
        tags: ['a', 'b'],
      });
      const uri = api.getUri({ url: '/api/modules', params });
      expect(uri).toContain('onlyActive=true');
      expect(uri).toContain('search=billing');
      expect(uri).toContain('tags=a&tags=b');
    });
  });

  describe('response interceptor - refresh flow', () => {
    it('refreshes once after a 401 and retries the original request with the new access token', async () => {
      setAccessToken('expired-access-token');
      let secureRequestCount = 0;
      mock.onPost('/api/auth/refresh').reply((config) => {
        expect(config.data).toBeUndefined();
        return [200, { data: { accessToken: 'rotated-access-token', accessTokenExpiresAt: '2026-09-11T00:00:00Z' } }];
      });
      mock.onGet('/api/secure').reply((config) => {
        secureRequestCount += 1;
        if (secureRequestCount === 1) {
          expect(config.headers?.Authorization).toBe('Bearer expired-access-token');
          return [401];
        }
        expect(config.headers?.Authorization).toBe('Bearer rotated-access-token');
        return [200, { success: true }];
      });

      await expect(api.get('/api/secure')).resolves.toMatchObject({ data: { success: true } });
      expect(mock.history.post.filter((request) => request.url === '/api/auth/refresh')).toHaveLength(1);
      expect(getAccessToken()).toBe('rotated-access-token');
    });

    it('shares one refresh request between concurrent 401 responses', async () => {
      setAccessToken('expired-access-token');
      let refreshCount = 0;
      mock.onPost('/api/auth/refresh').reply(async () => {
        refreshCount += 1;
        await Promise.resolve();
        return [200, { data: { accessToken: 'rotated-access-token' } }];
      });
      for (const url of ['/api/secure-a', '/api/secure-b']) {
        let count = 0;
        mock.onGet(url).reply(() => {
          count += 1;
          return count === 1 ? [401] : [200, { success: true }];
        });
      }

      await expect(Promise.all([api.get('/api/secure-a'), api.get('/api/secure-b')])).resolves.toHaveLength(2);
      expect(refreshCount).toBe(1);
    });

    it('clears in-memory state and redirects when refresh fails', async () => {
      setAccessToken('expired-access-token');
      mock.onGet('/api/secure').reply(401);
      mock.onPost('/api/auth/refresh').reply(401);

      await expect(api.get('/api/secure')).rejects.toBeTruthy();
      expect(getAccessToken()).toBeNull();
      expect(window.location.href).toBe('/admin/login');
    });

    it('does not attempt to refresh a failed login request', async () => {
      mock.onPost('/api/auth/admin-login').reply(401);

      await expect(api.post('/api/auth/admin-login', { email: 'admin@example.test', password: 'bad' })).rejects.toBeTruthy();
      expect(mock.history.post.filter((request) => request.url === '/api/auth/refresh')).toHaveLength(0);
    });

    it('keeps a valid in-memory token after a non-401 error', async () => {
      setAccessToken('still-valid-token');
      mock.onGet('/api/secure').reply(500, { success: false });

      await expect(api.get('/api/secure')).rejects.toBeTruthy();
      expect(getAccessToken()).toBe('still-valid-token');
      expect(window.location.href).toBe('');
    });
  });

  it('updates performance metrics for successful requests', async () => {
    const { apiMetrics } = await import('@/lib/axios');
    const before = apiMetrics.requestCount;
    mock.onGet('/api/ping').reply(200, { success: true });

    await api.get('/api/ping');
    expect(apiMetrics.requestCount).toBe(before + 1);
  });
});
