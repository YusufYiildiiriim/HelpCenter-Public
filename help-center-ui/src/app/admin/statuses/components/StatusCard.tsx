import React from "react";
import { Clock, MessageSquare, CheckCircle2, Info } from "lucide-react";
import type { RequestStatus } from "@/services/admin/AdminStatusService";
import { cn } from "@/lib/utils";

interface StatusCardProps {
  status: RequestStatus;
}

export const StatusCard: React.FC<StatusCardProps> = ({ status }) => {
  const getStatusIcon = (id: number) => {
    switch (id) {
        case 1: return <Clock className="text-amber-400" size={24} />;
        case 2: return <MessageSquare className="text-indigo-400" size={24} />;
        case 3: return <CheckCircle2 className="text-emerald-400" size={24} />;
        default: return <Info className="text-slate-400" size={24} />;
    }
  };

  const getStatusBg = (id: number) => {
    switch (id) {
        case 1: return "bg-amber-500/15 border-amber-500/30";
        case 2: return "bg-indigo-500/15 border-indigo-500/30";
        case 3: return "bg-emerald-500/15 border-emerald-500/30";
        default: return "bg-slate-800 border-slate-700";
    }
  };

  const getStatusBadge = (id: number) => {
    switch (id) {
        case 1: return "bg-amber-500/15 text-amber-300 border-amber-500/30";
        case 2: return "bg-indigo-500/15 text-indigo-300 border-indigo-500/30";
        case 3: return "bg-emerald-500/15 text-emerald-300 border-emerald-500/30";
        default: return "bg-slate-800 text-slate-400 border-slate-700";
    }
  };

  return (
    <div className="group bg-slate-900/90 p-6 rounded-2xl border border-slate-800 shadow-xl shadow-slate-950/20 hover:shadow-2xl hover:shadow-indigo-600/10 transition-all duration-500 hover:-translate-y-1.5 relative overflow-hidden">
        <div className={cn(
            "absolute -right-4 -top-4 w-20 h-20 rounded-full opacity-10 transition-transform duration-700 group-hover:scale-150",
            getStatusBg(status.id)
        )}></div>

        <div className="flex items-start justify-between mb-6 relative z-10">
            <div className={cn(
                "w-12 h-12 rounded-xl flex items-center justify-center border transition-transform group-hover:scale-110 duration-500",
                getStatusBg(status.id)
            )}>
                {getStatusIcon(status.id)}
            </div>
        </div>

        <div className="space-y-3 relative z-10">
            <h3 className="text-lg font-black text-slate-100 group-hover:text-indigo-400 transition-colors tracking-tight">
                {status.name}
            </h3>
            <p className="text-slate-400 text-xs leading-relaxed font-medium line-clamp-3 h-12">
                {status.description}
            </p>
        </div>

        <div className="mt-6 pt-6 border-t border-slate-800 flex items-center justify-between relative z-10">
            <div className={cn(
                "px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest border",
                getStatusBadge(status.id)
            )}>
                Sistem Sabiti
            </div>
            <div className="flex items-center gap-2">
                <div className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
                <span className="text-[10px] font-bold text-slate-500 uppercase tracking-tighter">Aktif Kullanımda</span>
            </div>
        </div>
    </div>
  );
};

export const StatusSkeleton: React.FC = () => (
    <div className="bg-slate-900/60 p-6 rounded-2xl border border-slate-800 shadow-sm animate-pulse">
        <div className="w-10 h-10 bg-slate-800 rounded-xl mb-4"></div>
        <div className="h-5 bg-slate-800 rounded-lg w-2/3 mb-3"></div>
        <div className="h-3.5 bg-slate-800 rounded-lg w-full mb-2"></div>
        <div className="h-3.5 bg-slate-800 rounded-lg w-5/6"></div>
    </div>
);
