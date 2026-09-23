"use client";

import React from "react";
import { Plus, MessageSquare, Sparkles } from "lucide-react";

interface SubjectHeaderProps {
  canCreate: boolean;
  onAddClick: () => void;
}

export const SubjectHeader: React.FC<SubjectHeaderProps> = ({ canCreate, onAddClick }) => {
  return (
    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 sm:gap-6 bg-slate-900/95 p-4 sm:p-6 md:p-8 rounded-2xl sm:rounded-3xl border border-slate-800 shadow-xl shadow-slate-950/20 backdrop-blur-xl overflow-hidden relative group">
      {/* Premium Background Accents */}
      <div className="absolute top-0 right-0 w-64 h-64 bg-indigo-600/20 -mr-32 -mt-32 rounded-full blur-3xl opacity-60 pointer-events-none group-hover:bg-indigo-600/30 transition-colors duration-500" />
      <div className="absolute bottom-0 left-0 w-32 h-32 bg-indigo-500/10 -ml-16 -mb-16 rounded-full blur-2xl opacity-40 pointer-events-none" />
      
      <div className="relative flex items-center gap-5">
        <div className="w-14 h-14 md:w-16 md:h-16 rounded-2xl md:rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 flex items-center justify-center text-white shadow-xl shadow-indigo-600/30 shrink-0">
            <MessageSquare size={28} className="md:w-8 md:h-8" />
        </div>
        <div>
            <div className="flex items-center gap-2">
                <h1 className="text-2xl md:text-4xl font-black text-white tracking-tight leading-none">Talep Konuları</h1>
                <div className="hidden md:flex items-center gap-1 px-2 py-0.5 bg-indigo-500/15 text-indigo-300 rounded-full border border-indigo-500/30">
                    <Sparkles size={10} />
                    <span className="text-[9px] font-black uppercase tracking-widest">Kategori</span>
                </div>
            </div>
            <p className="text-slate-400 mt-2 text-xs md:text-sm font-medium max-w-[280px] md:max-w-none">
                Sistemdeki destek talebi kategorilerini ve konularını yönetin.
            </p>
        </div>
      </div>

      {canCreate && (
        <button 
            onClick={onAddClick}
            className="w-full sm:w-auto bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white px-8 py-4 md:py-5 rounded-2xl flex items-center justify-center gap-3 shadow-lg shadow-indigo-600/20 transition-all font-black text-[10px] md:text-xs uppercase tracking-widest group active:scale-95 relative z-10"
        >
            <div className="bg-white/20 p-1.5 rounded-lg group-hover:rotate-90 transition-transform duration-300">
                <Plus size={18} />
            </div>
            <span className="relative z-10">Yeni Konu Ekle</span>
        </button>
      )}
    </div>
  );
};
