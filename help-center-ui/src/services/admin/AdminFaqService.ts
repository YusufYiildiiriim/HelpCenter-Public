import api from "@/lib/axios";

export interface Faq {
  id?: number;
  publicId?: string;
  title: string;
  description: string;
  projectId?: number | null;
  projectName?: string | null;
  moduleId?: number | null;
  moduleName?: string | null;
  isActive: boolean;
  isPublic: boolean;
  rowVersion?: string;
}

export const AdminFaqService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string; projectId?: number | null; moduleId?: number | null }): Promise<Faq[]> => {
    const response = await api.get("/api/admin/faq/get-all", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string; projectId?: number | null; moduleId?: number | null }): Promise<{ items: Faq[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
    const response = await api.get("/api/admin/faq/get-all", { params });
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

  create: async (faq: Omit<Faq, "id">): Promise<boolean> => {
    const response = await api.post("/api/admin/faq/create-faq", faq);
    return response.data;
  },

  update: async (faq: Faq): Promise<boolean> => {
    const response = await api.put("/api/admin/faq/update-faq", faq);
    return response.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/faq/delete-faq/${id}`);
    return response.data;
  },
};
