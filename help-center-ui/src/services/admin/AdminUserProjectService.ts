import api from "@/lib/axios";

export interface ProjectUserDto {
  id: string;
  userId: string;
  fullName: string;
  email: string;
}

export const AdminUserProjectService = {
  getByProjectId: async (projectId: string): Promise<ProjectUserDto[]> => {
    const response = await api.get(`/api/admin/projects/${projectId}/users`);
    return response.data;
  },

  assign: async (projectId: string, userId: string): Promise<boolean> => {
    const response = await api.post("/api/admin/projects/assign-user", { projectId, userId });
    return response.data;
  },

  remove: async (id: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/projects/remove-user/${id}`);
    return response.data;
  },
};
