import api from "@/lib/axios";

export interface FeaturePermissionPair {
  resource: string;  // "Projects"
  action: string;    // "ManageMembers"
}

export interface FeaturePackage {
  key: string;              // "ProjectManager"
  displayName: string;      // "Project Manager"
  description: string;
  permissions: FeaturePermissionPair[];
}

/**
 * Data source for the "select package" tab in the role management modal.
 * Packages are statically defined in the backend — not persisted, just a bulk toggle preset.
 */
export const AdminFeatureService = {
  getFeatures: async (): Promise<FeaturePackage[]> => {
    const response = await api.get("/api/admin/meta/features");
    return Array.isArray(response.data) ? response.data : [];
  },
};
