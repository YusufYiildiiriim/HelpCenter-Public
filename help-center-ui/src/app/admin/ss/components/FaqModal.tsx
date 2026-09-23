import React, { useMemo } from "react";
import type { Faq } from "@/services/admin/AdminFaqService";
import type { Project } from "@/services/admin/AdminProjectService";
import type { Module } from "@/services/admin/AdminModuleService";
import { cn } from "@/lib/utils";
import { HelpCircle, X, ChevronDown } from "lucide-react";
import { FormField } from "@/components/common/FormField";
import { AppDialog } from "@/components/common/AppDialog";
import { useValidatedForm } from "@/hooks/useValidatedForm";

interface FaqModalProps {
  isEditMode: boolean;
  faq: Faq;
  projects: Project[];
  modules: Module[];
  onFaqChange: (faq: Faq) => void;
  onSave: (e: React.FormEvent) => void;
  onClose: () => void;
}

export const FaqModal: React.FC<FaqModalProps> = ({
  isEditMode,
  faq,
  projects,
  modules,
  onFaqChange,
  onSave,
  onClose
}) => {
  const values = {
    title: faq.title,
    description: faq.description,
    projectId: faq.projectId ? String(faq.projectId) : "",
    moduleId: faq.moduleId ? String(faq.moduleId) : "",
  };
  const { register, handleSubmit } = useValidatedForm<typeof values>(values, {
    title: { label: "Soru Başlığı", rules: { required: true } },
    description: { label: "Cevap Metni", rules: { required: true } },
    projectId: { label: "Proje", rules: { required: true } },
    moduleId: { label: "Modül", rules: { required: true } },
  });

  const selectedProject = projects.find(p => p.id === Number(faq.projectId));
  const availableModules = useMemo(() => {
    if (!faq.projectId || !selectedProject) return [];
    const assignedModuleIds = selectedProject.moduleIds || [];
    return modules.filter(m => assignedModuleIds.includes(m.id));
  }, [faq.projectId, selectedProject, modules]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    onFaqChange({ ...faq, [e.target.name]: e.target.value });
  };

  const handleProjectChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const val = e.target.value ? parseInt(e.target.value) : null;
    onFaqChange({ ...faq, projectId: val, moduleId: null });
  };

  const handleModuleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const val = e.target.value ? parseInt(e.target.value) : null;
    onFaqChange({ ...faq, moduleId: val });
  };

  return (
    <AppDialog
      title={isEditMode ? "Soru Düzenle" : "Yeni Soru Ekle"}
      description="SSS içerik yönetimi"
      onClose={onClose}
      className="w-[calc(100%-1.5rem)] max-w-lg overflow-hidden rounded-2xl sm:rounded-3xl border border-slate-800 bg-slate-900 shadow-2xl flex flex-col max-h-[80vh] sm:max-h-[84vh] animate-in zoom-in duration-300"
    >
        <div className="border-b border-slate-800 bg-slate-900 p-3.5 sm:p-5 flex justify-between items-center shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-to-br from-indigo-600 to-violet-600 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-600/20">
              <HelpCircle size={18} />
            </div>
            <div>
              <h2 className="text-base font-black tracking-tight text-slate-100">
                {isEditMode ? "Soru Düzenle" : "Yeni Soru Ekle"}
              </h2>
              <p className="mt-0.5 text-[10px] font-bold uppercase tracking-[0.18em] text-slate-500">SSS İçerik Yönetimi</p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Pencereyi kapat"
            className="w-8 h-8 rounded-xl bg-slate-800/70 hover:bg-slate-700 flex items-center justify-center text-slate-400 hover:text-white transition-all"
          >
            <X size={16} />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSave)} noValidate className="flex-1 overflow-y-auto p-3.5 sm:p-5 space-y-4 custom-scrollbar">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <FormField
              as="select"
              name="projectId"
              label="Proje"
              value={faq.projectId ?? ""}
              onChange={handleProjectChange}
              onBlur={register("projectId").onBlur}
              error={register("projectId").error}
              required
              right={<ChevronDown size={16} />}
            >
              <option value="">Proje Seçin</option>
              {projects.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </FormField>

            <FormField
              as="select"
              name="moduleId"
              label="Modül"
              value={faq.moduleId ?? ""}
              disabled={!faq.projectId}
              onChange={handleModuleChange}
              onBlur={register("moduleId").onBlur}
              error={register("moduleId").error}
              required
              right={<ChevronDown size={16} />}
            >
              <option value="">
                {!faq.projectId
                  ? "— Önce Proje Seçin —"
                  : availableModules.length === 0
                  ? "Atanmış modül yok"
                  : "Modül Seçin"}
              </option>
              {availableModules.map((m) => (
                <option key={m.id} value={m.id}>{m.name}</option>
              ))}
            </FormField>
          </div>

          <FormField
            name="title"
            label="Soru Başlığı"
            value={faq.title}
            onChange={handleChange}
            onBlur={register("title").onBlur}
            error={register("title").error}
            required
            placeholder="Müşterilerin en çok sorduğu soru nedir?"
          />

          <FormField
            as="textarea"
            name="description"
            label="Cevap Metni"
            rows={4}
            value={faq.description}
            onChange={handleChange}
            onBlur={register("description").onBlur}
            error={register("description").error}
            required
            placeholder="Detaylı bir cevap yazın..."
          />

          <div className="flex items-center justify-between rounded-xl border border-slate-700 bg-slate-800/50 p-4">
            <div>
              <p className="text-xs font-black uppercase tracking-tight text-slate-200">Soru Durumu</p>
              <p className="text-[10px] font-medium text-slate-500">Sistemde aktif olarak gösterilsin mi?</p>
            </div>
            <button
              type="button"
              onClick={() => onFaqChange({ ...faq, isActive: !faq.isActive })}
              className={cn("relative h-6 w-11 rounded-full transition-all", faq.isActive ? "bg-emerald-500" : "bg-slate-700")}
            >
              <div className={cn("absolute top-1 h-4 w-4 rounded-full bg-white shadow-sm transition-all", faq.isActive ? "left-6" : "left-1")} />
            </button>
          </div>

          <div className="flex items-center justify-between rounded-xl border border-slate-700 bg-slate-800/50 p-4">
            <div>
              <p className="text-xs font-black uppercase tracking-tight text-slate-200">Herkese Açık</p>
              <p className="text-[10px] font-medium text-slate-500">Giriş yapılmadan görüntülenebilsin mi?</p>
            </div>
            <button
              type="button"
              onClick={() => onFaqChange({ ...faq, isPublic: !faq.isPublic })}
              className={cn("relative h-6 w-11 rounded-full transition-all", faq.isPublic ? "bg-indigo-500" : "bg-slate-700")}
            >
              <div className={cn("absolute top-1 h-4 w-4 rounded-full bg-white shadow-sm transition-all", faq.isPublic ? "left-6" : "left-1")} />
            </button>
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="h-11 flex-1 rounded-xl border border-slate-700 text-[10px] font-black uppercase tracking-widest text-slate-300 transition-all hover:bg-slate-800 active:scale-95"
            >
              Vazgeç
            </button>
            <button
              type="submit"
              className="h-11 flex-1 rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 text-[10px] font-black uppercase tracking-[0.18em] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 active:scale-95"
            >
              {isEditMode ? "Güncelle" : "Kaydet"}
            </button>
          </div>
        </form>
    </AppDialog>
  );
};
