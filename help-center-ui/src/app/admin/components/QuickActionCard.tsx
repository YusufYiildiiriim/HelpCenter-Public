"use client";

import React from "react";
import Link from "next/link";
import { ArrowRight, Inbox, Sparkles } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface QuickActionCardProps {
  activeRequests: number;
}

export const QuickActionCard: React.FC<QuickActionCardProps> = ({ activeRequests }) => {
  return (
    <Card className="group relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -right-20 h-48 w-48 rounded-full bg-indigo-600/10 blur-3xl transition-transform duration-700 group-hover:scale-110" />
      <div className="pointer-events-none absolute -bottom-20 -left-20 h-48 w-48 rounded-full bg-violet-600/10 blur-3xl" />

      <div className="relative z-10">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3.5">
            <div className="relative shrink-0">
              <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-60" />
              <div className="relative flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-500 text-white shadow-lg shadow-indigo-500/25">
                <Inbox size={20} />
              </div>
            </div>
            <div>
              <h2 className="text-lg font-black tracking-tight text-white">Bekleyen Talepler</h2>
              <p className="mt-0.5 text-[10px] font-semibold uppercase tracking-[0.15em] text-slate-400">
                Hızlı İşlemler
              </p>
            </div>
          </div>
          <span className="inline-flex items-center gap-1 rounded-full border border-amber-500/30 bg-amber-500/10 px-2.5 py-1 text-[10px] font-black uppercase tracking-wider text-amber-400">
            <Sparkles size={10} className="animate-pulse" />
            Aktif
          </span>
        </div>

        <div className="mt-6 flex items-center justify-between rounded-2xl border border-slate-800 bg-slate-800/40 p-4">
          <div>
            <p className="text-[10px] font-black uppercase tracking-[0.18em] text-slate-400">
              İşlem Bekleyen
            </p>
            <div className="mt-1 flex items-baseline gap-1.5">
              <span className="font-mono text-3xl font-black tabular-nums text-white">
                {activeRequests}
              </span>
              <span className="text-xs font-bold text-slate-400">talep</span>
            </div>
          </div>
          <div className="flex flex-col items-end gap-1">
            <span className="text-[10px] font-bold text-emerald-400 flex items-center gap-1.5">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 animate-pulse" />
              Kuyrukta
            </span>
            <span className="text-[9px] font-medium text-slate-500">Müdahale açık</span>
          </div>
        </div>

        <p className="mt-4 text-xs font-medium leading-relaxed text-slate-400">
          Yönetim ekranından bekleyen talepleri inceleyebilir, önceliklendirebilir ve uzmanlara yönlendirebilirsiniz.
        </p>

        <Link href="/admin/requests" className="mt-6 block">
          <Button className="w-full rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 py-5 text-xs font-black uppercase tracking-wider text-white shadow-lg shadow-indigo-600/25 transition-all hover:from-indigo-500 hover:to-violet-500 active:scale-[0.98]">
            Talepleri Yönet
            <ArrowRight size={14} className="transition-transform group-hover:translate-x-0.5" />
          </Button>
        </Link>
      </div>
    </Card>
  );
};
