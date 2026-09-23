import React from "react";
import { ArrowLeft, CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";

import type { CustomerRequest } from "@/services/customer/CustomerRequestService";

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
                    <h2 className="font-black text-white tracking-tight">Talep Yazışmaları</h2>
                    <span className="text-[10px] font-black text-indigo-400 bg-indigo-500/10 px-2 py-0.5 rounded-full uppercase tracking-tighter">#{ticket?.ticketId ?? requestId}</span>
                    {ticket && (
                        <span className={cn(
                            "text-[9px] font-black px-2 py-0.5 rounded-full uppercase tracking-tighter",
                            ticket.priority === 3 ? "bg-red-500/10 text-red-400" :
                            ticket.priority === 2 ? "bg-orange-500/10 text-orange-400" :
                            ticket.priority === 1 ? "bg-indigo-500/10 text-indigo-400" :
                            "bg-slate-800 text-slate-400"
                        )}>
                            {ticket.priority === 3 ? "KRİTİK" :
                             ticket.priority === 2 ? "YÜKSEK" :
                             ticket.priority === 1 ? "NORMAL" : "DÜŞÜK"}
                        </span>
                    )}
                </div>
                <p className="text-xs text-slate-400 font-medium">Müşteri Hizmetleri ile Görüşüyorsunuz</p>
            </div>
        </div>

        <div className="flex items-center gap-4">
            {ticket?.status !== "Tamamlandı" ? (
                <button
                    onClick={onCloseTicket}
                    className="bg-emerald-600 hover:bg-emerald-500 text-white px-6 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest flex items-center gap-2 transition-all shadow-lg shadow-emerald-600/20 active:scale-95"
                >
                    <CheckCircle2 size={16} />
                    Talebi Kapat
                </button>
            ) : (
                <div className="bg-slate-800 text-slate-400 px-6 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest flex items-center gap-2 border border-slate-700">
                    <CheckCircle2 size={16} />
                    Talep Tamamlandı
                </div>
            )}
        </div>
    </div>
  );
};
