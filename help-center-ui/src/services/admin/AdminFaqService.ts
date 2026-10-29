import api from "@/lib/axios";

export interface Faq {
  id?: string;
  publicId?: string;
  title: string;
  description: string;
  projectId?: string | null;
  projectName?: string | null;
  moduleId?: string | null;
  moduleName?: string | null;
  isActive: boolean;
  isPublic: boolean;
  rowVersion?: string;
}

export type UpdateFaqRequest = Faq & { rowVersion: string };

export const AdminFaqService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string; projectId?: string | null; moduleId?: string | null }): Promise<Faq[]> => {
    const response = await api.get("/api/admin/faq/get-all", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string; projectId?: string | null; moduleId?: string | null }): Promise<{ items: Faq[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
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

  update: async (faq: UpdateFaqRequest): Promise<boolean> => {
    const response = await api.put("/api/admin/faq/update-faq", faq);
    return response.data;
  },

  delete: async (id: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/faq/delete-faq/${id}`);
    return response.data;
  },
};
