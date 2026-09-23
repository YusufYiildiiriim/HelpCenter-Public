import React from "react";
import { Mail, Edit, Trash2, Users, Shield, Clock } from "lucide-react";
import { cn } from "@/lib/utils";
import type { User } from "@/services/admin/AdminUserService";

interface UserTableProps {
  loading: boolean;
  users: User[];
  canUpdate: boolean;
  canDelete: boolean;
  onEdit: (user: User) => void;
  onDelete: (id: number) => void;
  onManageRoles: (user: User) => void;
}

export const UserTable: React.FC<UserTableProps> = ({
  loading,
  users,
  canUpdate,
  canDelete,
  onEdit,
  onDelete,
  onManageRoles
}) => {
  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4">
        <div className="w-12 h-12 border-4 border-indigo-100 border-t-indigo-600 rounded-2xl animate-spin" />
        <p className="text-slate-500 font-bold text-xs uppercase tracking-wider">Kullanıcılar yükleniyor...</p>
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 px-6 gap-3 text-center">
        <div className="w-14 h-14 rounded-2xl bg-slate-100 flex items-center justify-center text-slate-400">
          <Users size={28} />
        </div>
        <p className="text-sm font-black text-slate-700">Kullanıcı Bulunamadı</p>
        <p className="text-xs text-slate-400 max-w-xs font-medium">
          Arama kriterlerine uygun herhangi bir kullanıcı kaydı mevcut değil.
        </p>
      </div>
    );
  }

  return (
    <div>
      {/* Desktop Table View */}
      <div className="hidden lg:block overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/80 border-b border-slate-100">
              <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Kullanıcı</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Durum</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">E-posta</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Roller</th>
              <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100/70">
            {users.map((user) => (
              <tr
                key={user.id}
                className={cn(
                  "hover:bg-slate-50/80 transition-all duration-150 group",
                  !user.isActive && "bg-slate-50/40 opacity-75"
                )}
              >
                <td className="px-6 py-3.5">
                  <div className="flex items-center gap-3.5">
                    <div className={cn(
                      "w-10 h-10 rounded-2xl flex items-center justify-center font-black text-xs transition-transform duration-300 shrink-0",
                      user.isActive
                        ? "bg-gradient-to-br from-indigo-500 to-violet-600 text-white shadow-md shadow-indigo-500/20 group-hover:scale-105"
                        : "bg-slate-200 text-slate-400"
                    )}>
                      {user.name.charAt(0)}{user.lastName.charAt(0)}
                    </div>
                    <div className="min-w-0">
                      <p className="text-sm font-bold text-white tracking-tight truncate">
                        {user.name} {user.lastName}
                      </p>
                      {user.createdAt && (
                        <p className="text-[10px] font-mono text-slate-400 flex items-center gap-1 mt-0.5">
                          <Clock size={10} />
                          {new Date(user.createdAt).toLocaleDateString("tr-TR")}
                        </p>
                      )}
                    </div>
                  </div>
                </td>

                <td className="px-6 py-3.5">
                  <span className={cn(
                    "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl text-[10px] font-black uppercase tracking-wider border",
                    user.isActive
                      ? "bg-emerald-50 text-emerald-600 border-emerald-200/60"
                      : "bg-slate-100 text-slate-400 border-slate-200"
                  )}>
                    <span className={cn("w-1.5 h-1.5 rounded-full", user.isActive ? "bg-emerald-500" : "bg-slate-400")} />
                    {user.isActive ? "Aktif" : "Pasif"}
                  </span>
                </td>

                <td className="px-6 py-3.5">
                  <div className="flex items-center gap-2 text-slate-600 font-medium text-xs sm:text-sm">
                    <Mail size={13} className="text-slate-400 shrink-0" />
                    <span className="truncate">{user.email}</span>
                  </div>
                </td>

                <td className="px-6 py-3.5">
                  <div className="flex flex-wrap gap-1.5 max-w-[240px]">
                    {user.userRoles?.length > 0 ? (
                      <>
                        {user.userRoles.slice(0, 3).map((ur) => (
                          <span key={ur.roleId} className="px-2 py-0.5 bg-indigo-50 text-indigo-700 rounded-lg text-[9px] font-extrabold uppercase tracking-wider border border-indigo-100">
                            {ur.roleName}
                          </span>
                        ))}
                        {user.userRoles.length > 3 && (
                          <span className="px-2 py-0.5 bg-slate-100 text-slate-500 rounded-lg text-[9px] font-bold">
                            +{user.userRoles.length - 3}
                          </span>
                        )}
                      </>
                    ) : (
                      <span className="text-[10px] text-slate-400 font-medium italic">Rol atanmamış</span>
                    )}
                  </div>
                </td>

                <td className="px-6 py-3.5 text-right">
                  <div className="flex items-center justify-end gap-1 opacity-80 group-hover:opacity-100 transition-opacity">
                    {canUpdate && (
                      <button
                        onClick={() => onManageRoles(user)}
                        className="p-2 hover:bg-indigo-50 rounded-xl text-slate-400 hover:text-indigo-600 transition-all active:scale-95"
                        title="Rolleri Yönet"
                      >
                        <Shield size={16} />
                      </button>
                    )}
                    {canUpdate && (
                      <button
                        onClick={() => onEdit(user)}
                        className="p-2 hover:bg-blue-50 rounded-xl text-slate-400 hover:text-blue-600 transition-all active:scale-95"
                        title="Düzenle"
                      >
                        <Edit size={16} />
                      </button>
                    )}
                    {canDelete && (
                      <button
                        onClick={() => onDelete(user.id)}
                        className="p-2 hover:bg-rose-50 rounded-xl text-slate-400 hover:text-rose-600 transition-all active:scale-95"
                        title="Sil"
                      >
                        <Trash2 size={16} />
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile & Tablet Card Layout */}
      <div className="lg:hidden flex flex-col gap-3 p-3.5 sm:p-4">
        {users.map((user) => (
          <div
            key={user.id}
            className={cn(
              "bg-white rounded-2xl border p-4 shadow-xs space-y-3 transition-all",
              !user.isActive ? "border-slate-200/60 bg-slate-50/50 opacity-80" : "border-slate-200 hover:border-indigo-200"
            )}
          >
            <div className="flex items-start justify-between gap-3">
              <div className="flex items-center gap-3 min-w-0 flex-1">
                <div className={cn(
                  "w-10 h-10 rounded-2xl flex items-center justify-center font-black text-xs shrink-0 text-white",
                  user.isActive ? "bg-gradient-to-br from-indigo-500 to-violet-600" : "bg-slate-300"
                )}>
                  {user.name.charAt(0)}{user.lastName.charAt(0)}
                </div>
                <div className="min-w-0">
                  <p className="text-sm font-bold text-slate-900 dark:text-white tracking-tight truncate">{user.name} {user.lastName}</p>
                  <p className="text-xs text-slate-500 font-medium truncate flex items-center gap-1.5">
                    <Mail size={11} className="text-slate-400 shrink-0" />
                    {user.email}
                  </p>
                </div>
              </div>

              <span className={cn(
                "px-2.5 py-0.5 rounded-lg text-[9px] font-black uppercase tracking-wider border shrink-0",
                user.isActive ? "bg-emerald-50 text-emerald-600 border-emerald-100" : "bg-slate-100 text-slate-400 border-slate-200"
              )}>
                {user.isActive ? "Aktif" : "Pasif"}
              </span>
            </div>

            {/* Roles */}
            <div className="flex flex-wrap gap-1">
              {user.userRoles?.length > 0 ? (
                user.userRoles.map((ur) => (
                  <span key={ur.roleId} className="px-2 py-0.5 bg-indigo-50 text-indigo-600 rounded-md text-[9px] font-bold uppercase tracking-wider border border-indigo-100">
                    {ur.roleName}
                  </span>
                ))
              ) : (
                <span className="text-[10px] text-slate-400 italic">Rol atanmamış</span>
              )}
            </div>

            {/* Action buttons */}
            <div className="flex items-center gap-2 pt-2 border-t border-slate-100">
              {canUpdate && (
                <button
                  onClick={() => onManageRoles(user)}
                  className="flex-1 flex items-center justify-center gap-1 py-2 rounded-xl bg-indigo-50 text-indigo-600 text-xs font-bold hover:bg-indigo-100 active:scale-95 transition-all"
                >
                  <Shield size={13} />
                  Roller
                </button>
              )}
              {canUpdate && (
                <button
                  onClick={() => onEdit(user)}
                  className="flex-1 flex items-center justify-center gap-1 py-2 rounded-xl bg-slate-100 text-slate-700 text-xs font-bold hover:bg-slate-200 active:scale-95 transition-all"
                >
                  <Edit size={13} />
                  Düzenle
                </button>
              )}
              {canDelete && (
                <button
                  onClick={() => onDelete(user.id)}
                  className="flex-1 flex items-center justify-center gap-1 py-2 rounded-xl bg-rose-50 text-rose-600 text-xs font-bold hover:bg-rose-100 active:scale-95 transition-all"
                >
                  <Trash2 size={13} />
                  Sil
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};