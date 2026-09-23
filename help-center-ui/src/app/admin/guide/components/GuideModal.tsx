import React, { useEffect, useMemo } from "react";
import { X, Edit3, Plus, ChevronDown } from "lucide-react";
import type { Module } from "@/services/admin/AdminModuleService";
import type { Guide, GuideDocument } from "@/services/admin/AdminGuideService";
import { FilePreview } from "./FilePreview";
import dynamic from "next/dynamic";
import { cn } from "@/lib/utils";
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
  module: string;
  previousGuideId: number | null;
  youtubeUrl: string;
  files: FileList | null;
  existingDocuments: GuideDocument[];
  isActive: boolean;
  isPublic: boolean;
}

interface GuideModalProps {
  isOpen: boolean;
  isEditMode: boolean;
  onClose: () => void;
  onSubmit: (e: React.FormEvent) => void;
  formData: GuideFormData;
  setFormData: (data: GuideFormData) => void;
  modules: Module[];
  guides: Guide[];
  editingId: number | null;
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
    setFormData({ ...formData, [e.target.name]: e.target.value });
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
                <FormField
                  name="title"
                  label="Soru / Başlık"
                  value={formData.title}
                  onChange={handleChange}
                  onBlur={register("title").onBlur}
                  error={register("title").error}
                  required
                  placeholder="Örn: Sisteme nasıl giriş yapılır?"
                />

                <FormField
                  as="select"
                  name="module"
                  label="Kategori (Modül)"
                  value={formData.module}
                  onChange={handleChange}
                  onBlur={register("module").onBlur}
                  error={register("module").error}
                  required
                  right={<ChevronDown size={16} />}
                >
                  <option value="">Modül Seçin</option>
                  {safeModules.map((m) => (
                    <option key={m.id} value={m.name}>{m.name}</option>
                  ))}
                  {safeModules.length === 0 && <option value="Genel">Genel</option>}
                </FormField>

                <FormField
                  name="youtubeUrl"
                  label="Youtube Video Linki"
                  value={formData.youtubeUrl}
                  onChange={handleChange}
                  placeholder="https://youtube.com/..."
                />

                <div className="space-y-1.5">
                  <label className="block text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Önceki Adım (Zincirleme)</label>
                  <div className="relative">
                    <select
                      value={formData.previousGuideId || ""}
                      onChange={(e) => setFormData({ ...formData, previousGuideId: e.target.value ? parseInt(e.target.value) : null })}
                      className="w-full px-4 py-2.5 bg-slate-800/70 border border-slate-700 rounded-xl focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 focus:outline-none transition-all appearance-none font-bold text-slate-100 text-sm"
                    >
                      <option value="">Bağlantı Yok (İlk Adım)</option>
                      {safeGuides
                        .filter(g => g.module === formData.module && g.id !== editingId)
                        .map((g) => (
                          <option key={g.id} value={g.id}>{g.title}</option>
                        ))}
                    </select>
                    <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-500 pointer-events-none" size={16} />
                  </div>
                </div>

                <div className="space-y-1.5">
                  <label className="block text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Dosya Ekle</label>
                  <div className="relative">
                    <input
                      type="file"
                      id="file-upload"
                      multiple
                      onChange={(e) => setFormData({ ...formData, files: e.target.files })}
                      className="hidden"
                    />
                    <label
                      htmlFor="file-upload"
                      className="w-full flex items-center justify-between px-4 py-2.5 bg-slate-800/70 border border-slate-700 rounded-xl cursor-pointer hover:border-indigo-500/50 transition-all group shadow-sm"
                    >
                      <span className="text-xs font-bold text-slate-400 truncate mr-2">
                        {formData.files && formData.files.length > 0
                          ? `${formData.files.length} dosya seçildi`
                          : "Henüz dosya seçilmedi"}
                      </span>
                      <div className="bg-linear-to-r from-indigo-600 to-violet-600 text-white px-3 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-wider group-hover:from-indigo-500 group-hover:to-violet-500 transition-colors shrink-0">
                        Dosya Seç
                      </div>
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
                        onClick={() => setFormData({ ...formData, isActive: !formData.isActive })}
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
                        onClick={() => setFormData({ ...formData, isPublic: !formData.isPublic })}
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
              <label className="flex items-center justify-between text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">
                Rehber İçeriği
                <span className="text-[9px] font-black text-indigo-400 bg-indigo-500/10 px-3 py-1 rounded-full uppercase tracking-tighter border border-indigo-500/20">CKEditor 5 PRO</span>
              </label>
              <div className={cn("flex-1 min-h-105 border rounded-2xl overflow-hidden bg-slate-800/60", register("description").error ? "border-rose-500/70" : "border-slate-700")}>
                  <CustomEditor
                  value={formData.description}
                  onChange={(data) => setFormData({ ...formData, description: data })}
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
