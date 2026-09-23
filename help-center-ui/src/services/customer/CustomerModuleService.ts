import api from "@/lib/axios";

export interface CustomerModule {
  id: number;
  name: string;
}

export const CustomerModuleService = {
  getAll: async (): Promise<CustomerModule[]> => {
    const response = await api.get("/api/customer/modules/get-all-modules");
    return response.data;
  },
};
