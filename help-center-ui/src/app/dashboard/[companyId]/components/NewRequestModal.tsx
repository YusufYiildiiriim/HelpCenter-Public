import React, { useEffect, useMemo } from "react";
import { Send, X, Loader2, Paperclip, FileText, FileSpreadsheet, Image as ImageIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import type { CustomerModule } from "@/services/customer/CustomerModuleService";
import type { CustomerRequestSubject } from "@/services/customer/CustomerRequestSubjectService";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

export interface NewRequestFormData {
  title: string;
  description: string;
  moduleId: string;
  requestSubjectId: string;
  priority: number;
}

interface NewRequestModalProps {
  isOpen: boolean;
  onClose: () => void;
  subjects: CustomerRequestSubject[];
  modules: CustomerModule[];
  newRequest: NewRequestFormData;
  setNewRequest: React.Dispatch<React.SetStateAction<NewRequestFormData>>;
  selectedFiles: File[];
  removeFile: (index: number) => void;
  isFileTypeMenuOpen: boolean;
  setIsFileTypeMenuOpen: (open: boolean) => void;
  handleFileSelect: (type: "pdf" | "excel" | "image") => void;
  fileInputRef: React.RefObject<HTMLInputElement | null>;
  acceptedType: string;
  onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onSubmit: (e: React.FormEvent) => void;
  submitting: boolean;
  menuRef: React.RefObject<HTMLDivElement | null>;
}

export const NewRequestModal: React.FC<NewRequestModalProps> = ({
  isOpen,
  onClose,
  subjects,
  modules,
  newRequest,
  setNewRequest,
  selectedFiles,
  removeFile,
  isFileTypeMenuOpen,
  setIsFileTypeMenuOpen,
  handleFileSelect,
  fileInputRef,
  acceptedType,
  onFileChange,
  onSubmit,
  submitting,
  menuRef
}) => {
  const values = useMemo(
    () => ({ title: newRequest.title, requestSubjectId: newRequest.requestSubjectId, moduleId: newRequest.moduleId, description: newRequest.description }),
    [newRequest.title, newRequest.requestSubjectId, newRequest.moduleId, newRequest.description]
  );

  const fieldConfig = useMemo(() => ({
    title: { label: "Talep Başlığı", rules: { required: true } },
    requestSubjectId: { label: "Talep Konusu", rules: { required: true } },
    moduleId: { label: "İlgili Modül", rules: { required: true } },
    description: { label: "Detaylı Açıklama", rules: { required: true } },
  }), []);

  const { register, handleSubmit, reset } = useValidatedForm<typeof values>(values, fieldConfig);

  useEffect(() => {
    if (isOpen) reset();
  }, [isOpen, reset]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setNewRequest({ ...newRequest, [e.target.name]: e.target.value });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-md z-[100] flex items-center justify-center p-4 animate-in fade-in duration-300">
        <div className="bg-slate-900/95 backdrop-blur-xl rounded-[2rem] shadow-2xl w-full max-w-xl overflow-hidden relative border border-slate-800">
            <div className="px-6 py-5 border-b border-slate-800 flex justify-between items-center bg-slate-900/50">
                <div className="flex items-center gap-4">
                    <div className="w-10 h-10 bg-indigo-600 text-white rounded-xl flex items-center justify-center shadow-lg shadow-indigo-600/20">
                        <Send size={20} />
                    </div>
                    <div>
                        <h2 className="text-xl font-black text-white tracking-tight">Yeni Destek Talebi</h2>
                        <p className="text-slate-400 text-[10px] font-bold uppercase tracking-widest">Teknik Destek Hattı</p>
                    </div>
                </div>
                <button
                    onClick={onClose}
                    className="w-10 h-10 flex items-center justify-center rounded-full hover:bg-slate-800 text-slate-400 hover:text-white transition-colors"
                >
                    <X size={20} />
                </button>
            </div>

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="p-6 space-y-4">
                <FormField
                    name="title"
                    label="Talep Başlığı"
                    value={newRequest.title}
                    onChange={handleChange}
                    onBlur={register("title").onBlur}
                    error={register("title").error}
                    required
                    placeholder="Örn: SQL Yedekleme Hatası"
                />

                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        as="select"
                        name="requestSubjectId"
                        label="Talep Konusu"
                        value={newRequest.requestSubjectId}
                        onChange={handleChange}
                        onBlur={register("requestSubjectId").onBlur}
                        error={register("requestSubjectId").error}
                        required
                    >
                        <option value="">Seçiniz...</option>
                        {subjects.map((subject) => (
                            <option key={subject.id} value={subject.id}>{subject.name}</option>
                        ))}
                    </FormField>
                    <FormField
                        as="select"
                        name="moduleId"
                        label="İlgili Modül"
                        value={newRequest.moduleId}
                        onChange={handleChange}
                        onBlur={register("moduleId").onBlur}
                        error={register("moduleId").error}
                        required
                    >
                        <option value="">Seçiniz...</option>
                        {modules.map((module) => (
                            <option key={module.id} value={module.id}>{module.name}</option>
                        ))}
                    </FormField>
                </div>

                <FormField
                    as="select"
                    name="priority"
                    label="Aciliyet / Öncelik"
                    value={newRequest.priority}
                    onChange={(e) => setNewRequest({ ...newRequest, priority: Number(e.target.value) })}
                    className={cn(
                        "font-bold",
                        newRequest.priority === 3 ? "text-red-400" :
                        newRequest.priority === 2 ? "text-orange-400" :
                        newRequest.priority === 1 ? "text-indigo-400" :
                        "text-slate-300"
                    )}
                >
                    <option value={0}>Düşük (İvediliği Yok)</option>
                    <option value={1}>Normal (Standart)</option>
                    <option value={2}>Yüksek (Hızlı Çözüm Gerekli)</option>
                    <option value={3}>Kritik (Sistem Kesintisi / Acil)</option>
                </FormField>

                <FormField
                    as="textarea"
                    name="description"
                    label="Detaylı Açıklama"
                    rows={3}
                    value={newRequest.description}
                    onChange={handleChange}
                    onBlur={register("description").onBlur}
                    error={register("description").error}
                    required
                    placeholder="Yaşadığınız sorunu detaylandırın..."
                />

                <div className="space-y-3">
                    <div className="flex items-center justify-between">
                        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Dosya Ekleri</label>
                        <div className="relative" ref={menuRef}>
                            <button 
                                type="button"
                                onClick={() => setIsFileTypeMenuOpen(!isFileTypeMenuOpen)}
                                className="flex items-center gap-1.5 text-[10px] font-black text-indigo-400 hover:text-indigo-300 transition-colors uppercase tracking-widest"
                            >
                                <Paperclip size={14} />
                                Dosya Ekle
                            </button>

                            {isFileTypeMenuOpen && (
                                <div className="absolute bottom-full mb-2 right-0 bg-slate-800 border border-slate-700 rounded-xl shadow-2xl p-1.5 w-40 animate-in fade-in zoom-in slide-in-from-bottom-2 z-50">
                                    <button 
                                        type="button"
                                        onClick={() => handleFileSelect("pdf")}
                                        className="w-full flex items-center gap-2.5 p-2 rounded-lg hover:bg-red-500/10 text-slate-300 hover:text-red-400 transition-all text-[10px] font-bold"
                                    >
                                        <FileText size={14} className="text-red-400" />
                                        PDF
                                    </button>
                                    <button 
                                        type="button"
                                        onClick={() => handleFileSelect("excel")}
                                        className="w-full flex items-center gap-2.5 p-2 rounded-lg hover:bg-emerald-500/10 text-slate-300 hover:text-emerald-400 transition-all text-[10px] font-bold"
                                    >
                                        <FileSpreadsheet size={14} className="text-emerald-400" />
                                        Excel
                                    </button>
                                    <button 
                                        type="button"
                                        onClick={() => handleFileSelect("image")}
                                        className="w-full flex items-center gap-2.5 p-2 rounded-lg hover:bg-indigo-500/10 text-slate-300 hover:text-indigo-400 transition-all text-[10px] font-bold"
                                    >
                                        <ImageIcon size={14} className="text-indigo-400" />
                                        Görsel
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                    
                    {selectedFiles.length > 0 && (
                        <div className="flex flex-wrap gap-1.5 animate-in fade-in slide-in-from-bottom-1">
                            {selectedFiles.map((file, index) => (
                                <div key={index} className="flex items-center gap-1.5 bg-slate-800 text-slate-300 px-2 py-1 rounded-lg text-[10px] font-bold border border-slate-700">
                                    <span className="max-w-[100px] truncate">{file.name}</span>
                                    <button type="button" onClick={() => removeFile(index)} className="hover:text-red-400 transition-colors">
                                        <X size={12} />
                                    </button>
                                </div>
                            ))}
                        </div>
                    )}

                    <input 
                        type="file"
                        ref={fileInputRef}
                        className="hidden"
                        accept={acceptedType}
                        multiple
                        onChange={onFileChange}
                    />
                </div>

                <div className="flex gap-3 pt-4">
                    <button
                        type="button"
                        onClick={onClose}
                        className="flex-1 px-6 py-3.5 rounded-xl font-black text-[10px] uppercase tracking-widest text-slate-400 hover:bg-slate-800 hover:text-white transition-colors"
                    >
                        Vazgeç
                    </button>
                    <button
                        disabled={submitting}
                        className="flex-[2] bg-indigo-600 hover:bg-indigo-500 text-white font-black py-3.5 rounded-xl shadow-lg shadow-indigo-600/20 transition-all active:scale-[0.98] disabled:opacity-50 flex items-center justify-center gap-2 text-[10px] uppercase tracking-widest"
                    >
                        {submitting ? <Loader2 className="animate-spin" size={18} /> : (
                            <>
                                Talebi Gönder
                                <Send size={16} />
                            </>
                        )}
                    </button>
                </div>
            </form>
        </div>
    </div>
  );
};
