import React, { useEffect, useMemo, useState } from "react";
import { X, Building2, Users, Zap, Loader2, Key, AtSign, FolderKanban, Check, ChevronRight, ChevronLeft, Sparkles } from "lucide-react";
import type { Module } from "@/services/admin/AdminModuleService";
import type { Project } from "@/services/admin/AdminProjectService";
import { cn } from "@/lib/utils";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

export interface CompanyFormData {
  name: string;
  mail: string;
  phone: string;
  address: string;
  branchCount: number;
  contactPersonName: string;
  contactPersonSurname: string;
  contactPersonEmail: string;
  contactPersonPhone: string;
  contactPersonUsername?: string;
  previousSystem?: string;
  password?: string;
  isDemoActive: boolean;
  moduleIds: number[];
  projectId: number | null;
  rowVersion?: string;
}

interface CompanyModalProps {
  isOpen: boolean;
  isEditMode: boolean;
  onClose: () => void;
  onSubmit: (e: React.FormEvent) => void;
  formData: CompanyFormData;
  setFormData: React.Dispatch<React.SetStateAction<CompanyFormData>>;
  modules: Module[];
  projects: Project[];
  loading: boolean;
  toggleModule: (moduleId: number) => void;
  handleChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => void;
}

const TABS = [
  { key: "info", label: "Firma Bilgileri", icon: <Building2 size={14} /> },
  { key: "contact", label: "Yetkili Kişi", icon: <Users size={14} /> },
  { key: "system", label: "Sistem & Demo", icon: <Zap size={14} /> },
];

export const CompanyModal: React.FC<CompanyModalProps> = ({
  isOpen,
  isEditMode,
  onClose,
  onSubmit,
  formData,
  setFormData,
  modules,
  projects,
  loading,
  toggleModule,
  handleChange
}) => {
  const [activeTab, setActiveTab] = useState(0);
  const [modulePickerValue, setModulePickerValue] = useState("");

  const availableModules = useMemo(() => {
    const selectedProject = projects.find((project) => project.id === formData.projectId);
    if (!selectedProject) return [];

    return modules.filter((module) => selectedProject.moduleIds.includes(module.id));
  }, [formData.projectId, modules, projects]);

  const selectedModules = availableModules.filter((module) => formData.moduleIds.includes(module.id));
  const selectableModules = availableModules.filter((module) => !formData.moduleIds.includes(module.id));

  const fieldConfig = useMemo(() => ({
    name: { label: "Firma Adı", rules: { required: true } },
    mail: { label: "Firma E-Posta", rules: { required: true, type: "email" as const } },
    phone: { label: "Telefon", rules: { required: true, type: "phone" as const } },
    branchCount: { label: "Şube Sayısı", rules: { required: true, type: "number" as const } },
    address: { label: "Adres", rules: { required: true } },
    contactPersonName: { label: "Yetkili Kişi Adı", rules: { required: true } },
    contactPersonSurname: { label: "Yetkili Kişi Soyadı", rules: { required: true } },
    contactPersonEmail: { label: "Yetkili E-Posta", rules: { required: true, type: "email" as const } },
    contactPersonPhone: { label: "Yetkili Telefon", rules: { required: true, type: "phone" as const } },
    previousSystem: { label: "Önceki Sistem", rules: { required: true } },
    password: {
      label: isEditMode ? "Demo Şifresi" : "Demo Kullanıcı Şifresi",
      rules: { required: formData.isDemoActive && !isEditMode },
    },
  }), [formData.isDemoActive, isEditMode]);

  const { register, handleSubmit, validateFields, reset } = useValidatedForm<CompanyFormData>(formData, fieldConfig);

  useEffect(() => {
    if (isOpen) reset();
  }, [isOpen, reset]);

  if (!isOpen) return null;

  const TAB_FIELDS: Record<number, (keyof CompanyFormData)[]> = {
    0: ["name", "mail", "phone", "branchCount", "address"],
    1: ["contactPersonName", "contactPersonSurname", "contactPersonEmail", "contactPersonPhone"],
    2: ["previousSystem", "password"],
  };

  const goNext = () => {
    const fields = TAB_FIELDS[activeTab] || [];
    if (!validateFields(fields)) return;
    setActiveTab((t) => Math.min(t + 1, TABS.length - 1));
  };
  const goBack = () => setActiveTab((t) => Math.max(t - 1, 0));

  const handleTabClick = (targetIndex: number) => {
    if (targetIndex <= activeTab) {
      setActiveTab(targetIndex);
      return;
    }
    for (let s = activeTab; s < targetIndex; s++) {
      const fields = TAB_FIELDS[s] || [];
      if (fields.length > 0 && !validateFields(fields)) {
        setActiveTab(s);
        return;
      }
    }
    setActiveTab(targetIndex);
  };

  const sectionHeader = (icon: React.ReactNode, title: string) => (
    <div className="flex items-center gap-2 pb-2 border-b border-slate-800 mb-4">
      {icon}
      <h3 className="text-xs font-black text-slate-400 uppercase tracking-widest">{title}</h3>
    </div>
  );

  return (
    <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-md z-[100] flex justify-center items-center p-2 sm:p-4 md:p-6 lg:pl-76 lg:pr-6 overflow-y-auto animate-in fade-in duration-300">
      <div className="bg-slate-900 shadow-2xl w-full max-w-2xl my-auto overflow-hidden relative border border-slate-800 rounded-2xl flex flex-col max-h-[92vh] sm:max-h-[90vh]">
        {/* Header */}
        <div className="p-4 sm:p-6 border-b border-slate-800 flex justify-between items-center bg-slate-950/60 shrink-0">
          <div className="flex items-center gap-3 sm:gap-4 min-w-0">
            <div className="w-10 h-10 sm:w-12 sm:h-12 shrink-0 bg-linear-to-br from-indigo-600 to-violet-600 text-white rounded-xl sm:rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-600/20">
              <Building2 size={22} className="sm:hidden" />
              <Building2 size={24} className="hidden sm:block" />
            </div>
            <div className="min-w-0">
              <h2 className="text-base sm:text-lg md:text-xl font-black text-white tracking-tight truncate">
                {isEditMode ? "Firmayı Güncelle" : "Firma Tanımla"}
              </h2>
              <p className="text-slate-400 text-[10px] font-bold uppercase tracking-widest mt-0.5">
                {isEditMode ? "Bilgileri Düzenle" : "Yeni Müşteri Kaydı"}
              </p>
            </div>
          </div>
          <button 
            onClick={onClose} 
            className="w-9 h-9 sm:w-10 sm:h-10 shrink-0 flex items-center justify-center rounded-xl bg-slate-800/60 hover:bg-slate-800 text-slate-400 hover:text-white transition-colors cursor-pointer"
          >
            <X size={20} />
          </button>
        </div>

        {/* Tab Navigation */}
        <div className="px-4 sm:px-6 md:px-8 pt-4 sm:pt-5 shrink-0">
          <div className="flex gap-1.5 sm:gap-2 overflow-x-auto custom-scrollbar pb-2 scrollbar-none">
            {TABS.map((tab, i) => (
              <button
                key={tab.key}
                type="button"
                onClick={() => handleTabClick(i)}
                className={cn(
                  "flex items-center gap-1.5 px-3 sm:px-4 py-2 rounded-xl text-[10px] sm:text-xs font-black uppercase tracking-wider transition-all whitespace-nowrap border shrink-0 cursor-pointer",
                  activeTab === i
                    ? "bg-linear-to-r from-indigo-600 to-violet-600 text-white border-transparent shadow-md shadow-indigo-600/20"
                    : i < activeTab
                    ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-400 hover:bg-emerald-500/20"
                    : "bg-white/5 border-slate-700/80 text-slate-400 hover:bg-white/10 hover:text-slate-200"
                )}
              >
                {i < activeTab ? <Check size={12} /> : tab.icon}
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        <form
          onSubmit={handleSubmit(onSubmit)}
          onKeyDown={(e) => {
            // Prevent pressing Enter in a single-line input during intermediate steps from
            // submitting the form in an incomplete state — only allow on the last step.
            if (e.key === "Enter" && (e.target as HTMLElement).tagName === "INPUT" && activeTab !== TABS.length - 1) {
              e.preventDefault();
            }
          }}
          noValidate
          className="flex-1 overflow-y-auto custom-scrollbar p-4 sm:p-6 md:p-8 space-y-5"
        >
          {/* ─── Tab 1: Company Information ─── */}
          {activeTab === 0 && (
            <div className="space-y-4 animate-in fade-in slide-in-from-right-2 duration-300">
              {sectionHeader(<Building2 size={16} className="text-indigo-400" />, "Firma Bilgileri")}
              
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                <FormField
                  name="name"
                  label="Firma Adı"
                  value={formData.name}
                  onChange={handleChange}
                  onBlur={register("name").onBlur}
                  error={register("name").error}
                  required
                  placeholder="Örn: Akel Teknoloji"
                />
                <FormField
                  name="mail"
                  label="Firma E-Posta"
                  type="email"
                  value={formData.mail}
                  onChange={handleChange}
                  onBlur={register("mail").onBlur}
                  error={register("mail").error}
                  required
                  placeholder="destek@firma.com"
                />
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                <FormField
                  name="phone"
                  label="Telefon"
                  value={formData.phone}
                  onChange={handleChange}
                  onBlur={register("phone").onBlur}
                  error={register("phone").error}
                  required
                  placeholder="0212 --- -- --"
                />
                <FormField
                  name="branchCount"
                  label="Şube Sayısı"
                  type="number"
                  value={formData.branchCount}
                  onChange={handleChange}
                  onBlur={register("branchCount").onBlur}
                  error={register("branchCount").error}
                  required
                />
              </div>

              <FormField
                as="textarea"
                name="address"
                label="Adres"
                rows={2}
                value={formData.address}
                onChange={handleChange}
                onBlur={register("address").onBlur}
                error={register("address").error}
                required
                placeholder="Firmanın resmi adresi..."
              />
            </div>
          )}

          {/* ─── Tab 2: Contact Person ─── */}
          {activeTab === 1 && (
            <div className="space-y-4 animate-in fade-in slide-in-from-right-2 duration-300">
              {sectionHeader(<Users size={16} className="text-orange-400" />, "Yetkili Kişi Bilgileri")}

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                <FormField
                  name="contactPersonName"
                  label="Ad"
                  value={formData.contactPersonName}
                  onChange={handleChange}
                  onBlur={register("contactPersonName").onBlur}
                  error={register("contactPersonName").error}
                  required
                  placeholder="Örn: Ahmet"
                />
                <FormField
                  name="contactPersonSurname"
                  label="Soyad"
                  value={formData.contactPersonSurname}
                  onChange={handleChange}
                  onBlur={register("contactPersonSurname").onBlur}
                  error={register("contactPersonSurname").error}
                  required
                  placeholder="Örn: Yılmaz"
                />
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                <FormField
                  name="contactPersonEmail"
                  label="Yetkili E-Posta"
                  type="email"
                  value={formData.contactPersonEmail}
                  onChange={handleChange}
                  onBlur={register("contactPersonEmail").onBlur}
                  error={register("contactPersonEmail").error}
                  required
                  placeholder="ahmet@firma.com"
                />
                <FormField
                  name="contactPersonPhone"
                  label="Yetkili Telefon"
                  value={formData.contactPersonPhone}
                  onChange={handleChange}
                  onBlur={register("contactPersonPhone").onBlur}
                  error={register("contactPersonPhone").error}
                  required
                  placeholder="0530 --- -- --"
                />
              </div>

              <FormField
                name="contactPersonUsername"
                label="Kullanıcı Adı (boş bırakılırsa ad soyaddan otomatik oluşturulur)"
                value={formData.contactPersonUsername ?? ""}
                onChange={handleChange}
                icon={<AtSign size={16} />}
                placeholder="ahmet.yilmaz"
              />
            </div>
          )}

          {/* ─── Tab 3: System Definitions ─── */}
          {activeTab === 2 && (
            <div className="space-y-4 animate-in fade-in slide-in-from-right-2 duration-300">
              {sectionHeader(<Zap size={16} className="text-indigo-400" />, "Sistem Tanımlamaları")}

              <FormField
                name="previousSystem"
                label="Önceki Sistem"
                value={formData.previousSystem}
                onChange={handleChange}
                onBlur={register("previousSystem").onBlur}
                error={register("previousSystem").error}
                required
                placeholder="Örn: Logo, Akınsoft"
              />

              <div className="space-y-1.5">
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1 flex items-center gap-1.5">
                  <FolderKanban size={12} className="text-indigo-400" />
                  Proje
                </label>
                <select
                  name="projectId"
                  value={formData.projectId ?? ""}
                  onChange={(e) => {
                    const newProjectId = e.target.value ? Number(e.target.value) : null;
                    const newProject = projects.find((p) => p.id === newProjectId);
                    setFormData({
                      ...formData,
                      projectId: newProjectId,
                      moduleIds: (formData.moduleIds || []).filter((id: number) => newProject?.moduleIds.includes(id)),
                    });
                    setModulePickerValue("");
                  }}
                  className="w-full px-4 py-2.5 bg-slate-800/70 border border-slate-700 rounded-xl text-slate-100 focus:bg-slate-800 focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all text-sm font-bold"
                >
                  <option value="">Proje seçilmedi</option>
                  {projects.map((project) => (
                    <option key={project.id} value={project.id}>{project.name}</option>
                  ))}
                </select>
                <p className="text-[10px] text-slate-500 ml-1">Bu firmanın talepleri, atandığı projede çalışan kullanıcılara görünür olacaktır.</p>
              </div>

              <div className="space-y-1.5">
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Modüller</label>
                {!formData.projectId ? (
                  <div className="p-3 bg-slate-800/60 border border-slate-700 rounded-xl text-[11px] text-slate-400 font-bold">
                    Modül seçebilmek için önce bir proje seçin.
                  </div>
                ) : availableModules.length === 0 ? (
                  <div className="p-3 bg-slate-800/60 border border-slate-700 rounded-xl text-[11px] text-slate-400 font-bold">
                    Seçili projeye ait tanımlı modül bulunmuyor.
                  </div>
                ) : (
                  <div className="space-y-2 rounded-xl border border-slate-700 bg-slate-800/60 p-3">
                    <select
                      aria-label="Modül ekle"
                      value={modulePickerValue}
                      onChange={(event) => {
                        const moduleId = Number(event.target.value);
                        if (!moduleId) return;

                        toggleModule(moduleId);
                        setModulePickerValue("");
                      }}
                      className="w-full rounded-xl border border-slate-700 bg-slate-900 px-3.5 py-2.5 text-xs font-bold text-slate-100 outline-none transition-colors focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 disabled:cursor-not-allowed disabled:opacity-50"
                      disabled={selectableModules.length === 0}
                    >
                      <option value="">
                        {selectableModules.length === 0 ? "Tüm modüller eklendi" : "Modül seçip ekleyin..."}
                      </option>
                      {selectableModules.map((module) => (
                        <option key={module.id} value={module.id}>{module.name}</option>
                      ))}
                    </select>

                    <p className="px-1 text-[10px] font-medium text-slate-500">
                      Seçim kutusunu açtıktan sonra modül adı yazarak doğrudan bulabilirsiniz.
                    </p>

                    {selectedModules.length > 0 && (
                      <div className="flex flex-wrap gap-2 border-t border-slate-700/70 pt-2">
                        {selectedModules.map((module) => (
                          <button
                            key={module.id}
                            type="button"
                            onClick={() => toggleModule(module.id)}
                            className="inline-flex items-center gap-1.5 rounded-lg border border-indigo-500/30 bg-indigo-500/15 px-2.5 py-1.5 text-xs font-bold text-indigo-200 transition-colors hover:border-red-500/30 hover:bg-red-500/15 hover:text-red-300"
                            title={`${module.name} modülünü kaldır`}
                          >
                            {module.name}
                            <X size={13} aria-hidden="true" />
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Demo Status */}
              <div className="pt-3">
                <div className="flex items-center gap-2 pb-2 border-b border-slate-800 mb-3">
                  <Sparkles size={16} className="text-indigo-400" />
                  <h3 className="text-xs font-black text-slate-400 uppercase tracking-widest">Demo Statüsü</h3>
                </div>

                <div className="flex flex-col sm:flex-row sm:items-center justify-between p-4 sm:p-5 bg-slate-800/60 rounded-2xl border border-slate-700 mb-4 gap-3 sm:gap-0">
                  <div>
                    <p className="text-sm font-black text-slate-100 tracking-tight">Demo Statüsü</p>
                    <p className="text-[10px] text-slate-400 font-medium tracking-tight">Firma sistemi demo olarak mı kullanacak?</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, isDemoActive: !formData.isDemoActive, password: "" })}
                    className={cn(
                      "w-14 h-7 rounded-full transition-all relative shrink-0 cursor-pointer self-start sm:self-auto",
                      formData.isDemoActive ? "bg-orange-500" : "bg-slate-600"
                    )}
                  >
                    <div className={cn(
                      "absolute top-1 w-5 h-5 bg-white rounded-full transition-all shadow-sm",
                      formData.isDemoActive ? "left-8" : "left-1"
                    )} />
                  </button>
                </div>

                {formData.isDemoActive && !isEditMode && (
                  <div className="space-y-1.5">
                    <div className="flex items-center gap-2 pb-2 border-b border-slate-800 mb-2">
                      <Key size={16} className="text-orange-400" />
                      <h3 className="text-xs font-black text-slate-400 uppercase tracking-widest">Demo Giriş Bilgileri</h3>
                    </div>
                    <FormField
                      name="password"
                      label="Demo Kullanıcı Şifresi"
                      type="password"
                      value={formData.password ?? ""}
                      onChange={handleChange}
                      onBlur={register("password").onBlur}
                      error={register("password").error}
                      required
                      placeholder="••••••••"
                    />
                    <p className="text-[10px] text-slate-500 ml-1 mt-1">Yetkili kişi ilk girişte şifresini değiştirmek zorunda kalacak.</p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Footer Navigation */}
          <div className="flex items-center justify-between gap-3 pt-4 border-t border-slate-800 shrink-0">
            {activeTab > 0 ? (
              <button
                type="button"
                onClick={goBack}
                className="flex items-center gap-1.5 sm:gap-2 px-4 sm:px-6 py-2.5 sm:py-3 bg-white/10 hover:bg-white/20 text-slate-200 rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] transition-all active:scale-95 cursor-pointer"
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
                className="flex items-center gap-1.5 sm:gap-2 px-4 sm:px-6 py-2.5 sm:py-3 bg-linear-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] shadow-lg shadow-indigo-600/20 transition-all active:scale-95 cursor-pointer"
              >
                Devam Et
                <ChevronRight size={16} />
              </button>
            ) : (
              <button 
                type="submit"
                disabled={loading}
                className="flex items-center gap-1.5 sm:gap-2 px-4 sm:px-6 py-2.5 sm:py-3 bg-linear-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] shadow-lg shadow-indigo-600/20 transition-all active:scale-95 disabled:opacity-50 cursor-pointer"
              >
                {loading ? <Loader2 className="animate-spin" size={16} /> : <Check size={16} />}
                {loading ? "İşleniyor..." : (isEditMode ? "Kaydet" : "Firmayı Oluştur")}
              </button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
};
