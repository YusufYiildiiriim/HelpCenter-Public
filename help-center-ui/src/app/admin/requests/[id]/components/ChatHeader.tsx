import React from "react";
import { ArrowLeft, CheckCircle2, Clock3, Hash, UserPlus } from "lucide-react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";

interface TicketSummary {
  ticketId?: string;
  title?: string;
  status?: string;
  customerName?: string;
  companyName?: string;
}

interface ChatHeaderProps {
  requestId: string;
  ticket: TicketSummary | null;
  onConsultClick: () => void;
}

export const ChatHeader: React.FC<ChatHeaderProps> = ({ requestId, ticket, onConsultClick }) => {
  const router = useRouter();
  const isCompleted = ticket?.status === "Tamamlandı";
  const displayTicketId = ticket?.ticketId || `#${requestId.substring(0, 8)}`;

  return (
    <div className="shrink-0 border-b border-slate-800 bg-slate-950 px-4 py-3 backdrop-blur md:px-5">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex min-w-0 items-center gap-3">
          <button
            onClick={() => router.back()}
            aria-label="Geri don"
            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-slate-700 bg-slate-900 text-slate-400 transition-all hover:border-slate-600 hover:bg-slate-800 hover:text-white active:scale-95"
          >
            <ArrowLeft size={18} />
          </button>

          <div className="min-w-0">
            <div className="flex min-w-0 flex-wrap items-center gap-2">
              <span className="inline-flex items-center gap-1.5 rounded-lg bg-slate-800 px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider text-slate-300">
                <Hash size={11} />
                {displayTicketId.replace("#", "")}
              </span>
              {ticket?.status && (
                <span
                  className={cn(
                    "inline-flex items-center gap-1.5 rounded-lg border px-2.5 py-1 text-[10px] font-black uppercase leading-none tracking-wider",
                    isCompleted
                      ? "border-emerald-500/20 bg-emerald-500/10 text-emerald-400"
                      : "border-indigo-500/20 bg-indigo-500/10 text-indigo-400"
                  )}
                >
                  {isCompleted ? <CheckCircle2 size={12} /> : <Clock3 size={12} />}
                  {ticket.status}
                </span>
              )}
            </div>

            <h2 className="mt-1.5 truncate text-sm font-black tracking-tight text-white md:text-base">
              {ticket?.title || "Talep Yazışmaları"}
            </h2>
            <p className="truncate text-[11px] font-semibold text-slate-500">
              {[ticket?.customerName, ticket?.companyName].filter(Boolean).join(" / ") || "Müşteri temsilcisi olarak yanıtlıyorsunuz"}
            </p>
          </div>
        </div>

        <div className="flex items-center justify-between gap-3 lg:justify-end">
          <div className="hidden min-w-0 text-right sm:block">
            <p className="text-[11px] font-black uppercase tracking-wider text-slate-500">Aktif Oturum</p>
            <p className="mt-0.5 truncate text-xs font-bold text-slate-300">Müşteri Talebi</p>
          </div>

          {isCompleted ? (
            <div className="inline-flex h-10 items-center gap-2 rounded-xl border border-slate-700 bg-slate-800/50 px-3 text-[10px] font-black uppercase tracking-wider text-slate-400">
              <CheckCircle2 size={15} />
              Talep Tamamlandı
            </div>
          ) : (
            <button
              onClick={onConsultClick}
              className="inline-flex h-10 items-center gap-2 rounded-xl border border-indigo-500/30 bg-indigo-500/10 px-3.5 text-[10px] font-black uppercase tracking-wider text-indigo-400 shadow-sm transition-all hover:border-indigo-500 hover:bg-indigo-600 hover:text-white active:scale-95"
            >
              <UserPlus size={15} />
              Danış
            </button>
          )}
        </div>
      </div>
    </div>
  );
};
