import api from "@/lib/axios";

export interface PublicGuide {
  publicId: string;
  title: string;
  description: string;
  module: string;
  youtubeUrl?: string | null;
  previousGuidePublicId?: string | null;
  documents?: { path: string; fileName: string }[];
}

export interface PaginatedPublicGuides {
  items: PublicGuide[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

type PublicGuideParams = {
  projectId?: number | null;
  module?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
};

function buildParams(params: PublicGuideParams): Record<string, string | number> {
  const query: Record<string, string | number> = {};
  if (params.projectId) query.projectId = params.projectId;
  if (params.module) query.module = params.module;
  if (params.search) query.search = params.search;
  if (params.pageNumber !== undefined) query.pageNumber = params.pageNumber;
  if (params.pageSize !== undefined) query.pageSize = params.pageSize;
  return query;
}

export const PublicGuideService = {
  getPaginated: async (params: PublicGuideParams): Promise<PaginatedPublicGuides> => {
    const response = await api.get("/api/public/guides", { params: buildParams(params) });
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

  // Keeps modal and mind-map navigation independent from grid pagination.
  getAllForChain: async (params: { projectId?: number | null; module?: string }): Promise<PublicGuide[]> => {
    const result = await PublicGuideService.getPaginated({ ...params, pageSize: 0 });
    return result.items;
  },
};
