import React from "react";
import { X, Users, UserPlus, ArrowUpRight, Edit, Trash2 } from "lucide-react";
import type { Company } from "@/services/admin/AdminCompanyService";
import type { Customer } from "@/services/admin/AdminCustomerService";
import { cn } from "@/lib/utils";
import { toast } from "sonner";

interface UserManagementModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedCompany: Company | null;
  companyUsers: Customer[];
  onAddUserClick: () => void;
  onEditUser: (user: Customer) => void;
  onDeleteUser: (user: Customer) => void;
  copiedId: number | null;
  setCopiedId: (id: number | null) => void;
}

export const UserManagementModal: React.FC<UserManagementModalProps> = ({
  isOpen,
  onClose,
  selectedCompany,
  companyUsers,
  onAddUserClick,
  onEditUser,
  onDeleteUser,
  copiedId,
  setCopiedId
}) => {
  if (!isOpen || !selectedCompany) return null;

  return (
    <div className="fixed inset-0 z-[101] flex items-center justify-center overflow-y-auto bg-slate-950/80 p-2 backdrop-blur-md animate-in fade-in duration-300 sm:p-4 md:p-6 lg:pl-76 lg:pr-6">
      <div className="relative my-auto flex max-h-[92vh] w-full max-w-4xl flex-col overflow-hidden rounded-2xl border border-slate-800 bg-slate-900 shadow-2xl shadow-slate-950/50 sm:max-h-[90vh] sm:rounded-3xl">
        <div className="flex shrink-0 flex-col items-start justify-between gap-4 border-b border-slate-800 bg-slate-900 p-4 sm:flex-row sm:items-center sm:p-6 md:p-8">
          <div className="flex items-center gap-3 sm:gap-4 min-w-0 w-full sm:w-auto">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20 sm:h-12 sm:w-12 sm:rounded-2xl">
              <Users size={20} className="sm:hidden" />
              <Users size={24} className="hidden sm:block" />
            </div>
            <div className="min-w-0 flex-1">
              <h2 className="truncate text-base font-black tracking-tight text-white sm:text-xl">
                {selectedCompany.name} - Kullanıcılar
              </h2>
              <p className="mt-0.5 text-[10px] font-bold uppercase tracking-widest text-slate-400 sm:text-xs">
                Müşteri Portal Erişimi
              </p>
            </div>
          </div>
          <div className="flex w-full shrink-0 items-center justify-between gap-2 border-t border-slate-800 pt-3 sm:w-auto sm:justify-end sm:gap-3 sm:border-t-0 sm:pt-0">
            <button
              type="button"
              onClick={onAddUserClick}
              className={cn(
                "flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-all sm:px-5 sm:py-2.5 sm:text-sm",
                selectedCompany.isDemoActive
                  ? "cursor-pointer bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20 hover:from-indigo-500 hover:to-violet-500"
                  : "cursor-not-allowed border border-slate-700 bg-slate-800 text-slate-500"
              )}
            >
              <UserPlus size={16} />
              <span>Yeni Kullanıcı</span>
            </button>
            <button
              type="button"
              onClick={onClose}
              className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-slate-700 bg-slate-800 text-slate-400 transition-colors hover:border-red-500/30 hover:bg-red-500/15 hover:text-red-400 sm:h-10 sm:w-10"
            >
              <X size={20} />
            </button>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-4 sm:p-6 md:p-8 custom-scrollbar space-y-4">
          {companyUsers.length === 0 ? (
            <div className="flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-slate-700/70 bg-slate-800/30 p-4 py-12 sm:py-20">
              <div className="mb-3 flex h-14 w-14 items-center justify-center rounded-2xl border border-slate-700 bg-slate-800 shadow-sm sm:h-16 sm:w-16">
                <Users size={24} className="text-slate-400" />
              </div>
              <h3 className="text-center text-base font-black text-slate-100 sm:text-lg">Henüz kullanıcı tanımlanmamış</h3>
              <p className="mt-1 max-w-xs text-center text-xs font-medium text-slate-400">Bu firma için sistem erişimi olan çalışanları yukarıdaki butonu kullanarak ekleyebilirsiniz.</p>
            </div>
          ) : (
            <div className="space-y-3">
              <div className="hidden grid-cols-12 border-b border-slate-800 px-5 py-3 text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 sm:grid">
                <div className="col-span-6">Kullanıcı Bilgileri</div>
                <div className="col-span-3 text-center">Durum / İletişim</div>
                <div className="col-span-3 text-right">İşlemler</div>
              </div>
              {companyUsers.map((user) => (
                <div key={user.id} className={cn(
                  "flex flex-col gap-3 rounded-2xl border border-slate-700/70 bg-slate-800/60 p-4 transition-all hover:border-indigo-500/50 hover:shadow-xl hover:shadow-indigo-600/5 sm:grid sm:grid-cols-12 sm:items-center sm:gap-0 sm:px-5 sm:py-3.5",
                  !user.isActive && "bg-slate-800/30 grayscale-[0.5] opacity-80"
                )}>
                  <div className="sm:col-span-6 flex items-center gap-3">
                    <div className={cn(
                      "w-10 h-10 rounded-xl flex items-center justify-center font-black text-xs transition-all shrink-0 uppercase",
                      user.isActive ? "border border-indigo-500/30 bg-indigo-500/15 text-indigo-300" : "bg-slate-700/60 text-slate-400"
                    )}>
                      {(user.firstName?.[0] || user.email?.[0] || "?")}
                    </div>
                    <div className="min-w-0 flex-1">
                      <h4 className="truncate text-sm font-bold text-slate-100">
                        {user.firstName} {user.lastName}
                      </h4>
                      <p className="truncate text-[10px] font-bold uppercase tracking-tight text-slate-400">{user.email}</p>
                      {user.username && <p className="truncate text-[10px] font-bold tracking-tight text-indigo-300">@{user.username}</p>}
                    </div>
                  </div>
                  <div className="sm:col-span-3 flex items-center justify-between sm:flex-col sm:justify-center gap-1">
                    {user.isActive ? (
                      <span className="rounded-lg border border-emerald-500/30 bg-emerald-500/15 px-2.5 py-1 text-[9px] font-black uppercase tracking-widest text-emerald-400">Aktif</span>
                    ) : (
                      <span className="rounded-lg border border-slate-700 bg-slate-800 px-2.5 py-1 text-[9px] font-black uppercase tracking-widest text-slate-400">Pasif</span>
                    )}
                    {user.phoneNumber && <span className="text-[10px] font-bold tracking-tight text-slate-400">{user.phoneNumber}</span>}
                  </div>
                  <div className="flex items-center justify-end gap-1.5 border-t border-slate-800 pt-2 sm:col-span-3 sm:border-t-0 sm:pt-0">
                    <button
                      type="button"
                      onClick={() => {
                        const link = `${window.location.origin}/dashboard/login`;
                        navigator.clipboard.writeText(link);
                        setCopiedId(user.id);
                        setTimeout(() => setCopiedId(null), 2000);
                        toast.success("Giriş bağlantısı kopyalandı!");
                      }}
                      title="Giriş Linkini Kopyala"
                      className={cn(
                        "p-2 rounded-xl transition-all relative overflow-hidden border border-transparent cursor-pointer",
                        copiedId === user.id ? "border-emerald-500/30 bg-emerald-500/15 text-emerald-400" : "bg-slate-800 text-slate-400 hover:border-indigo-500/30 hover:bg-indigo-500/15 hover:text-indigo-300"
                      )}
                    >
                      <ArrowUpRight size={16} />
                    </button>
                    <button
                      type="button"
                      onClick={() => onEditUser(user)}
                      title="Kullanıcıyı Düzenle"
                      className="cursor-pointer rounded-xl border border-transparent bg-slate-800 p-2 text-slate-400 transition-all hover:border-amber-500/30 hover:bg-amber-500/15 hover:text-amber-300"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      type="button"
                      onClick={() => onDeleteUser(user)}
                      title="Kullanıcıyı Sil"
                      className="cursor-pointer rounded-xl border border-transparent bg-slate-800 p-2 text-slate-400 transition-all hover:border-red-500/30 hover:bg-red-500/15 hover:text-red-400"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
