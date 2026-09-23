import api from "@/lib/axios";

export interface Customer {
  id: number;
  publicId: string;
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  phoneNumber: string;
  companyId: number;
  companyName: string;
  requestCount: number;
  isActive: boolean;
}

export const AdminCustomerService = {
  getAll: async (companyPublicId: string): Promise<Customer[]> => {
    const response = await api.get("/api/admin/customers/get-all-customers", {
      params: { companyPublicId, pageSize: 1000 },
    });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  create: async (data: { firstName: string; lastName: string; email: string; phoneNumber: string; password?: string; companyId: number; isActive: boolean; username?: string }): Promise<boolean> => {
    const response = await api.post("/api/admin/customers/add-customer", data);
    return response.data;
  },

  delete: async (publicId: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/customers/${publicId}`);
    return response.data;
  },

  getById: async (publicId: string): Promise<Customer> => {
    const response = await api.get(`/api/admin/customers/${publicId}`);
    return response.data;
  },

  update: async (data: { publicId: string; firstName: string; lastName: string; email: string; username?: string; phoneNumber: string; isActive: boolean; companyId: number }): Promise<boolean> => {
    const response = await api.put("/api/admin/customers/update-customer", data);
    return response.data;
  },
};
