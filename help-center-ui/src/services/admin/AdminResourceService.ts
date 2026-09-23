import api from "@/lib/axios";

export interface ActionDefinition {
  key: string;         // "Read", "Create", "ManageMembers", etc.
  displayName: string; // "View", "Manage Members", etc.
}

export interface ResourceDefinition {
  key: string;         // "Projects", "Modules", "Users", etc.
  displayName: string; // "Projects"
  actions: ActionDefinition[];
}

/**
 * Data source for the checkbox tree in the role management screen.
 * Only requires authenticated access (does not return sensitive data).
 * When a new action is added, backend `AppResourceDefinitions` is updated,
 * UI code does not change.
 */
export const AdminResourceService = {
  getDefinitions: async (): Promise<ResourceDefinition[]> => {
    const response = await api.get("/api/admin/meta/resources");
    return Array.isArray(response.data) ? response.data : [];
  },
};
