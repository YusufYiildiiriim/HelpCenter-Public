import React from "react";
import { LayoutList, ShieldCheck } from "lucide-react";

export const StatusHeader: React.FC = () => {
  return (
    <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 sm:gap-6 bg-slate-900/95 p-4 sm:p-6 rounded-2xl sm:rounded-3xl border border-slate-800 shadow-xl shadow-slate-950/20 backdrop-blur-xl overflow-hidden relative">
        <div className="flex items-center gap-6">
            <div className="w-14 h-14 bg-gradient-to-br from-indigo-600 to-violet-700 rounded-2xl flex items-center justify-center shadow-xl shadow-indigo-600/30 rotate-3 transition-transform hover:rotate-0">
                <LayoutList className="text-white" size={28} />
            </div>
            <div>
                <h1 className="text-2xl font-black text-white tracking-tight">Talep Durumları</h1>
                <p className="text-slate-400 font-medium text-sm mt-0.5">Sistemdeki sabit talep aşamaları ve açıklamaları</p>
            </div>
        </div>
        <div className="flex items-center gap-3 px-5 py-2.5 bg-indigo-500/15 rounded-xl border border-indigo-500/30">
            <ShieldCheck className="text-indigo-300" size={18} />
            <span className="text-indigo-300 font-bold text-xs">Salt Okunur Sistem Verisi</span>
        </div>
    </div>
  );
};
