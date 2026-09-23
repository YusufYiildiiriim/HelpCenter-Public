"use client";

import React, { useEffect, useState } from "react";
import { AtSign, Lock, Loader2, ArrowRight, Eye, EyeOff } from "lucide-react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { toast } from "sonner";
import { AuthService } from "@/services/common/AuthService";
import { CustomerForgotPasswordModal } from "./components/CustomerForgotPasswordModal";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";
import { PublicOrganizationService } from "@/services/public/PublicOrganizationService";
import { usePageTitle } from "@/lib/usePageTitle";
import { getApiErrorMessage } from "@/lib/api/errors";
import { toApiUrl } from "@/lib/env";

export default function DashboardLoginPage() {
  usePageTitle("Müşteri Girişi");

  const [formData, setFormData] = useState({ username: "", password: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [isForgotPasswordOpen, setIsForgotPasswordOpen] = useState(false);
  const [orgName, setOrgName] = useState("HelpCenter");
  const [logoUrl, setLogoUrl] = useState("/logo.png");
  const router = useRouter();

  useEffect(() => {
    PublicOrganizationService.getOrganizationInfo()
      .then((data) => {
        setOrgName(data.organizationName);
        if (data.logoUrl) {
          const resolved = data.logoUrl.startsWith("data:") || data.logoUrl.startsWith("http") || data.logoUrl.startsWith("/logo")
            ? data.logoUrl
            : toApiUrl(data.logoUrl);
          setLogoUrl(resolved);
        }
      })
      .catch(() => {});
  }, []);

  const { register, handleSubmit } = useValidatedForm<typeof formData>(formData, {
    username: { label: "Kullanıcı Adı veya E-posta", rules: { required: true } },
    password: { label: "Şifre", rules: { required: true } },
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { companyPublicId } = await AuthService.customerLogin({ email: formData.username, password: formData.password });
      if (!companyPublicId) throw new Error("Login response did not contain a company identifier.");

      router.push(`/dashboard/${companyPublicId}`);
    } catch (err: unknown) {
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPasswordSuccess = (resetUsername: string) => {
    setFormData({ username: resetUsername, password: "" });
  };

  return (
    <div className="min-h-screen w-full bg-slate-950 flex justify-center p-4 sm:p-6 relative overflow-hidden">
      {/* Premium Background */}
      <div className="absolute inset-0 pointer-events-none">
        <motion.div 
            animate={{ 
                scale: [1, 1.2, 1],
                opacity: [0.15, 0.25, 0.15],
            }}
            transition={{ duration: 8, repeat: Infinity }}
            className="absolute -top-40 -left-40 w-[600px] h-[600px] bg-indigo-600/20 rounded-full blur-[100px]" 
        />
        <motion.div 
            animate={{ 
                scale: [1.2, 1, 1.2],
                opacity: [0.1, 0.2, 0.1],
            }}
            transition={{ duration: 12, repeat: Infinity }}
            className="absolute -bottom-40 -right-40 w-[600px] h-[600px] bg-violet-600/20 rounded-full blur-[100px]" 
        />
      </div>

      <motion.div 
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="w-full max-w-md relative z-10 my-auto"
      >
        <div className="bg-slate-900/90 backdrop-blur-xl rounded-3xl sm:rounded-[3rem] shadow-2xl shadow-slate-950/40 border border-slate-800 overflow-hidden">
          <div className="p-8 sm:p-12 text-center bg-slate-900/50 border-b border-slate-800">
            <motion.div 
                whileHover={{ rotate: 15 }}
                className="w-16 h-16 sm:w-24 sm:h-24 rounded-[1.6rem] sm:rounded-[2.2rem] flex items-center justify-center mx-auto shadow-2xl shadow-indigo-600/20 mb-6 sm:mb-8 overflow-hidden bg-slate-800 border border-slate-700 p-2"
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={logoUrl} alt={`${orgName} Logo`} className="w-full h-full object-contain" />
            </motion.div>
            <h1 className="text-2xl sm:text-4xl font-black text-white tracking-tighter">{orgName}</h1>
            <p className="text-slate-400 mt-2 sm:mt-3 font-medium italic text-sm sm:text-base">Müşteri Portalı - Güvenli Giriş</p>
          </div>

          <form onSubmit={handleSubmit(handleLogin)} noValidate className="p-6 sm:p-12 space-y-6 sm:space-y-8">
            <div className="space-y-5 sm:space-y-6">
              <FormField
                name="username"
                label="Kullanıcı Adı veya E-posta"
                value={formData.username}
                onChange={handleChange}
                onBlur={register("username").onBlur}
                error={register("username").error}
                required
                icon={<AtSign size={20} />}
                placeholder="Kullanıcı adı veya e-posta adresiniz"
              />

              <div>
                <div className="flex justify-between items-center px-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Şifre</label>
                  <button 
                    type="button" 
                    onClick={() => setIsForgotPasswordOpen(true)}
                    className="text-[9px] font-black text-indigo-400 uppercase tracking-widest hover:underline cursor-pointer"
                  >
                    Şifremi Unuttum
                  </button>
                </div>
                <div className="mt-2 sm:mt-3">
                  <FormField
                    name="password"
                    label="Şifre"
                    hideLabel
                    type={showPassword ? "text" : "password"}
                    value={formData.password}
                    onChange={handleChange}
                    onBlur={register("password").onBlur}
                    error={register("password").error}
                    required
                    icon={<Lock size={20} />}
                    placeholder="••••••••"
                    right={
                      <button
                        type="button"
                        onClick={() => setShowPassword((prev) => !prev)}
                        tabIndex={-1}
                        className="p-2 text-slate-500 hover:text-indigo-400 transition-colors"
                        aria-label={showPassword ? "Şifreyi gizle" : "Şifreyi göster"}
                      >
                        {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                      </button>
                    }
                  />
                </div>
              </div>
            </div>

            <motion.button
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              disabled={loading}
              className="w-full bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 sm:py-6 rounded-2xl sm:rounded-[1.8rem] shadow-2xl shadow-indigo-600/20 transition-all disabled:opacity-50 flex items-center justify-center gap-4 mt-2 sm:mt-4 group"
            >
              {loading ? (
                <Loader2 className="animate-spin" size={22} />
              ) : (
                <>
                  DASHBOARD&apos;A GİRİŞ
                  <ArrowRight size={20} className="group-hover:translate-x-2 transition-transform" />
                </>
              )}
            </motion.button>
          </form>

          <div className="p-6 sm:p-10 bg-slate-900/50 border-t border-slate-800 text-center">
            <p className="text-slate-500 text-[10px] font-black uppercase tracking-widest">
                {orgName} &copy; {new Date().getFullYear()}
            </p>
          </div>
        </div>
      </motion.div>

      <CustomerForgotPasswordModal
        isOpen={isForgotPasswordOpen}
        onClose={() => setIsForgotPasswordOpen(false)}
        onSuccess={handleForgotPasswordSuccess}
      />
    </div>
  );
}
