import React from "react";
import { Trash2, Loader2, ShieldCheck } from "lucide-react";
import type { Role } from "@/services/admin/AdminRoleService";
import { cn } from "@/lib/utils";

interface RoleCardProps {
  role: Role;
  onDelete: (id: number) => void;
  onManagePermissions: (role: Role) => void;
  deletingId: number | null;
  canDelete: boolean;
  canUpdate: boolean;
}

export const RoleCard: React.FC<RoleCardProps> = ({ 
  role, 
  onDelete, 
  onManagePermissions, 
  deletingId,
  canDelete,
  canUpdate
}) => {
  return (
    <div className="group relative bg-slate-900/90 border border-slate-800 rounded-2xl p-1 shadow-xl shadow-slate-950/30 hover:shadow-2xl hover:shadow-indigo-950/40 hover:border-indigo-500/40 transition-all duration-500 overflow-hidden h-full flex flex-col">
        {/* Decorative Background */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-slate-800/40 rounded-full -mr-16 -mt-16 group-hover:bg-indigo-600/10 transition-colors duration-500" />
        
        <div className="relative p-5 md:p-7 space-y-6 flex-1 flex flex-col">
            <div className="flex justify-between items-start gap-4">
                <div className="space-y-2 flex-1">
                    <div className="flex items-center gap-2">
                        <div className={cn(
                            "w-2 h-2 rounded-full",
                            role.name === "Admin" ? "bg-amber-500" : "bg-emerald-500"
                        )} />
                        <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">
                            {role.name === "Admin" ? "Sistem Rolü" : "Özel Rol"}
                        </span>
                    </div>
                    <h3 className="text-xl md:text-2xl font-black text-white tracking-tight leading-none group-hover:text-indigo-300 transition-colors">
                        {role.name}
                    </h3>
                    <p className="text-sm text-slate-400 font-medium line-clamp-2 min-h-[40px]">
                        {role.description || "Bu rol için herhangi bir açıklama tanımlanmamış."}
                    </p>
                </div>
                
                {role.name !== "Admin" && canDelete && (
                    <button
                        onClick={() => !role.hasUsers && onDelete(role.id)}
                        disabled={deletingId === role.id || role.hasUsers}
                        className={cn(
                            "p-3 rounded-2xl transition-all shadow-sm bg-slate-800/80 border border-slate-700 shrink-0 md:opacity-0 md:group-hover:opacity-100 md:translate-x-4 md:group-hover:translate-x-0",
                            role.hasUsers
                                ? "text-slate-600 cursor-not-allowed"
                                : "text-slate-400 hover:text-red-400 hover:bg-red-500/15 hover:border-red-500/30"
                        )}
                        title={role.hasUsers ? "Bu role atanmış kullanıcılar var, silinemez" : "Rolü Sil"}
                    >
                        {deletingId === role.id ? <Loader2 className="animate-spin" size={18} /> : <Trash2 size={18} />}
                    </button>
                )}
            </div>

            {/* Permission Summary */}
            <div className="flex items-center gap-3">
                <div className={cn(
                    "flex items-center gap-2 px-3 py-2 rounded-2xl text-xs font-bold border",
                    role.permissionCount > 0
                        ? "bg-indigo-500/10 border-indigo-500/30 text-indigo-300"
                        : "bg-slate-800 border-slate-700 text-slate-400"
                )}>
                    <ShieldCheck size={13} />
                    {role.permissionCount > 0
                        ? `${role.permissionCount} kaynak erişimi`
                        : "Erişim yok"}
                </div>
            </div>

            <div className="pt-4 mt-auto space-y-2">
                {canUpdate && (
                    <button 
                        onClick={() => onManagePermissions(role)}
                        className="w-full py-3 bg-gradient-to-r from-indigo-600 to-violet-600 text-white rounded-2xl font-black uppercase tracking-widest text-[10px] hover:from-indigo-500 hover:to-violet-500 shadow-lg shadow-indigo-600/20 transition-all flex items-center justify-center gap-2 active:scale-95"
                    >
                        <ShieldCheck size={16} />
                        Yetkiler
                    </button>
                )}
            </div>
        </div>
    </div>
  );
};
