import api from "@/lib/axios";

export interface Module {
  id: number;
  publicId: string;
  name: string;
  description: string;
  usageCount: number;
  isActive: boolean;
  isLocked: boolean;
}

export const AdminModuleService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string; onlyActive?: boolean }): Promise<Module[]> => {
    const response = await api.get("/api/admin/modules/get-all", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return response.data?.data ?? response.data?.$values ?? [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string; onlyActive?: boolean }): Promise<{ items: Module[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
    const response = await api.get("/api/admin/modules/get-all", { params });
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

  create: async (name: string, description: string, isActive: boolean = true, expertUserIds: number[] = []): Promise<boolean> => {
    const response = await api.post("/api/admin/modules/add-module", { name, description, isActive, expertUserIds });
    return response.data;
  },

  // Note: Id is resolved on the server side from the publicId route parameter (see AdminModulesController.Update);
  // there is no need to send the raw numeric id in the body.
  update: async (publicId: string, name: string, description: string, isActive: boolean, expertUserIds: number[] = []): Promise<boolean> => {
    const response = await api.put(`/api/admin/modules/${publicId}`, { name, description, isActive, expertUserIds });
    return response.data;
  },

  delete: async (publicId: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/modules/${publicId}`);
    return response.data;
  },

  // Lookup: minimal user list for module expert selector.
  // Requires Modules.ManageExperts permission; does not require Users.Read.
  getAvailableExperts: async (): Promise<{ id: number; name: string }[]> => {
    const response = await api.get(`/api/admin/modules/available-experts`);
    return Array.isArray(response.data) ? response.data : [];
  },
};
