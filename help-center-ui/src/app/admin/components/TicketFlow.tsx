"use client";

import React, { useMemo } from "react";
import { motion } from "framer-motion";
import { Radio } from "lucide-react";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { AnimatedCounter } from "./AnimatedCounter";
import { cn } from "@/lib/utils";
import { Card } from "@/components/ui/card";

interface TicketFlowProps {
  requests: AdminRequest[];
  loading?: boolean;
}

interface Stage {
  key: string;
  label: string;
  match: (status: string) => boolean;
  color: string;
  dot: string;
  track: string;
}

const STAGES: Stage[] = [
  {
    key: "waiting",
    label: "Cevap Bekliyor",
    match: (s) => s === "Cevap Bekliyor",
    color: "#f59e0b",
    dot: "bg-amber-400",
    track: "linear-gradient(90deg,#f59e0b,#fbbf24)",
  },
  {
    key: "answered",
    label: "Cevaplandı",
    match: (s) => ["Cevaplandi", "Cevaplandı"].includes(s),
    color: "#3b82f6",
    dot: "bg-blue-500",
    track: "linear-gradient(90deg,#3b82f6,#60a5fa)",
  },
  {
    key: "review",
    label: "Teknik İncelemede",
    match: (s) => ["Teknik Incelemede", "Teknik İncelemede"].includes(s),
    color: "#6366f1",
    dot: "bg-indigo-500",
    track: "linear-gradient(90deg,#6366f1,#818cf8)",
  },
  {
    key: "done",
    label: "Tamamlandı",
    match: (s) => ["Tamamlandi", "Tamamlandı"].includes(s),
    color: "#10b981",
    dot: "bg-emerald-500",
    track: "linear-gradient(90deg,#10b981,#34d399)",
  },
];

export const TicketFlow: React.FC<TicketFlowProps> = ({ requests, loading }) => {
  const { stages, total } = useMemo(() => {
    const computed = STAGES.map((stage) => ({
      ...stage,
      count: requests.filter((r) => stage.match(r.status)).length,
    }));
    return { stages: computed, total: requests.length };
  }, [requests]);

  const maxStage = useMemo(() => {
    const max = stages.reduce((a, b) => (b.count > a.count ? b : a), stages[0]);
    return max.count > 0 ? max : null;
  }, [stages]);

  const positions = useMemo(() => {
    const result = stages.reduce<{
      acc: number;
      list: Array<Stage & { count: number; pct: number; center: number }>;
    }>(
      (state, s) => {
        const pct = total > 0 ? (s.count / total) * 100 : 0;
        const center = state.acc + pct / 2;
        return {
          acc: state.acc + pct,
          list: [...state.list, { ...s, pct, center }],
        };
      },
      { acc: 0, list: [] }
    );
    return result.list;
  }, [stages, total]);

  return (
    <Card className="relative overflow-hidden rounded-3xl border-transparent bg-[#0B1220] p-6 text-white shadow-xl shadow-slate-900/20 md:p-7">
      {/* Ambient */}
      <div
        className="pointer-events-none absolute inset-0 opacity-[0.06]"
        style={{
          backgroundImage: "radial-gradient(circle, rgba(255,255,255,0.6) 1px, transparent 1px)",
          backgroundSize: "20px 20px",
        }}
      />
      <div className="pointer-events-none absolute -right-24 -top-28 h-72 w-72 rounded-full bg-indigo-600/20 blur-3xl" />
      <div className="pointer-events-none absolute inset-x-0 top-0 h-px bg-linear-to-r from-transparent via-white/20 to-transparent" />

      <div className="relative z-10">
        {/* Top row: live + total */}
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-2">
            <span className="relative flex h-2 w-2">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75" />
              <span className="relative inline-flex h-2 w-2 rounded-full bg-emerald-400" />
            </span>
            <span className="font-mono text-[10px] font-bold uppercase tracking-[0.2em] text-slate-500">
              Canlı · son güncelleme az önce
            </span>
          </div>
          <div className="flex items-baseline gap-2">
            <span className="text-[9px] font-bold uppercase tracking-[0.25em] text-slate-500">Toplam</span>
            {loading ? (
              <span className="h-7 w-14 animate-pulse rounded-md bg-white/10" />
            ) : (
              <AnimatedCounter
                value={total}
                className="font-mono text-3xl font-bold tabular-nums tracking-tighter text-white"
              />
            )}
          </div>
        </div>

        {/* Title */}
        <h2 className="mt-5 text-sm font-semibold tracking-tight text-slate-300">
          Talep Akışı <span className="text-slate-600">· yaşam döngüsündeki dağılım</span>
        </h2>

        {/* Flow band */}
        <div className="mt-4">
          <div className="relative">
            <div className="flex h-3 w-full overflow-hidden rounded-full bg-white/[0.07]">
              {positions.map((s, i) =>
                s.pct > 0 ? (
                  <motion.div
                    key={s.key}
                    initial={{ width: 0 }}
                    animate={{ width: `${s.pct}%` }}
                    transition={{ delay: 0.2 + i * 0.1, duration: 0.9, ease: [0.22, 1, 0.36, 1] }}
                    className={cn(
                      "h-full",
                      maxStage?.key === s.key && "relative"
                    )}
                    style={{ background: s.track }}
                  />
                ) : null
              )}
            </div>

            {/* Bottleneck marker */}
            {maxStage && !loading && (
              <motion.div
                initial={{ opacity: 0, y: -4 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.9, duration: 0.4 }}
                className="absolute -top-7 -translate-x-1/2"
                style={{
                  left: `${positions.find((p) => p.key === maxStage.key)!.center}%`,
                }}
              >
                <span className="whitespace-nowrap rounded-full border border-white/10 bg-white/8 px-2 py-0.5 font-mono text-[9px] font-bold uppercase tracking-[0.15em] text-slate-300 backdrop-blur-sm">
                  ● Yoğunluk burada
                </span>
              </motion.div>
            )}
          </div>

          {/* Legend */}
          <div className="mt-4 grid grid-cols-2 gap-x-4 gap-y-3 sm:grid-cols-4">
            {positions.map((s, i) => (
              <motion.div
                key={s.key}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.3 + i * 0.1, duration: 0.5 }}
                className="min-w-0"
              >
                <div className="flex items-center gap-1.5">
                  <span className={cn("h-1.5 w-1.5 shrink-0 rounded-full", s.dot)} />
                  <span className="truncate text-[9px] font-bold uppercase tracking-[0.12em] text-slate-500">
                    {s.label}
                  </span>
                </div>
                {loading ? (
                  <div className="mt-1 h-5 w-10 animate-pulse rounded bg-white/8" />
                ) : (
                  <p className="mt-1 font-mono text-xl font-bold tabular-nums tracking-tight text-white">
                    {s.count.toLocaleString("tr-TR")}
                  </p>
                )}
                <p className="text-[9px] font-semibold text-slate-600">
                  {total > 0 ? Math.round(s.pct) : 0}%
                </p>
              </motion.div>
            ))}
          </div>
        </div>

        {/* Footer meta */}
        <div className="mt-5 flex items-center gap-1.5 border-t border-white/6 pt-3 text-[9px] font-semibold uppercase tracking-[0.2em] text-slate-600">
          <Radio size={11} className="animate-pulse" />
          Kuyruk durumu taleplerin gerçek zamanlı durumuna göre hesaplanır
        </div>
      </div>
    </Card>
  );
};
