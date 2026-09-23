"use client";

import React from "react";
import { Inbox, Sparkles } from "lucide-react";
import { cn } from "@/lib/utils";

interface AssignedRequestHeaderProps {
  filter: string;
  setFilter: (filter: string) => void;
  totalCount: number;
}

const STATUS_FILTERS = ["Hepsi", "Cevap Bekliyor", "Cevaplandı", "Teknik İncelemede", "Tamamlandı"];

export const AssignedRequestHeader: React.FC<AssignedRequestHeaderProps> = ({
  filter,
  setFilter,
  totalCount,
}) => {
  return (
    <div className="relative overflow-hidden rounded-3xl bg-slate-900/95 p-6 md:p-8 text-white shadow-xl shadow-slate-950/20 backdrop-blur-xl border border-slate-800">
      {/* Animated background glows */}
      <div className="pointer-events-none absolute -top-24 -right-24 h-72 w-72 rounded-full bg-gradient-to-br from-indigo-600/30 to-violet-600/20 blur-3xl animate-pulse" />
      <div className="pointer-events-none absolute -bottom-20 -left-16 h-64 w-64 rounded-full bg-blue-600/10 blur-3xl" />
      
      {/* Grid pattern overlay */}
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(#38bdf8_1px,transparent_1px)] [background-size:16px_16px] opacity-10" />

      <div className="relative z-10 flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6">
        <div className="flex items-center gap-4">
          <div className="relative group shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-70 group-hover:opacity-100 transition duration-500" />
            <div className="relative flex h-12 w-12 sm:h-14 sm:w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 text-white shadow-xl shadow-indigo-600/30 transition-transform duration-500 group-hover:scale-105">
              <Inbox className="h-6 w-6 sm:h-7 sm:w-7" />
            </div>
          </div>

          <div>
            <div className="flex flex-wrap items-center gap-2.5">
              <h1 className="text-xl sm:text-2xl md:text-3xl font-black tracking-tight text-white">
                Bana Atanan Talepler
              </h1>
              <span className="inline-flex items-center gap-1.5 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-2.5 py-0.5 text-[10px] font-black uppercase tracking-wider text-indigo-300">
                <Sparkles size={11} className="animate-pulse text-indigo-400" />
                Kişisel Atamalar
              </span>
            </div>
            <p className="mt-1.5 text-xs sm:text-sm font-medium text-slate-300 max-w-xl">
              {totalCount > 0
                ? `${totalCount} aktif talep sizin takibinizde.`
                : "Şu an size atanmış talep bulunmuyor."}
            </p>
          </div>
        </div>

        {/* Scrollable Status Filters */}
        <div className="flex items-center gap-1.5 bg-slate-950/80 p-1.5 rounded-2xl border border-slate-800 overflow-x-auto scrollbar-none w-full lg:w-auto">
          {STATUS_FILTERS.map((f) => {
            const isActive = filter === f;
            return (
              <button
                key={f}
                onClick={() => setFilter(f)}
                className={cn(
                  "px-3.5 py-2 rounded-xl text-[10px] font-black uppercase tracking-wider whitespace-nowrap transition-all duration-200 active:scale-95 shrink-0",
                  isActive
                    ? "bg-indigo-600 text-white shadow-md shadow-indigo-600/30"
                    : "text-slate-400 hover:text-white hover:bg-slate-800"
                )}
              >
                {f}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};
