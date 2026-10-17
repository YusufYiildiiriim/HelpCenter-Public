import api from "@/lib/axios";

export interface PublicModule {
  id: string;
  name: string;
  description: string;
}

export const PublicModuleService = {
  getAll: async (projectId?: string | null): Promise<PublicModule[]> => {
    const response = await api.get("/api/public/modules", {
      params: projectId ? { projectId } : undefined,
    });
    return Array.isArray(response.data) ? response.data : [];
  },
};
