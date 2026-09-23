import api from "@/lib/axios";

export interface PublicOrganizationInfo {
  organizationName: string;
  logoUrl?: string;
  footerText?: string;
}

export const PublicOrganizationService = {
  getOrganizationInfo: async (): Promise<PublicOrganizationInfo> => {
    const response = await api.get("/api/public/organization");
    return response.data;
  },
};
