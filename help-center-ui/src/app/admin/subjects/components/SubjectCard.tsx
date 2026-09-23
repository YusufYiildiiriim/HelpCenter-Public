"use client";

import React from "react";
import { MessageSquare, Edit, Trash2, Lock, Sparkles } from "lucide-react";
import type { RequestSubject } from "@/services/admin/AdminRequestSubjectService";
import { cn } from "@/lib/utils";

interface SubjectCardProps {
  subject: RequestSubject;
  canUpdate: boolean;
  canDelete: boolean;
  onEdit: (subject: RequestSubject) => void;
  onDelete: (id: number) => void;
}

export const SubjectCard: React.FC<SubjectCardProps> = ({ 
  subject, 
  canUpdate, 
  canDelete, 
  onEdit, 
  onDelete 
}) => {
  return (
    <div className={cn(
      "bg-white border border-slate-200 rounded-[2.5rem] p-6 shadow-sm hover:shadow-2xl hover:shadow-indigo-500/10 hover:border-indigo-200 transition-all duration-500 group relative overflow-hidden flex flex-col h-full",
      !subject.isActive && "opacity-75 grayscale-[0.2]"
    )}>
        {/* Decorative elements */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-slate-50 rounded-full -mr-16 -mt-16 group-hover:bg-indigo-50 transition-colors duration-500" />
        
        <div className="flex items-start justify-between mb-6 relative z-10">
            <div className={cn(
              "w-12 h-12 rounded-2xl flex items-center justify-center text-white shadow-lg transition-transform group-hover:scale-110 duration-500 shrink-0",
              subject.isActive 
                ? "bg-indigo-600 shadow-indigo-600/20" 
                : "bg-slate-400 shadow-slate-400/20"
            )}>
                <MessageSquare size={24} />
            </div>
            
            <div className="flex gap-1.5 md:translate-x-4 md:opacity-0 md:group-hover:translate-x-0 md:group-hover:opacity-100 transition-all duration-300">
                {canUpdate && (
                    <button 
                        onClick={() => onEdit(subject)}
                        disabled={subject.isLocked}
                        className={cn(
                            "w-10 h-10 flex items-center justify-center rounded-xl bg-white border border-slate-100 transition-all shadow-sm active:scale-90",
                            subject.isLocked ? "text-slate-200 opacity-50 cursor-not-allowed" : "text-slate-400 hover:text-amber-600 hover:bg-amber-50 hover:border-amber-100"
                        )}
                        title={subject.isLocked ? "Kilitli" : "Düzenle"}
                    >
                        <Edit size={18} />
                    </button>
                )}
                {canDelete && (
                    <button 
                        onClick={() => onDelete(subject.id)}
                        disabled={subject.isLocked}
                        className={cn(
                            "w-10 h-10 flex items-center justify-center rounded-xl bg-white border border-slate-100 transition-all shadow-sm active:scale-90",
                            subject.isLocked ? "text-slate-200 opacity-50 cursor-not-allowed" : "text-slate-400 hover:text-red-600 hover:bg-red-50 hover:border-red-100"
                        )}
                        title={subject.isLocked ? "Kilitli" : "Sil"}
                    >
                        <Trash2 size={18} />
                    </button>
                )}
            </div>
        </div>
        
        <div className="space-y-3 flex-1 relative z-10">
          <div className="flex items-center gap-2">
            <h3 className="text-xl font-black text-slate-900 tracking-tight truncate group-hover:text-indigo-600 transition-colors">
              {subject.name}
            </h3>
            {subject.isActive ? (
               <Sparkles size={14} className="text-indigo-500 animate-pulse hidden group-hover:block" />
            ) : (
              <span className="bg-slate-100 text-slate-500 text-[8px] font-black px-1.5 py-0.5 rounded-lg border border-slate-200 uppercase tracking-tight shrink-0">Pasif</span>
            )}
          </div>
          
          <p className="text-[13px] text-slate-500 leading-relaxed line-clamp-2 min-h-[40px] font-medium italic">
              {subject.description || "Bu kategori için açıklama tanımlanmamış."}
          </p>
        </div>

        <div className="mt-8 flex items-center justify-between gap-2 relative z-10">
            <div className="flex items-center gap-2.5">
                <div className={cn(
                  "w-2 h-2 rounded-full", 
                  subject.isActive ? "bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]" : "bg-slate-300"
                )} />
                <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">
                  {subject.isActive ? "Erişilebilir" : "Gizlenmiş"}
                </span>
            </div>
            
            <div className="flex items-center gap-2">
                {subject.isLocked && (
                  <div className="flex items-center gap-1.5 bg-amber-50 text-amber-600 text-[9px] font-black px-3 py-1.5 rounded-full uppercase tracking-widest border border-amber-100">
                     <Lock size={12} /> Kilitli
                  </div>
                )}
                <div className="flex items-center gap-1.5 bg-indigo-50 text-indigo-600 text-[9px] font-black px-3 py-1.5 rounded-full uppercase tracking-widest border border-indigo-100 italic">
                    {subject.usageCount} Kayıt
                </div>
            </div>
        </div>
    </div>
  );
};
