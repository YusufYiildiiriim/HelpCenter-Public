import api from "@/lib/axios";

export interface OrganizationInfoDto {
  id: number;
  organizationName: string;
  logoUrl?: string;
  phone?: string;
  email?: string;
  address?: string;
  website?: string;
  taxNumber?: string;
  taxOffice?: string;
  footerText?: string;
  rowVersion: string;
}

export const AdminOrganizationService = {
  getOrganizationInfo: async (): Promise<OrganizationInfoDto> => {
    const response = await api.get("/api/admin/organization");
    return response.data;
  },

  updateOrganizationInfo: async (formData: FormData): Promise<void> => {
    await api.put("/api/admin/organization", formData);
  },
};
