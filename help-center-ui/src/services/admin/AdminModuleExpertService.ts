import api from "@/lib/axios";

export interface ModuleExpertDto {
  id: number;
  userId: number;
  fullName: string;
  email: string;
}

export const AdminModuleExpertService = {
  getByModuleId: async (moduleId: number): Promise<ModuleExpertDto[]> => {
    const response = await api.get(`/api/admin/module-experts/${moduleId}`);
    return response.data;
  },

  assign: async (moduleId: number, userId: number): Promise<boolean> => {
    const response = await api.post("/api/admin/module-experts/assign", { moduleId, userId });
    return response.data;
  },

  remove: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/module-experts/${id}`);
    return response.data;
  },
};
