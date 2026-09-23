import React from "react";
import Link from "next/link";
import { MessageSquare, FileText, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import type { CustomerRequest } from "@/services/customer/CustomerRequestService";

interface RequestCardProps {
  request: CustomerRequest;
  companyId: string;
}

export const RequestCard: React.FC<RequestCardProps> = ({ request, companyId }) => {
  return (
    <Link 
        href={`/dashboard/${companyId}/requests/${request.publicId}`}
        className="bg-slate-900/90 backdrop-blur-xl p-8 rounded-[2.5rem] border border-slate-800 shadow-xl shadow-slate-950/30 hover:border-indigo-500/50 hover:shadow-xl hover:shadow-indigo-500/5 transition-all group cursor-pointer relative overflow-hidden block text-left"
    >
        <div className="absolute top-0 left-0 w-1.5 h-full bg-indigo-600 opacity-0 group-hover:opacity-100 transition-opacity"></div>
        
        <div className="flex flex-col sm:flex-row justify-between items-start gap-4 mb-6">
            <div>
                <div className="flex items-center gap-3">
                    <span className="text-[10px] font-black text-indigo-400 bg-indigo-500/10 px-3 py-1 rounded-full uppercase tracking-widest">{request.ticketId}</span>
                    <span className="text-[10px] font-black text-violet-400 bg-violet-500/10 px-3 py-1 rounded-full uppercase tracking-widest">{request.requestSubjectName}</span>
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tarih: {new Date(request.createdAt).toLocaleDateString('tr-TR')}</span>
                </div>
                <h4 className="text-xl font-black text-white mt-3 group-hover:text-indigo-400 transition-colors leading-snug">{request.title}</h4>
            </div>
            <div className="flex flex-col items-end gap-2">
                <div className={cn(
                    "px-4 py-2 rounded-xl text-[11px] font-black uppercase tracking-widest shadow-sm",
                    request.status === "Cevap Bekliyor" ? "bg-amber-500/10 text-amber-400 border border-amber-500/30" : 
                    request.status === "Tamamlandı" ? "bg-emerald-500/10 text-emerald-400 border border-emerald-500/30" :
                    "bg-indigo-500/10 text-indigo-400 border border-indigo-500/30"
                )}>
                    {request.status}
                </div>
                <div className={cn(
                    "px-3 py-1 rounded-lg text-[9px] font-black uppercase tracking-widest",
                    request.priority === 3 ? "bg-red-500/10 text-red-400" :
                    request.priority === 2 ? "bg-orange-500/10 text-orange-400" :
                    request.priority === 1 ? "bg-indigo-500/10 text-indigo-400" :
                    "bg-slate-800 text-slate-400"
                )}>
                    {request.priority === 3 ? "KRİTİK" :
                     request.priority === 2 ? "YÜKSEK" :
                     request.priority === 1 ? "NORMAL" : "DÜŞÜK"}
                </div>
            </div>
        </div>

        <p className="text-slate-400 font-medium line-clamp-2 leading-relaxed text-sm mb-8">
            {request.description}
        </p>

        <div className="pt-6 border-t border-slate-800 flex flex-wrap justify-between items-center gap-4">
            <div className="flex items-center gap-6">
                <div className="flex items-center gap-2 text-xs text-slate-400 font-bold">
                    <MessageSquare size={16} className="text-indigo-400" /> {request.messageCount} Mesaj
                </div>
                <div className="flex items-center gap-2 text-xs text-slate-400 font-bold">
                    <FileText size={16} className="text-indigo-400" /> {request.documentCount} Dosya
                </div>
            </div>
            <div className="flex items-center gap-1.5 text-[10px] text-slate-400 font-black uppercase tracking-tighter group-hover:text-indigo-400 transition-colors">
                Detayları Görüntüle <ChevronRight size={14} className="group-hover:translate-x-1 transition-transform" />
            </div>
        </div>
    </Link>
  );
};
