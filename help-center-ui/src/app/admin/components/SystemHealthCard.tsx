"use client";

import React from "react";
import { motion } from "framer-motion";
import { Activity, ShieldCheck, AlertCircle, CheckCircle2, Timer, Zap } from "lucide-react";
import { cn } from "@/lib/utils";
import { Card } from "@/components/ui/card";

interface SystemHealthCardProps {
  metrics: {
    averageDuration: number;
    lastDuration: number;
    slowRequests: { url: string; duration: number; time: Date }[];
  };
}

export const SystemHealthCard: React.FC<SystemHealthCardProps> = ({ metrics }) => {
  const slowCount = metrics.slowRequests.length;

  let statusText = "Stabil";
  let statusDesc = "Her şey yolunda";
  let statusColor = "bg-emerald-500";
  let statusBg = "bg-emerald-500/15 text-emerald-400 border-emerald-500/30";

  if (slowCount > 0 && slowCount <= 5) {
    statusText = "İyi";
    statusDesc = "Performans iyi";
    statusColor = "bg-indigo-500";
    statusBg = "bg-indigo-500/15 text-indigo-400 border-indigo-500/30";
  } else if (slowCount > 5 && slowCount <= 7) {
    statusText = "Orta";
    statusDesc = "Performans orta";
    statusColor = "bg-amber-500";
    statusBg = "bg-amber-500/15 text-amber-400 border-amber-500/30";
  } else if (slowCount > 7) {
    statusText = "Kritik";
    statusDesc = "Bakım gerekli";
    statusColor = "bg-red-500";
    statusBg = "bg-red-500/15 text-red-400 border-red-500/30";
  }

  const avgPct = Math.min(((metrics.averageDuration || 1) / 500) * 100, 100);
  const lastPct = Math.min(((metrics.lastDuration || 1) / 1000) * 100, 100);

  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -bottom-20 -left-20 h-48 w-48 rounded-full bg-emerald-600/10 blur-3xl" />

      <div className="relative z-10">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3.5">
            <div className="relative shrink-0">
              <div className="absolute -inset-1 bg-gradient-to-r from-emerald-500 to-teal-600 rounded-2xl blur-md opacity-60" />
              <div className="relative flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-500 text-white shadow-lg shadow-emerald-500/25">
                <Activity size={20} />
              </div>
            </div>
            <div>
              <h2 className="text-lg font-black tracking-tight text-white">Sistem Sağlığı</h2>
              <p className="mt-0.5 text-[10px] font-semibold uppercase tracking-[0.15em] text-slate-400">
                Gerçek zamanlı
              </p>
            </div>
          </div>
          <span className="relative flex h-2 w-2">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75" />
            <span className={cn("relative inline-flex h-2 w-2 rounded-full", statusColor)} />
          </span>
        </div>

        <div className="mt-6 space-y-5">
          <div>
            <div className="flex items-center justify-between">
              <span className="flex items-center gap-1.5 text-[9px] font-black uppercase tracking-[0.18em] text-slate-400">
                <Timer size={11} />
                Ortalama yanıt
              </span>
              <span className="text-sm font-black tabular-nums text-white">
                {metrics.averageDuration}
                <span className="text-[10px] font-bold text-slate-400"> ms</span>
              </span>
            </div>
            <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-slate-800">
              <motion.div
                initial={{ width: 0 }}
                animate={{ width: `${avgPct}%` }}
                transition={{ duration: 1, ease: [0.22, 1, 0.36, 1] }}
                className={cn(
                  "h-full rounded-full",
                  metrics.averageDuration > 300 ? "bg-gradient-to-r from-amber-400 to-orange-400" : "bg-gradient-to-r from-emerald-400 to-teal-400"
                )}
              />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between">
              <span className="flex items-center gap-1.5 text-[9px] font-black uppercase tracking-[0.18em] text-slate-400">
                <Zap size={11} />
                Son işlem
              </span>
              <span className="text-sm font-black tabular-nums text-white">
                {metrics.lastDuration}
                <span className="text-[10px] font-bold text-slate-400"> ms</span>
              </span>
            </div>
            <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-slate-800">
              <motion.div
                initial={{ width: 0 }}
                animate={{ width: `${lastPct}%` }}
                transition={{ duration: 1, ease: [0.22, 1, 0.36, 1] }}
                className={cn(
                  "h-full rounded-full",
                  metrics.lastDuration > 500 ? "bg-gradient-to-r from-red-400 to-rose-400" : "bg-gradient-to-r from-indigo-400 to-blue-400"
                )}
              />
            </div>
          </div>
        </div>

        <div className={cn("mt-6 flex items-center gap-3 rounded-2xl border p-3.5", statusBg)}>
          <div className={cn("flex h-9 w-9 shrink-0 items-center justify-center rounded-xl text-white shadow-md", statusColor)}>
            {slowCount > 5 ? <AlertCircle size={16} /> : <CheckCircle2 size={16} />}
          </div>
          <div>
            <p className="text-xs font-black">
              {slowCount > 0 ? `${slowCount} yavaş sorgu` : "Sistem stabil"}
            </p>
            <p className="text-[9px] font-semibold uppercase tracking-widest opacity-70">{statusDesc}</p>
          </div>
          <div className="ml-auto flex items-center gap-1 text-[9px] font-black uppercase tracking-widest opacity-70">
            <ShieldCheck size={12} />
            {statusText}
          </div>
        </div>
      </div>
    </Card>
  );
};
