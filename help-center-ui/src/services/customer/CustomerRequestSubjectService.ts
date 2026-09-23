import api from "@/lib/axios";

export interface CustomerRequestSubject {
  id: number;
  name: string;
}

export const CustomerRequestSubjectService = {
  getAll: async (): Promise<CustomerRequestSubject[]> => {
    const response = await api.get("/api/customer/subjects/get-all");
    return response.data;
  },
};
