import api from "@/lib/axios";

export type Base64String = string;

export interface Role {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  rowVersion: Base64String;
  hasUsers: boolean;
  permissionCount: number;
}

export interface RolePermissionDto {
  id?: number;
  roleId: number;
  resourceKey: string;
  /**
   * Source of truth: action keys granted for this resource.
   * E.g.: ["Read", "Create", "ManageMembers"].
   * Empty list → no permission (soft-deleted in backend).
   */
  actions: string[];
  allowedFieldsJson?: string | null;
}

export const AdminRoleService = {
  getAll: async (): Promise<Role[]> => {
    const response = await api.get("/api/admin/role/get-all", { params: { pageSize: 0 } });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  update: async (role: Role): Promise<boolean> => {
    const response = await api.post("/api/admin/role/update", role);
    return response.data;
  },

  getPermissions: async (roleId: number): Promise<RolePermissionDto[]> => {
    const response = await api.get(`/api/admin/role/get-permissions/${roleId}`);
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    if (response.data && Array.isArray(response.data.data)) return response.data.data;
    return [];
  },

  updatePermissions: async (roleId: number, permissions: RolePermissionDto[]): Promise<boolean> => {
    const response = await api.post("/api/admin/role/update-permissions", { roleId, permissions });
    return response.data;
  },

  create: async (data: { name: string; description?: string }): Promise<Role> => {
    const response = await api.post("/api/admin/role/create", data);
    return response.data;
  },

  delete: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/admin/role/delete/${id}`);
    return response.data;
  },
};
