import api from "@/lib/axios";

export interface PublicProject {
  id: string;
  publicId: string;
  name: string;
  description: string;
}

export const PublicProjectService = {
  getAll: async (): Promise<PublicProject[]> => {
    const response = await api.get("/api/public/projects");
    return Array.isArray(response.data) ? response.data : [];
  },
};
