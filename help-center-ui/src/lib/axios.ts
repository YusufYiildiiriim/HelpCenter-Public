import axios from 'axios';
import type { AxiosResponse } from 'axios';
import type { ApiResponse } from './api/types';
import { clearAccessToken, getAccessToken, setAccessToken } from './authSession';
import { apiBaseUrl } from './env';

declare module 'axios' {
  export interface InternalAxiosRequestConfig {
    metadata?: { startTime: Date };
    skipAuthRefresh?: boolean;
    _retry?: boolean;
  }
  export interface AxiosRequestConfig {
    metadata?: { startTime: Date };
    skipAuthRefresh?: boolean;
    _retry?: boolean;
  }
}

/** Names `AxiosResponse<ApiResponse<T>>` in one place instead of writing `api.get<ApiAxiosResponse<Company[]>>(...)`. */
export type ApiAxiosResponse<T> = AxiosResponse<ApiResponse<T>>;

const api = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // For cookie transmission via CORS
  // Backend [FromQuery] List<T>/T[] binding expects `key=v1&key=v2`; axios's
  // default (`indexes: false`) produces `key[]=v1&key[]=v2` and ASP.NET Core
  // does not bind it. `indexes: null` produces repeated, bracketless keys.
  paramsSerializer: { indexes: null },
});

// Simple State for Performance Tracking
export const apiMetrics = {
  lastDuration: 0,
  averageDuration: 0,
  requestCount: 0,
  totalDuration: 0,
  slowRequests: [] as { url: string, duration: number, time: Date }[]
};

let refreshPromise: Promise<string> | null = null;

const AUTH_ENDPOINTS = new Set([
  '/api/auth/admin-login',
  '/api/auth/customer-login',
  '/api/auth/dashboard-login',
  '/api/auth/refresh',
  '/api/auth/logout',
]);

function isAuthEndpoint(url?: string): boolean {
  return AUTH_ENDPOINTS.has(url?.split('?')[0] ?? '');
}

function redirectToLogin(): void {
  if (typeof window === 'undefined') return;

  const currentPath = window.location.pathname;
  if (currentPath.includes('/login')) return;

  if (currentPath.startsWith('/admin')) {
    // eslint-disable-next-line @next/next/no-location-assign-relative-destination
    window.location.href = '/admin/login';
  } else if (currentPath.startsWith('/dashboard')) {
    // eslint-disable-next-line @next/next/no-location-assign-relative-destination
    window.location.href = '/dashboard/login';
  }
}

async function refreshAccessToken(): Promise<string> {
  if (!refreshPromise) {
    refreshPromise = api.post('/api/auth/refresh', undefined, { skipAuthRefresh: true })
      .then((response) => {
        const payload = response.data?.data ?? response.data;
        const token = payload?.accessToken ?? payload?.token;
        if (typeof token !== 'string' || token.length === 0) {
          throw new Error('Refresh response did not contain an access token.');
        }
        setAccessToken(token, payload?.accessTokenExpiresAt ?? payload?.expiresAt);
        return token;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

// Request Interceptor: start timing and attach the in-memory access token.
api.interceptors.request.use(
  (config) => {
    config.metadata = { startTime: new Date() };
    const token = getAccessToken();
      
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }

    if (config.data instanceof FormData) {
      delete config.headers['Content-Type'];
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor: compute performance, refresh once on 401, then retry.
api.interceptors.response.use(
  (response) => {
    const startTime = response.config.metadata?.startTime;
    if (startTime) {
      const duration = new Date().getTime() - startTime.getTime();
      
      // Update Metrics
      apiMetrics.lastDuration = duration;
      apiMetrics.requestCount++;
      apiMetrics.totalDuration += duration;
      apiMetrics.averageDuration = Math.round(apiMetrics.totalDuration / apiMetrics.requestCount);
      
      if (duration > 500) {
        apiMetrics.slowRequests.unshift({
          url: response.config.url || 'Unknown',
          duration,
          time: new Date()
        });
        apiMetrics.slowRequests = apiMetrics.slowRequests.slice(0, 5); // Last 5 slow queries
      }
    }

    return response;
  },
  async (error) => {
    if (error.config?.metadata?.startTime) {
      const startTime = error.config.metadata.startTime;
      const duration = new Date().getTime() - startTime.getTime();
      apiMetrics.lastDuration = duration;
    }

    const config = error.config;
    const shouldRefresh = error.response?.status === 401
      && config
      && !config._retry
      && !config.skipAuthRefresh
      && !isAuthEndpoint(config.url);

    if (shouldRefresh) {
      config._retry = true;
      try {
        const token = await refreshAccessToken();
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
        return api(config);
      } catch {
        clearAccessToken();
        redirectToLogin();
      }
    } else if (error.response?.status === 401 && (config?.skipAuthRefresh || isAuthEndpoint(config?.url))) {
      clearAccessToken();
      redirectToLogin();
    }
    return Promise.reject(error);
  }
);

export default api;
