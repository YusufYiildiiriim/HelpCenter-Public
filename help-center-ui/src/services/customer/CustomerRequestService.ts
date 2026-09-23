import api from "@/lib/axios";

export interface CustomerRequest {
  id: number;
  publicId: string;
  ticketId: string;
  title: string;
  status: string;
  statusId: number;
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
  id: number;
  messageText: string;
  isAgent: boolean;
  createdAt: string;
  senderName: string;
  senderUserId: number;
  isRead: boolean;
  type: number;
  documents?: { id: number; fileName: string; path: string }[];
}

export const CustomerRequestService = {
  getMyRequests: async (): Promise<CustomerRequest[]> => {
    const response = await api.get("/api/customer/requests/my-requests");
    return response.data;
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
