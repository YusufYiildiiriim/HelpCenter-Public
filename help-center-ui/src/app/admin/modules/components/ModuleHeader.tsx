"use client";

import React from "react";
import { Plus, LayoutGrid, Sparkles } from "lucide-react";

interface ModuleHeaderProps {
  onAddClick: () => void;
  canCreate: boolean;
}

export const ModuleHeader: React.FC<ModuleHeaderProps> = ({ onAddClick, canCreate }) => {
  return (
    <div className="flex flex-col sm:flex-row justify-between items-center gap-4 sm:gap-6 bg-slate-900/95 p-4 sm:p-6 md:p-8 rounded-2xl sm:rounded-3xl border border-slate-800 shadow-xl shadow-slate-950/20 backdrop-blur-xl overflow-hidden relative group">
      {/* Premium Background Accents */}
      <div className="absolute top-0 right-0 w-48 h-48 bg-indigo-600/20 -mr-24 -mt-24 rounded-full blur-[80px] opacity-40 group-hover:bg-indigo-600/30 transition-colors duration-700" />
      
      <div className="relative flex items-center gap-4">
        <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 flex items-center justify-center text-white shadow-xl shadow-indigo-600/30 group-hover:scale-105 transition-transform duration-500">
            <LayoutGrid size={24} />
        </div>
        <div>
            <div className="flex items-center gap-2">
                <h1 className="text-xl md:text-2xl font-black text-white tracking-tight leading-none">Modüller</h1>
                <div className="hidden md:flex items-center gap-1.5 px-2 py-0.5 bg-indigo-500/15 text-indigo-300 rounded-full border border-indigo-500/30">
                    <Sparkles size={10} className="animate-pulse" />
                    <span className="text-[8px] font-black uppercase tracking-widest">Sistem</span>
                </div>
            </div>
            <p className="text-slate-400 mt-1.5 text-xs font-medium">
                Departman ve operasyonel modülleri yönetin.
            </p>
        </div>
      </div>

      {canCreate && (
        <button 
          onClick={onAddClick}
          className="w-full sm:w-auto bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white px-5 py-3 rounded-xl flex items-center justify-center gap-2.5 shadow-lg shadow-indigo-600/20 transition-all font-black text-[11px] uppercase tracking-widest active:scale-95 relative z-10"
        >
          <Plus size={16} />
          <span>Yeni Modül Ekle</span>
        </button>
      )}
    </div>
  );
};
