import api from "@/lib/axios";

export interface PublicRequestSubject {
  id: number;
  name: string;
}

export const PublicRequestSubjectService = {
  getAll: async (): Promise<PublicRequestSubject[]> => {
    const response = await api.get("/api/public/subjects");
    return response.data;
  },
};
