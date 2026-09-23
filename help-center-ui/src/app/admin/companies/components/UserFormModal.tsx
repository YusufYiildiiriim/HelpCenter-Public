import React, { useEffect, useMemo } from "react";
import { X, UserPlus, Mail, User, Phone, Key, Loader2, AtSign } from "lucide-react";
import { cn } from "@/lib/utils";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

interface UserFormData {
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  password: string;
  phoneNumber: string;
  isActive: boolean;
}

interface UserFormModalProps {
  isOpen: boolean;
  isEditMode: boolean;
  onClose: () => void;
  onSubmit: (e: React.FormEvent) => void;
  formData: UserFormData;
  setFormData: (data: UserFormData) => void;
  loading: boolean;
  handleChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => void;
}

export const UserFormModal: React.FC<UserFormModalProps> = ({
  isOpen,
  isEditMode,
  onClose,
  onSubmit,
  formData,
  setFormData,
  loading,
  handleChange
}) => {
  const fieldConfig = useMemo(() => ({
    firstName: { label: "Ad", rules: { required: true } },
    lastName: { label: "Soyad", rules: { required: true } },
    email: { label: "E-Posta Adresi", rules: { required: true, type: "email" as const } },
    phoneNumber: { label: "Telefon", rules: { type: "phone" as const } },
    password: { label: isEditMode ? "Şifre" : "Şifre", rules: { required: !isEditMode } },
  }), [isEditMode]);

  const { register, handleSubmit, reset } = useValidatedForm<UserFormData>(formData, fieldConfig);

  useEffect(() => {
    if (isOpen) reset();
  }, [isOpen, reset]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[102] flex items-center justify-center overflow-y-auto bg-slate-950/80 p-2 backdrop-blur-md animate-in zoom-in duration-300 sm:p-4 md:p-6 lg:pl-76 lg:pr-6">
      <div className="relative my-auto flex max-h-[92vh] w-full max-w-lg flex-col overflow-hidden rounded-2xl border border-slate-800 bg-slate-900 shadow-2xl shadow-slate-950/50 sm:max-h-[90vh] sm:rounded-3xl">
        <div className="flex shrink-0 items-center justify-between border-b border-slate-800 bg-slate-900 p-4 sm:p-6 md:p-8">
          <div className="flex items-center gap-3 sm:gap-4 min-w-0">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-600/20 sm:h-12 sm:w-12 sm:rounded-2xl">
              <UserPlus size={20} className="sm:hidden" />
              <UserPlus size={22} className="hidden sm:block" />
            </div>
            <div className="min-w-0">
              <h3 className="truncate text-base font-black tracking-tight text-white sm:text-xl">
                {isEditMode ? "Kullanıcı Güncelle" : "Yeni Kullanıcı Ekle"}
              </h3>
              <p className="mt-0.5 text-[10px] font-bold uppercase tracking-widest text-slate-400">
                Erişim Yetkilendirme
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose} 
            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-slate-700 bg-slate-800 text-slate-400 transition-colors hover:border-red-500/30 hover:bg-red-500/15 hover:text-red-400 sm:h-10 sm:w-10"
          >
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex-1 overflow-y-auto custom-scrollbar p-4 sm:p-6 md:p-8 space-y-4 sm:space-y-5">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
            <FormField
              name="firstName"
              label="Ad"
              value={formData.firstName}
              onChange={handleChange}
              onBlur={register("firstName").onBlur}
              error={register("firstName").error}
              required
              icon={<User size={16} />}
              placeholder="Ahmet"
            />
            <FormField
              name="lastName"
              label="Soyad"
              value={formData.lastName}
              onChange={handleChange}
              onBlur={register("lastName").onBlur}
              error={register("lastName").error}
              required
              placeholder="Yılmaz"
            />
          </div>

          <FormField
            name="username"
            label="Kullanıcı Adı (boş bırakılırsa otomatik oluşturulur)"
            value={formData.username ?? ""}
            onChange={handleChange}
            icon={<AtSign size={16} />}
            placeholder="ahmet.yilmaz"
          />

          <FormField
            name="email"
            label="E-Posta Adresi"
            type="email"
            value={formData.email}
            onChange={handleChange}
            onBlur={register("email").onBlur}
            error={register("email").error}
            required
            icon={<Mail size={16} />}
            placeholder="ahmet@firma.com"
          />

          <FormField
            name="phoneNumber"
            label="Telefon"
            value={formData.phoneNumber}
            onChange={handleChange}
            onBlur={register("phoneNumber").onBlur}
            error={register("phoneNumber").error}
            icon={<Phone size={16} />}
            placeholder="05-- --- -- --"
          />

          {!isEditMode && (
            <FormField
              name="password"
              label="Şifre"
              type="password"
              value={formData.password}
              onChange={handleChange}
              onBlur={register("password").onBlur}
              error={register("password").error}
              required
              icon={<Key size={16} />}
              placeholder="••••••••"
            />
          )}

          <div className="flex items-center justify-between rounded-xl border border-slate-700 bg-slate-800/60 p-3.5 sm:rounded-2xl sm:p-4">
            <div>
              <p className="text-xs font-black tracking-tight text-slate-100">Hesap Durumu</p>
              <p className="text-[10px] font-medium text-slate-400">Kullanıcı sisteme giriş yapabilsin mi?</p>
            </div>
            <button
              type="button"
              onClick={() => setFormData({ ...formData, isActive: !formData.isActive })}
              className={cn(
                "w-12 h-6 rounded-full transition-all relative shrink-0 cursor-pointer",
                formData.isActive ? "bg-emerald-500" : "bg-slate-600"
              )}
            >
              <div className={cn(
                "absolute top-1 w-4 h-4 bg-white rounded-full transition-all shadow-sm",
                formData.isActive ? "left-7" : "left-1"
              )} />
            </button>
          </div>

          <button 
            type="submit"
            disabled={loading}
            className="w-full bg-indigo-600 hover:bg-indigo-500 text-white font-black py-3 sm:py-3.5 rounded-xl sm:rounded-2xl shadow-lg shadow-indigo-600/20 transition-all active:scale-[0.98] disabled:opacity-50 flex items-center justify-center gap-2 mt-4 text-xs sm:text-sm uppercase tracking-wider cursor-pointer"
          >
            {loading ? <Loader2 className="animate-spin" size={18} /> : (isEditMode ? "Kullanıcıyı Güncelle" : "Kullanıcıyı Kaydet")}
          </button>
        </form>
      </div>
    </div>
  );
};
