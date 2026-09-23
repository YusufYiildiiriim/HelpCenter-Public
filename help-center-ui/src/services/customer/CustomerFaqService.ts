import api from "@/lib/axios";

export interface CustomerFaq {
  title: string;
  description: string;
}

export const CustomerFaqService = {
  getAll: async (): Promise<CustomerFaq[]> => {
    const response = await api.get("/api/customer/faq/get-all-faq");
    return response.data;
  },
};
