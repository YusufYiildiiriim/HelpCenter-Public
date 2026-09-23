import React from "react";
import { Building2, LogOut } from "lucide-react";
import type { CustomerProfile } from "@/services/customer/CustomerProfileService";

interface DashboardNavProps {
  profile: CustomerProfile | null;
  onLogout: () => void;
}

export const DashboardNav: React.FC<DashboardNavProps> = ({ profile, onLogout }) => {
  return (
    <nav className="h-20 bg-slate-900/90 backdrop-blur-xl border-b border-slate-800 px-8 flex items-center justify-between shadow-lg shadow-slate-950/40 sticky top-0 z-50">
        <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-linear-to-br from-indigo-600 to-violet-700 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20">
                <Building2 size={26} />
            </div>
            <div>
                <h2 className="font-black text-white tracking-tight text-lg leading-tight">{profile?.companyName || "Müşteri Portalı"}</h2>
                <div className="flex items-center gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Kurumsal Destek Paneli</span>
                </div>
            </div>
        </div>

        <div className="flex items-center gap-6">
            <div className="flex items-center gap-4">
                <div className="text-right hidden sm:block">
                    <p className="text-sm font-black text-white">{profile ? `${profile.firstName} ${profile.lastName}` : "Müşteri Yetkilisi"}</p>
                    <p className="text-[10px] text-slate-500 font-bold uppercase tracking-tighter">Firma Yöneticisi</p>
                </div>
                <button
                    onClick={onLogout}
                    className="w-12 h-12 rounded-2xl bg-slate-800 border border-slate-700 flex items-center justify-center text-slate-400 hover:text-red-400 hover:bg-red-500/10 hover:border-red-500/30 transition-all"
                >
                    <LogOut size={20} />
                </button>
            </div>
        </div>
    </nav>
  );
};
