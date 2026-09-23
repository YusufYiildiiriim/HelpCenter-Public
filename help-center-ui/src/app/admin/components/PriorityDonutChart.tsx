"use client";

import React, { useMemo, useState } from "react";
import { motion } from "framer-motion";
import { Gauge } from "lucide-react";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { cn } from "@/lib/utils";
import { Card } from "@/components/ui/card";

interface PriorityDonutChartProps {
  requests: AdminRequest[];
  loading?: boolean;
}

const PRIORITY_ORDER = ["Critical", "High", "Medium", "Low"];

const PRIORITY_META: Record<string, { label: string; color: string; dot: string }> = {
  Critical: { label: "Kritik", color: "#ef4444", dot: "bg-red-500" },
  High: { label: "Yüksek", color: "#f59e0b", dot: "bg-amber-500" },
  Medium: { label: "Orta", color: "#3b82f6", dot: "bg-blue-500" },
  Low: { label: "Düşük", color: "#94a3b8", dot: "bg-slate-400" },
};

const SIZE = 168;
const STROKE = 17;
const RADIUS = (SIZE - STROKE) / 2;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;

export const PriorityDonutChart: React.FC<PriorityDonutChartProps> = ({ requests, loading }) => {
  const [hovered, setHovered] = useState<number | null>(null);

  const segments = useMemo(() => {
    const counts: Record<string, number> = {};
    requests.forEach((r) => {
      const key = r.priorityName || "Low";
      counts[key] = (counts[key] || 0) + 1;
    });

    const keys = [
      ...PRIORITY_ORDER.filter((k) => counts[k] > 0),
      ...Object.keys(counts).filter((k) => !PRIORITY_ORDER.includes(k)),
    ];

    const total = keys.reduce((sum, k) => sum + counts[k], 0);

    let cumulative = 0;
    const data = keys.map((key) => {
      const item = {
        key,
        count: counts[key],
        meta: PRIORITY_META[key] ?? { label: key, color: "#64748b", dot: "bg-slate-500" },
        offset: -cumulative,
        pct: total > 0 ? Math.round((counts[key] / total) * 100) : 0,
      };
      cumulative += counts[key];
      return item;
    });

    return { data, total };
  }, [requests]);

  const donutHovered = hovered !== null && hovered !== undefined;

  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -left-20 h-48 w-48 rounded-full bg-violet-600/10 blur-3xl" />

      <div className="relative z-10">
        <div className="flex items-center gap-3.5">
          <div className="relative shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-violet-500 to-fuchsia-600 rounded-2xl blur-md opacity-60" />
            <div className="relative flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-500 to-fuchsia-500 text-white shadow-lg shadow-violet-500/25">
              <Gauge size={20} />
            </div>
          </div>
          <div>
            <h2 className="text-lg font-black tracking-tight text-white">Öncelik Dağılımı</h2>
            <p className="mt-0.5 text-[10px] font-semibold uppercase tracking-[0.15em] text-slate-400">
              Aktif talepler
            </p>
          </div>
        </div>

        {loading ? (
          <div className="mt-6 h-[260px] animate-pulse rounded-2xl bg-slate-800/70" />
        ) : (
          <div className="mt-6 flex flex-col items-center gap-6 sm:flex-row sm:items-center">
            {/* Donut */}
            <div className="relative shrink-0" style={{ width: SIZE, height: SIZE }}>
              <svg width={SIZE} height={SIZE} className="-rotate-90">
                <circle
                  cx={SIZE / 2}
                  cy={SIZE / 2}
                  r={RADIUS}
                  fill="none"
                  strokeWidth={STROKE}
                  className="stroke-slate-800"
                />
                {segments.data.length === 0 && (
                  <circle
                    cx={SIZE / 2}
                    cy={SIZE / 2}
                    r={RADIUS}
                    fill="none"
                    strokeWidth={STROKE}
                    className="stroke-slate-800"
                  />
                )}
                {segments.data.map((seg, i) => {
                  const len = CIRCUMFERENCE * (seg.count / Math.max(segments.total, 1));
                  return (
                    <motion.circle
                      key={seg.key}
                      cx={SIZE / 2}
                      cy={SIZE / 2}
                      r={RADIUS}
                      fill="none"
                      stroke={seg.meta.color}
                      strokeWidth={STROKE}
                      strokeLinecap="round"
                      strokeDasharray={`${len} ${CIRCUMFERENCE}`}
                      strokeDashoffset={seg.offset}
                      initial={{ strokeDasharray: `0 ${CIRCUMFERENCE}`, opacity: 0 }}
                      animate={{
                        strokeDasharray: `${len} ${CIRCUMFERENCE}`,
                        opacity: donutHovered && hovered !== i ? 0.25 : 1,
                      }}
                      transition={{ duration: 1, delay: 0.25 + i * 0.12, ease: [0.22, 1, 0.36, 1] }}
                    />
                  );
                })}
              </svg>
              <div className="absolute inset-0 flex flex-col items-center justify-center">
                <motion.p
                  key={segments.total}
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                  className="font-mono text-3xl font-bold tabular-nums tracking-tight text-white"
                >
                  {segments.total}
                </motion.p>
                <p className="text-[8px] font-black uppercase tracking-[0.2em] text-slate-400">Talep</p>
              </div>
            </div>

            {/* Legend */}
            <div className="w-full min-w-0 flex-1 space-y-1.5">
              {segments.data.length === 0 && (
                <p className="text-xs font-medium text-slate-400">Henüz talep bulunmuyor.</p>
              )}
              {segments.data.map((seg, i) => (
                <button
                  key={seg.key}
                  onMouseEnter={() => setHovered(i)}
                  onMouseLeave={() => setHovered(null)}
                  className={cn(
                    "flex w-full items-center gap-2.5 rounded-xl px-2.5 py-1.5 text-left transition-all",
                    hovered === i ? "bg-slate-800" : "hover:bg-slate-800/60"
                  )}
                >
                  <span className={cn("h-2.5 w-2.5 shrink-0 rounded-full", seg.meta.dot)} />
                  <span className="min-w-0 flex-1 truncate text-xs font-bold text-slate-300">{seg.meta.label}</span>
                  <span className="text-[10px] font-black tabular-nums text-slate-400">{seg.pct}%</span>
                  <span className="w-8 text-right text-xs font-black tabular-nums text-white">{seg.count}</span>
                </button>
              ))}
            </div>
          </div>
        )}
      </div>
    </Card>
  );
};
