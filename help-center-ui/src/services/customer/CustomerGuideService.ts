import api from "@/lib/axios";

export interface CustomerGuide {
  publicId: string;
  title: string;
  description: string;
  module: string;
  youtubeUrl?: string | null;
  previousGuidePublicId?: string | null;
  documents?: { path: string; fileName: string }[];
}

export const CustomerGuideService = {
  getAll: async (): Promise<CustomerGuide[]> => {
    const response = await api.get("/api/customer/guides/get-all");
    return response.data;
  },
};
