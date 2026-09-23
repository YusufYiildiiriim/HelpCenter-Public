"use client";

import React from "react";
import { CheckCircle2, Loader2, Plus, ShieldCheck, Sparkles, Trash2, UserPlus, Users, X } from "lucide-react";
import type { User } from "@/services/admin/AdminUserService";
import type { ProjectUserDto } from "@/services/admin/AdminUserProjectService";

interface UserManagementModalProps {
  onClose: () => void;
  userLoading: boolean;
  currentUsers: ProjectUserDto[];
  allUsers: User[];
  onAssignUser: (userId: number) => void;
  onRemoveUser: (id: number) => void;
}

export const UserManagementModal: React.FC<UserManagementModalProps> = ({
  onClose,
  userLoading,
  currentUsers,
  allUsers,
  onAssignUser,
  onRemoveUser,
}) => {
  const assignableUsers = allUsers.filter((user) => !currentUsers.some((currentUser) => currentUser.userId === user.id));

  return (
    <div className="fixed inset-0 z-[110] flex items-center justify-center overflow-y-auto bg-slate-950/80 p-2 backdrop-blur-md animate-in fade-in duration-300 sm:p-4 md:p-6 lg:pl-76 lg:pr-6">
      <div className="relative my-auto flex max-h-[92vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl border border-slate-800 bg-slate-900 shadow-2xl shadow-slate-950/50 sm:max-h-[85vh] sm:rounded-3xl">
        <header className="flex shrink-0 items-center justify-between gap-4 border-b border-slate-800 bg-slate-900 p-4 sm:p-6 md:p-8">
          <div className="flex min-w-0 items-center gap-3 sm:gap-4">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20 sm:h-12 sm:w-12 sm:rounded-2xl">
              <Users size={20} className="sm:hidden" />
              <Users size={24} className="hidden sm:block" />
            </div>
            <div className="min-w-0">
              <h2 className="truncate text-base font-black tracking-tight text-white sm:text-xl md:text-2xl">Proje Kullanıcı Havuzu</h2>
              <p className="mt-1 flex items-center gap-1.5 truncate text-[9px] font-bold uppercase tracking-widest text-slate-400 sm:text-xs">
                <ShieldCheck size={12} className="text-indigo-400" />
                Proje Bazlı Yetkilendirme Paneli
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Kullanıcı yönetimini kapat"
            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-slate-700 bg-slate-800 text-slate-400 transition-colors hover:border-red-500/30 hover:bg-red-500/15 hover:text-red-400 sm:h-10 sm:w-10"
          >
            <X size={20} />
          </button>
        </header>

        <main className="grid flex-1 grid-cols-1 gap-8 overflow-y-auto p-4 custom-scrollbar sm:p-6 md:grid-cols-2 md:gap-10 md:p-8">
          <section className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest text-slate-400 md:text-xs">
                <CheckCircle2 size={14} className="text-emerald-400" />
                Projede Çalışanlar
              </h3>
              <span className="rounded-full border border-emerald-500/30 bg-emerald-500/15 px-2 py-0.5 text-[10px] font-black text-emerald-400">
                {currentUsers.length}
              </span>
            </div>

            {userLoading ? (
              <div className="flex flex-col items-center justify-center gap-3 py-12 text-slate-400">
                <Loader2 className="animate-spin text-indigo-400" size={24} />
                <p className="text-[10px] font-black uppercase tracking-widest">Liste güncelleniyor...</p>
              </div>
            ) : currentUsers.length > 0 ? (
              <div className="space-y-3">
                {currentUsers.map((user) => (
                  <div key={user.id} className="flex items-center justify-between gap-3 rounded-2xl border border-slate-700/70 bg-slate-800/60 p-3.5 transition-all hover:border-indigo-500/50 hover:shadow-xl hover:shadow-indigo-600/5 sm:p-4">
                    <div className="flex min-w-0 items-center gap-3">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-indigo-500/15 text-[10px] font-black text-indigo-300 ring-1 ring-indigo-500/30">
                        {user.fullName.substring(0, 2).toUpperCase()}
                      </div>
                      <div className="min-w-0">
                        <p className="truncate text-[11px] font-black uppercase text-slate-100">{user.fullName}</p>
                        <p className="mt-0.5 truncate font-mono text-[9px] font-bold tracking-tighter text-slate-400">{user.email || "E-posta bilgisi yok"}</p>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => onRemoveUser(user.id)}
                      aria-label={`${user.fullName} kullanıcısını projeden kaldır`}
                      title="Kullanıcıyı kaldır"
                      className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-transparent bg-slate-800 text-slate-400 transition-all hover:border-red-500/30 hover:bg-red-500/15 hover:text-red-400"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center gap-4 rounded-2xl border-2 border-dashed border-slate-700/70 bg-slate-800/30 px-4 py-14">
                <div className="flex h-12 w-12 items-center justify-center rounded-2xl border border-slate-700 bg-slate-800 text-slate-400">
                  <Users size={24} />
                </div>
                <p className="text-center text-[10px] font-black uppercase tracking-widest text-slate-400">Bu projeye henüz bir kullanıcı atanmamış.</p>
              </div>
            )}
          </section>

          <section className="space-y-4">
            <h3 className="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest text-slate-400 md:text-xs">
              <UserPlus size={14} className="text-indigo-400" />
              Havuzdan Kullanıcı Ata
            </h3>

            {userLoading ? (
              <div className="flex items-center justify-center py-12">
                <Loader2 className="animate-spin text-indigo-400" size={24} />
              </div>
            ) : assignableUsers.length > 0 ? (
              <div className="space-y-3">
                {assignableUsers.map((user) => (
                  <button
                    key={user.id}
                    type="button"
                    onClick={() => onAssignUser(user.id)}
                    className="group flex w-full items-center justify-between gap-3 rounded-2xl border border-slate-700/70 bg-slate-800/60 p-3.5 text-left transition-all hover:border-indigo-500/50 hover:bg-indigo-500/10 hover:shadow-xl hover:shadow-indigo-600/5 sm:p-4"
                  >
                    <div className="flex min-w-0 items-center gap-3">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-slate-700 text-[10px] font-black text-slate-300 transition-all group-hover:bg-indigo-600 group-hover:text-white">
                        {user.name.substring(0, 1).toUpperCase()}{user.lastName.substring(0, 1).toUpperCase()}
                      </div>
                      <div className="min-w-0">
                        <p className="truncate text-[11px] font-black uppercase text-slate-100 transition-colors group-hover:text-indigo-200">{user.name} {user.lastName}</p>
                        <p className="mt-0.5 truncate font-mono text-[9px] font-bold tracking-tighter text-slate-400 group-hover:text-indigo-300">{user.email || "E-posta bilgisi yok"}</p>
                      </div>
                    </div>
                    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-700 text-slate-400 transition-all group-hover:bg-indigo-600 group-hover:text-white">
                      <Plus size={16} strokeWidth={3} />
                    </span>
                  </button>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-dashed border-slate-700/70 bg-slate-800/30 px-4 py-14 text-center">
                <UserPlus size={22} className="text-slate-500" />
                <p className="text-[10px] font-black uppercase tracking-widest text-slate-400">Atanabilecek kullanıcı kalmadı.</p>
              </div>
            )}
          </section>
        </main>

        <footer className="flex shrink-0 flex-col items-center justify-between gap-4 border-t border-slate-800 bg-slate-900 p-4 sm:flex-row sm:p-6 md:p-8">
          <div className="flex items-center gap-2">
            <Sparkles size={14} className="text-indigo-400" />
            <p className="text-[9px] font-black uppercase tracking-[0.2em] text-slate-400">Proje Erişim Havuzu</p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="w-full rounded-xl border border-slate-700 bg-slate-800 px-7 py-3 text-[10px] font-black uppercase tracking-widest text-slate-200 transition-all hover:bg-slate-700 hover:text-white active:scale-95 sm:w-auto"
          >
            Yönetimi Tamamla
          </button>
        </footer>
      </div>
    </div>
  );
};
