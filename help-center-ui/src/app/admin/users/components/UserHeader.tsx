"use client";

import React from "react";
import { Users, UserPlus, Sparkles } from "lucide-react";

interface UserHeaderProps {
  canCreate: boolean;
  onAddClick: () => void;
}

export const UserHeader: React.FC<UserHeaderProps> = ({ canCreate, onAddClick }) => {
  return (
    <div className="relative overflow-hidden rounded-2xl sm:rounded-3xl bg-slate-900/95 p-4 sm:p-6 md:p-8 text-white shadow-xl shadow-slate-950/20 backdrop-blur-xl border border-slate-800">
      {/* Animated background glows */}
      <div className="pointer-events-none absolute -top-24 -right-24 h-72 w-72 rounded-full bg-gradient-to-br from-indigo-600/30 to-violet-600/20 blur-3xl transition-transform duration-700 animate-pulse" />
      <div className="pointer-events-none absolute -bottom-20 -left-16 h-64 w-64 rounded-full bg-indigo-600/10 blur-3xl" />
      
      {/* Grid pattern overlay */}
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(#818cf8_1px,transparent_1px)] [background-size:16px_16px] opacity-10" />

      <div className="relative z-10 flex flex-col sm:flex-row sm:items-center justify-between gap-6">
        <div className="flex items-start sm:items-center gap-4">
          {/* Main Icon */}
          <div className="relative group shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-70 group-hover:opacity-100 transition duration-500" />
            <div className="relative flex h-12 w-12 sm:h-14 sm:w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 text-white shadow-xl shadow-indigo-600/30 transition-transform duration-500 group-hover:scale-105">
              <Users className="h-6 w-6 sm:h-7 sm:w-7" />
            </div>
          </div>

          <div>
            <div className="flex flex-wrap items-center gap-2.5">
              <h1 className="text-xl sm:text-2xl md:text-3xl font-black tracking-tight text-white">
                Kullanıcı Yönetimi
              </h1>
              <span className="inline-flex items-center gap-1.5 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-2.5 py-0.5 text-[10px] font-black uppercase tracking-wider text-indigo-300">
                <Sparkles size={11} className="animate-pulse text-indigo-400" />
                Personel & Yetkiler
              </span>
            </div>
            <p className="mt-1.5 text-xs sm:text-sm font-medium text-slate-300 max-w-xl">
              Sistem personellerini yönetin, rol ve erişim izinlerini düzenleyin.
            </p>
          </div>
        </div>

        {canCreate && (
          <button
            onClick={onAddClick}
            className="group relative flex items-center justify-center gap-2.5 bg-indigo-600 hover:bg-indigo-500 text-white px-5 py-3 sm:px-6 sm:py-3.5 rounded-2xl text-xs sm:text-sm font-black tracking-wide transition-all shadow-lg shadow-indigo-600/30 active:scale-95 shrink-0"
          >
            <div className="flex h-6 w-6 items-center justify-center rounded-lg bg-white/20 text-white transition-transform group-hover:scale-110">
              <UserPlus size={15} />
            </div>
            <span>Yeni Kullanıcı Ekle</span>
          </button>
        )}
      </div>
    </div>
  );
};