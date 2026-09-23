import api from "@/lib/axios";

export interface LookupItem {
  id: number;
  name: string;
}

export interface Project {
  id: number;
  publicId: string;
  name: string;
  description: string;
  isActive: boolean;
  companyCount: number;
  userCount: number;
  moduleIds: number[];
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export const AdminProjectService = {
  getAll: async (paramsOrOnlyActive?: boolean | { onlyActive?: boolean; search?: string }): Promise<Project[]> => {
    let params: Record<string, unknown> = { pageSize: 0 };
    if (typeof paramsOrOnlyActive === "boolean") {
      params.onlyActive = paramsOrOnlyActive;
    } else if (paramsOrOnlyActive && typeof paramsOrOnlyActive === "object") {
      params = { ...params, ...paramsOrOnlyActive };
    }
    const response = await api.get("/api/admin/projects/get-all", { params });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string; onlyActive?: boolean }): Promise<PaginatedResult<Project>> => {
    const response = await api.get("/api/admin/projects/get-all", { params });
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

  create: async (name: string, description: string, isActive: boolean = true, userIds: number[] = [], moduleIds: number[] = []): Promise<boolean> => {
    const response = await api.post("/api/admin/projects/add-project", { name, description, isActive, userIds, moduleIds });
    return response.data;
  },

  // Note: Id is resolved on the server side from the publicId route parameter (see AdminProjectsController.Update);
  // there is no need to send the raw numeric id in the body.
  update: async (publicId: string, name: string, description: string, isActive: boolean, userIds: number[] = [], moduleIds: number[] = []): Promise<boolean> => {
    const response = await api.put(`/api/admin/projects/${publicId}`, { name, description, isActive, userIds, moduleIds });
    return response.data;
  },

  delete: async (publicId: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/projects/${publicId}`);
    return response.data;
  },

  // Lookup: minimal user list for project member selector.
  // Requires Projects.ManageMembers permission; does not require Users.Read.
  getAvailableUsers: async (): Promise<LookupItem[]> => {
    const response = await api.get(`/api/admin/projects/available-users`);
    return Array.isArray(response.data) ? response.data : [];
  },

  // Lookup: minimal module list for project module selector.
  // Requires Projects.ManageModules permission.
  getAvailableModules: async (): Promise<LookupItem[]> => {
    const response = await api.get(`/api/admin/projects/available-modules`);
    return Array.isArray(response.data) ? response.data : [];
  },
};
