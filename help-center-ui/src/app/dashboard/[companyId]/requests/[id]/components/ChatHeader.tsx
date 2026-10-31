import React from "react";
import { ArrowLeft, CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";

import type { CustomerRequest } from "@/services/customer/CustomerRequestService";
import { getRequestStatusLabel, isCompletedRequest, RequestStatus } from "@/lib/requestStatus";
import { useLocale } from "@/context/LocaleContext";

interface ChatHeaderProps {
  onBack: () => void;
  requestId: string;
  ticket: CustomerRequest | null;
  onCloseTicket: () => void;
}

export const ChatHeader: React.FC<ChatHeaderProps> = ({ 
  onBack, 
  requestId, 
  ticket, 
  onCloseTicket 
}) => {
  const { locale, t } = useLocale();
  const statusLabel = getRequestStatusLabel(ticket?.statusId, ticket?.status, locale);
  const isCompleted = isCompletedRequest(ticket?.statusId, ticket?.status);
  const priorityLabel = ticket?.priority === 3 ? t("priorityCritical") : ticket?.priority === 2 ? t("priorityHigh") : ticket?.priority === 1 ? t("priorityNormal") : t("priorityLow");
  return (
    <div className="p-6 bg-slate-900/90 backdrop-blur-xl border-b border-slate-800 flex items-center justify-between">
        <div className="flex items-center gap-4">
            <button 
                onClick={onBack}
                className="w-10 h-10 rounded-full hover:bg-slate-800 flex items-center justify-center text-slate-400 hover:text-white transition-colors"
            >
                <ArrowLeft size={20} />
            </button>
            <div>
                <div className="flex items-center gap-2">
                    <h2 className="font-black text-white tracking-tight">{t("requestConversation")}</h2>
                    <span className="text-[10px] font-black text-indigo-400 bg-indigo-500/10 px-2 py-0.5 rounded-full uppercase tracking-tighter">#{ticket?.ticketId ?? requestId}</span>
                    {ticket && (
                        <span className={cn(
                            "text-[9px] font-black px-2 py-0.5 rounded-full uppercase tracking-tighter",
                            ticket.priority === 3 ? "bg-red-500/10 text-red-400" :
                            ticket.priority === 2 ? "bg-orange-500/10 text-orange-400" :
                            ticket.priority === 1 ? "bg-indigo-500/10 text-indigo-400" :
                            "bg-slate-800 text-slate-400"
                        )}>
                            {priorityLabel}
                        </span>
                    )}
                    {ticket?.status && (
                        <span
                            aria-label={`${t("requestStatus")}: ${statusLabel}`}
                            className={cn(
                                "text-[9px] font-black px-2 py-0.5 rounded-full uppercase tracking-tighter border",
                    isCompleted
                                    ? "bg-emerald-500/10 text-emerald-400 border-emerald-500/20"
                                    : ticket.statusId === RequestStatus.Replied
                                        ? "bg-indigo-500/10 text-indigo-400 border-indigo-500/20"
                                        : ticket.statusId === RequestStatus.UnderTechnicalReview
                                            ? "bg-amber-500/10 text-amber-400 border-amber-500/20"
                                            : "bg-slate-800 text-slate-300 border-slate-700"
                            )}
                        >
                            {statusLabel}
                        </span>
                    )}
                </div>
                <p className="text-xs text-slate-400 font-medium">{t("speakingWithSupport")}</p>
            </div>
        </div>

        <div className="flex items-center gap-4">
            {!isCompleted ? (
                <button
                    onClick={onCloseTicket}
                    className="bg-emerald-600 hover:bg-emerald-500 text-white px-6 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest flex items-center gap-2 transition-all shadow-lg shadow-emerald-600/20 active:scale-95"
                >
                    <CheckCircle2 size={16} />
                    {t("closeRequest")}
                </button>
            ) : (
                <div className="bg-slate-800 text-slate-400 px-6 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest flex items-center gap-2 border border-slate-700">
                    <CheckCircle2 size={16} />
                    {t("requestCompleted")}
                </div>
            )}
        </div>
    </div>
  );
};
