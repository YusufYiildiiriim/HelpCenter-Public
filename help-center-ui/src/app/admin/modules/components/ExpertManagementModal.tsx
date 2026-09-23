"use client";

import React from "react";
import { X, Users, CheckCircle2, Trash2, UserPlus, Plus, Loader2, ShieldCheck, Sparkles } from "lucide-react";
import type { User } from "@/services/admin/AdminUserService";
import type { ModuleExpertDto } from "@/services/admin/AdminModuleExpertService";

interface ExpertManagementModalProps {
  onClose: () => void;
  expertLoading: boolean;
  currentExperts: ModuleExpertDto[];
  allUsers: User[];
  onAssignExpert: (userId: number) => void;
  onRemoveExpert: (expertId: number) => void;
}

export const ExpertManagementModal: React.FC<ExpertManagementModalProps> = ({
  onClose,
  expertLoading,
  currentExperts,
  allUsers,
  onAssignExpert,
  onRemoveExpert
}) => {
  return (
    <div className="fixed inset-0 z-[110] flex items-center justify-center p-0 md:p-4 lg:pl-76 lg:pr-6">
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-slate-900/60 backdrop-blur-md animate-in fade-in duration-300" 
        onClick={onClose} 
      />
      
      {/* Modal Container */}
      <div className="relative w-full max-w-3xl bg-slate-900 md:rounded-2xl shadow-2xl border border-slate-800 flex flex-col h-full md:h-auto md:max-h-[85vh] animate-in zoom-in slide-in-from-bottom-4 duration-300 overflow-hidden">
        
        {/* Header */}
        <div className="p-6 md:p-8 border-b border-slate-800 flex justify-between items-center bg-slate-900 shrink-0">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20">
                <Users size={24} />
            </div>
            <div>
                <h2 className="text-xl md:text-2xl font-black text-slate-100 leading-none tracking-tight">Modül Uzman Havuzu</h2>
                <p className="text-slate-500 text-[10px] md:text-xs font-bold uppercase tracking-widest mt-1.5 flex items-center gap-2">
                   <ShieldCheck size={12} className="text-indigo-400" />
                   Departman Yetkilendirme Paneli
                </p>
            </div>
          </div>
          <button 
            onClick={onClose} 
            className="w-10 h-10 flex items-center justify-center rounded-xl bg-slate-800/70 border border-slate-700 text-slate-400 hover:bg-red-500/10 hover:text-red-400 hover:border-red-500/30 transition-all shadow-sm active:scale-90"
          >
            <X size={20} />
          </button>
        </div>
        
        <div className="flex-1 p-6 md:p-8 grid grid-cols-1 md:grid-cols-2 gap-8 md:gap-12 overflow-y-auto custom-scrollbar">
            {/* Current Experts Section */}
            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <h3 className="text-[10px] md:text-xs font-black text-slate-400 uppercase tracking-widest flex items-center gap-2">
                        <CheckCircle2 size={14} className="text-emerald-400" />
                        Aktif Uzmanlar
                    </h3>
                    <span className="text-[10px] font-black px-2 py-0.5 bg-emerald-500/15 text-emerald-300 rounded-full border border-emerald-500/30">
                        {currentExperts.length}
                    </span>
                </div>

                <div className="space-y-3">
                    {expertLoading ? (
                        <div className="flex flex-col items-center justify-center py-12 gap-3 opacity-40">
                          <Loader2 className="animate-spin text-indigo-400" size={24} />
                          <p className="text-[10px] font-black uppercase tracking-widest text-slate-400">Liste Güncelleniyor...</p>
                        </div>
                    ) : currentExperts.length > 0 ? currentExperts.map((exp) => (
                        <div key={exp.id} className="flex items-center justify-between p-4 bg-slate-800/50 rounded-2xl border border-slate-700 group hover:bg-slate-800 hover:shadow-lg hover:shadow-indigo-600/10 hover:border-indigo-500/40 transition-all duration-300">
                            <div className="flex items-center gap-4 min-w-0">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center text-white font-black text-[10px] shadow-lg shadow-indigo-600/20">
                                    {exp.fullName.substring(0, 2).toUpperCase()}
                                </div>
                                <div className="min-w-0">
                                    <p className="text-[11px] font-black uppercase text-slate-100 truncate">{exp.fullName}</p>
                                    <p className="text-[9px] text-slate-500 font-bold truncate font-mono tracking-tighter mt-0.5">{exp.email}</p>
                                </div>
                            </div>
                            <button 
                                onClick={() => onRemoveExpert(exp.id)}
                                className="w-9 h-9 flex items-center justify-center rounded-xl text-slate-500 hover:text-red-400 hover:bg-red-500/10 hover:border-red-500/30 border border-transparent transition-all shrink-0 active:scale-90"
                                title="Uzmanı Kaldır"
                            >
                                <Trash2 size={16} />
                            </button>
                        </div>
                    )) : (
                        <div className="flex flex-col items-center justify-center py-16 gap-4 border-2 border-dashed border-slate-700 rounded-2xl bg-slate-800/30">
                           <div className="w-12 h-12 rounded-2xl bg-slate-800 flex items-center justify-center text-slate-600 shadow-sm">
                             <Users size={24} />
                           </div>
                           <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest text-center px-6 leading-relaxed">
                              Bu modüle henüz atanmış bir uzman bulunmuyor.
                           </p>
                        </div>
                    )}
                </div>
            </div>

            {/* Assignable Users Section */}
            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <h3 className="text-[10px] md:text-xs font-black text-slate-400 uppercase tracking-widest flex items-center gap-2">
                        <UserPlus size={14} className="text-indigo-400" />
                        Havuzdan Uzman Ata
                    </h3>
                </div>

                <div className="space-y-3">
                    {expertLoading ? (
                        <div className="flex flex-col items-center justify-center py-12 gap-3 opacity-40">
                          <Loader2 className="animate-spin text-indigo-400" size={24} />
                        </div>
                    ) : allUsers.filter(u => !currentExperts.some(e => e.userId === u.id)).map((user) => (
                        <button 
                            key={user.id}
                            onClick={() => onAssignExpert(user.id)}
                            className="w-full flex items-center justify-between p-4 bg-slate-800/50 hover:bg-indigo-600/10 border border-slate-700 hover:border-indigo-500/40 rounded-2xl transition-all duration-300 group text-left active:scale-[0.98] hover:shadow-xl hover:shadow-indigo-600/10"
                        >
                            <div className="flex items-center gap-4 min-w-0">
                                <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center text-slate-500 font-black text-[10px] group-hover:bg-gradient-to-br group-hover:from-indigo-600 group-hover:to-violet-600 group-hover:text-white group-hover:shadow-lg group-hover:shadow-indigo-600/20 transition-all duration-500">
                                    {user.name.substring(0, 1).toUpperCase()}{user.lastName.substring(0, 1).toUpperCase()}
                                </div>
                                <div className="min-w-0">
                                    <p className="text-[11px] font-black uppercase text-slate-200 truncate group-hover:text-slate-100 transition-colors">{user.name} {user.lastName}</p>
                                    <p className="text-[9px] text-slate-500 font-bold truncate font-mono tracking-tighter mt-0.5 group-hover:text-indigo-300 transition-colors">{user.email}</p>
                                </div>
                            </div>
                            <div className="w-8 h-8 rounded-full bg-slate-800 flex items-center justify-center text-slate-500 group-hover:bg-gradient-to-br group-hover:from-indigo-600 group-hover:to-violet-600 group-hover:text-white transition-all shadow-sm">
                                <Plus size={16} strokeWidth={3} />
                            </div>
                        </button>
                    ))}
                </div>
            </div>
        </div>

        {/* Footer */}
        <div className="p-6 md:p-8 bg-slate-900 border-t border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-4 shrink-0">
            <div className="flex items-center gap-2 opacity-50">
               <Sparkles size={14} className="text-indigo-400" />
               <p className="text-[9px] font-black uppercase tracking-[0.2em] text-slate-500">Global Havuz Erişimi</p>
            </div>
            <button
                onClick={onClose}
                className="w-full sm:w-auto px-10 py-4 bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white rounded-2xl text-[10px] font-black uppercase tracking-widest transition-all active:scale-95 shadow-lg shadow-indigo-600/20"
            >
                Yönetimi Tamamla
            </button>
        </div>
      </div>
    </div>
  );
};
