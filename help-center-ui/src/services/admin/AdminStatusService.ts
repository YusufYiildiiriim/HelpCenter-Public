import api from "@/lib/axios";

export interface RequestStatus {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
}

export const AdminStatusService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<RequestStatus[]> => {
    const response = await api.get("/api/admin/statuses/get-all", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<{ items: RequestStatus[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
    const response = await api.get("/api/admin/statuses/get-all", { params });
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
};
