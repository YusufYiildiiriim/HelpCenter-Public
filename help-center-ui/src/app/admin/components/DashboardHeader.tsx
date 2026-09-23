"use client";

import React from "react";
import Link from "next/link";
import { Plus, Users, LayoutGrid, CalendarDays, Sparkles } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

const quickActions = [
  { label: "Firma Ekle", icon: Plus, href: "/admin/companies" },
  { label: "Kullanıcılar", icon: Users, href: "/admin/users" },
];

export const DashboardHeader: React.FC = () => {
  const today = new Date().toLocaleDateString("tr-TR", {
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  });

  return (
    <Card className="group relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -right-24 -top-24 h-64 w-64 rounded-full bg-indigo-600/20 blur-3xl transition-colors duration-700 group-hover:bg-indigo-600/30" />
      <div className="pointer-events-none absolute -bottom-28 -left-16 h-56 w-56 rounded-full bg-violet-600/10 blur-3xl" />
      <div
        className="pointer-events-none absolute inset-0 opacity-[0.06]"
        style={{
          backgroundImage: "radial-gradient(circle at 100% 0%, rgba(129,140,248,0.3), transparent 40%)",
        }}
      />

      <div className="relative z-10 flex flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex items-center gap-4">
          <div className="relative shrink-0">
            <div className="absolute -inset-1 bg-linear-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-70" />
            <div className="relative flex h-12 w-12 items-center justify-center rounded-2xl bg-linear-to-br from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/25 transition-transform duration-500 group-hover:scale-105">
              <LayoutGrid size={22} />
            </div>
          </div>
          <div>
            <div className="flex items-center gap-2.5">
              <h1 className="text-xl font-black tracking-tight text-white md:text-2xl">Dashboard</h1>
              <span className="hidden items-center gap-1 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-2.5 py-1 text-[9px] font-black uppercase tracking-widest text-indigo-300 sm:inline-flex">
                <Sparkles size={10} className="animate-pulse" />
                Kontrol Paneli
              </span>
            </div>
            <p className="mt-1.5 text-xs font-medium text-slate-300">
              Operasyonel süreçlerin genel görünümü.
            </p>
            <div className="mt-2 flex items-center gap-1.5 font-mono text-[10px] font-bold uppercase tracking-widest text-slate-400">
              <CalendarDays size={12} />
              {today}
            </div>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {quickActions.map((action) => (
            <Link key={action.label} href={action.href}>
              <Button
                variant="outline"
                size="sm"
                className="gap-2 border-slate-700 bg-white/5 text-slate-200 hover:border-indigo-500 hover:bg-indigo-600 hover:text-white"
              >
                <div className="flex h-6 w-6 items-center justify-center rounded-lg bg-white/10 transition-colors">
                  <action.icon size={12} />
                </div>
                {action.label}
              </Button>
            </Link>
          ))}
        </div>
      </div>
    </Card>
  );
};
