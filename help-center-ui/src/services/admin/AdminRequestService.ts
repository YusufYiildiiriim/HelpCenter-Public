import api from "@/lib/axios";

export interface AdminMessageDocument {
  id: number;
  fileName: string;
  path: string;
}

export interface AdminTicketMessage {
  id: number;
  messageText: string;
  isAgent: boolean;
  createdAt: string;
  senderName: string;
  senderUserId: number;
  isRead: boolean;
  type: number;
  documents?: AdminMessageDocument[];
}

export interface AdminRequest {
  id: number;
  publicId: string;
  ticketId: string;
  title: string;
  description: string;
  status: string;
  statusId: number;
  customerName: string;
  companyName: string;
  moduleName: string;
  moduleId?: number;
  documentCount: number;
  messageCount: number;
  priority: number;
  priorityName: string;
  assignedUserName: string;
  currentExpertName: string;
  requestSubjectName: string;
  createdAt: string;
}

export interface AdminMessagesResponse {
  messages: AdminTicketMessage[];
  hasMore: boolean;
  totalCount: number;
}

export interface PaginatedAdminRequests {
  items: AdminRequest[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

type AdminRequestQueryParams = {
  companyPublicId?: string;
  status?: string;
  priority?: number;
  assignedUserId?: number;
  pageNumber?: number;
  pageSize?: number;
  search?: string;
};

function buildRequestQueryParams(params: AdminRequestQueryParams): URLSearchParams {
  const queryParams = new URLSearchParams();
  if (params.companyPublicId) queryParams.append("companyPublicId", params.companyPublicId);
  if (params.status && params.status !== "Hepsi") queryParams.append("status", params.status);
  if (params.priority !== undefined) queryParams.append("priority", params.priority.toString());
  if (params.assignedUserId !== undefined) queryParams.append("assignedUserId", params.assignedUserId.toString());
  if (params.search) queryParams.append("search", params.search);
  if (params.pageNumber !== undefined) queryParams.append("pageNumber", params.pageNumber.toString());
  if (params.pageSize !== undefined) queryParams.append("pageSize", params.pageSize.toString());
  return queryParams;
}

export const AdminRequestService = {
  getAll: async (params: AdminRequestQueryParams): Promise<AdminRequest[]> => {
    const queryParams = buildRequestQueryParams({ pageSize: 1000, ...params });
    const url = `/api/admin/requests/get-all${queryParams.toString() ? "?" + queryParams.toString() : ""}`;
    const response = await api.get(url);
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },

  getPaginated: async (params: AdminRequestQueryParams): Promise<PaginatedAdminRequests> => {
    const queryParams = buildRequestQueryParams(params);
    const url = `/api/admin/requests/get-all${queryParams.toString() ? "?" + queryParams.toString() : ""}`;
    const response = await api.get(url);
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

  getById: async (publicId: string): Promise<AdminRequest> => {
    const response = await api.get(`/api/admin/requests/get-by-id/${publicId}`);
    return response.data;
  },

  getAssignedById: async (publicId: string): Promise<AdminRequest> => {
    const response = await api.get(`/api/admin/requests/assigned/${publicId}`);
    return response.data;
  },

  getMessages: async (requestPublicId: string, pageSize: number = 20, beforeId?: number, assignedOnly = false): Promise<AdminMessagesResponse> => {
    const params = new URLSearchParams();
    params.append("pageSize", pageSize.toString());
    if (beforeId) params.append("beforeId", beforeId.toString());
    const path = assignedOnly
      ? `/api/admin/requests/assigned/${requestPublicId}/messages`
      : `/api/admin/requests/${requestPublicId}/messages`;
    const response = await api.get(`${path}?${params}`);
    return response.data;
  },

  sendMessage: async (data: { requestPublicId: string; senderUserId: number; messageText: string; type?: number; files?: File[] }): Promise<boolean> => {
    const formData = new FormData();
    formData.append("requestPublicId", data.requestPublicId);
    formData.append("senderUserId", data.senderUserId.toString());
    formData.append("messageText", data.messageText);
    if (data.type !== undefined) formData.append("type", data.type.toString());
    if (data.files) {
      data.files.forEach((file) => formData.append("Files", file));
    }
    const response = await api.post("/api/admin/requests/send-message", formData);
    return response.data;
  },

  consultExpert: async (data: { requestPublicId: string; expertId: number; agentUserId: number; note: string }): Promise<boolean> => {
    const response = await api.post("/api/admin/requests/consult-expert", data);
    return response.data;
  },

  closeTicket: async (id: number, note?: string, agentUserId?: number): Promise<boolean> => {
    const response = await api.post("/api/admin/requests/close", { id, note, agentUserId });
    return response.data;
  },

  markAsRead: async (requestPublicId: string): Promise<void> => {
    await api.post(`/api/admin/requests/${requestPublicId}/mark-read`);
  },

  getAssigned: async (params: { status?: string; priority?: number; pageNumber?: number; pageSize?: number }): Promise<PaginatedAdminRequests> => {
    const queryParams = new URLSearchParams();
    if (params.status && params.status !== "Hepsi") queryParams.append("status", params.status);
    if (params.priority !== undefined) queryParams.append("priority", params.priority.toString());
    if (params.pageNumber !== undefined) queryParams.append("pageNumber", params.pageNumber.toString());
    if (params.pageSize !== undefined) queryParams.append("pageSize", params.pageSize.toString());
    const response = await api.get(`/api/admin/requests/assigned?${queryParams.toString()}`);
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

  getCompanies: async (): Promise<{ publicId: string; name: string }[]> => {
    const response = await api.get("/api/admin/requests/companies");
    if (Array.isArray(response.data)) return response.data;
    if (response.data && Array.isArray(response.data.items)) return response.data.items;
    return [];
  },
};
