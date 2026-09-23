import api from "@/lib/axios";

export interface AdminStatistics {
  totalRequests?: number | null;
  totalMessages?: number | null;
  totalCustomers?: number | null;
  totalCompanies?: number | null;
  completedRequests?: number | null;
  activeRequests?: number | null;
  allowedFields?: string[] | null;
}

export interface NameCount {
  id: number;
  name: string;
  count: number;
}

export interface DailyTrendPoint {
  date: string;
  opened: number;
  closed: number;
}

export interface AgentPerformance {
  userId: number;
  fullName: string;
  assigned: number;
  completed: number;
  avgResolutionMinutes: number;
}

export interface ReopenStats {
  totalCompleted: number;
  reopened: number;
  ratePercent: number;
}

export interface AdminReports {
  statusDistribution?: NameCount[] | null;
  moduleDistribution?: NameCount[] | null;
  companyTopN?: NameCount[] | null;
  dailyTrend?: DailyTrendPoint[] | null;
  avgResolutionMinutes?: number | null;
  agentPerformance?: AgentPerformance[] | null;
  reopenStats?: ReopenStats | null;
  allowedFields?: string[] | null;
}

export const AdminStatisticsService = {
  getStatistics: async (): Promise<AdminStatistics> => {
    const response = await api.get("/api/admin/statistics");
    return response.data;
  },
  getReports: async (): Promise<AdminReports> => {
    const response = await api.get("/api/admin/reports");
    return response.data;
  },
};
