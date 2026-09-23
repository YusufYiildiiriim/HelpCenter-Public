"use client";

import React, { useEffect, useMemo } from "react";
import { X, CheckCircle2, Loader2, AtSign, User, Mail, Key, ToggleLeft } from "lucide-react";
import { cn } from "@/lib/utils";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

export interface UserFormData {
  id: number;
  name: string;
  lastName: string;
  email: string;
  username: string;
  password?: string;
  roleIds: number[];
  isActive: boolean;
}

interface UserModalProps {
  isOpen: boolean;
  onClose: () => void;
  isEditMode: boolean;
  userData: UserFormData;
  setUserData: React.Dispatch<React.SetStateAction<UserFormData>>;
  onSubmit: (e: React.FormEvent) => void;
  submitting: boolean;
  successMessage: string;
}

export const UserModal: React.FC<UserModalProps> = ({
  isOpen,
  onClose,
  isEditMode,
  userData,
  setUserData,
  onSubmit,
  submitting,
  successMessage
}) => {
  const fieldConfig = useMemo(() => ({
    name: { label: "Ad", rules: { required: true } },
    lastName: { label: "Soyad", rules: { required: true } },
    email: { label: "E-posta", rules: { required: true, type: "email" as const } },
    password: { label: isEditMode ? "Yeni Şifre" : "Geçici Şifre", rules: { required: !isEditMode } },
  }), [isEditMode]);

  const { register, handleSubmit, reset } = useValidatedForm<UserFormData>(userData, fieldConfig);

  useEffect(() => {
    if (isOpen) reset();
  }, [isOpen, reset]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setUserData({ ...userData, [e.target.name]: e.target.value });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-4 lg:pl-76 lg:pr-6 overflow-y-auto">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm transition-opacity" onClick={() => !submitting && onClose()} />

      <div className="bg-slate-900 w-full max-w-lg rounded-2xl sm:rounded-3xl shadow-2xl relative overflow-hidden flex flex-col max-h-[80vh] sm:max-h-[84vh] animate-in zoom-in-95 fade-in duration-200 my-auto z-10 border border-slate-800">
        {/* Header */}
        <div className={cn(
          "p-3.5 sm:p-5 flex justify-between items-center relative overflow-hidden bg-gradient-to-br from-indigo-600 to-violet-700 text-white shrink-0"
        )}>
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-2xl bg-white/20 flex items-center justify-center text-white backdrop-blur-md">
              <User size={20} />
            </div>
            <div>
              <h3 className="text-base sm:text-lg font-black tracking-tight">
                {isEditMode ? "Kullanıcıyı Düzenle" : "Yeni Kullanıcı Ekle"}
              </h3>
              <p className="text-xs text-white/80 font-medium">
                {isEditMode ? `${userData.name} ${userData.lastName} bilgilerini güncelleyin` : "Sisteme yeni personel kaydı ekleyin"}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            disabled={submitting}
            className="w-8 h-8 rounded-xl bg-white/10 hover:bg-white/20 flex items-center justify-center text-white transition-all"
          >
            <X size={16} />
          </button>
        </div>

        {/* Content / Form */}
        {successMessage ? (
          <div className="px-6 py-12 flex flex-col items-center text-center gap-3">
            <div className="w-14 h-14 bg-emerald-500/15 text-emerald-400 rounded-2xl flex items-center justify-center border border-emerald-500/30">
              <CheckCircle2 size={30} />
            </div>
            <p className="text-emerald-400 font-black text-base">{successMessage}</p>
            <p className="text-slate-400 text-xs font-medium">Kullanıcı listesi güncelleniyor...</p>
          </div>
        ) : (
          <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex-1 overflow-y-auto p-3.5 sm:p-6 space-y-4 sm:space-y-5 custom-scrollbar">
            <div className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                <FormField
                  name="name"
                  label="Ad"
                  value={userData.name}
                  onChange={handleChange}
                  onBlur={register("name").onBlur}
                  error={register("name").error}
                  required
                  icon={<User size={12} />}
                  placeholder="Ad"
                />
                <FormField
                  name="lastName"
                  label="Soyad"
                  value={userData.lastName}
                  onChange={handleChange}
                  onBlur={register("lastName").onBlur}
                  error={register("lastName").error}
                  required
                  icon={<User size={12} />}
                  placeholder="Soyad"
                />

                <div className="sm:col-span-2">
                  <FormField
                    name="username"
                    label="Kullanıcı Adı (opsiyonel)"
                    value={userData.username ?? ""}
                    onChange={handleChange}
                    icon={<AtSign size={12} />}
                    placeholder="kullanici.adi"
                  />
                </div>

                <div className="sm:col-span-2">
                  <FormField
                    name="email"
                    label="E-posta"
                    type="email"
                    value={userData.email}
                    onChange={handleChange}
                    onBlur={register("email").onBlur}
                    error={register("email").error}
                    required
                    icon={<Mail size={12} />}
                    placeholder="eposta@sirket.com"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                {!isEditMode && (
                  <FormField
                    name="password"
                    label="Geçici Şifre"
                    type="password"
                    value={userData.password}
                    onChange={handleChange}
                    onBlur={register("password").onBlur}
                    error={register("password").error}
                    required
                    icon={<Key size={12} />}
                    placeholder="••••••••"
                  />
                )}

                <div className="space-y-1.5">
                  <label className="flex items-center gap-1 text-[11px] font-bold text-slate-300">
                    <ToggleLeft size={12} className="text-slate-500" /> Hesap Durumu
                  </label>
                  <button
                    type="button"
                    onClick={() => setUserData({ ...userData, isActive: !userData.isActive })}
                    className={cn(
                      "w-full flex items-center justify-between px-3.5 py-2.5 rounded-2xl border text-xs font-bold transition-all",
                      userData.isActive
                        ? "bg-emerald-500/15 border-emerald-500/30 text-emerald-400"
                        : "bg-slate-800/70 border-slate-700 text-slate-400"
                    )}
                  >
                    <span>{userData.isActive ? "Aktif" : "Pasif"}</span>
                    <div className={cn(
                      "w-9 h-5 rounded-full transition-all relative shrink-0",
                      userData.isActive ? "bg-emerald-500" : "bg-slate-600"
                    )}>
                      <div className={cn(
                        "absolute top-0.5 w-4 h-4 bg-white rounded-full transition-all",
                        userData.isActive ? "left-4" : "left-0.5"
                      )} />
                    </div>
                  </button>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3 pt-3 border-t border-slate-800">
              <button
                type="button"
                onClick={onClose}
                disabled={submitting}
                className="flex-1 px-4 py-3 rounded-2xl border border-slate-700 text-slate-300 text-xs font-bold hover:bg-slate-800 transition-all active:scale-95 disabled:opacity-50"
              >
                İptal
              </button>
              <button
                type="submit"
                disabled={submitting}
                className={cn(
                  "flex-1 flex items-center justify-center gap-2 py-3 rounded-2xl text-xs font-bold text-white transition-all active:scale-95 disabled:opacity-50 shadow-md bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 shadow-indigo-600/20"
                )}
              >
                {submitting ? (
                  <>
                    <Loader2 className="animate-spin" size={15} />
                    Kaydediliyor...
                  </>
                ) : (
                  isEditMode ? "Güncelle" : "Oluştur"
                )}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
