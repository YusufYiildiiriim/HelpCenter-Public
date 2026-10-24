import api from "@/lib/axios";

export interface CustomerRequest {
  id: string;
  publicId: string;
  ticketId: string;
  title: string;
  status: string;
  statusId: string;
  priority: number;
  priorityName: string;
  moduleName: string;
  requestSubjectName?: string;
  description?: string;
  createdAt: string;
  messageCount?: number;
  documentCount?: number;
  hasUnread?: boolean;
}

export interface TicketMessage {
  id: string;
  messageText: string;
  isAgent: boolean;
  createdAt: string;
  senderName: string;
  senderUserId: string;
  isRead: boolean;
  type: number;
  documents?: { id: string; fileName: string; path: string }[];
}

export const CustomerRequestService = {
  getMyRequests: async (): Promise<CustomerRequest[]> => {
    const response = await api.get("/api/customer/requests/my-requests");
    const payload = response.data as unknown;
    if (Array.isArray(payload)) return payload as CustomerRequest[];
    if (payload && typeof payload === "object" && Array.isArray((payload as { data?: unknown }).data)) {
      return (payload as { data: CustomerRequest[] }).data;
    }
    // A valid empty collection must render the empty state, not be treated as
    // a missing request or cause a downstream `.filter` crash.
    return [];
  },

  getById: async (publicId: string): Promise<CustomerRequest> => {
    const response = await api.get(`/api/customer/requests/get-by-id/${publicId}`);
    return response.data;
  },

  create: async (data: FormData): Promise<{ publicId: string; ticketId: string }> => {
    const response = await api.post("/api/customer/requests/create", data);
    return response.data;
  },

  getMessages: async (requestPublicId: string): Promise<TicketMessage[]> => {
    const response = await api.get(`/api/customer/requests/${requestPublicId}/messages`);
    return response.data;
  },

  sendMessage: async (data: { requestPublicId: string; messageText: string; files?: File[] }): Promise<boolean> => {
    const formData = new FormData();
    formData.append("requestPublicId", data.requestPublicId);
    formData.append("messageText", data.messageText);
    if (data.files) {
      data.files.forEach((file) => formData.append("Files", file));
    }
    const response = await api.post("/api/customer/requests/send-message", formData);
    return response.data;
  },

  markAsRead: async (requestPublicId: string): Promise<void> => {
    await api.post(`/api/customer/requests/${requestPublicId}/mark-read`);
  },

  close: async (data: { publicId: string; note?: string; rating: number }): Promise<boolean> => {
    const response = await api.post("/api/customer/requests/close", data);
    return response.data;
  },
};
