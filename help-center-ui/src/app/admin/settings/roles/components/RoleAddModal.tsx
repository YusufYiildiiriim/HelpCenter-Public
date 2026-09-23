import React, { useEffect, useMemo } from "react";
import { Plus, X, Loader2, Save } from "lucide-react";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

interface RoleAddModalProps {
  isOpen: boolean;
  onClose: () => void;
  newRole: { name: string; description: string };
  setNewRole: (role: { name: string; description: string }) => void;
  onSubmit: (e: React.FormEvent) => void;
  saving: boolean;
}

export const RoleAddModal: React.FC<RoleAddModalProps> = ({
  isOpen,
  onClose,
  newRole,
  setNewRole,
  onSubmit,
  saving
}) => {
  const fieldConfig = useMemo(() => ({
    name: { label: "Rol Adı", rules: { required: true } },
  }), []);

  const { register, handleSubmit, reset } = useValidatedForm<{ name: string; description: string }>(newRole, fieldConfig);

  useEffect(() => {
    if (isOpen) reset();
  }, [isOpen, reset]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setNewRole({ ...newRole, [e.target.name]: e.target.value });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-4 lg:pl-76 lg:pr-6 overflow-y-auto">
        <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-md animate-in fade-in duration-300" onClick={() => !saving && onClose()} />
        <div className="relative w-full max-w-md bg-slate-900 rounded-2xl sm:rounded-3xl shadow-2xl border border-slate-800 p-4 sm:p-6 md:p-8 my-auto max-h-[84vh] overflow-y-auto animate-in zoom-in slide-in-from-bottom-4 duration-300">
            <div className="flex justify-between items-center mb-6 sm:mb-8">
                <div className="flex items-center gap-3 sm:gap-4 min-w-0">
                    <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl sm:rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center text-white shadow-lg shadow-indigo-600/20 shrink-0">
                        <Plus size={20} className="sm:hidden" />
                        <Plus size={24} className="hidden sm:block" />
                    </div>
                    <div className="min-w-0">
                        <h2 className="text-lg sm:text-xl md:text-2xl font-black text-white leading-none truncate">Yeni Rol Ekle</h2>
                        <p className="text-slate-400 text-xs font-medium mt-1 truncate">Sistem için yeni yetki grubu oluşturun.</p>
                    </div>
                </div>
                <button 
                    onClick={onClose}
                    className="w-8 h-8 sm:w-10 sm:h-10 rounded-xl bg-slate-800 border border-slate-700 flex items-center justify-center text-slate-400 hover:bg-slate-700 hover:text-white transition-all shrink-0 cursor-pointer"
                >
                    <X size={18} />
                </button>
            </div>

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4 sm:space-y-6">
                <FormField
                    name="name"
                    label="Rol Adı"
                    value={newRole.name}
                    onChange={handleChange}
                    onBlur={register("name").onBlur}
                    error={register("name").error}
                    required
                    placeholder="Örn: Destek Personeli"
                />
                <FormField
                    as="textarea"
                    name="description"
                    label="Açıklama"
                    rows={3}
                    value={newRole.description}
                    onChange={handleChange}
                    placeholder="Bu rolün görevlerini kısaca açıklayın..."
                />

                <div className="flex gap-2.5 sm:gap-3 pt-2 sm:pt-4">
                    <button 
                        type="button"
                        onClick={onClose}
                        className="flex-1 px-4 sm:px-6 py-3 sm:py-3.5 bg-white/10 text-slate-300 rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] hover:bg-white/20 transition-all active:scale-95 cursor-pointer"
                    >
                        Vazgeç
                    </button>
                    <button 
                        type="submit"
                        disabled={saving}
                        className="flex-[2] px-4 sm:px-6 py-3 sm:py-3.5 bg-gradient-to-r from-indigo-600 to-violet-600 text-white rounded-xl sm:rounded-2xl font-black uppercase tracking-widest text-[10px] hover:from-indigo-500 hover:to-violet-500 shadow-lg shadow-indigo-600/20 transition-all disabled:opacity-50 flex items-center justify-center gap-2 active:scale-95 cursor-pointer"
                    >
                        {saving ? <Loader2 className="animate-spin" size={14} /> : <Save size={14} />}
                        {saving ? "Oluşturuluyor..." : "Rolü Oluştur"}
                    </button>
                </div>
            </form>
        </div>
    </div>
  );
};
