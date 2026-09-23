import api from "@/lib/axios";

export interface RequestSubject {
  id: number;
  publicId: string;
  name: string;
  description: string;
  usageCount: number;
  isActive: boolean;
  isLocked: boolean;
}

export const AdminRequestSubjectService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<RequestSubject[]> => {
    const response = await api.get("/api/admin/subjects/get-all", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<{ items: RequestSubject[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
    const response = await api.get("/api/admin/subjects/get-all", { params });
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

  create: async (name: string, description: string, isActive: boolean = true): Promise<boolean> => {
    const response = await api.post("/api/admin/subjects/add", { name, description, isActive });
    return response.data;
  },

  update: async (id: number, name: string, description: string, isActive: boolean): Promise<boolean> => {
    const response = await api.put("/api/admin/subjects/update", { id, name, description, isActive });
    return response.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/subjects/delete/${id}`);
    return response.data;
  },
};
