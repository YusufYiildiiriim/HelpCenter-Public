"use client";

import React from "react";
import { FolderKanban, Users, Edit, Trash2, Building2, Sparkles } from "lucide-react";
import type { Project } from "@/services/admin/AdminProjectService";
import { cn } from "@/lib/utils";

interface ProjectCardProps {
  project: Project;
  onEdit: (project: Project) => void;
  onDelete: (publicId: string) => void;
  onManageUsers: (project: Project) => void;
}

export const ProjectCard: React.FC<ProjectCardProps> = ({
  project,
  onEdit,
  onDelete,
  onManageUsers
}) => {
  return (
    <div className={cn(
      "bg-white border border-slate-200 rounded-[2.5rem] p-6 shadow-sm hover:shadow-2xl hover:shadow-indigo-500/10 hover:border-indigo-200 transition-all duration-500 group relative overflow-hidden flex flex-col h-full",
      !project.isActive && "opacity-75 grayscale-[0.2]"
    )}>
        {/* Decorative elements */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-slate-50 rounded-full -mr-16 -mt-16 group-hover:bg-indigo-50 transition-colors duration-500" />

        <div className="flex items-start justify-between mb-6 relative z-10">
            <div className={cn(
              "w-12 h-12 rounded-2xl flex items-center justify-center text-white shadow-lg transition-transform group-hover:scale-110 duration-500 shrink-0",
              project.isActive
                ? "bg-indigo-600 shadow-indigo-600/20"
                : "bg-slate-400 shadow-slate-400/20"
            )}>
                <FolderKanban size={24} />
            </div>

            {/* Actions: Visible on mobile, hover-only on desktop */}
            <div className="flex gap-1.5 md:translate-x-4 md:opacity-0 md:group-hover:translate-x-0 md:group-hover:opacity-100 transition-all duration-300">
                <button
                  onClick={() => onManageUsers(project)}
                  className="w-9 h-9 md:w-10 md:h-10 flex items-center justify-center rounded-xl bg-white border border-slate-100 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 hover:border-indigo-100 shadow-sm transition-all active:scale-90"
                  title="Kullanıcıları Yönet"
                >
                    <Users size={18} />
                </button>
                <button
                  onClick={() => onEdit(project)}
                  className="w-9 h-9 md:w-10 md:h-10 flex items-center justify-center rounded-xl bg-white border border-slate-100 text-slate-400 hover:text-amber-600 hover:bg-amber-50 hover:border-amber-100 transition-all shadow-sm active:scale-90"
                  title="Düzenle"
                >
                    <Edit size={18} />
                </button>
                <button
                  onClick={() => onDelete(project.publicId)}
                  className="w-9 h-9 md:w-10 md:h-10 flex items-center justify-center rounded-xl bg-white border border-slate-100 text-slate-400 hover:text-red-600 hover:bg-red-50 hover:border-red-100 transition-all shadow-sm active:scale-90"
                  title="Sil"
                >
                    <Trash2 size={18} />
                </button>
            </div>
        </div>

        <div className="space-y-3 flex-1 relative z-10">
          <div className="flex items-center gap-2">
            <h3 className="text-xl font-black text-slate-900 tracking-tight truncate group-hover:text-indigo-600 transition-colors">
              {project.name}
            </h3>
            {project.isActive ? (
               <Sparkles size={14} className="text-indigo-500 animate-pulse hidden group-hover:block" />
            ) : (
              <span className="bg-slate-100 text-slate-500 text-[8px] font-black px-1.5 py-0.5 rounded-lg border border-slate-200 uppercase tracking-tight shrink-0">Pasif</span>
            )}
          </div>

          <p className="text-[13px] text-slate-500 leading-relaxed line-clamp-2 min-h-[40px] font-medium">
              {project.description || "Bu proje için açıklama tanımlanmamış."}
          </p>
        </div>

        <div className="mt-8 flex items-center justify-between gap-2 relative z-10">
            <div className="flex items-center gap-2.5">
                <div className={cn(
                  "w-2 h-2 rounded-full",
                  project.isActive ? "bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]" : "bg-slate-300"
                )} />
                <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">
                  {project.isActive ? "Operasyonel" : "Devre Dışı"}
                </span>
            </div>

            <div className="flex items-center gap-2">
                <div className="flex items-center gap-1.5 bg-slate-50 text-slate-500 text-[9px] font-black px-3 py-1.5 rounded-full uppercase tracking-widest border border-slate-100">
                    <Building2 size={12} className="mr-1" />{project.companyCount} Şirket
                </div>
                <div className="flex items-center gap-1.5 bg-indigo-50 text-indigo-600 text-[9px] font-black px-3 py-1.5 rounded-full uppercase tracking-widest border border-indigo-100 italic">
                    {project.userCount} Kullanıcı
                </div>
            </div>
        </div>
    </div>
  );
};
