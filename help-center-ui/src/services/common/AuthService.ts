import api from "@/lib/axios";
import { clearAccessToken, getAccessToken, setAccessToken } from "@/lib/authSession";
import type { components } from "@/api/schema";

export type ChangePasswordRequest = components["schemas"]["ChangePasswordCommand"];

// Strict type definitions — we do not use any
export interface AdminLoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token?: string;
  accessToken?: string;
  accessTokenExpiresAt?: string;
  companyPublicId?: string;
  customerPublicId?: string;
  role?: string;
  success?: boolean;
  message?: string;
  isPasswordChangeRequired?: boolean;
}

export interface ResourcePermission {
  resourceKey: string;
  actions: string[];
}

export type ModulePermission = ResourcePermission; // Alias for backward compatibility

export interface VerifyResponse {
  authenticated: boolean;
  role: string;
  username: string;
  userId: number;
  isPasswordChangeRequired: boolean;
  companyId?: string;
  permissions?: {
    modules: ResourcePermission[];
  };
}

export type UserPermissions = {
  modules: ResourcePermission[];
};

function storeLoginAccessToken(response: LoginResponse): void {
  const token = response.accessToken ?? response.token;
  if (!token) throw new Error("Login response did not contain an access token.");
  setAccessToken(token, response.accessTokenExpiresAt);
}

export interface MenuItemDto {
  id: number;
  label: string;
  icon: string;
  route: string;
  resourceKey: string;
  order: number;
  groupTitle?: string;
  parentId?: number;
}

export const AuthService = {
  adminLogin: async (data: AdminLoginRequest): Promise<LoginResponse> => {
    const response = await api.post("/api/auth/admin-login", data);
    storeLoginAccessToken(response.data);
    return response.data;
  },

  dashboardLogin: async (companyId: number) => {
    const response = await api.post("/api/auth/dashboard-login", companyId);
    storeLoginAccessToken(response.data);
    return response.data;
  },

  customerLogin: async (data: { email: string; password: string }) => {
    const response = await api.post("/api/auth/customer-login", data);
    storeLoginAccessToken(response.data);
    return response.data;
  },

  /**
   * Verifies token validity and role from the backend.
   * If JWT is invalid or 5 minutes have elapsed, returns 401 and axios interceptor clears the session.
   */
  verify: async (): Promise<VerifyResponse> => {
    const response = await api.get("/api/auth/verify");
    return response.data;
  },

  getMenuItems: async (): Promise<MenuItemDto[]> => {
    const response = await api.get("/api/admin/menu-items");
    return response.data;
  },

  logout: async () => {
    try {
      await api.post("/api/auth/logout", undefined, { skipAuthRefresh: true });
    } finally {
      clearAccessToken();
    }
  },

  changePassword: async (data: ChangePasswordRequest): Promise<boolean> => {
    const response = await api.post<boolean>("/api/auth/change-password", data);
    return response.data;
  },

  forgotPassword: async (emailOrUsername: string, isCustomer: boolean) => {
    const response = await api.post("/api/auth/forgot-password", { emailOrUsername, isCustomer });
    return response.data;
  },

  verifyResetCode: async (emailOrUsername: string, code: string, isCustomer: boolean) => {
    const response = await api.post("/api/auth/verify-reset-code", { emailOrUsername, code, isCustomer });
    return response.data;
  },

  resetPasswordWithCode: async (data: { emailOrUsername: string; code: string; newPassword: string; confirmPassword: string; isCustomer: boolean }) => {
    const response = await api.post("/api/auth/reset-password", data);
    return response.data;
  },

  isAuthenticated: (): boolean => {
    return getAccessToken() !== null;
  },
};
