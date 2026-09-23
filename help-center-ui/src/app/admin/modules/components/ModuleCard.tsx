"use client";

import React from "react";
import { LayoutGrid, Users, Edit, Trash2, Lock, Sparkles } from "lucide-react";
import type { Module } from "@/services/admin/AdminModuleService";
import { cn } from "@/lib/utils";

interface ModuleCardProps {
  module: Module;
  onEdit: (module: Module) => void;
  onDelete: (publicId: string) => void;
  onManageExperts: (module: Module) => void;
}

export const ModuleCard: React.FC<ModuleCardProps> = ({ 
  module, 
  onEdit, 
  onDelete, 
  onManageExperts 
}) => {
  return (
    <div className={cn(
      "bg-slate-900/90 border border-slate-800 rounded-2xl p-6 shadow-xl shadow-slate-950/20 hover:shadow-2xl hover:shadow-indigo-600/10 hover:border-indigo-500/40 transition-all duration-500 group relative overflow-hidden flex flex-col h-full",
      !module.isActive && "opacity-75 grayscale-[0.2]"
    )}>
        {/* Decorative elements */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-600/10 rounded-full -mr-16 -mt-16 group-hover:bg-indigo-600/20 transition-colors duration-500" />
        
        <div className="flex items-start justify-between mb-6 relative z-10">
            <div className={cn(
              "w-12 h-12 rounded-2xl flex items-center justify-center text-white shadow-lg transition-transform group-hover:scale-110 duration-500 shrink-0",
              module.isActive 
                ? "bg-gradient-to-br from-indigo-600 to-violet-600 shadow-indigo-600/30" 
                : "bg-slate-700 shadow-slate-700/20"
            )}>
                <LayoutGrid size={24} />
            </div>
            
            {/* Actions: Visible on mobile, hover-only on desktop */}
            <div className="flex gap-1.5 md:translate-x-4 md:opacity-0 md:group-hover:translate-x-0 md:group-hover:opacity-100 transition-all duration-300">
                <button 
                  onClick={() => onManageExperts(module)}
                  className="w-9 h-9 md:w-10 md:h-10 flex items-center justify-center rounded-xl bg-slate-800/70 border border-slate-700 text-slate-400 hover:text-indigo-400 hover:bg-indigo-600/10 hover:border-indigo-500/40 shadow-sm transition-all active:scale-90"
                  title="Uzmanları Yönet"
                >
                    <Users size={18} />
                </button>
                <button 
                  onClick={() => onEdit(module)}
                  disabled={module.isLocked}
                  className={cn(
                    "w-9 h-9 md:w-10 md:h-10 flex items-center justify-center rounded-xl bg-slate-800/70 border border-slate-700 transition-all shadow-sm active:scale-90",
                    module.isLocked ? "text-slate-600 opacity-50 cursor-not-allowed" : "text-slate-400 hover:text-amber-400 hover:bg-amber-500/10 hover:border-amber-500/30"
                  )}
                  title={module.isLocked ? "Bu modül kilitli" : "Düzenle"}
                >
                    <Edit size={18} />
                </button>
                <button 
                  onClick={() => onDelete(module.publicId)}
                  disabled={module.isLocked}
                  className={cn(
                    "w-9 h-9 md:w-10 md:h-10 flex items-center justify-center rounded-xl bg-slate-800/70 border border-slate-700 transition-all shadow-sm active:scale-90",
                    module.isLocked ? "text-slate-600 opacity-50 cursor-not-allowed" : "text-slate-400 hover:text-red-400 hover:bg-red-500/10 hover:border-red-500/30"
                  )}
                  title={module.isLocked ? "Bu modül kilitli" : "Sil"}
                >
                    <Trash2 size={18} />
                </button>
            </div>
        </div>
        
        <div className="space-y-3 flex-1 relative z-10">
          <div className="flex items-center gap-2">
            <h3 className="text-xl font-black text-slate-100 tracking-tight truncate group-hover:text-indigo-400 transition-colors">
              {module.name}
            </h3>
            {module.isActive ? (
               <Sparkles size={14} className="text-indigo-400 animate-pulse hidden group-hover:block" />
            ) : (
              <span className="bg-slate-800 text-slate-500 text-[8px] font-black px-1.5 py-0.5 rounded-lg border border-slate-700 uppercase tracking-tight shrink-0">Pasif</span>
            )}
          </div>
          
          <p className="text-[13px] text-slate-400 leading-relaxed line-clamp-2 min-h-[40px] font-medium">
              {module.description || "Bu modül için açıklama tanımlanmamış."}
          </p>
        </div>

        <div className="mt-8 flex items-center justify-between gap-2 relative z-10">
            <div className="flex items-center gap-2.5">
                <div className={cn(
                  "w-2 h-2 rounded-full", 
                  module.isActive ? "bg-emerald-400 shadow-[0_0_8px_rgba(52,211,153,0.5)]" : "bg-slate-600"
                )} />
                <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest">
                  {module.isActive ? "Operasyonel" : "Devre Dışı"}
                </span>
            </div>
            
            <div className="flex items-center gap-2">
                {module.isLocked && (
                  <div className="flex items-center gap-1.5 bg-amber-500/15 text-amber-300 text-[9px] font-black px-3 py-1.5 rounded-full uppercase tracking-widest border border-amber-500/30">
                     <Lock size={12} /> Kilitli
                  </div>
                )}
                <div className="flex items-center gap-1.5 bg-indigo-500/15 text-indigo-300 text-[9px] font-black px-3 py-1.5 rounded-full uppercase tracking-widest border border-indigo-500/30 italic">
                    {module.usageCount} Kayıt
                </div>
            </div>
        </div>
    </div>
  );
};
