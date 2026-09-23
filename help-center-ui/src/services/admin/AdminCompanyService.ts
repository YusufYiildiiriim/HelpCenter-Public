import api from "@/lib/axios";

export interface Company {
  id: number;
  publicId: string;
  name: string;
  address: string;
  phone: string;
  mail: string;
  contactPersonName: string;
  contactPersonSurname: string;
  contactPersonEmail: string;
  contactPersonPhone: string;
  contactPersonUsername?: string;
  isDemoActive: boolean;
  branchCount: number;
  moduleIds: number[];
  previousSystem?: string;
  password?: string;
  projectId?: number | null;
  projectName?: string;
  rowVersion?: string;
}

export interface RequestCompany {
  publicId: string;
  name: string;
}

// Matches backend's UpdateCompanyRequest exactly (AdminCompaniesController.Update).
// Id is resolved on server-side from route parameter (publicId); the full entity is not sent.
export type UpdateCompanyRequest = Omit<Company, "id" | "publicId" | "projectName">;

export interface PaginatedCompanies {
  items: Company[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export const AdminCompanyService = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<Company[]> => {
    const response = await api.get("/api/admin/companies/get-all-companies", {
      params: { pageSize: 0, ...params },
    });
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<PaginatedCompanies> => {
    const response = await api.get("/api/admin/companies/get-all-companies", { params });
    if (response.data && Array.isArray(response.data.items)) {
      return response.data;
    }
    const items = Array.isArray(response.data) ? response.data : [];
    return {
      items,
      totalCount: items.length,
      totalPages: 1,
      currentPage: 1,
      pageSize: items.length || 10,
    };
  },

  getRequestCompanies: async (): Promise<RequestCompany[]> => {
    const response = await api.get("/api/admin/requests/companies");
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  create: async (data: Omit<Company, "id" | "publicId">): Promise<boolean> => {
    const response = await api.post("/api/admin/companies/add-company", data);
    return response.data;
  },

  update: async (publicId: string, data: UpdateCompanyRequest): Promise<boolean> => {
    const response = await api.put(`/api/admin/companies/${publicId}`, data);
    return response.data;
  },

  delete: async (publicId: string): Promise<boolean> => {
    const response = await api.delete(`/api/admin/companies/${publicId}`);
    return response.data;
  },
};
