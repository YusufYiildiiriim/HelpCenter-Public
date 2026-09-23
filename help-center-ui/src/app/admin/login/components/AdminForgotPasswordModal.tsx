"use client";

import React, { useState } from "react";
import { Mail, Lock, Loader2, KeyRound, Eye, EyeOff, X, CheckCircle2, AlertCircle, ArrowLeft, Send, ArrowRight } from "lucide-react";
import { AuthService } from "@/services/common/AuthService";
import { getApiErrorMessage } from "@/lib/api/errors";

interface AdminForgotPasswordModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (email: string) => void;
}

const inputClass =
  "w-full pl-11 pr-12 py-3.5 bg-slate-800/70 border border-slate-700 rounded-xl text-slate-100 focus:bg-slate-800 focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 outline-none transition-all placeholder:text-slate-500 font-bold text-sm";

export const AdminForgotPasswordModal: React.FC<AdminForgotPasswordModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [step, setStep] = useState<1 | 2 | 3>(1);
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  if (!isOpen) return null;

  // Step 1: Send OTP Code
  const handleSendCode = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccessMessage(null);

    if (!email.trim()) {
      setError("Lütfen e-posta adresinizi veya kullanıcı adınızı girin.");
      return;
    }

    setLoading(true);

    try {
      await AuthService.forgotPassword(email.trim(), false);
      setError(null);
      setSuccessMessage(null);
      setStep(2);
    } catch (err: unknown) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  // Step 2: Verify Code
  const handleVerifyCode = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccessMessage(null);

    if (code.trim().length !== 6) {
      setError("Doğrulama kodu 6 haneli sayısal kod olmalıdır.");
      return;
    }

    setLoading(true);

    try {
      await AuthService.verifyResetCode(email.trim(), code.trim(), false);
      setError(null);
      setSuccessMessage(null);
      setStep(3);
    } catch (err: unknown) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  // Step 3: Set New Password
  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccessMessage(null);

    if (newPassword.length < 6) {
      setError("Yeni şifre en az 6 karakter olmalıdır.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("Şifreler uyuşmuyor. Lütfen tekrar kontrol edin.");
      return;
    }

    setLoading(true);

    try {
      const res = await AuthService.resetPasswordWithCode({
        emailOrUsername: email.trim(),
        code: code.trim(),
        newPassword,
        confirmPassword,
        isCustomer: false,
      });

      setSuccessMessage(res.message || "Şifreniz başarıyla sıfırlandı! Giriş ekranına aktarılıyorsunuz...");

      setTimeout(() => {
        onSuccess(email.trim());
        resetForm();
        onClose();
      }, 1500);
    } catch (err: unknown) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setStep(1);
    setEmail("");
    setCode("");
    setNewPassword("");
    setConfirmPassword("");
    setError(null);
    setSuccessMessage(null);
    setShowPassword(false);
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-md animate-fade-in">
      <div className="relative w-full max-w-md overflow-hidden rounded-2xl border border-slate-800 bg-slate-900 shadow-2xl shadow-indigo-600/20">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-slate-800 px-6 py-4 bg-slate-950/50">
          <div className="flex items-center gap-3">
            {step > 1 ? (
              <button
                type="button"
                onClick={() => {
                  setStep((prev) => (prev - 1) as 1 | 2);
                  setError(null);
                  setSuccessMessage(null);
                }}
                className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-800 text-slate-300 hover:text-white transition-colors cursor-pointer"
                title="Geri dön"
              >
                <ArrowLeft size={18} />
              </button>
            ) : (
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-indigo-500/10 border border-indigo-500/30 text-indigo-400">
                <KeyRound size={20} />
              </div>
            )}
            <div>
              <h2 className="text-base font-black text-white">Şifremi Unuttum</h2>
              <p className="text-xs text-slate-400 font-medium">
                {step === 1 && "Adım 1: Kod Talebi"}
                {step === 2 && "Adım 2: Onay Kodunu Girin"}
                {step === 3 && "Adım 3: Yeni Şifre Oluşturun"}
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={handleClose}
            className="rounded-lg p-1.5 text-slate-400 hover:bg-slate-800 hover:text-white transition-colors cursor-pointer"
          >
            <X size={18} />
          </button>
        </div>

        {/* Form Body */}
        <div className="p-6 space-y-5">
          {error && (
            <div className="flex items-center gap-2.5 rounded-xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs font-semibold text-red-400">
              <AlertCircle size={16} className="shrink-0" />
              <span>{error}</span>
            </div>
          )}

          {successMessage && (
            <div className="flex items-center gap-2.5 rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-3.5 text-xs font-semibold text-emerald-400">
              <CheckCircle2 size={16} className="shrink-0" />
              <span>{successMessage}</span>
            </div>
          )}

          {/* STEP 1: Request Code */}
          {step === 1 && (
            <form onSubmit={handleSendCode} className="space-y-5">
              <p className="text-xs text-slate-400 leading-relaxed font-medium">
                Sistemde kayıtlı e-posta adresinizi veya kullanıcı adınızı girin. Hesabınıza 6 haneli şifre sıfırlama doğrulama kodu gönderilecektir.
              </p>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  E-Posta / Kullanıcı Adı
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <Mail size={18} />
                  </div>
                  <input
                    type="text"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="admin@helpcenter.com veya kullanıcı adı"
                    className={inputClass}
                    required
                  />
                </div>
              </div>

              <div className="flex items-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={handleClose}
                  className="w-1/2 py-3 px-4 rounded-xl border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="w-1/2 flex items-center justify-center gap-2 rounded-xl bg-linear-to-r from-indigo-600 to-violet-600 py-3 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 disabled:opacity-50 cursor-pointer"
                >
                  {loading ? <Loader2 className="animate-spin" size={18} /> : (
                    <>
                      Kodu Gönder
                      <Send size={14} />
                    </>
                  )}
                </button>
              </div>
            </form>
          )}

          {/* STEP 2: Verify Code */}
          {step === 2 && (
            <form onSubmit={handleVerifyCode} className="space-y-5">
              <div className="p-3 bg-slate-800/50 rounded-xl border border-slate-800 flex items-center justify-between text-xs">
                <span className="text-slate-400 font-medium">Hedef Hesap: <strong className="text-slate-200">{email}</strong></span>
                <button
                  type="button"
                  onClick={() => {
                    setStep(1);
                    setError(null);
                    setSuccessMessage(null);
                  }}
                  className="text-indigo-400 hover:underline font-bold text-[11px] cursor-pointer"
                >
                  Değiştir
                </button>
              </div>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  E-Postanıza Gelen 6 Haneli Kod (OTP)
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <KeyRound size={18} />
                  </div>
                  <input
                    type="text"
                    maxLength={6}
                    value={code}
                    onChange={(e) => setCode(e.target.value.replace(/\D/g, ""))}
                    placeholder="123456"
                    className={`${inputClass} tracking-widest text-center text-base font-mono`}
                    required
                  />
                </div>
              </div>

              <div className="flex items-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={handleClose}
                  className="w-1/2 py-3 px-4 rounded-xl border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  disabled={loading || code.trim().length !== 6}
                  className="w-1/2 flex items-center justify-center gap-2 rounded-xl bg-linear-to-r from-indigo-600 to-violet-600 py-3 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 disabled:opacity-50 cursor-pointer"
                >
                  {loading ? <Loader2 className="animate-spin" size={18} /> : (
                    <>
                      İlerle
                      <ArrowRight size={14} />
                    </>
                  )}
                </button>
              </div>
            </form>
          )}

          {/* STEP 3: Create New Password */}
          {step === 3 && (
            <form onSubmit={handleResetPassword} className="space-y-5">
              <p className="text-xs text-slate-400 leading-relaxed font-medium">
                Kod başarıyla doğrulandı. Hesabınız için yeni şifrenizi belirleyin.
              </p>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  Yeni Şifre
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type={showPassword ? "text" : "password"}
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder="En az 6 karakter"
                    className={inputClass}
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((prev) => !prev)}
                    tabIndex={-1}
                    className="absolute inset-y-0 right-0 flex items-center pr-4 text-slate-500 hover:text-indigo-400 transition-colors cursor-pointer"
                    aria-label={showPassword ? "Şifreyi gizle" : "Şifreyi göster"}
                  >
                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  Yeni Şifre Tekrar
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type={showPassword ? "text" : "password"}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    placeholder="Yeni şifrenizi tekrar girin"
                    className={inputClass}
                    required
                  />
                </div>
              </div>

              <div className="flex items-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={handleClose}
                  className="w-1/2 py-3 px-4 rounded-xl border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  disabled={loading || !!successMessage}
                  className="w-1/2 flex items-center justify-center gap-2 rounded-xl bg-linear-to-r from-indigo-600 to-violet-600 py-3 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 disabled:opacity-50 cursor-pointer"
                >
                  {loading ? <Loader2 className="animate-spin" size={18} /> : "Şifreyi Sıfırla"}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};
