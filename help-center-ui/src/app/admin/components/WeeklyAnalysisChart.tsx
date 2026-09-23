"use client";

import React, { useMemo, useState } from "react";
import { motion } from "framer-motion";
import { Activity, TrendingUp, TrendingDown } from "lucide-react";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { cn } from "@/lib/utils";
import { Card } from "@/components/ui/card";

interface WeeklyAnalysisChartProps {
  requests: AdminRequest[];
  loading?: boolean;
}

const DAY_LABELS = ["Paz", "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt"];

const W = 560;
const H = 200;
const PAD = { top: 18, right: 12, bottom: 8, left: 12 };
const innerW = W - PAD.left - PAD.right;
const innerH = H - PAD.top - PAD.bottom;

export const WeeklyAnalysisChart: React.FC<WeeklyAnalysisChartProps> = ({ requests, loading }) => {
  const [active, setActive] = useState<number | null>(null);

  const { days, total, deltaPct } = useMemo(() => {
    const now = new Date();

    const dayStart = (offset: number) => {
      const d = new Date(now);
      d.setHours(0, 0, 0, 0);
      d.setDate(d.getDate() - offset);
      return d;
    };

    const inRange = (r: AdminRequest, start: Date, end: Date) => {
      const t = new Date(r.createdAt).getTime();
      return t >= start.getTime() && t < end.getTime();
    };

    const arr: { label: string; count: number }[] = [];
    let sum = 0;
    for (let i = 6; i >= 0; i--) {
      const start = dayStart(i);
      const end = dayStart(i - 1);
      const count = requests.filter((r) => inRange(r, start, end)).length;
      sum += count;
      arr.push({ label: DAY_LABELS[start.getDay()], count });
    }

    const prevStart = dayStart(13);
    const prevEnd = dayStart(6);
    const prevCount = requests.filter((r) => inRange(r, prevStart, prevEnd)).length;
    const deltaPct = prevCount > 0 ? Math.round(((sum - prevCount) / prevCount) * 100) : sum > 0 ? 100 : 0;

    return { days: arr, total: sum, deltaPct };
  }, [requests]);

  const maxVal = Math.max(...days.map((d) => d.count), 1);
  const stepX = innerW / Math.max(days.length - 1, 1);

  const points = days.map((d, i) => ({
    x: PAD.left + i * stepX,
    y: PAD.top + innerH - (d.count / maxVal) * innerH,
    ...d,
  }));

  const linePath = points.map((p, i) => `${i === 0 ? "M" : "L"} ${p.x} ${p.y}`).join(" ");
  const areaPath = `${linePath} L ${points[points.length - 1].x} ${PAD.top + innerH} L ${points[0].x} ${PAD.top + innerH} Z`;

  const gridLines = [1, 0.75, 0.5, 0.25].map((f) => ({
    y: PAD.top + innerH * (1 - f),
    label: Math.round(maxVal * f),
  }));

  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -right-20 h-48 w-48 rounded-full bg-indigo-600/15 blur-3xl" />

      <div className="relative z-10 flex items-start justify-between gap-4">
        <div className="flex items-center gap-3.5">
          <div className="relative shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-60" />
            <div className="relative flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-500 text-white shadow-lg shadow-indigo-500/25">
              <Activity size={20} />
            </div>
          </div>
          <div>
            <h2 className="text-lg font-black tracking-tight text-white">Haftalık Talep Akışı</h2>
            <p className="mt-0.5 text-[10px] font-semibold uppercase tracking-[0.15em] text-slate-400">
              Son 7 gün · gerçek veri
            </p>
          </div>
        </div>
        <div className="text-right">
          <p className="font-mono text-2xl font-bold tabular-nums tracking-tight text-white">{total.toLocaleString("tr-TR")}</p>
          <p className="text-[9px] font-semibold uppercase tracking-widest text-slate-400">Toplam</p>
        </div>
      </div>

      <div className="mt-4 flex items-center gap-2.5">
        <span
          className={cn(
            "inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-[10px] font-black uppercase tracking-wider",
            deltaPct >= 0 ? "bg-emerald-500/15 text-emerald-400" : "bg-rose-500/15 text-rose-400"
          )}
        >
          {deltaPct >= 0 ? <TrendingUp size={12} /> : <TrendingDown size={12} />}
          %{Math.abs(deltaPct)} {deltaPct >= 0 ? "artış" : "azalış"}
        </span>
        <span className="text-[10px] font-medium text-slate-400">önceki haftaya göre</span>
      </div>

      <div className="relative z-10 mt-6">
        {loading ? (
          <div className="h-[200px] animate-pulse rounded-2xl bg-slate-800/70" />
        ) : (
          <div className="relative" onMouseLeave={() => setActive(null)}>
            <svg viewBox={`0 0 ${W} ${H}`} className="w-full">
              <defs>
                <linearGradient id="hc-week-grad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#6366f1" stopOpacity="0.28" />
                  <stop offset="100%" stopColor="#6366f1" stopOpacity="0" />
                </linearGradient>
                <linearGradient id="hc-week-line" x1="0" y1="0" x2="1" y2="0">
                  <stop offset="0%" stopColor="#818cf8" />
                  <stop offset="100%" stopColor="#a855f7" />
                </linearGradient>
              </defs>

              {gridLines.map((g, i) => (
                <g key={i}>
                  <line
                    x1={PAD.left}
                    x2={W - PAD.right}
                    y1={g.y}
                    y2={g.y}
                    className="stroke-slate-700"
                    strokeWidth={1}
                    strokeDasharray="4 6"
                  />
                  <text x={W - PAD.right} y={g.y + 3} textAnchor="end" className="fill-slate-500 text-[8px] font-semibold">
                    {g.label}
                  </text>
                </g>
              ))}

              <motion.path
                d={areaPath}
                fill="url(#hc-week-grad)"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 1, delay: 0.4 }}
              />

              <motion.path
                d={linePath}
                fill="none"
                stroke="url(#hc-week-line)"
                strokeWidth={3}
                strokeLinecap="round"
                strokeLinejoin="round"
                initial={{ pathLength: 0 }}
                animate={{ pathLength: 1 }}
                transition={{ duration: 1.4, ease: [0.22, 1, 0.36, 1], delay: 0.15 }}
              />

              {points.map((p, i) => (
                <g key={i} onMouseEnter={() => setActive(i)}>
                  {active === i && (
                    <line x1={p.x} x2={p.x} y1={PAD.top} y2={PAD.top + innerH} className="stroke-slate-600" strokeWidth={1} />
                  )}
                  <circle
                    cx={p.x}
                    cy={p.y}
                    r={active === i ? 6 : 4}
                    className={active === i ? "fill-indigo-500" : "fill-slate-800"}
                    stroke={active === i ? "none" : "url(#hc-week-line)"}
                    strokeWidth={2.5}
                  />
                </g>
              ))}
            </svg>

            <div className="mt-2 grid grid-cols-7">
              {points.map((p, i) => (
                <span
                  key={i}
                  className={cn(
                    "text-center text-[9px] font-bold uppercase tracking-wider transition-colors",
                    active === i ? "text-indigo-400" : "text-slate-500"
                  )}
                >
                  {p.label}
                </span>
              ))}
            </div>

            {active !== null && points[active] && (
              <div
                className="pointer-events-none absolute z-20 -translate-x-1/2 -translate-y-full rounded-xl border border-slate-700 bg-slate-800 px-3 py-2 shadow-xl"
                style={{ left: `${(points[active].x / W) * 100}%`, top: `${(points[active].y / H) * 100 - 10}%` }}
              >
                <p className="font-mono text-xs font-bold tabular-nums text-white">{points[active].count} talep</p>
                <p className="text-[9px] font-semibold uppercase tracking-widest text-slate-400">{points[active].label}</p>
              </div>
            )}
          </div>
        )}
      </div>
    </Card>
  );
};
