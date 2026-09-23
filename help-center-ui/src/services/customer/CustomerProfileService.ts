import api from "@/lib/axios";

export interface CustomerProfile {
  publicId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  companyName: string;
}

export const CustomerProfileService = {
  getMyProfile: async (): Promise<CustomerProfile> => {
    const response = await api.get("/api/customer/profile/get-my-profile");
    return response.data;
  },

  updateMyProfile: async (data: { firstName: string; lastName: string; phoneNumber: string }): Promise<boolean> => {
    const response = await api.put("/api/customer/profile/update-my-profile", data);
    return response.data;
  },
};
