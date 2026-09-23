"use client";

import React from "react";
import Link from "next/link";
import { ArrowRight, History, Inbox } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { cn } from "@/lib/utils";

interface RecentRequestsTableProps {
  requests: AdminRequest[];
  onRowClick: (publicId: string) => void;
  getTimeAgo: (date: string) => string;
}

const AVATAR_GRADIENTS = [
  "from-indigo-500 to-violet-500",
  "from-blue-500 to-cyan-500",
  "from-emerald-500 to-teal-500",
  "from-amber-500 to-orange-500",
  "from-rose-500 to-pink-500",
  "from-fuchsia-500 to-purple-500",
];

const hashString = (s: string) => s.split("").reduce((acc, c) => acc + c.charCodeAt(0), 0);

const statusVariant = (status: string): "warning" | "info" | "destructive" | "success" | "secondary" => {
  switch (status) {
    case "Cevap Bekliyor":
      return "warning";
    case "Cevaplandi":
    case "Cevaplandı":
      return "info";
    case "Teknik Incelemede":
    case "Teknik İncelemede":
      return "destructive";
    case "Tamamlandi":
    case "Tamamlandı":
      return "success";
    default:
      return "secondary";
  }
};

const priorityDot = (priorityName: string) => {
  switch (priorityName) {
    case "Critical":
      return "bg-red-500";
    case "High":
      return "bg-amber-500";
    case "Medium":
      return "bg-blue-500";
    default:
      return "bg-slate-500";
  }
};

export const RecentRequestsTable: React.FC<RecentRequestsTableProps> = ({
  requests,
  onRowClick,
  getTimeAgo,
}) => {
  return (
    <Card className="relative overflow-hidden rounded-2xl border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
      <div className="pointer-events-none absolute -top-20 -right-20 h-48 w-48 rounded-full bg-indigo-600/10 blur-3xl" />

      <div className="relative z-10 mb-5 flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5">
          <div className="relative shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-60" />
            <div className="relative flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/25">
              <History size={20} />
            </div>
          </div>
          <div>
            <h2 className="text-lg font-black tracking-tight text-white">Son Hareketler</h2>
            <p className="mt-0.5 text-[10px] font-semibold uppercase tracking-[0.15em] text-slate-400">
              En güncel talepler
            </p>
          </div>
        </div>
        <Link href="/admin/requests">
          <Button
            variant="outline"
            size="sm"
            className="text-[10px] font-black uppercase tracking-wider border-slate-700 bg-white/5 text-slate-200 hover:border-indigo-500 hover:bg-indigo-600 hover:text-white"
          >
            Tümü
            <ArrowRight size={13} className="transition-transform group-hover:translate-x-0.5" />
          </Button>
        </Link>
      </div>

      {requests.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-3 py-14 text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 text-slate-400">
            <Inbox size={24} />
          </div>
          <p className="text-sm font-black text-white">Henüz talep yok</p>
          <p className="max-w-xs text-xs font-medium text-slate-400">
            Yeni bir talep oluşturulduğunda burada görünecek.
          </p>
        </div>
      ) : (
        <div className="space-y-1.5">
          {requests.map((req) => {
            const grad = AVATAR_GRADIENTS[hashString(req.companyName || req.customerName) % AVATAR_GRADIENTS.length];
            return (
              <div
                key={req.id}
                onClick={() => onRowClick(req.publicId)}
                className="group flex cursor-pointer items-center gap-3.5 rounded-2xl border border-transparent p-3 transition-all hover:border-slate-700 hover:bg-slate-800/60 active:scale-[0.99]"
              >
                <Avatar className="h-10 w-10 shrink-0 rounded-xl">
                  <AvatarFallback
                    className={cn("rounded-xl bg-gradient-to-br text-sm font-black text-white shadow-sm", grad)}
                  >
                    {(req.companyName || req.customerName).charAt(0).toUpperCase()}
                  </AvatarFallback>
                </Avatar>

                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
                    <span className="font-mono text-[10px] font-bold tabular-nums text-slate-400">{req.ticketId}</span>
                    <Badge variant={statusVariant(req.status)} className="px-1.5 py-0.5 text-[9px]">
                      {req.status}
                    </Badge>
                    <span className={cn("h-1.5 w-1.5 rounded-full", priorityDot(req.priorityName))} title={req.priorityName} />
                  </div>
                  <p className="mt-1 truncate text-sm font-bold text-slate-100 transition-colors group-hover:text-indigo-300">
                    {req.title}
                  </p>
                </div>

                <div className="hidden shrink-0 flex-col items-end gap-1 sm:flex">
                  <span className="font-mono text-[9px] font-semibold uppercase tracking-widest text-slate-400">
                    {getTimeAgo(req.createdAt)}
                  </span>
                  <span className="max-w-[120px] truncate text-[10px] font-medium text-slate-400">
                    {req.companyName || req.customerName}
                  </span>
                </div>

                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-xl bg-white/10 text-slate-400 transition-all group-hover:bg-indigo-600 group-hover:text-white group-hover:shadow-lg group-hover:shadow-indigo-600/25">
                  <ArrowRight size={14} />
                </div>
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
};
