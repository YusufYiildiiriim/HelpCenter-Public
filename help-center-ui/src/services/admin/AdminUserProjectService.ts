import api from "@/lib/axios";

export interface ProjectUserDto {
  id: number;
  userId: number;
  fullName: string;
  email: string;
}

export const AdminUserProjectService = {
  getByProjectId: async (projectId: number): Promise<ProjectUserDto[]> => {
    const response = await api.get(`/api/admin/projects/${projectId}/users`);
    return response.data;
  },

  assign: async (projectId: number, userId: number): Promise<boolean> => {
    const response = await api.post("/api/admin/projects/assign-user", { projectId, userId });
    return response.data;
  },

  remove: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/projects/remove-user/${id}`);
    return response.data;
  },
};
