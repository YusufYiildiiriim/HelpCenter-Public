"use client";

import React, { useRef, useState } from "react";
import { motion } from "framer-motion";
import {
  ClipboardList,
  AlertCircle,
  MessageSquare,
  Building2,
  Users,
  Loader2,
  ArrowUpRight,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { AdminStatistics } from "@/services/admin/AdminStatisticsService";
import { AnimatedCounter } from "./AnimatedCounter";

interface DashboardStatsProps {
  stats: AdminStatistics | null;
  loading: boolean;
  canWidget?: (key: string) => boolean;
}

interface TileConfig {
  key: keyof Pick<
    AdminStatistics,
    "activeRequests" | "totalMessages" | "totalCompanies" | "totalCustomers"
  >;
  label: string;
  icon: React.ElementType;
  text: string;
  chip: string;
  glow: string;
}

const tiles: TileConfig[] = [
  {
    key: "activeRequests",
    label: "Aktif Talepler",
    icon: AlertCircle,
    text: "text-amber-400",
    chip: "bg-amber-500/15",
    glow: "rgba(245,158,11,0.18)",
  },
  {
    key: "totalMessages",
    label: "Toplam Mesaj",
    icon: MessageSquare,
    text: "text-purple-400",
    chip: "bg-purple-500/15",
    glow: "rgba(168,85,247,0.18)",
  },
  {
    key: "totalCompanies",
    label: "Toplam Firma",
    icon: Building2,
    text: "text-blue-400",
    chip: "bg-blue-500/15",
    glow: "rgba(59,130,246,0.18)",
  },
  {
    key: "totalCustomers",
    label: "Toplam Müşteri",
    icon: Users,
    text: "text-rose-400",
    chip: "bg-rose-500/15",
    glow: "rgba(244,63,94,0.18)",
  },
];

const MiniRing: React.FC<{ percent: number; loading: boolean }> = ({ percent, loading }) => {
  const size = 48;
  const stroke = 5;
  const radius = (size - stroke) / 2;
  const circumference = 2 * Math.PI * radius;

  return (
    <div className="relative shrink-0" style={{ width: size, height: size }}>
      <svg width={size} height={size} className="-rotate-90">
        <defs>
          <linearGradient id="hc-mini-ring" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#10b981" />
            <stop offset="100%" stopColor="#34d399" />
          </linearGradient>
        </defs>
        <circle cx={size / 2} cy={size / 2} r={radius} strokeWidth={stroke} className="fill-none stroke-slate-700" />
        <motion.circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          strokeWidth={stroke}
          strokeLinecap="round"
          className="fill-none"
          stroke="url(#hc-mini-ring)"
          strokeDasharray={circumference}
          initial={{ strokeDashoffset: circumference }}
          animate={{ strokeDashoffset: circumference - (percent / 100) * circumference }}
          transition={{ duration: 1.4, ease: [0.22, 1, 0.36, 1], delay: 0.3 }}
        />
      </svg>
      {loading ? (
        <Loader2 size={14} className="absolute inset-0 m-auto animate-spin text-slate-400" />
      ) : (
        <span className="absolute inset-0 m-auto flex h-full w-full items-center justify-center font-mono text-[9px] font-bold tabular-nums text-white">
          {percent}%
        </span>
      )}
    </div>
  );
};

const StatTile: React.FC<{
  item: TileConfig;
  value: number;
  loading: boolean;
  index: number;
}> = ({ item, value, loading, index }) => {
  const ref = useRef<HTMLDivElement>(null);
  const [spot, setSpot] = useState({ x: 50, y: 50, opacity: 0 });

  const handleMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = ref.current?.getBoundingClientRect();
    if (!rect) return;
    setSpot({
      x: ((e.clientX - rect.left) / rect.width) * 100,
      y: ((e.clientY - rect.top) / rect.height) * 100,
      opacity: 1,
    });
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.15 + index * 0.07, duration: 0.5, ease: [0.22, 1, 0.36, 1] }}
      className="h-full"
    >
      <Card
        ref={ref}
        onMouseMove={handleMove}
        onMouseLeave={() => setSpot((s) => ({ ...s, opacity: 0 }))}
        className="group relative h-full overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-4 shadow-xl shadow-slate-950/30 transition-all duration-300 hover:-translate-y-0.5 hover:border-slate-700 hover:shadow-[0_8px_24px_-8px_rgba(15,23,42,0.6)]"
      >
        {/* Spotlight */}
        <div
          className="pointer-events-none absolute inset-0 transition-opacity duration-300"
          style={{
            background: `radial-gradient(160px circle at ${spot.x}% ${spot.y}%, ${item.glow}, transparent 70%)`,
            opacity: spot.opacity,
          }}
        />

        <div className="relative z-10">
          <div className="flex items-start justify-between">
            <div
              className={cn(
                "flex h-8 w-8 items-center justify-center rounded-lg transition-transform duration-300 group-hover:scale-110",
                item.chip
              )}
            >
              <item.icon size={15} className={item.text} />
            </div>
            <ArrowUpRight
              size={13}
              className={cn(
                "text-slate-500 opacity-0 transition-all duration-300 group-hover:opacity-100",
                item.text
              )}
            />
          </div>

          <p className="mt-3.5 text-[9px] font-semibold uppercase tracking-[0.15em] text-slate-400">
            {item.label}
          </p>

          {loading ? (
            <Loader2 size={18} className="mt-1 animate-spin text-slate-500" />
          ) : (
            <AnimatedCounter
              value={value || 0}
              delay={0.25 + index * 0.07}
              className="mt-0.5 block font-mono text-2xl font-bold tabular-nums tracking-tight text-white"
            />
          )}
        </div>
      </Card>
    </motion.div>
  );
};

export const DashboardStats: React.FC<DashboardStatsProps> = ({ stats, loading, canWidget }) => {
  const total = stats?.totalRequests ?? 0;
  const completed = stats?.completedRequests ?? 0;
  const completionRate = total > 0 ? Math.round((completed / total) * 100) : 0;

  // Filter tiles based on server-side allowed (non-null) metrics AND widget permissions
  const availableTiles = tiles.filter(
    (item) =>
      (canWidget ? canWidget(item.key) : true) &&
      stats?.[item.key] !== null &&
      stats?.[item.key] !== undefined
  );

  const showTotalRequests =
    (canWidget ? canWidget("totalRequests") : true) &&
    stats?.totalRequests !== null &&
    stats?.totalRequests !== undefined;

  const showCompletedRequests =
    (canWidget ? canWidget("completedRequests") : true) &&
    stats?.completedRequests !== null &&
    stats?.completedRequests !== undefined;

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
      {/* Featured: Toplam Talep (Only rendered if server allowed totalRequests) */}
      {showTotalRequests && (
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, ease: [0.22, 1, 0.36, 1] }}
          className="h-full"
        >
          <Card className="group relative h-full overflow-hidden rounded-2xl border-indigo-500/30 bg-slate-900/90 p-4 shadow-xl shadow-indigo-950/40 transition-all duration-300 hover:-translate-y-0.5 hover:shadow-[0_8px_24px_-8px_rgba(79,70,229,0.4)]">
            {/* Top accent line */}
            <div className="absolute inset-x-0 top-0 h-0.5 bg-gradient-to-r from-indigo-500 via-violet-500 to-indigo-500" />
            <div className="pointer-events-none absolute -top-10 -right-10 h-24 w-24 rounded-full bg-indigo-600/20 blur-2xl" />

            <div className="relative z-10">
              <div className="flex items-start justify-between">
                <div className="flex items-center gap-2.5">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-500/20 text-indigo-400 transition-transform duration-300 group-hover:scale-110">
                    <ClipboardList size={15} />
                  </div>
                  <Badge variant="outline" className="h-4 w-4 rounded-full border-transparent p-0">
                    <span className="relative flex h-1.5 w-1.5">
                      <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75" />
                      <span className="relative inline-flex h-1.5 w-1.5 rounded-full bg-emerald-500" />
                    </span>
                  </Badge>
                </div>
                <ArrowUpRight size={13} className="text-indigo-400 opacity-0 transition-opacity duration-300 group-hover:opacity-100" />
              </div>

              <p className="mt-3.5 text-[9px] font-semibold uppercase tracking-[0.15em] text-slate-400">
                Toplam Talep
              </p>

              <div className="mt-2.5 flex items-center justify-between gap-2">
                {loading ? (
                  <Loader2 size={18} className="animate-spin text-slate-500" />
                ) : (
                  <AnimatedCounter
                    value={total}
                    className="font-mono text-2xl font-bold tabular-nums tracking-tight text-white"
                  />
                )}
                <MiniRing percent={completionRate} loading={loading} />
              </div>

              <p className="mt-1.5 text-[10px] font-medium text-slate-400">
                {showCompletedRequests ? (
                  <>
                    <span className="font-semibold text-emerald-400">{completed.toLocaleString("tr-TR")}</span>{" "}
                    kapanan · <span className="font-semibold text-slate-300">{completionRate}%</span> tamamlandı
                  </>
                ) : (
                  "Talep hacmi görüntüleniyor"
                )}
              </p>
            </div>
          </Card>
        </motion.div>
      )}

      {availableTiles.map((item, i) => (
        <StatTile key={item.key} item={item} value={stats?.[item.key] ?? 0} loading={loading} index={i} />
      ))}
    </div>
  );
};
