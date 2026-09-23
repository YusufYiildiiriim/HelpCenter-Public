"use client";

import React from "react";
import { Search, X, Users } from "lucide-react";

interface UserFiltersProps {
  searchTerm: string;
  setSearchTerm: (value: string) => void;
  totalCount: number;
}

export const UserFilters: React.FC<UserFiltersProps> = ({ searchTerm, setSearchTerm, totalCount }) => {
  return (
    <div className="p-4 sm:p-5 border-b border-slate-100 flex flex-col sm:flex-row justify-between items-stretch sm:items-center gap-3.5 bg-white/90 backdrop-blur-xl">
      {/* Search Input */}
      <div className="relative flex-1 max-w-md">
        <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" size={17} />
        <input
          type="text"
          placeholder="Ad, soyad veya e-posta ile ara..."
          className="w-full bg-slate-50 hover:bg-slate-100/80 focus:bg-white text-slate-900 placeholder:text-slate-400 text-xs sm:text-sm font-semibold pl-10 pr-9 py-2.5 rounded-2xl border border-slate-200/80 focus:border-indigo-500 focus:outline-hidden transition-all shadow-xs"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        {searchTerm && (
          <button
            onClick={() => setSearchTerm("")}
            className="absolute right-3 top-1/2 -translate-y-1/2 p-1 hover:bg-slate-200/60 rounded-lg text-slate-400 hover:text-slate-600 transition-all"
          >
            <X size={14} />
          </button>
        )}
      </div>

      {/* User Count Badge */}
      <div className="flex items-center justify-between sm:justify-end gap-3 shrink-0">
        <div className="flex items-center gap-2.5 bg-slate-50 border border-slate-200/80 px-3.5 py-2 rounded-2xl">
          <div className="w-6 h-6 rounded-lg bg-indigo-100 text-indigo-600 flex items-center justify-center">
            <Users size={13} />
          </div>
          <div>
            <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest block leading-none">Toplam</span>
            <span className="text-xs font-black text-slate-800">{totalCount} kullanıcı</span>
          </div>
        </div>
      </div>
    </div>
  );
};