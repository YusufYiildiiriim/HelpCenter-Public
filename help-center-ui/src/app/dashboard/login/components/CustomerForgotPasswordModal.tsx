"use client";

import React, { useState } from "react";
import { AtSign, Lock, Loader2, KeyRound, Eye, EyeOff, X, ArrowLeft, Send, ArrowRight } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { toast } from "sonner";
import { AuthService } from "@/services/common/AuthService";
import { getApiErrorMessage } from "@/lib/api/errors";

interface CustomerForgotPasswordModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (username: string) => void;
}

export const CustomerForgotPasswordModal: React.FC<CustomerForgotPasswordModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [step, setStep] = useState<1 | 2 | 3>(1);
  const [username, setUsername] = useState("");
  const [code, setCode] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [resetComplete, setResetComplete] = useState(false);

  // Step 1: Send OTP Code
  const handleSendCode = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!username.trim()) {
      toast.error("Lütfen kullanıcı adınızı veya e-posta adresinizi girin.");
      return;
    }

    setLoading(true);

    try {
      await AuthService.forgotPassword(username.trim(), true);
      setStep(2);
    } catch (err: unknown) {
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  // Step 2: Verify Code
  const handleVerifyCode = async (e: React.FormEvent) => {
    e.preventDefault();

    if (code.trim().length !== 6) {
      toast.error("Doğrulama kodu 6 haneli olmalıdır.");
      return;
    }

    setLoading(true);

    try {
      await AuthService.verifyResetCode(username.trim(), code.trim(), true);
      setStep(3);
    } catch (err: unknown) {
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  // Step 3: Reset Password
  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();

    if (newPassword.length < 6) {
      toast.error("Şifre en az 6 karakter olmalıdır.");
      return;
    }

    if (newPassword !== confirmPassword) {
      toast.error("Şifreler uyuşmuyor. Lütfen kontrol edin.");
      return;
    }

    setLoading(true);

    try {
      const res = await AuthService.resetPasswordWithCode({
        emailOrUsername: username.trim(),
        code: code.trim(),
        newPassword,
        confirmPassword,
        isCustomer: true,
      });

      toast.success(res.message || "Şifreniz başarıyla yenilendi! Giriş ekranına aktarılıyorsunuz...");
      setResetComplete(true);

      setTimeout(() => {
        onSuccess(username.trim());
        resetForm();
        onClose();
      }, 1500);
    } catch (err: unknown) {
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setStep(1);
    setUsername("");
    setCode("");
    setNewPassword("");
    setConfirmPassword("");
    setResetComplete(false);
    setShowPassword(false);
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  return (
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-xl">
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 10 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 10 }}
            className="relative w-full max-w-md overflow-hidden rounded-[2.5rem] bg-slate-900 border border-slate-800 shadow-2xl shadow-slate-950/50"
          >
            {/* Header */}
            <div className="p-8 pb-4 border-b border-slate-800/80 flex items-center justify-between">
              <div className="flex items-center gap-4">
                {step > 1 ? (
                  <button
                    type="button"
                    onClick={() => {
                      setStep((prev) => (prev - 1) as 1 | 2);
                    }}
                    className="w-10 h-10 rounded-2xl bg-slate-800/80 flex items-center justify-center text-slate-300 hover:text-white transition-colors cursor-pointer"
                    title="Geri dön"
                  >
                    <ArrowLeft size={20} />
                  </button>
                ) : (
                  <div className="w-12 h-12 bg-indigo-600/20 border border-indigo-500/30 rounded-2xl flex items-center justify-center text-indigo-400">
                    <KeyRound size={24} />
                  </div>
                )}
                <div>
                  <h2 className="text-xl font-black text-white tracking-tight">Şifremi Unuttum</h2>
                  <p className="text-xs text-slate-400 font-medium">
                    {step === 1 && "1. Aşama: Kod Talebi"}
                    {step === 2 && "2. Aşama: Onay Kodu Girişi"}
                    {step === 3 && "3. Aşama: Yeni Şifre"}
                  </p>
                </div>
              </div>
              <button
                type="button"
                onClick={handleClose}
                className="w-10 h-10 rounded-2xl bg-slate-800/60 flex items-center justify-center text-slate-400 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer"
              >
                <X size={20} />
              </button>
            </div>

            {/* Form */}
            <div className="p-8 space-y-6">
              {/* STEP 1: Request Code */}
              {step === 1 && (
                <form onSubmit={handleSendCode} className="space-y-6">
                  <p className="text-xs text-slate-400 leading-relaxed font-medium">
                    Portal hesabınıza kayıtlı kullanıcı adı veya e-posta adresinizi girin. E-posta adresinize 6 haneli doğrulama kodu gönderilecektir.
                  </p>

                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] ml-2">
                      Kullanıcı Adı veya E-Posta
                    </label>
                    <div className="relative group">
                      <AtSign
                        className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-indigo-400 transition-colors"
                        size={20}
                      />
                      <input
                        type="text"
                        required
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        className="w-full pl-14 pr-6 py-4 bg-slate-800 border border-slate-700 rounded-[1.4rem] text-white focus:ring-8 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all placeholder:text-slate-500 font-bold text-sm"
                        placeholder="Kullanıcı adı veya e-posta"
                      />
                    </div>
                  </div>

                  <div className="flex items-center gap-4 pt-2">
                    <button
                      type="button"
                      onClick={handleClose}
                      className="w-1/2 py-4 px-6 rounded-[1.4rem] border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                    >
                      İptal
                    </button>
                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      disabled={loading}
                      type="submit"
                      className="w-1/2 bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-[1.4rem] shadow-xl shadow-indigo-600/20 transition-all disabled:opacity-50 flex items-center justify-center gap-2 text-[10px] uppercase tracking-widest cursor-pointer"
                    >
                      {loading ? <Loader2 className="animate-spin" size={20} /> : (
                        <>
                          KODU GÖNDER
                          <Send size={14} />
                        </>
                      )}
                    </motion.button>
                  </div>
                </form>
              )}

              {/* STEP 2: Verify Code */}
              {step === 2 && (
                <form onSubmit={handleVerifyCode} className="space-y-6">
                  <div className="p-4 bg-slate-800/50 rounded-2xl border border-slate-800 flex items-center justify-between text-xs">
                    <span className="text-slate-400 font-medium">Hesap: <strong className="text-slate-200">{username}</strong></span>
                    <button
                      type="button"
                      onClick={() => {
                        setStep(1);
                      }}
                      className="text-indigo-400 hover:underline font-bold text-xs cursor-pointer"
                    >
                      Değiştir
                    </button>
                  </div>

                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] ml-2">
                      E-Postanıza Gelen 6 Haneli Kod (OTP)
                    </label>
                    <div className="relative group">
                      <KeyRound
                        className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-indigo-400 transition-colors"
                        size={20}
                      />
                      <input
                        type="text"
                        required
                        maxLength={6}
                        value={code}
                        onChange={(e) => setCode(e.target.value.replace(/\D/g, ""))}
                        className="w-full pl-14 pr-6 py-4 bg-slate-800 border border-slate-700 rounded-[1.4rem] text-white focus:ring-8 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all placeholder:text-slate-500 font-mono tracking-widest text-center text-lg font-bold"
                        placeholder="123456"
                      />
                    </div>
                  </div>

                  <div className="flex items-center gap-4 pt-2">
                    <button
                      type="button"
                      onClick={handleClose}
                      className="w-1/2 py-4 px-6 rounded-[1.4rem] border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                    >
                      İptal
                    </button>
                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      disabled={loading || code.trim().length !== 6}
                      type="submit"
                      className="w-1/2 bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-[1.4rem] shadow-xl shadow-indigo-600/20 transition-all disabled:opacity-50 flex items-center justify-center gap-2 text-[10px] uppercase tracking-widest cursor-pointer"
                    >
                      {loading ? <Loader2 className="animate-spin" size={20} /> : (
                        <>
                          İLERLE
                          <ArrowRight size={14} />
                        </>
                      )}
                    </motion.button>
                  </div>
                </form>
              )}

              {/* STEP 3: Create New Password */}
              {step === 3 && (
                <form onSubmit={handleResetPassword} className="space-y-6">
                  <p className="text-xs text-slate-400 leading-relaxed font-medium">
                    Kod başarıyla doğrulandı. Portal hesabınız için yeni şifrenizi belirleyin.
                  </p>

                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] ml-2">
                      Yeni Şifre
                    </label>
                    <div className="relative group">
                      <Lock
                        className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-indigo-400 transition-colors"
                        size={20}
                      />
                      <input
                        type={showPassword ? "text" : "password"}
                        required
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        className="w-full pl-14 pr-14 py-4 bg-slate-800 border border-slate-700 rounded-[1.4rem] text-white focus:ring-8 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all placeholder:text-slate-500 font-bold text-sm"
                        placeholder="En az 6 karakter"
                      />
                      <button
                        type="button"
                        onClick={() => setShowPassword((prev) => !prev)}
                        tabIndex={-1}
                        className="absolute right-5 top-1/2 -translate-y-1/2 text-slate-500 hover:text-indigo-400 transition-colors cursor-pointer"
                        aria-label={showPassword ? "Şifreyi gizle" : "Şifreyi göster"}
                      >
                        {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                      </button>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] ml-2">
                      Yeni Şifre Tekrar
                    </label>
                    <div className="relative group">
                      <Lock
                        className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-indigo-400 transition-colors"
                        size={20}
                      />
                      <input
                        type={showPassword ? "text" : "password"}
                        required
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        className="w-full pl-14 pr-6 py-4 bg-slate-800 border border-slate-700 rounded-[1.4rem] text-white focus:ring-8 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all placeholder:text-slate-500 font-bold text-sm"
                        placeholder="Yeni şifrenizi tekrar girin"
                      />
                    </div>
                  </div>

                  <div className="flex items-center gap-4 pt-2">
                    <button
                      type="button"
                      onClick={handleClose}
                      className="w-1/2 py-4 px-6 rounded-[1.4rem] border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                    >
                      İptal
                    </button>
                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      disabled={loading || resetComplete}
                      type="submit"
                      className="w-1/2 bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-[1.4rem] shadow-xl shadow-indigo-600/20 transition-all disabled:opacity-50 flex items-center justify-center text-[10px] uppercase tracking-widest cursor-pointer"
                    >
                      {loading ? <Loader2 className="animate-spin" size={20} /> : "ŞİFREYİ SIFIRLA"}
                    </motion.button>
                  </div>
                </form>
              )}
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
};
