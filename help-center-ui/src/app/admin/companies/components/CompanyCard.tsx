import React from "react";
import { Building2, Key, Trash2, Users, Zap, MapPin, Phone, Mail, ChevronRight } from "lucide-react";
import type { Company } from "@/services/admin/AdminCompanyService";

interface CompanyCardProps {
  company: Company;
  canUpdate: boolean;
  canDelete: boolean;
  onEdit: (company: Company) => void;
  onDelete: (publicId: string) => void;
  onManageUsers: (company: Company) => void;
}

export const CompanyCard: React.FC<CompanyCardProps> = ({ 
  company, 
  canUpdate, 
  canDelete, 
  onEdit, 
  onDelete, 
  onManageUsers 
}) => {
  return (
    <div className="group bg-white border border-slate-200 rounded-3xl overflow-hidden shadow-sm hover:shadow-lg hover:shadow-slate-200/50 transition-all duration-300 flex flex-col">
      {/* Upper Section: Brand & Primary Actions */}
      <div className="p-5 pb-3">
        <div className="flex items-start justify-between gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <div className="w-12 h-12 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-400 group-hover:bg-indigo-600 group-hover:text-white transition-all duration-300 shrink-0">
              <Building2 size={24} />
            </div>
            <div className="min-w-0">
              <h2 className="text-lg font-black text-slate-900 tracking-tight truncate">{company.name}</h2>
              <div className="flex flex-wrap gap-2 mt-1">
                {company.isDemoActive ? (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-amber-50 text-amber-600 rounded-md text-[9px] font-black uppercase tracking-widest border border-amber-100/50">
                    <Zap size={8} className="fill-amber-600" /> Demo
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-indigo-50 text-indigo-600 rounded-md text-[9px] font-black uppercase tracking-widest border border-indigo-100/50">
                    <Users size={8} className="fill-indigo-600" /> Kurumsal
                  </span>
                )}
              </div>
            </div>
          </div>
          <div className="flex gap-1">
            {canUpdate && (
              <button 
                onClick={() => onEdit(company)}
                className="p-2 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-all"
                title="Şirket Ayarları"
              >
                <Key size={16} />
              </button>
            )}
            {canDelete && (
              <button 
                onClick={() => onDelete(company.publicId)}
                className="p-2 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-all"
                title="Şirketi Sil"
              >
                <Trash2 size={16} />
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Middle Section: Contact Info Widgets */}
      <div className="px-5 py-3 space-y-2 border-t border-slate-50">
        <div className="flex items-center gap-2 text-slate-600">
          <MapPin size={14} className="text-slate-400 shrink-0" />
          <p className="text-xs font-medium truncate">{company.address || "Adres belirtilmemiş"}</p>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
          <div className="flex items-center gap-2 text-slate-600 min-w-0">
            <Mail size={14} className="text-slate-400 shrink-0" />
            <p className="text-xs font-medium truncate">{company.mail || "E-posta yok"}</p>
          </div>
          <div className="flex items-center gap-2 text-slate-600 min-w-0">
            <Phone size={14} className="text-slate-400 shrink-0" />
            <p className="text-xs font-medium truncate">{company.phone || "Telefon yok"}</p>
          </div>
        </div>
      </div>

      {/* Footer Section: Main Action */}
      <div className="p-4 mt-auto border-t border-slate-50">
        <button 
          onClick={() => onManageUsers(company)}
          className="w-full flex items-center justify-between px-4 py-2.5 bg-slate-900 text-white rounded-xl font-black text-[9px] uppercase tracking-widest shadow-lg shadow-slate-900/5 hover:bg-indigo-600 transition-all active:scale-[0.98] group/btn"
        >
          <div className="flex items-center gap-2">
            <Users size={14} />
            <span>Kişileri Yönet</span>
          </div>
          <ChevronRight size={14} className="group-hover/btn:translate-x-1 transition-transform" />
        </button>
      </div>
    </div>
  );
};
