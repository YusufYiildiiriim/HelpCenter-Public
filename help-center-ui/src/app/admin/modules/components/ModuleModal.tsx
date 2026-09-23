"use client";

import React, { useState } from "react";
import { X, Edit, Plus, Loader2, Check, Shield, LayoutGrid, Info, Users, Search, ChevronRight, ChevronLeft } from "lucide-react";
import type { User } from "@/services/admin/AdminUserService";
import { cn } from "@/lib/utils";
import { FormField } from "@/components/common/FormField";
import { AppDialog } from "@/components/common/AppDialog";
import { useValidatedForm } from "@/hooks/useValidatedForm";

interface ModuleModalProps {
  isEditMode: boolean;
  name: string;
  setName: (name: string) => void;
  description: string;
  setDescription: (description: string) => void;
  isActive: boolean;
  setIsActive: (isActive: boolean) => void;
  allUsers: User[];
  selectedExpertIds: number[];
  setSelectedExpertIds: (ids: number[]) => void;
  loading: boolean;
  onSave: () => void;
  onClose: () => void;
}

const TABS = [
  { key: "info", label: "Modül Bilgileri", icon: <LayoutGrid size={14} /> },
  { key: "experts", label: "Atanmış Uzmanlar", icon: <Users size={14} /> },
];

export const ModuleModal: React.FC<ModuleModalProps> = ({
  isEditMode,
  name,
  setName,
  description,
  setDescription,
  isActive,
  setIsActive,
  allUsers,
  selectedExpertIds,
  setSelectedExpertIds,
  loading,
  onSave,
  onClose
}) => {
  const [activeTab, setActiveTab] = useState(0);
  const [expertSearch, setExpertSearch] = useState("");

  const values = { name, description };
  const { register, validateFields } = useValidatedForm<typeof values>(values, {
    name: { label: "Modül Adı", rules: { required: true } },
    description: { label: "Açıklama", rules: { required: true } },
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    if (e.target.name === "name") setName(e.target.value);
    else if (e.target.name === "description") setDescription(e.target.value);
  };

  const filteredExperts = allUsers.filter((u) => {
    const q = expertSearch.toLowerCase().trim();
    if (!q) return true;
    return (
      u.name.toLowerCase().includes(q) ||
      u.lastName.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q)
    );
  });

  const TAB_FIELDS: Record<number, (keyof typeof values)[]> = {
    0: ["name", "description"],
    1: [],
  };

  const validateStep = (step: number) => {
    const fields = TAB_FIELDS[step] || [];
    if (fields.length === 0) return true;
    return validateFields(fields);
  };

  const goNext = () => {
    if (!validateStep(activeTab)) return;
    setActiveTab((t) => Math.min(t + 1, TABS.length - 1));
  };
  const goBack = () => setActiveTab((t) => Math.max(t - 1, 0));

  const saveModule = () => {
    if (!validateStep(0)) {
      setActiveTab(0);
      return;
    }

    onSave();
  };

  const handleFormSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    if (activeTab < TABS.length - 1) {
      goNext();
      return;
    }

    saveModule();
  };

  const handleTabClick = (targetIndex: number) => {
    if (targetIndex <= activeTab) {
      setActiveTab(targetIndex);
      return;
    }
    for (let s = activeTab; s < targetIndex; s++) {
      if (!validateStep(s)) {
        setActiveTab(s);
        return;
      }
    }
    setActiveTab(targetIndex);
  };

  const sectionHeader = (icon: React.ReactNode, title: string, count?: number) => (
    <div className="flex items-center justify-between pb-2 border-b border-slate-800 mb-4">
      <div className="flex items-center gap-2">
        {icon}
        <h3 className="text-xs font-black text-slate-400 uppercase tracking-widest">{title}</h3>
      </div>
      {count !== undefined && (
        <span className="text-[10px] font-black text-indigo-400 bg-indigo-500/10 border border-indigo-500/30 px-2 py-0.5 rounded-full uppercase tracking-tighter">
          {count} Seçildi
        </span>
      )}
    </div>
  );

  return (
    <AppDialog
      title={isEditMode ? "Modülü Düzenle" : "Yeni Modül Tanımla"}
      description={isEditMode ? "Mevcut yapılandırmayı güncelleyin" : "Sisteme yeni bir departman ekleyin"}
      onClose={onClose}
      className="w-[calc(100%-1rem)] max-w-lg bg-slate-900 rounded-2xl sm:rounded-3xl shadow-2xl border border-slate-800 flex flex-col max-h-[80vh] sm:max-h-[84vh] overflow-hidden animate-in zoom-in slide-in-from-bottom-4 duration-300"
    >

        {/* Header */}
        <div className="p-3.5 sm:p-6 border-b border-slate-800 flex justify-between items-center bg-slate-900 shrink-0">
          <div className="flex items-center gap-3 sm:gap-4 min-w-0">
            <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl sm:rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20 shrink-0">
                {isEditMode ? <Edit size={20} className="sm:hidden" /> : <Plus size={20} className="sm:hidden" />}
                {isEditMode ? <Edit size={24} className="hidden sm:block" /> : <Plus size={24} className="hidden sm:block" />}
            </div>
            <div className="min-w-0 flex-1">
              <h2 className="text-base sm:text-xl md:text-2xl font-black text-white leading-tight truncate">
                {isEditMode ? "Modülü Düzenle" : "Yeni Modül Tanımla"}
              </h2>
              <p className="text-slate-400 text-[9px] sm:text-[10px] md:text-xs font-bold uppercase tracking-widest mt-0.5 truncate">
                {isEditMode ? "Mevcut yapılandırmayı güncelleyin" : "Sisteme yeni bir departman ekleyin"}
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Pencereyi kapat"
            className="w-8 h-8 sm:w-10 sm:h-10 flex items-center justify-center rounded-xl sm:rounded-full bg-slate-800 border border-slate-700 text-slate-400 hover:bg-red-500/15 hover:text-red-400 hover:border-red-500/30 transition-all shadow-sm active:scale-90 shrink-0 ml-2"
          >
            <X size={18} />
          </button>
        </div>

        {/* Tab Navigation */}
        <div className="px-3.5 sm:px-6 pt-3 shrink-0">
          <div className="flex gap-1.5 sm:gap-2 overflow-x-auto scrollbar-none pb-1">
            {TABS.map((tab, i) => (
              <button
                key={tab.key}
                type="button"
                onClick={() => handleTabClick(i)}
                className={cn(
                  "flex items-center gap-1.5 px-3 sm:px-3.5 py-1.5 sm:py-2 rounded-xl text-[10px] font-black uppercase tracking-wider transition-all whitespace-nowrap border shrink-0 cursor-pointer",
                  activeTab === i
                    ? "bg-gradient-to-r from-indigo-600 to-violet-600 text-white border-transparent shadow-md shadow-indigo-600/20"
                    : i < activeTab
                    ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-400 hover:bg-emerald-500/20"
                    : "bg-white/5 border-slate-700 text-slate-400 hover:bg-white/10 hover:text-slate-200"
                )}
              >
                {i < activeTab ? <Check size={12} /> : tab.icon}
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        <form
          onSubmit={handleFormSubmit}
          onKeyDown={(e) => {
            // Prevent pressing Enter in a single-line input during intermediate steps from
            // submitting the form in an incomplete state — only allow on the last step.
            if (e.key === "Enter" && (e.target as HTMLElement).tagName === "INPUT" && activeTab !== TABS.length - 1) {
              e.preventDefault();
            }
          }}
          noValidate
          className="flex-1 overflow-y-auto p-3.5 sm:p-6 space-y-4 sm:space-y-6 custom-scrollbar"
        >
          {/* ─── Tab 1: Module Information ─── */}
          {activeTab === 0 && (
            <div className="space-y-4 sm:space-y-6 animate-in fade-in slide-in-from-right-2 duration-300">
              {sectionHeader(<LayoutGrid size={16} className="text-indigo-400" />, "Modül Bilgileri")}

              <FormField
                name="name"
                label="Modül Adı"
                value={name}
                onChange={handleChange}
                onBlur={register("name").onBlur}
                error={register("name").error}
                required
                icon={<LayoutGrid size={12} />}
                placeholder="Örn: Teknik Destek, Satış Sonrası..."
              />

              <FormField
                as="textarea"
                name="description"
                label="Açıklama"
                rows={3}
                value={description}
                onChange={handleChange}
                onBlur={register("description").onBlur}
                error={register("description").error}
                required
                icon={<Info size={12} />}
                placeholder="Modülün görev kapsamını ve amacını belirtin."
              />

              {/* Active Status */}
              <div className="p-3.5 sm:p-5 bg-slate-800/60 rounded-xl sm:rounded-2xl border border-slate-700 flex items-center justify-between group">
                <div className="flex items-center gap-3 sm:gap-4">
                  <div className={cn(
                    "w-9 h-9 sm:w-10 sm:h-10 rounded-xl flex items-center justify-center transition-colors shrink-0",
                    isActive ? "bg-indigo-600 text-white shadow-lg shadow-indigo-600/20" : "bg-slate-700 text-slate-400"
                  )}>
                    <Shield size={18} />
                  </div>
                  <div>
                    <p className="text-xs font-black text-slate-100 tracking-tight">Operasyonel Durum</p>
                    <p className="text-[10px] text-slate-400 font-bold tracking-tight">Modül şu an {isActive ? 'aktif' : 'pasif'} durumda</p>
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => setIsActive(!isActive)}
                  className={cn(
                    "w-12 h-6 sm:w-14 sm:h-7 rounded-full transition-all relative p-1 shrink-0 cursor-pointer",
                    isActive ? "bg-indigo-600 shadow-inner" : "bg-slate-600 shadow-inner"
                  )}
                >
                  <div className={cn(
                    "w-4 h-4 sm:w-5 sm:h-5 bg-white rounded-full shadow-md transition-all duration-300",
                    isActive ? "translate-x-6 sm:translate-x-7" : "translate-x-0"
                  )} />
                </button>
              </div>
            </div>
          )}

          {/* ─── Tab 2: Assigned Experts ─── */}
          {activeTab === 1 && (
            <div className="space-y-3 sm:space-y-4 animate-in fade-in slide-in-from-right-2 duration-300">
              {sectionHeader(<Users size={16} className="text-indigo-400" />, "Atanmış Uzmanlar", selectedExpertIds.length)}

              {/* Expert Search */}
              <div className="relative">
                <Search size={15} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500" />
                <input
                  type="text"
                  value={expertSearch}
                  onChange={(e) => setExpertSearch(e.target.value)}
                  placeholder="İsim, soyisim veya e-posta ile ara..."
                  className="w-full pl-10 pr-4 py-2 sm:py-2.5 bg-slate-800/70 border border-slate-700 rounded-xl text-slate-100 focus:bg-slate-800 focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all placeholder:text-slate-500 font-bold text-xs"
                />
              </div>

              <div className="grid grid-cols-1 gap-2 max-h-[220px] sm:max-h-[300px] overflow-y-auto pr-1 custom-scrollbar p-2.5 sm:p-3 bg-slate-800/60 rounded-xl sm:rounded-2xl border border-slate-700">
                {filteredExperts.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-8 gap-2 opacity-40">
                    {allUsers.length === 0 ? <Loader2 className="animate-spin" size={20} /> : <Users size={20} className="text-slate-500" />}
                    <p className="text-[10px] font-black uppercase tracking-widest text-slate-400">
                      {allUsers.length === 0 ? "Kullanıcılar Alınıyor..." : "Aramanıza uygun kullanıcı yok."}
                    </p>
                  </div>
                ) : filteredExperts.map(user => {
                  const isSelected = selectedExpertIds.includes(user.id);
                  return (
                    <button
                      key={user.id}
                      type="button"
                      onClick={() => {
                        if (isSelected) {
                          setSelectedExpertIds(selectedExpertIds.filter(id => id !== user.id));
                        } else {
                          setSelectedExpertIds([...selectedExpertIds, user.id]);
                        }
                      }}
                      className={cn(
                        "flex items-center justify-between p-2.5 sm:p-3 rounded-xl transition-all border text-left active:scale-[0.98] cursor-pointer",
                        isSelected
                          ? "bg-indigo-600/20 border-indigo-500/50 shadow-md shadow-indigo-600/10"
                          : "bg-slate-900/60 border-slate-700/60 hover:bg-slate-800 hover:border-slate-600"
                      )}
                    >
                      <div className="flex items-center gap-2.5 sm:gap-3 min-w-0">
                        <div className={cn(
                          "w-8 h-8 sm:w-9 sm:h-9 rounded-xl flex items-center justify-center font-black text-[10px] transition-all shrink-0",
                          isSelected ? "bg-indigo-600 text-white shadow-lg shadow-indigo-600/20" : "bg-slate-800 border border-slate-700 text-slate-400 shadow-sm"
                        )}>
                          {((user.name || "").substring(0, 1) + (user.lastName || "").substring(0, 1)).toUpperCase() || "U"}
                        </div>
                        <div className="min-w-0 flex-1">
                          <p className={cn(
                            "text-[10px] sm:text-[11px] font-black uppercase leading-none transition-colors truncate",
                            isSelected ? "text-white" : "text-slate-300"
                          )}>{user.name || ""} {user.lastName || ""}</p>
                          <p className="text-[9px] font-bold text-slate-500 mt-1 font-mono tracking-tighter truncate">{user.email || "-"}</p>
                        </div>
                      </div>
                      {isSelected && (
                        <div className="w-5 h-5 rounded-full bg-emerald-500 text-white flex items-center justify-center shadow-lg shadow-emerald-500/20 animate-in zoom-in shrink-0">
                          <Check size={12} strokeWidth={4} />
                        </div>
                      )}
                    </button>
                  );
                })}
              </div>
            </div>
          )}

          {/* Footer Navigation */}
          <div className="flex items-center justify-between gap-2 sm:gap-3 pt-3 border-t border-slate-800 shrink-0">
            {activeTab > 0 ? (
              <button
                type="button"
                onClick={goBack}
                className="flex items-center gap-1.5 sm:gap-2 px-4 sm:px-6 py-2.5 sm:py-3.5 bg-white/10 hover:bg-white/20 text-slate-200 rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] sm:text-xs transition-all active:scale-95 cursor-pointer"
              >
                <ChevronLeft size={16} />
                Geri
              </button>
            ) : (
              <span />
            )}

            {activeTab < TABS.length - 1 ? (
              <button
                type="button"
                onClick={goNext}
                className="flex items-center gap-1.5 sm:gap-2 px-4 sm:px-6 py-2.5 sm:py-3.5 bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] sm:text-xs shadow-lg shadow-indigo-600/20 transition-all active:scale-95 cursor-pointer"
              >
                Devam Et
                <ChevronRight size={16} />
              </button>
            ) : (
              <button
                type="button"
                onClick={saveModule}
                disabled={loading}
                className="flex items-center gap-1.5 sm:gap-2 px-4 sm:px-6 py-2.5 sm:py-3.5 bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] sm:text-xs shadow-lg shadow-indigo-600/20 transition-all active:scale-95 disabled:opacity-50 cursor-pointer"
              >
                {loading ? <Loader2 className="animate-spin" size={16} /> : (isEditMode ? <Edit size={16} /> : <Check size={16} />)}
                {loading ? "İşleniyor..." : (isEditMode ? "Değişiklikleri Kaydet" : "Modülü Tanımla")}
              </button>
            )}
          </div>
        </form>
    </AppDialog>
  );
};
