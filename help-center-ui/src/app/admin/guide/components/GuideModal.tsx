import React, { useEffect, useMemo, useState } from "react";
import { X, Edit3, Plus } from "lucide-react";
import type { Module } from "@/services/admin/AdminModuleService";
import type { Guide, GuideDocument } from "@/services/admin/AdminGuideService";
import { FilePreview } from "./FilePreview";
import dynamic from "next/dynamic";
import { cn } from "@/lib/utils";
import { ModalSelect } from "@/components/common/ModalSelect";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

// CKEditor needs to be loaded client-side only
const CustomEditor = dynamic(() => import("@/components/CustomEditor"), {
  ssr: false,
  loading: () => <div className="h-50 w-full bg-slate-800/60 animate-pulse rounded-4xl border border-slate-700 flex items-center justify-center text-slate-500 font-bold uppercase text-[10px] tracking-widest">Editör yükleniyor...</div>
});

export interface GuideFormData {
  title: string;
  description: string;
  sourceLocale: string;
  translations: { locale: string; title: string; description: string }[];
  englishFiles: FileList | null;
  module: string;
  previousGuideId: string | null;
  youtubeUrl: string;
  files: FileList | null;
  existingDocuments: GuideDocument[];
  isActive: boolean;
  isPublic: boolean;
  rowVersion?: string;
}

interface GuideModalProps {
  isOpen: boolean;
  isEditMode: boolean;
  onClose: () => void;
  onSubmit: (e: React.FormEvent) => void;
  formData: GuideFormData;
  setFormData: React.Dispatch<React.SetStateAction<GuideFormData>>;
  modules: Module[];
  guides: Guide[];
  editingId: string | null;
  loading: boolean;
}

export const GuideModal: React.FC<GuideModalProps> = ({
  isOpen,
  isEditMode,
  onClose,
  onSubmit,
  formData,
  setFormData,
  modules,
  guides,
  editingId,
  loading
}) => {
  const [activeLocale, setActiveLocale] = useState(formData.sourceLocale || "tr-TR");
  const sourceLocale = formData.sourceLocale || "";
  const activeTranslation = formData.translations?.find((item) => item.locale === activeLocale);
  const activeTitle = activeLocale === sourceLocale || (!sourceLocale && activeLocale === "tr-TR") ? formData.title : activeTranslation?.title ?? "";
  const activeDescription = activeLocale === sourceLocale || (!sourceLocale && activeLocale === "tr-TR") ? formData.description : activeTranslation?.description ?? "";
  const updateLocalizedField = (field: "title" | "description", value: string) => {
    setFormData((current) => {
      if (activeLocale === current.sourceLocale || (!current.sourceLocale && activeLocale === "tr-TR")) {
        return { ...current, [field]: value };
      }
      const translations = [...(current.translations ?? [])];
      const index = translations.findIndex((item) => item.locale === activeLocale);
      const item = index >= 0 ? { ...translations[index] } : { locale: activeLocale, title: "", description: "" };
      item[field] = value;
      if (index >= 0) translations[index] = item; else translations.push(item);
      return { ...current, translations };
    });
  };

  const fieldConfig = useMemo(() => ({
    title: { label: "Soru / Başlık", rules: { required: true } },
    module: { label: "Kategori (Modül)", rules: { required: true } },
    description: {
      label: "Rehber İçeriği",
      rules: {
        required: true,
        validate: (value: string) => {
          if (value === "<p>&nbsp;</p>") return "Rehber içeriği zorunludur.";
          return undefined;
        },
      },
    },
  }), []);

  const { register, handleSubmit, reset } = useValidatedForm<GuideFormData>(formData, fieldConfig);

  useEffect(() => {
    if (isOpen) reset();
  }, [isOpen, reset]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setFormData((current) => ({ ...current, [e.target.name]: e.target.value }));
  };

  const safeModules = Array.isArray(modules) ? modules : [];
  const safeGuides = Array.isArray(guides) ? guides : [];

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-4 lg:pl-76 lg:pr-6 bg-slate-900/60 backdrop-blur-xl animate-in fade-in duration-300">
      <div className="bg-slate-900 rounded-2xl sm:rounded-3xl shadow-2xl w-full max-w-5xl max-h-[80vh] sm:max-h-[84vh] overflow-hidden relative z-10 flex flex-col animate-in zoom-in duration-500 border border-slate-800 my-auto">
        <div className="flex justify-between items-center px-4 py-3.5 sm:px-7 sm:py-4 border-b border-slate-800 bg-slate-900 shrink-0">
          <div className="flex items-center gap-3">
            <div className={cn(
                "w-10 h-10 rounded-2xl flex items-center justify-center transition-all shadow-lg",
                isEditMode ? "bg-linear-to-br from-indigo-600 to-violet-600 text-white shadow-indigo-600/20" : "bg-linear-to-br from-indigo-600 to-violet-600 text-white shadow-indigo-600/20"
            )}>
              {isEditMode ? <Edit3 size={18} /> : <Plus size={18} />}
            </div>
            <div>
              <h2 className="text-base sm:text-lg font-black text-slate-100 tracking-tight leading-tight">{isEditMode ? "Rehberi Güncelle" : "Yeni Rehber Ekle"}</h2>
              <p className="text-slate-500 text-[10px] font-bold uppercase tracking-[0.2em] opacity-80">Zengin İçerikli Dökümantasyon</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="w-8 h-8 sm:w-9 sm:h-9 flex items-center justify-center rounded-xl text-slate-400 hover:text-red-400 hover:bg-red-500/10 transition-all active:scale-90 shrink-0"
          >
            <X size={18} />
          </button>
        </div>

        <form id="guide-form" onSubmit={handleSubmit(onSubmit)} noValidate className="flex-1 overflow-y-auto px-4 py-4 sm:px-7 sm:py-5 custom-scrollbar space-y-4 sm:space-y-5">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            {/* Left Column: Basic Info */}
            <div className="lg:col-span-4 space-y-4">
              <div className="space-y-3">
                <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400">Kaynak dil</label>
                <select aria-label="Rehber kaynak dili" value={sourceLocale} onChange={(event) => { setFormData((current) => ({ ...current, sourceLocale: event.target.value })); setActiveLocale(event.target.value || "tr-TR"); }} className="w-full rounded-xl border border-slate-700 bg-slate-800 px-3 py-2 text-sm text-white">
                  <option value="">Kaydı sınıflandırın…</option><option value="tr-TR">Türkçe (kaynak)</option><option value="en-US">English (source)</option>
                </select>
                {!sourceLocale && <p role="status" className="text-xs text-amber-300">Bu içeriğin kaynak dilini seçin. Eski metinler otomatik sınıflandırılmaz.</p>}
                <FormField name="title" label={activeLocale === "tr-TR" ? "Başlık · Türkçe" : "Title · English"} value={activeTitle} onChange={(event) => updateLocalizedField("title", event.target.value)} onBlur={register("title").onBlur} error={register("title").error} required placeholder="Rehber başlığı" />

                <ModalSelect
                    name="module"
                    label="Kategori (Modül)"
                    value={formData.module}
                    options={[{ value: "", label: "Modül Seçin" }, ...safeModules.map(m => ({ value: m.name, label: m.name })), ...(safeModules.length === 0 ? [{ value: "Genel", label: "Genel" }] : [])]}
                    onValueChange={(value) => setFormData((current) => ({ ...current, module: value, previousGuideId: null }))}
                    onBlur={register("module").onBlur}
                    error={register("module").error}
                  />

                <FormField
                  name="youtubeUrl"
                  label="Youtube Video Linki"
                  value={formData.youtubeUrl}
                  onChange={handleChange}
                  placeholder="https://youtube.com/..."
                />

                <div className="space-y-1.5">
                  <div className="relative">
                    <ModalSelect
                      label="Önceki Adım (Zincirleme)"
                      value={String(formData.previousGuideId ?? "")}
                      options={[{ value: "", label: "Bağlantı Yok (İlk Adım)" }, ...safeGuides.filter(g => g.module === formData.module && g.id !== editingId).map(g => ({ value: String(g.id), label: g.title }))]}
                      onValueChange={(value) => setFormData((current) => ({ ...current, previousGuideId: value || null }))}
                    />
                  </div>
                </div>

                <div className="space-y-1.5">
                  <label className="block text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Dosya Ekle</label>
                  <div className="relative">
                    <input type="file" id="file-upload" multiple onChange={(e) => setFormData((current) => ({ ...current, files: e.target.files }))} className="hidden" />
                    <label htmlFor="file-upload" className="w-full flex items-center justify-between px-4 py-2.5 bg-slate-800/70 border border-slate-700 rounded-xl cursor-pointer hover:border-indigo-500/50 transition-all group shadow-sm">
                      <span className="text-xs font-bold text-slate-400 truncate mr-2">{formData.files?.length ? formData.files.length + " source language file(s)" : "Source language files"}</span>
                      <span className="text-[9px] font-black uppercase tracking-widest text-indigo-400">Seç</span>
                    </label>
                  </div>
                  <div className="relative">
                    <input type="file" id="english-file-upload" multiple onChange={(e) => setFormData((current) => ({ ...current, englishFiles: e.target.files }))} className="hidden" />
                    <label htmlFor="english-file-upload" className="w-full flex items-center justify-between px-4 py-2.5 bg-slate-800/70 border border-slate-700 rounded-xl cursor-pointer hover:border-indigo-500/50 transition-all group shadow-sm">
                      <span className="text-xs font-bold text-slate-400 truncate mr-2">{formData.englishFiles?.length ? formData.englishFiles.length + " English file(s)" : "English translation files"}</span>
                      <span className="text-[9px] font-black uppercase tracking-widest text-indigo-400">Select</span>
                    </label>
                  </div>
                  {(formData.existingDocuments.length > 0 || (formData.files && formData.files.length > 0)) && (
                    <div className="mt-3 space-y-2">
                      <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">
                        {formData.files && formData.files.length > 0 ? "Yüklenecek Dosyalar" : "Mevcut Dosyalar"}
                      </p>
                      <div className="flex flex-wrap gap-2 p-3 bg-slate-800/50 rounded-2xl border border-slate-700">
                        {(!formData.files || formData.files.length === 0) ? (
                          formData.existingDocuments.map((doc: GuideDocument, idx: number) => (
                            <FilePreview key={`existing-${idx}`} file={doc} isExisting={true} />
                          ))
                        ) : (
                          Array.from(formData.files as FileList).map((file, idx) => (
                            <FilePreview key={`new-${idx}`} file={file} isExisting={false} />
                          ))
                        )}
                      </div>
                    </div>
                  )}
                </div>

                <div className="flex items-center justify-between p-4 bg-slate-800/50 rounded-2xl border border-slate-700">
                    <div>
                        <p className="text-xs font-black text-slate-200 tracking-tight uppercase">Aktif / Pasif</p>
                        <p className="text-[10px] text-slate-500 font-medium tracking-tight">Müşterilere gösterilsin mi?</p>
                    </div>
                    <button
                        type="button"
                        onClick={() => setFormData((current) => ({ ...current, isActive: !current.isActive }))}
                        className={cn(
                            "w-12 h-6 rounded-full transition-all relative",
                            formData.isActive ? "bg-emerald-500" : "bg-slate-700"
                        )}
                    >
                        <div className={cn(
                            "absolute top-1 w-4 h-4 bg-white rounded-full transition-all shadow-sm",
                            formData.isActive ? "left-7" : "left-1"
                        )} />
                    </button>
                </div>

                <div className="flex items-center justify-between p-4 bg-slate-800/50 rounded-2xl border border-slate-700">
                    <div>
                        <p className="text-xs font-black text-slate-200 tracking-tight uppercase">Herkese Açık</p>
                        <p className="text-[10px] text-slate-500 font-medium tracking-tight">Giriş yapılmadan görüntülenebilsin mi?</p>
                    </div>
                    <button
                        type="button"
                        onClick={() => setFormData((current) => ({ ...current, isPublic: !current.isPublic }))}
                        className={cn(
                            "w-12 h-6 rounded-full transition-all relative",
                            formData.isPublic ? "bg-indigo-500" : "bg-slate-700"
                        )}
                    >
                        <div className={cn(
                            "absolute top-1 w-4 h-4 bg-white rounded-full transition-all shadow-sm",
                            formData.isPublic ? "left-7" : "left-1"
                        )} />
                    </button>
                </div>
              </div>
            </div>

            {/* Right Column: Content Editor */}
            <div className="lg:col-span-8 flex flex-col h-full space-y-2">
              <div role="tablist" aria-label="Rehber çeviri dilleri" className="flex gap-2">
                {["tr-TR", "en-US"].map((locale) => {
                  const ready = locale === sourceLocale || (!sourceLocale && locale === "tr-TR") || formData.translations?.some((item) => item.locale === locale && item.title && item.description);
                  return <button key={locale} type="button" role="tab" aria-selected={activeLocale === locale} onClick={() => setActiveLocale(locale)} className={cn("rounded-lg border px-3 py-2 text-xs font-bold", activeLocale === locale ? "border-indigo-400 bg-indigo-500/20 text-white" : "border-slate-700 text-slate-400")}>{locale === "tr-TR" ? "Türkçe" : "English"} · {ready ? "Hazır" : "Çeviri eksik"}</button>;
                })}
              </div>
              <label className="flex items-center justify-between text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">
                {activeLocale === "tr-TR" ? "Rehber içeriği · Türkçe" : "Guide content · English"}
                <span className="text-[9px] font-black text-indigo-400 bg-indigo-500/10 px-3 py-1 rounded-full uppercase tracking-tighter border border-indigo-500/20">CKEditor 5 PRO</span>
              </label>
              <div className={cn("flex-1 min-h-105 border rounded-2xl overflow-hidden bg-slate-800/60", register("description").error ? "border-rose-500/70" : "border-slate-700")}>
                  <CustomEditor
                  key={activeLocale}
                  value={activeDescription}
                  onChange={(data) => updateLocalizedField("description", data)}
                  placeholder="Rehber içeriğini buraya yazın..."
                />
              </div>
              {register("description").error && (
                <p className="text-[11px] font-bold text-rose-400 ml-1">{register("description").error}</p>
              )}
            </div>
          </div>
        </form>

        <div className="px-7 py-4 border-t border-slate-800 bg-slate-900 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-6 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest text-slate-400 hover:bg-slate-800 transition-all active:scale-95"
            >
              Vazgeç
            </button>
            <button
              type="submit"
              form="guide-form"
              disabled={loading}
              className="bg-linear-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white px-8 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-[0.2em] shadow-lg shadow-indigo-600/20 disabled:opacity-50 transition-all flex items-center gap-2 active:scale-95"
            >
              {loading ? (
                <>
                  <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  Kaydediliyor
                </>
              ) : (
                <>
                  {isEditMode ? "Güncellemeyi Kaydet" : "Rehberi Yayınla"}
                </>
              )}
            </button>
          </div>
      </div>
    </div>
  );
};
