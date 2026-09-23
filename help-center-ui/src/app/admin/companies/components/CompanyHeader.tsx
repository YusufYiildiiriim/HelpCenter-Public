import React from "react";
import { Plus, Building2 } from "lucide-react";

interface CompanyHeaderProps {
  canCreate: boolean;
  onAddClick: () => void;
}

export const CompanyHeader: React.FC<CompanyHeaderProps> = ({ canCreate, onAddClick }) => {
  return (
    <div className="relative overflow-hidden rounded-2xl bg-slate-900/95 p-6 md:p-8 text-white shadow-xl shadow-slate-950/20 backdrop-blur-xl border border-slate-800">
      {/* Animated background glows */}
      <div className="pointer-events-none absolute -top-24 -right-24 h-72 w-72 rounded-full bg-gradient-to-br from-indigo-600/30 to-violet-600/20 blur-3xl animate-pulse" />
      <div className="pointer-events-none absolute -bottom-20 -left-16 h-64 w-64 rounded-full bg-indigo-600/10 blur-3xl" />

      <div className="relative z-10 flex flex-col sm:flex-row sm:items-center justify-between gap-5">
        <div className="flex items-center gap-4">
          <div className="relative group shrink-0">
            <div className="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-violet-600 rounded-2xl blur-md opacity-70 group-hover:opacity-100 transition duration-500" />
            <div className="relative flex h-12 w-12 sm:h-14 sm:w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 text-white shadow-xl shadow-indigo-600/30 transition-transform duration-500 group-hover:scale-105">
              <Building2 className="h-6 w-6 sm:h-7 sm:w-7" />
            </div>
          </div>

          <div>
            <h1 className="text-xl sm:text-2xl md:text-3xl font-black tracking-tight text-white">
              Kurumsal Firmalar
            </h1>
            <p className="mt-1 text-xs sm:text-sm font-medium text-slate-300 max-w-xl">
              Sistemi kullanan şirketleri ve onların iletişim bilgilerini yönetin.
            </p>
          </div>
        </div>

        {canCreate && (
          <button
            onClick={onAddClick}
            className="group relative flex items-center justify-center gap-2.5 bg-indigo-600 hover:bg-indigo-500 text-white px-5 py-3 sm:px-6 sm:py-3.5 rounded-xl text-xs sm:text-sm font-black tracking-wide transition-all shadow-lg shadow-indigo-600/30 active:scale-95 shrink-0"
          >
            <div className="flex h-6 w-6 items-center justify-center rounded-lg bg-white/20 text-white transition-transform group-hover:scale-110">
              <Plus size={16} />
            </div>
            <span>Yeni Firma Tanımla</span>
          </button>
        )}
      </div>
    </div>
  );
};
