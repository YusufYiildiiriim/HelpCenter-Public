import React from "react";
import { ArrowUpRight, Building2, CalendarDays, FileText, MessageSquare, Search, User } from "lucide-react";
import type { AdminRequest } from "@/services/admin/AdminRequestService";
import { cn } from "@/lib/utils";
import Link from "next/link";

interface RequestCardProps {
  request: AdminRequest;
}

export const RequestCard: React.FC<RequestCardProps> = ({ request }) => {
  const createdDate = new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "short",
    hour: "2-digit",
    minute: "2-digit"
  }).format(new Date(request.createdAt));

  return (
    <div className="group relative overflow-hidden rounded-2xl border border-slate-200/80 bg-white p-4 shadow-[0_1px_2px_rgba(15,23,42,0.04)] transition-all hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-[0_14px_34px_rgba(15,23,42,0.08)] md:p-5">
      <div
        className={cn(
          "absolute inset-y-0 left-0 w-1",
          request.priorityName === "Critical" ? "bg-red-500" :
          request.priorityName === "High" ? "bg-amber-500" :
          request.priorityName === "Medium" ? "bg-blue-500" :
          "bg-slate-300"
        )}
      />

      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div className="min-w-0 flex-1 space-y-3 pl-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="rounded-md bg-slate-950 px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider text-white">
              {request.ticketId}
            </span>
            <span className={cn(
              "rounded-md border px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider",
              request.priorityName === "Critical" ? "border-red-200 bg-red-50 text-red-600" :
              request.priorityName === "High" ? "border-amber-200 bg-amber-50 text-amber-700" :
              request.priorityName === "Medium" ? "border-blue-200 bg-blue-50 text-blue-600" :
              "border-slate-200 bg-slate-50 text-slate-500"
            )}>
              {request.priorityName}
            </span>
            <span className={cn(
              "rounded-md border px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider",
              request.status === "Cevap Bekliyor" ? "border-amber-200 bg-amber-50 text-amber-700" :
              request.status === "Cevaplandi" || request.status === "Cevaplandı" ? "border-blue-200 bg-blue-50 text-blue-600" :
              request.status === "Teknik Incelemede" || request.status === "Teknik İncelemede" ? "border-indigo-200 bg-indigo-50 text-indigo-600" :
              "border-emerald-200 bg-emerald-50 text-emerald-700"
            )}>
              {request.status}
            </span>
          </div>

          <div className="min-w-0">
            <h3 className="truncate text-base font-black tracking-tight text-slate-900 transition-colors group-hover:text-blue-700 md:text-lg">
              {request.title}
            </h3>
            <p className="mt-1 line-clamp-1 max-w-3xl text-xs font-medium leading-5 text-slate-500 md:text-sm">
              {request.description}
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-[11px] font-bold text-slate-500">
            <span className="flex min-w-0 items-center gap-1.5">
              <User size={13} className="text-slate-500" />
              <span className="truncate max-w-35 text-slate-900">{request.customerName}</span>
            </span>
            <span className="flex min-w-0 items-center gap-1.5">
              <Building2 size={13} className="text-slate-500" />
              <span className="truncate max-w-45 text-slate-800">{request.companyName || "Bilinmeyen Firma"}</span>
            </span>
            <span className="flex min-w-0 items-center gap-1.5">
              <Search size={13} className="text-slate-500" />
              <span className="truncate max-w-50 text-slate-800">
                {request.currentExpertName
                  ? `Uzman: ${request.currentExpertName}`
                  : `Atanan: ${request.assignedUserName}`}
              </span>
            </span>
            <span className="flex items-center gap-1.5">
              <CalendarDays size={13} className="text-slate-500" />
              <span className="text-slate-800">{createdDate}</span>
            </span>
          </div>
        </div>

        <div className="flex items-center justify-between gap-3 border-t border-slate-100 pt-3 xl:w-auto xl:border-t-0 xl:pt-0">
          <div className="flex items-center gap-2">
            <div className="flex h-9 items-center gap-1.5 rounded-xl border border-slate-200 bg-slate-50 px-3 text-xs font-black text-slate-600">
              <MessageSquare size={14} className="text-blue-500" />
              {request.messageCount}
            </div>
            <div className="flex h-9 items-center gap-1.5 rounded-xl border border-slate-200 bg-slate-50 px-3 text-xs font-black text-slate-600">
              <FileText size={14} className="text-blue-500" />
              {request.documentCount}
            </div>
          </div>

          <Link
            href={`/admin/requests/${request.publicId}`}
            aria-label="Talep detayini ac"
            className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-slate-950 text-white shadow-sm transition-all hover:bg-blue-600 active:scale-95"
          >
            <ArrowUpRight size={18} />
          </Link>
        </div>
      </div>
    </div>
  );
};
