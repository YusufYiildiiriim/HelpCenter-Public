import api from "@/lib/axios";

export interface ModuleExpertDto {
  id: string;
  userId: string;
  fullName: string;
  email: string;
}

export const AdminModuleExpertService = {
  getByModuleId: async (moduleId: string): Promise<ModuleExpertDto[]> => {
    const response = await api.get(`/api/admin/module-experts/${moduleId}`);
    return response.data;
  },

  assign: async (moduleId: string, userId: string): Promise<boolean> => {
    const response = await api.post("/api/admin/module-experts/assign", { moduleId, userId });
    return response.data;
  },

  remove: async (id: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/module-experts/${id}`);
    return response.data;
  },
};
