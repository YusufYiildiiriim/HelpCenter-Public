"use client";

import React, { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Key, Lock, Eye, EyeOff, Loader2, ShieldCheck, LogOut } from "lucide-react";
import { AuthService, type ChangePasswordRequest } from "@/services/common/AuthService";
import { toast } from "sonner";
import { motion } from "framer-motion";
import { FormField } from "@/components/common/FormField";
import { getApiErrorMessage } from "@/lib/api/errors";

interface PasswordChangeModalProps {
  email: string;
  isCustomer: boolean;
  onSuccess: () => void;
}

const passwordChangeSchema = z
  .object({
    newPassword: z.string().min(6, "Şifre en az 6 karakter olmalıdır."),
    confirmPassword: z.string().min(6, "Şifre tekrarı en az 6 karakter olmalıdır."),
  })
  .refine(({ newPassword, confirmPassword }) => newPassword === confirmPassword, {
    message: "Şifreler uyuşmuyor.",
    path: ["confirmPassword"],
  });

type PasswordChangeFormValues = z.infer<typeof passwordChangeSchema>;

export default function PasswordChangeModal({ email, isCustomer, onSuccess }: PasswordChangeModalProps) {
  const form = useForm<PasswordChangeFormValues>({
    resolver: zodResolver(passwordChangeSchema),
    defaultValues: {
      newPassword: "",
      confirmPassword: "",
    },
  });
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const handlePasswordChange = async ({ newPassword, confirmPassword }: PasswordChangeFormValues) => {
    try {
      const identifier = email || (typeof window !== "undefined" ? (isCustomer ? localStorage.getItem("customer_email") : localStorage.getItem("admin_email")) : "") || "";
      const request: ChangePasswordRequest = {
        email: identifier,
        newPassword,
        confirmPassword,
        isCustomer,
      };
      await AuthService.changePassword(request);
      toast.success("Şifreniz başarıyla güncellendi.");
      onSuccess();
    } catch (err: unknown) {
      toast.error(getApiErrorMessage(err));
    }
  };

  const handleLogout = async () => {
    await AuthService.logout();
    window.location.href = isCustomer ? "/dashboard/login" : "/admin/login";
  };

  return (
    <div className="fixed inset-0 z-[9999] flex items-center justify-center p-4 sm:p-6 bg-slate-950/85 backdrop-blur-2xl overflow-y-auto">
      <motion.div
        initial={{ opacity: 0, scale: 0.95, y: 15 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        transition={{ duration: 0.25, ease: "easeOut" }}
        className="w-full max-w-md my-auto relative z-10"
      >
        <div className="bg-slate-900/95 backdrop-blur-2xl rounded-3xl sm:rounded-[2.5rem] shadow-2xl shadow-slate-950/80 border border-slate-800/80 overflow-hidden">
          <div className="p-6 sm:p-10 text-center space-y-4">
            <motion.div
              whileHover={{ rotate: 10, scale: 1.05 }}
              className="w-16 h-16 sm:w-20 sm:h-20 bg-indigo-600/20 border border-indigo-500/30 text-indigo-400 rounded-2xl sm:rounded-[1.8rem] flex items-center justify-center mx-auto shadow-xl shadow-indigo-600/10 mb-2"
            >
              <ShieldCheck size={36} className="sm:w-10 sm:h-10" />
            </motion.div>
            <div>
              <h2 className="text-xl sm:text-2xl font-black text-white tracking-tight">Güvenlik Doğrulaması</h2>
              <p className="text-slate-400 mt-1.5 text-xs sm:text-sm font-medium">Devam etmeden önce lütfen hesabınız için yeni bir şifre belirleyin.</p>
            </div>

            <form onSubmit={form.handleSubmit(handlePasswordChange)} noValidate className="space-y-4 text-left pt-2 sm:pt-4">
              <Controller
                control={form.control}
                name="newPassword"
                render={({ field, fieldState }) => (
                  <FormField
                    name={field.name}
                    label="Yeni Şifre"
                    type={showNewPassword ? "text" : "password"}
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    error={fieldState.error?.message}
                    required
                    icon={<Lock size={18} />}
                    placeholder="••••••••"
                    right={
                      <button
                        type="button"
                        onClick={() => setShowNewPassword((prev) => !prev)}
                        tabIndex={-1}
                        className="p-2 text-slate-500 hover:text-indigo-400 transition-colors cursor-pointer"
                        aria-label={showNewPassword ? "Şifreyi gizle" : "Şifreyi göster"}
                      >
                        {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                      </button>
                    }
                  />
                )}
              />

              <Controller
                control={form.control}
                name="confirmPassword"
                render={({ field, fieldState }) => (
                  <FormField
                    name={field.name}
                    label="Şifre Tekrar"
                    type={showConfirmPassword ? "text" : "password"}
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    error={fieldState.error?.message}
                    required
                    icon={<Lock size={18} />}
                    placeholder="••••••••"
                    right={
                      <button
                        type="button"
                        onClick={() => setShowConfirmPassword((prev) => !prev)}
                        tabIndex={-1}
                        className="p-2 text-slate-500 hover:text-indigo-400 transition-colors cursor-pointer"
                        aria-label={showConfirmPassword ? "Şifreyi gizle" : "Şifreyi göster"}
                      >
                        {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                      </button>
                    }
                  />
                )}
              />

              <div className="flex flex-col-reverse sm:flex-row gap-3 sm:gap-4 pt-3 sm:pt-4">
                <motion.button
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  type="button"
                  onClick={handleLogout}
                  className="w-full sm:w-auto px-5 py-3.5 sm:py-4 rounded-xl sm:rounded-2xl font-bold text-xs text-slate-400 hover:bg-slate-800/80 hover:text-slate-200 transition-all flex items-center justify-center gap-2 border border-slate-800 cursor-pointer"
                >
                  <LogOut size={16} /> Oturumu Kapat
                </motion.button>
                <motion.button
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  type="submit"
                  disabled={form.formState.isSubmitting}
                  className="flex-1 w-full sm:w-auto bg-indigo-600 hover:bg-indigo-500 text-white font-black py-3.5 sm:py-4 rounded-xl sm:rounded-2xl shadow-xl shadow-indigo-600/25 transition-all active:scale-[0.98] disabled:opacity-50 flex items-center justify-center gap-2.5 text-xs sm:text-sm cursor-pointer"
                >
                  {form.formState.isSubmitting ? <Loader2 className="animate-spin" size={18} /> : <><Key size={18} /> Şifreyi Güncelle</>}
                </motion.button>
              </div>
            </form>
          </div>
        </div>
      </motion.div>
    </div>
  );
}
