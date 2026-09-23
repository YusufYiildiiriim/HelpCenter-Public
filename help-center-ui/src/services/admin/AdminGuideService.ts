import api from "@/lib/axios";

export interface GuideDocument {
  path: string;
  fileName: string;
}

export interface Guide {
  id: number;
  title: string;
  description: string;
  module: string;
  moduleId?: number;
  previousGuideId?: number | null;
  youtubeUrl?: string | null;
  documents?: GuideDocument[];
  isActive: boolean;
  isPublic: boolean;
  rowVersion?: string;
}

export interface CreateGuideRequest {
  title: string;
  description: string;
  module: string;
  previousGuideId?: number | null;
  youtubeUrl?: string | null;
  isActive: boolean;
  isPublic: boolean;
  files?: File[];
}

export interface UpdateGuideRequest extends CreateGuideRequest {
  id: number;
}

const buildGuideFormData = (data: CreateGuideRequest | UpdateGuideRequest): FormData => {
  const formData = new FormData();
  Object.entries(data).forEach(([key, value]) => {
    if (key === "files" && value) {
      (value as File[]).forEach((file) => formData.append("Files", file));
    } else if (value !== null && value !== undefined) {
      formData.append(key, String(value));
    }
  });
  return formData;
};

export const AdminGuideService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string; module?: string }): Promise<Guide[]> => {
    const response = await api.get("/api/admin/guides/get-all", { params: { pageSize: 0, ...params } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string; module?: string }): Promise<{ items: Guide[]; totalCount: number; totalPages: number; currentPage: number; pageSize: number }> => {
    const response = await api.get("/api/admin/guides/get-all", { params });
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

  create: async (data: CreateGuideRequest): Promise<boolean> => {
    const formData = buildGuideFormData(data);
    const response = await api.post("/api/admin/guides/add", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    return response.data;
  },

  update: async (data: UpdateGuideRequest): Promise<boolean> => {
    const formData = buildGuideFormData(data);
    const response = await api.post("/api/admin/guides/update", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    return response.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/guides/delete/${id}`);
    return response.data;
  },
};
