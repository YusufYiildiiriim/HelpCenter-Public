import api from "@/lib/axios";

export interface PublicFaq {
  publicId: string;
  title: string;
  description: string;
  projectId?: string | null;
  projectName?: string | null;
  moduleId?: string | null;
  moduleName?: string | null;
}

export interface PaginatedPublicFaqs {
  items: PublicFaq[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

type PublicFaqParams = {
  projectId?: string | null;
  moduleId?: string | null;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
};

function buildParams(params: PublicFaqParams): Record<string, string | number> {
  const query: Record<string, string | number> = {};
  if (params.projectId) query.projectId = params.projectId;
  if (params.moduleId) query.moduleId = params.moduleId;
  if (params.search) query.search = params.search;
  if (params.pageNumber !== undefined) query.pageNumber = params.pageNumber;
  if (params.pageSize !== undefined) query.pageSize = params.pageSize;
  return query;
}

export const PublicFaqService = {
  getPaginated: async (params: PublicFaqParams): Promise<PaginatedPublicFaqs> => {
    const response = await api.get("/api/public/faq", { params: buildParams(params) });
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
};
