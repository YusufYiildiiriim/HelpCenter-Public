"use client";

import React, { useRef, useState } from "react";
import { X, Save, Plus, Loader2, CheckCircle2, Info, Shield, MessageSquare } from "lucide-react";
import { cn } from "@/lib/utils";
import { FormField } from "@/components/common/FormField";
import { AppDialog } from "@/components/common/AppDialog";
import type { RequestSubjectTranslation } from "@/services/admin/AdminRequestSubjectService";

interface SubjectModalProps {
  isEditMode: boolean;
  subjectData: { name: string; description: string; isActive: boolean; sourceLocale: string; translations: RequestSubjectTranslation[] };
  setSubjectData: React.Dispatch<React.SetStateAction<{ name: string; description: string; isActive: boolean; sourceLocale: string; translations: RequestSubjectTranslation[] }>>;
  onSubmit: (e: React.FormEvent) => void;
  onClose: () => void;
  submitting: boolean;
  success: boolean;
}

export const SubjectModal: React.FC<SubjectModalProps> = ({
  isEditMode,
  subjectData,
  setSubjectData,
  onSubmit,
  onClose,
  submitting,
  success
}) => {
  const formRef = useRef<HTMLFormElement>(null);
  const [activeLocale, setActiveLocale] = useState("tr-TR");
  const activeTranslation = subjectData.translations.find(item => item.locale === activeLocale) ?? { locale: activeLocale, name: "", description: "" };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    if (name === "sourceLocale") {
      setSubjectData(prev => ({ ...prev, sourceLocale: value }));
      return;
    }
    setSubjectData(prev => {
      const translations = prev.translations.map(item => item.locale === activeLocale ? { ...item, [name]: value } : item);
      if (!translations.some(item => item.locale === activeLocale)) translations.push({ locale: activeLocale, name: "", description: "", [name]: value });
      return { ...prev, translations, ...(prev.sourceLocale === activeLocale ? { [name]: value } : {}) };
    });
  };

  return (
    <AppDialog
      title={isEditMode ? "Konuyu Güncelle" : "Yeni Talep Konusu"}
      description={isEditMode ? "Mevcut kategoriyi düzenleyin" : "Sisteme yeni bir kategori tanımlayın"}
      onClose={onClose}
      className="w-[calc(100%-1.5rem)] max-w-lg max-h-[80vh] sm:max-h-[84vh] bg-slate-900 rounded-2xl sm:rounded-3xl shadow-2xl border border-slate-800 flex flex-col animate-in zoom-in slide-in-from-bottom-4 duration-300 overflow-hidden"
    >
        
        {/* Header */}
        <div className="p-3.5 sm:p-6 border-b border-slate-800 flex justify-between items-center bg-slate-900 shrink-0">
          <div className="flex items-center gap-3 sm:gap-4 min-w-0">
            <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl sm:rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20 shrink-0">
                {isEditMode ? <Save size={20} className="sm:hidden" /> : <Plus size={20} className="sm:hidden" />}
                {isEditMode ? <Save size={24} className="hidden sm:block" /> : <Plus size={24} className="hidden sm:block" />}
            </div>
            <div className="min-w-0 flex-1">
              <h2 className="text-base sm:text-xl md:text-2xl font-black text-white leading-none truncate">
                {isEditMode ? "Konuyu Güncelle" : "Yeni Talep Konusu"}
              </h2>
              <p className="text-slate-400 text-[9px] sm:text-[10px] md:text-xs font-bold uppercase tracking-widest mt-1 truncate">
                {isEditMode ? "Mevcut kategoriyi düzenleyin" : "Sisteme yeni bir kategori tanımlayın"}
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose} 
            aria-label="Pencereyi kapat"
            className="w-8 h-8 sm:w-10 sm:h-10 flex items-center justify-center rounded-xl bg-slate-800 border border-slate-700 text-slate-400 hover:bg-red-500/15 hover:text-red-400 hover:border-red-500/30 transition-all shadow-sm active:scale-90 shrink-0 ml-2 cursor-pointer"
          >
            <X size={18} />
          </button>
        </div>
        
        <form ref={formRef} onSubmit={onSubmit} noValidate className="flex-1 overflow-y-auto p-3.5 sm:p-6 space-y-4 sm:space-y-6 custom-scrollbar">
          {success ? (
            <div className="py-20 flex flex-col items-center justify-center text-center space-y-4 animate-in zoom-in">
              <div className="w-20 h-20 bg-emerald-500/15 text-emerald-400 rounded-full flex items-center justify-center border border-emerald-500/30">
                <CheckCircle2 size={48} strokeWidth={3} className="animate-bounce" />
              </div>
              <div>
                <h3 className="text-xl font-black text-slate-100 tracking-tight">İşlem Başarılı</h3>
                <p className="text-slate-400 text-sm font-medium mt-1">Veriler güvenli bir şekilde kaydedildi.</p>
              </div>
            </div>
          ) : (
            <div className="space-y-6">
              <label className="block text-xs font-bold text-slate-300">Kaynak dil
                <select name="sourceLocale" value={subjectData.sourceLocale} onChange={handleChange} required className="mt-2 w-full rounded-xl bg-slate-800 border border-slate-700 p-3 text-white">
                  <option value="">Kaynak dili seçin</option><option value="tr-TR">Türkçe</option><option value="en-US">English</option>
                </select>
              </label>
              <div className="flex gap-2" role="tablist" aria-label="İçerik dili">
                {[{ locale: "tr-TR", label: "Türkçe" }, { locale: "en-US", label: "English" }].map(item => {
                  const filled = subjectData.translations.find(value => value.locale === item.locale)?.name.trim();
                  return <button key={item.locale} type="button" role="tab" aria-selected={activeLocale === item.locale} onClick={() => setActiveLocale(item.locale)} className={`rounded-lg px-3 py-2 text-xs font-bold ${activeLocale === item.locale ? "bg-indigo-600 text-white" : "bg-slate-800 text-slate-300"}`}>{item.label}{filled ? "" : " · Eksik"}</button>;
                })}
              </div>
              <FormField
                name="name"
                label="Konu Başlığı"
                value={activeTranslation.name}
                onChange={handleChange}
                required={activeLocale === subjectData.sourceLocale}
                icon={<MessageSquare size={12} />}
                placeholder="Örn: Teknik Sorun, Fatura İtirazı..."
              />

              <FormField
                as="textarea"
                name="description"
                label="Açıklama"
                rows={4}
                value={activeTranslation.description}
                onChange={handleChange}
                required={activeLocale === subjectData.sourceLocale}
                icon={<Info size={12} />}
                placeholder="Bu kategori hangi tür talepleri kapsıyor?"
              />
              
              {/* Active Status */}
              <div className="p-5 bg-slate-800/60 rounded-2xl border border-slate-700 flex items-center justify-between group">
                <div className="flex items-center gap-4">
                  <div className={cn(
                    "w-10 h-10 rounded-xl flex items-center justify-center transition-colors",
                    subjectData.isActive ? "bg-indigo-600 text-white shadow-lg shadow-indigo-600/20" : "bg-slate-800 text-slate-500"
                  )}>
                    <Shield size={18} />
                  </div>
                  <div>
                    <p className="text-xs font-black text-slate-100 tracking-tight">Görünürlük Durumu</p>
                    <p className="text-[10px] text-slate-400 font-bold tracking-tight">Şu an kullanıcılara {subjectData.isActive ? 'açık' : 'kapalı'}</p>
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => setSubjectData({ ...subjectData, isActive: !subjectData.isActive })}
                  className={cn(
                    "w-14 h-7 rounded-full transition-all relative p-1",
                    subjectData.isActive ? "bg-emerald-500 shadow-inner" : "bg-slate-600 shadow-inner"
                  )}
                >
                  <div className={cn(
                    "w-5 h-5 bg-white rounded-full shadow-md transition-all duration-300",
                    subjectData.isActive ? "translate-x-7" : "translate-x-0"
                  )} />
                </button>
              </div>
            </div>
          )}
        </form>

        {/* Footer Actions */}
        {!success && (
          <div className="p-6 md:p-8 border-t border-slate-800 bg-slate-900 flex flex-col md:flex-row gap-3 shrink-0">
            <button
              type="button"
              onClick={onClose}
              className="w-full md:w-auto px-8 py-4 bg-slate-800/70 border border-slate-700 text-slate-300 rounded-2xl font-black uppercase tracking-widest text-[10px] hover:bg-slate-800 transition-all active:scale-95"
            >
              Vazgeç
            </button>
            <button
              onClick={() => formRef.current?.requestSubmit()}
              disabled={submitting}
              className="w-full md:flex-1 px-10 py-4 bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white rounded-2xl font-black uppercase tracking-widest text-[10px] shadow-lg shadow-indigo-600/20 transition-all disabled:opacity-50 flex items-center justify-center gap-2 active:scale-95 ml-auto"
            >
              {submitting ? <Loader2 className="animate-spin" size={16} /> : (isEditMode ? <Save size={16} /> : <Plus size={16} />)}
              {submitting ? "İşleniyor..." : (isEditMode ? "Değişiklikleri Kaydet" : "Konuyu Sisteme Ekle")}
            </button>
          </div>
        )}
    </AppDialog>
  );
};
