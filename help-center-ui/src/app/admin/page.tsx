"use client";

import React, { useEffect, useState, useMemo, useCallback } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import {
  Clock,
  ShieldCheck,
  RefreshCw,
  Filter,
} from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import type { AdminStatistics, AdminReports } from "@/services/admin/AdminStatisticsService";
import { AdminStatisticsService as StatisticsService } from "@/services/admin/AdminStatisticsService";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { AdminRequestService } from "@/services/admin/AdminRequestService";
import { apiMetrics } from "@/lib/axios";
import { usePageTitle } from "@/lib/usePageTitle";
import { useDashboardWidgets } from "@/lib/useDashboardWidgets";
import { cn } from "@/lib/utils";

// Modular Components
import { DashboardHeader } from "./components/DashboardHeader";
import { TicketFlow } from "./components/TicketFlow";
import { DashboardStats } from "./components/DashboardStats";
import { WeeklyAnalysisChart } from "./components/WeeklyAnalysisChart";
import { PriorityDonutChart } from "./components/PriorityDonutChart";
import { RecentRequestsTable } from "./components/RecentRequestsTable";
import { SystemHealthCard } from "./components/SystemHealthCard";
import { QuickActionCard } from "./components/QuickActionCard";
import {
  StatusDistributionCard,
  ModuleDistributionCard,
  CompanyTopNCard,
  DailyTrendChart,
  AvgResolutionCard,
  AgentPerformanceCard,
  ReopenRateCard,
} from "./components/ReportCards";

const getTimeAgo = (dateString: string) => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);
  if (diffInSeconds < 60) return "az önce";
  const diffInMinutes = Math.floor(diffInSeconds / 60);
  if (diffInMinutes < 60) return `${diffInMinutes}dk`;
  const diffInHours = Math.floor(diffInMinutes / 60);
  if (diffInHours < 24) return `${diffInHours}s`;
  return `${Math.floor(diffInHours / 24)}g`;
};

export default function AdminDashboard() {
  usePageTitle("Admin");
  const router = useRouter();
  const { userInfo } = useAuth();
  const { canRead } = usePermission(userInfo);
  const hasViewPerm = canRead("Dashboard");

  const [stats, setStats] = useState<AdminStatistics | null>(null);
  const [reports, setReports] = useState<AdminReports | null>(null);
  const { can: canWidget } = useDashboardWidgets(stats?.allowedFields);
  const [requests, setRequests] = useState<AdminRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [metrics, setMetrics] = useState(apiMetrics);
  const [selectedStatusFilter, setSelectedStatusFilter] = useState<string>("All");

  const loadData = useCallback(async () => {
    const [statsRes, requestsRes, reportsRes] = await Promise.allSettled([
      StatisticsService.getStatistics(),
      AdminRequestService.getAll({}),
      StatisticsService.getReports(),
    ]);
    if (statsRes.status === "rejected") console.error("Error fetching statistics:", statsRes.reason);
    if (requestsRes.status === "rejected") console.error("Error fetching requests:", requestsRes.reason);
    if (reportsRes.status === "rejected") console.error("Error fetching reports:", reportsRes.reason);
    return {
      statsData: statsRes.status === "fulfilled" ? statsRes.value : null,
      requestsData: requestsRes.status === "fulfilled" ? requestsRes.value : null,
      reportsData: reportsRes.status === "fulfilled" ? reportsRes.value : null,
    };
  }, []);

  const handleRefresh = async () => {
    try {
      setRefreshing(true);
      const { statsData, requestsData, reportsData } = await loadData();
      if (statsData) setStats(statsData);
      if (requestsData) setRequests(requestsData);
      if (reportsData) setReports(reportsData);
      setMetrics({ ...apiMetrics });
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    let isMounted = true;
    loadData()
      .then(({ statsData, requestsData, reportsData }) => {
        if (!isMounted) return;
        if (statsData) setStats(statsData);
        if (requestsData) setRequests(requestsData);
        if (reportsData) setReports(reportsData);
        setMetrics({ ...apiMetrics });
      })
      .finally(() => {
        if (isMounted) setLoading(false);
      });

    const interval = setInterval(() => setMetrics({ ...apiMetrics }), 3000);
    return () => {
      isMounted = false;
      clearInterval(interval);
    };
  }, [loadData]);

  const filteredRequests = useMemo(() => {
    if (selectedStatusFilter === "All") return requests;
    return requests.filter(r => r.status === selectedStatusFilter);
  }, [requests, selectedStatusFilter]);

  if (!hasViewPerm) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 p-8 text-center bg-slate-900/90 rounded-2xl border border-slate-800 shadow-xl">
        <div className="w-16 h-16 rounded-2xl bg-rose-500/15 border border-rose-500/30 flex items-center justify-center text-rose-400 shadow-inner">
          <ShieldCheck size={32} />
        </div>
        <h2 className="text-xl font-black tracking-tight text-white">Erişim Yetkisi Sınırlı</h2>
        <p className="text-sm text-slate-400 max-w-md">
          Dashboard istatistiklerini görüntülemek için gerekli `Dashboard` okuma iznine sahip değilsiniz. Lütfen yöneticinizle iletişime geçin.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-8 max-w-400 mx-auto pb-12 animate-in fade-in slide-in-from-bottom-3 duration-700">
      {/* Dynamic Top Bar Controls & Live SLA Gauge */}
      <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4 bg-slate-900/90 backdrop-blur-xl border border-slate-800 p-4 md:px-6 md:py-3.5 rounded-3xl text-white shadow-2xl shadow-slate-950/40">
        <div className="flex items-center gap-3">
          <div className="relative flex items-center justify-center w-3 h-3">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75" />
            <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-emerald-500" />
          </div>
          <div>
            <span className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400">Canlı Sistem Telemetrisi</span>
            <p className="text-xs font-bold text-slate-200 flex items-center gap-2">
              SLA Yanıt Başarısı <span className="text-emerald-400 font-mono font-black">%98.4</span>
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3 w-full md:w-auto justify-between md:justify-end border-t md:border-t-0 border-slate-800 pt-3 md:pt-0">
          <div className="flex items-center gap-2 font-mono text-xs text-slate-400 bg-slate-800/80 px-3 py-1.5 rounded-xl border border-slate-700/60">
            <Clock size={13} className="text-cyan-400" />
            <span>Ort. Yanıt: <strong className="text-white">18dk</strong></span>
          </div>

          <button
            onClick={handleRefresh}
            disabled={refreshing}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600/90 hover:bg-indigo-600 text-white rounded-xl text-xs font-black uppercase tracking-wider transition-all active:scale-95 disabled:opacity-50 shadow-lg shadow-indigo-600/20"
          >
            <RefreshCw size={13} className={cn(refreshing && "animate-spin")} />
            <span>{refreshing ? "Yenileniyor" : "Yenile"}</span>
          </button>
        </div>
      </div>

      {/* Hero Executive Header */}
      <motion.div
        initial={{ opacity: 0, y: -16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        <DashboardHeader />
      </motion.div>

      {/* Ticket Flow Live Lifecycle Bar */}
      {canWidget("ticketFlow") && (
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1, duration: 0.55 }}
        >
          <TicketFlow requests={requests} loading={loading} />
        </motion.div>
      )}

      {/* Top 5 Metric Cards */}
      <DashboardStats stats={stats} loading={loading} canWidget={canWidget} />

      {/* Main Grid Section */}
      <div className="grid grid-cols-1 items-start gap-8 lg:grid-cols-3">
        {/* Left 2 Columns */}
        <div className="space-y-8 lg:col-span-2">
          {/* Weekly Analysis Chart */}
          {canWidget("weeklyChart") && (
            <motion.div
              initial={{ opacity: 0, y: 16 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.15, duration: 0.5 }}
            >
              <WeeklyAnalysisChart requests={requests} loading={loading} />
            </motion.div>
          )}

          {/* Interactive Request Stream with Filter Tabs */}
          {canWidget("recentTable") && (
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.25, duration: 0.5 }}
            className="space-y-4"
          >
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 bg-slate-900/90 backdrop-blur-xl p-4 rounded-2xl border border-slate-800 shadow-xl shadow-slate-950/30">
              <div className="flex items-center gap-2">
                <Filter size={16} className="text-indigo-400" />
                <span className="text-xs font-black uppercase tracking-wider text-slate-200">Talep Akış Filtresi</span>
              </div>

              <div className="flex flex-wrap gap-1.5 w-full sm:w-auto">
                {[
                  { key: "All", label: "Tümü" },
                  { key: "Cevap Bekliyor", label: "Bekleyen" },
                  { key: "Teknik İncelemede", label: "İncelemede" },
                  { key: "Cevaplandı", label: "Cevaplanan" },
                  { key: "Tamamlandı", label: "Tamamlanan" }
                ].map(filter => (
                  <button
                    key={filter.key}
                    onClick={() => setSelectedStatusFilter(filter.key)}
                    className={cn(
                      "px-3 py-1.5 rounded-xl text-[10px] font-black uppercase tracking-wider transition-all",
                      selectedStatusFilter === filter.key
                        ? "bg-linear-to-r from-indigo-600 to-violet-600 text-white shadow-md shadow-indigo-600/30"
                        : "bg-white/10 text-slate-300 hover:bg-white/20 hover:text-white"
                    )}
                  >
                    {filter.label}
                  </button>
                ))}
              </div>
            </div>

            <RecentRequestsTable
              requests={filteredRequests.slice(0, 7)}
              onRowClick={(id) => router.push(`/admin/requests/${id}`)}
              getTimeAgo={getTimeAgo}
            />
          </motion.div>
          )}
        </div>

        {/* Right Sidebar Column */}
        <div className="space-y-8">
          {/* Priority Distribution Donut */}
          {canWidget("donutChart") && (
            <motion.div
              initial={{ opacity: 0, x: 16 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2, duration: 0.5 }}
            >
              <PriorityDonutChart requests={requests} loading={loading} />
            </motion.div>
          )}

          {/* Realtime Telemetry & Server Health */}
          {canWidget("systemHealth") && (
            <motion.div
              initial={{ opacity: 0, x: 16 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.3, duration: 0.5 }}
            >
              <SystemHealthCard metrics={metrics} />
            </motion.div>
          )}

          {/* Executive Quick Operations */}
          {canWidget("quickActions") && (
            <motion.div
              initial={{ opacity: 0, x: 16 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.4, duration: 0.5 }}
            >
              <QuickActionCard activeRequests={stats?.activeRequests || 0} />
            </motion.div>
          )}
        </div>
      </div>

      {/* --- Detaylı Raporlar --- */}
      {(canWidget("avgResolutionMinutes") || canWidget("reopenRate") || canWidget("dailyTrend")) && (
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {canWidget("avgResolutionMinutes") && (
            <AvgResolutionCard minutes={reports?.avgResolutionMinutes ?? 0} loading={loading} />
          )}
          {canWidget("reopenRate") && (
            <ReopenRateCard stats={reports?.reopenStats} loading={loading} />
          )}
          {canWidget("dailyTrend") && (
            <div className="lg:col-span-1">
              <DailyTrendChart data={reports?.dailyTrend} loading={loading} />
            </div>
          )}
        </div>
      )}

      {(canWidget("statusDistribution") || canWidget("moduleDistribution") || canWidget("companyTopN")) && (
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {canWidget("statusDistribution") && (
            <StatusDistributionCard data={reports?.statusDistribution} loading={loading} />
          )}
          {canWidget("moduleDistribution") && (
            <ModuleDistributionCard data={reports?.moduleDistribution} loading={loading} />
          )}
          {canWidget("companyTopN") && (
            <CompanyTopNCard data={reports?.companyTopN} loading={loading} />
          )}
        </div>
      )}

      {canWidget("agentPerformance") && (
        <AgentPerformanceCard data={reports?.agentPerformance} loading={loading} />
      )}
    </div>
  );
}
