"use client";

import React, { useMemo } from "react";
import { motion } from "framer-motion";
import { BarChart3, Layers, Building2, TrendingUp, Timer, Users, RotateCcw } from "lucide-react";
import { Card } from "@/components/ui/card";
import type { NameCount, DailyTrendPoint, AgentPerformance, ReopenStats } from "@/services/admin/AdminStatisticsService";
import { cn } from "@/lib/utils";

const BAR_COLORS = ["#6366f1", "#8b5cf6", "#ec4899", "#f59e0b", "#10b981", "#06b6d4", "#ef4444", "#84cc16", "#f97316", "#3b82f6"];

const CardHeader: React.FC<{ icon: React.ReactNode; title: string; subtitle?: string; gradient: string }> = ({ icon, title, subtitle, gradient }) => (
  <div className="flex items-center gap-3.5">
    <div className="relative shrink-0">
      <div className={cn("absolute -inset-1 rounded-2xl blur-md opacity-60", gradient)} />
      <div className={cn("relative flex h-11 w-11 items-center justify-center rounded-2xl text-white shadow-lg", gradient)}>
        {icon}
      </div>
    </div>
    <div>
      <h2 className="text-lg font-black tracking-tight text-white">{title}</h2>
      {subtitle && <p className="mt-0.5 text-[10px] font-semibold uppercase tracking-[0.15em] text-slate-400">{subtitle}</p>}
    </div>
  </div>
);

const BarRow: React.FC<{ name: string; count: number; max: number; color: string }> = ({ name, count, max, color }) => (
  <div className="space-y-1">
    <div className="flex justify-between text-xs">
      <span className="truncate font-bold text-slate-300">{name}</span>
      <span className="ml-2 font-black tabular-nums text-white">{count}</span>
    </div>
    <div className="h-2 overflow-hidden rounded-full bg-slate-800">
      <motion.div
        initial={{ width: 0 }}
        animate={{ width: `${max > 0 ? (count / max) * 100 : 0}%` }}
        transition={{ duration: 0.9, ease: [0.22, 1, 0.36, 1] }}
        style={{ background: color }}
        className="h-full rounded-full"
      />
    </div>
  </div>
);

const EmptyState: React.FC<{ msg?: string }> = ({ msg = "Veri bulunmuyor." }) => (
  <p className="py-4 text-center text-xs font-medium text-slate-400">{msg}</p>
);

// --- 1. Status Distribution ---
export const StatusDistributionCard: React.FC<{ data?: NameCount[] | null; loading?: boolean }> = ({ data, loading }) => {
  const max = useMemo(() => Math.max(1, ...(data ?? []).map(d => d.count)), [data]);
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -left-20 h-48 w-48 rounded-full bg-indigo-600/10 blur-3xl" />
      <div className="relative z-10 space-y-5">
        <CardHeader icon={<BarChart3 size={20} />} title="Statü Dağılımı" subtitle="Talep statülerinin kırılımı" gradient="bg-gradient-to-br from-indigo-500 to-blue-600" />
        {loading ? <div className="h-40 animate-pulse rounded-xl bg-slate-800/70" />
          : !data?.length ? <EmptyState />
          : <div className="space-y-2.5">{data.map((d, i) => <BarRow key={d.id} name={d.name} count={d.count} max={max} color={BAR_COLORS[i % BAR_COLORS.length]} />)}</div>}
      </div>
    </Card>
  );
};

// --- 2. Module Distribution ---
export const ModuleDistributionCard: React.FC<{ data?: NameCount[] | null; loading?: boolean }> = ({ data, loading }) => {
  const shown = (data ?? []).slice(0, 10);
  const max = useMemo(() => Math.max(1, ...shown.map(d => d.count)), [shown]);
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -right-20 h-48 w-48 rounded-full bg-teal-600/10 blur-3xl" />
      <div className="relative z-10 space-y-5">
        <CardHeader icon={<Layers size={20} />} title="Modül Dağılımı" subtitle="Modüle göre talep sayısı" gradient="bg-gradient-to-br from-teal-500 to-emerald-600" />
        {loading ? <div className="h-40 animate-pulse rounded-xl bg-slate-800/70" />
          : !shown.length ? <EmptyState />
          : <div className="space-y-2.5">{shown.map((d, i) => <BarRow key={d.id} name={d.name} count={d.count} max={max} color={BAR_COLORS[i % BAR_COLORS.length]} />)}</div>}
      </div>
    </Card>
  );
};

// --- 3. Company Top N ---
export const CompanyTopNCard: React.FC<{ data?: NameCount[] | null; loading?: boolean }> = ({ data, loading }) => {
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -bottom-20 -left-20 h-48 w-48 rounded-full bg-amber-600/10 blur-3xl" />
      <div className="relative z-10 space-y-5">
        <CardHeader icon={<Building2 size={20} />} title="En Aktif Firmalar" subtitle="Talep hacmine göre ilk 10" gradient="bg-gradient-to-br from-amber-500 to-orange-600" />
        {loading ? <div className="h-40 animate-pulse rounded-xl bg-slate-800/70" />
          : !data?.length ? <EmptyState />
          : <div className="space-y-2">
              {data.map((d, i) => (
                <div key={d.id} className="flex items-center gap-3 rounded-xl border border-slate-800 bg-slate-800/40 p-2.5">
                  <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-gradient-to-br from-amber-500/20 to-orange-500/20 text-[11px] font-black text-amber-400">{i + 1}</span>
                  <span className="min-w-0 flex-1 truncate text-xs font-bold text-slate-200">{d.name}</span>
                  <span className="text-sm font-black tabular-nums text-white">{d.count}</span>
                </div>
              ))}
            </div>}
      </div>
    </Card>
  );
};

// --- 4. Daily Trend (SVG line) ---
export const DailyTrendChart: React.FC<{ data?: DailyTrendPoint[] | null; loading?: boolean }> = ({ data, loading }) => {
  const points = data ?? [];
  const width = 640, height = 220, padding = 32;
  const maxY = Math.max(1, ...points.flatMap(p => [p.opened, p.closed]));
  const xStep = points.length > 1 ? (width - padding * 2) / (points.length - 1) : 0;
  const yScale = (v: number) => height - padding - (v / maxY) * (height - padding * 2);

  const buildPath = (key: "opened" | "closed") =>
    points.map((p, i) => `${i === 0 ? "M" : "L"} ${padding + i * xStep} ${yScale(p[key])}`).join(" ");

  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 right-1/4 h-48 w-48 rounded-full bg-emerald-600/10 blur-3xl" />
      <div className="relative z-10 space-y-4">
        <div className="flex items-center justify-between">
          <CardHeader icon={<TrendingUp size={20} />} title="Günlük Trend" subtitle="Son 30 gün açılan / kapanan" gradient="bg-gradient-to-br from-emerald-500 to-cyan-600" />
          <div className="flex items-center gap-4 text-[10px] font-bold uppercase tracking-wider">
            <span className="flex items-center gap-1.5 text-slate-300"><span className="h-2 w-2 rounded-full bg-cyan-400" /> Açılan</span>
            <span className="flex items-center gap-1.5 text-slate-300"><span className="h-2 w-2 rounded-full bg-emerald-400" /> Kapanan</span>
          </div>
        </div>
        {loading ? <div className="h-[220px] animate-pulse rounded-xl bg-slate-800/70" />
          : !points.length ? <EmptyState />
          : (
            <svg viewBox={`0 0 ${width} ${height}`} className="w-full">
              {[0, 0.25, 0.5, 0.75, 1].map(f => (
                <line key={f} x1={padding} x2={width - padding} y1={padding + f * (height - padding * 2)} y2={padding + f * (height - padding * 2)}
                  stroke="#1e293b" strokeDasharray="2 4" />
              ))}
              <motion.path initial={{ pathLength: 0 }} animate={{ pathLength: 1 }} transition={{ duration: 1.2 }}
                d={buildPath("opened")} fill="none" stroke="#22d3ee" strokeWidth={2.5} strokeLinejoin="round" strokeLinecap="round" />
              <motion.path initial={{ pathLength: 0 }} animate={{ pathLength: 1 }} transition={{ duration: 1.2, delay: 0.2 }}
                d={buildPath("closed")} fill="none" stroke="#34d399" strokeWidth={2.5} strokeLinejoin="round" strokeLinecap="round" />
            </svg>
          )}
      </div>
    </Card>
  );
};

// --- 5. Avg Resolution Minutes ---
export const AvgResolutionCard: React.FC<{ minutes?: number | null; loading?: boolean }> = ({ minutes, loading }) => {
  const m = minutes ?? 0;
  const label = m >= 1440 ? `${(m / 1440).toFixed(1)} gün` : m >= 60 ? `${(m / 60).toFixed(1)} sa` : `${m} dk`;
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -right-20 h-48 w-48 rounded-full bg-cyan-600/10 blur-3xl" />
      <div className="relative z-10 space-y-4">
        <CardHeader icon={<Timer size={20} />} title="Ort. Çözüm Süresi" subtitle="Tamamlanan talepler" gradient="bg-gradient-to-br from-cyan-500 to-sky-600" />
        {loading ? <div className="h-16 animate-pulse rounded-xl bg-slate-800/70" />
          : <div>
              <p className="font-mono text-4xl font-black tracking-tight text-white">{label}</p>
              <p className="mt-1 text-[10px] font-black uppercase tracking-[0.2em] text-slate-400">Ortalama süre</p>
            </div>}
      </div>
    </Card>
  );
};

// --- 6. Agent Performance ---
export const AgentPerformanceCard: React.FC<{ data?: AgentPerformance[] | null; loading?: boolean }> = ({ data, loading }) => {
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -bottom-20 -right-20 h-48 w-48 rounded-full bg-violet-600/10 blur-3xl" />
      <div className="relative z-10 space-y-5">
        <CardHeader icon={<Users size={20} />} title="Temsilci Performansı" subtitle="Atanan / Çözülen / Ort. süre" gradient="bg-gradient-to-br from-violet-500 to-purple-600" />
        {loading ? <div className="h-40 animate-pulse rounded-xl bg-slate-800/70" />
          : !data?.length ? <EmptyState />
          : (
            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="text-left text-[10px] font-black uppercase tracking-wider text-slate-500">
                    <th className="pb-2">Temsilci</th>
                    <th className="pb-2 text-right">Atanan</th>
                    <th className="pb-2 text-right">Çözülen</th>
                    <th className="pb-2 text-right">Ort. Süre</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800">
                  {data.map(a => {
                    const m = a.avgResolutionMinutes;
                    const label = m >= 1440 ? `${(m / 1440).toFixed(1)}g` : m >= 60 ? `${(m / 60).toFixed(1)}s` : `${m}dk`;
                    return (
                      <tr key={a.userId}>
                        <td className="py-2 font-bold text-slate-200">{a.fullName}</td>
                        <td className="py-2 text-right font-black tabular-nums text-slate-300">{a.assigned}</td>
                        <td className="py-2 text-right font-black tabular-nums text-emerald-400">{a.completed}</td>
                        <td className="py-2 text-right font-mono tabular-nums text-cyan-300">{label}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
      </div>
    </Card>
  );
};

// --- 7. Reopen Rate ---
export const ReopenRateCard: React.FC<{ stats?: ReopenStats | null; loading?: boolean }> = ({ stats, loading }) => {
  const pct = stats?.ratePercent ?? 0;
  const color = pct >= 20 ? "text-rose-400" : pct >= 10 ? "text-amber-400" : "text-emerald-400";
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -bottom-20 -left-20 h-48 w-48 rounded-full bg-rose-600/10 blur-3xl" />
      <div className="relative z-10 space-y-4">
        <CardHeader icon={<RotateCcw size={20} />} title="Yeniden Açılma Oranı" subtitle="Tamamlanmışlar arasında" gradient="bg-gradient-to-br from-rose-500 to-pink-600" />
        {loading ? <div className="h-24 animate-pulse rounded-xl bg-slate-800/70" />
          : (
            <div className="space-y-3">
              <p className={cn("font-mono text-5xl font-black tracking-tight tabular-nums", color)}>%{pct}</p>
              <div className="grid grid-cols-2 gap-3 text-xs">
                <div className="rounded-xl border border-slate-800 bg-slate-800/40 p-3">
                  <p className="text-[10px] font-black uppercase text-slate-500">Tamamlanan</p>
                  <p className="font-mono text-lg font-black text-white">{stats?.totalCompleted ?? 0}</p>
                </div>
                <div className="rounded-xl border border-slate-800 bg-slate-800/40 p-3">
                  <p className="text-[10px] font-black uppercase text-slate-500">Yeniden Açılan</p>
                  <p className="font-mono text-lg font-black text-white">{stats?.reopened ?? 0}</p>
                </div>
              </div>
            </div>
          )}
      </div>
    </Card>
  );
};
