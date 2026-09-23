import api from "@/lib/axios";

export interface UserRole {
  roleId: number;
  roleName: string;
}

export interface User {
  id: number;
  email: string;
  name: string;
  lastName: string;
  username?: string;
  roles: string[];
  userRoles: UserRole[];
  isActive: boolean;
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  name: string;
  lastName: string;
  password: string;
  roleIds?: number[];
  isActive: boolean;
  username?: string;
}

export interface UpdateUserRequest {
  id: number;
  email: string;
  name: string;
  lastName: string;
  password?: string;
  roleIds?: number[];
  isActive: boolean;
  username?: string;
}

export const AdminUserService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<User[]> => {
    const response = await api.get("/api/admin/users", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<{ items: User[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
    const response = await api.get("/api/admin/users", { params });
    if (response.data && Array.isArray(response.data.items)) {
      return response.data;
    }
    const items = Array.isArray(response.data) ? response.data : [];
    return {
      items,
      totalCount: items.length,
      totalPages: 1,
      currentPage: 1,
      pageSize: items.length || 10,
    };
  },

  create: async (data: CreateUserRequest): Promise<boolean> => {
    const response = await api.post("/api/admin/users/add-user", data);
    return response.data;
  },

  update: async (id: number, data: UpdateUserRequest): Promise<boolean> => {
    const response = await api.put(`/api/admin/users/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/users/${id}`);
    return response.data;
  },

  updateRoles: async (id: number, roleIds: number[]): Promise<boolean> => {
    const response = await api.put(`/api/admin/users/${id}`, { roleIds });
    return response.data;
  },
};
