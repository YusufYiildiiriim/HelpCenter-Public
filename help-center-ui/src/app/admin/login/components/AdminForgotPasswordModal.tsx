"use client";

import React, { useState } from "react";
import { Mail, Lock, Loader2, KeyRound, Eye, EyeOff, X, CheckCircle2, AlertCircle, ArrowLeft, Send, ArrowRight } from "lucide-react";
import { AuthService } from "@/services/common/AuthService";
import { getApiErrorMessage } from "@/lib/api/errors";
import { useLocale } from "@/context/LocaleContext";

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
  const { t } = useLocale();
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
      setError(t("enterEmailOrUsername"));
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
      setError(t("invalidSixDigitCode"));
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
      setError(t("passwordMinimum6"));
      return;
    }

    if (newPassword !== confirmPassword) {
      setError(t("passwordsMismatch"));
      return;
    }

    setLoading(true);

    try {
      await AuthService.resetPasswordWithCode({
        emailOrUsername: email.trim(),
        code: code.trim(),
        newPassword,
        confirmPassword,
        isCustomer: false,
      });

      setSuccessMessage(t("passwordResetSuccess"));

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
                title={t("back")}
              >
                <ArrowLeft size={18} />
              </button>
            ) : (
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-indigo-500/10 border border-indigo-500/30 text-indigo-400">
                <KeyRound size={20} />
              </div>
            )}
            <div>
              <h2 className="text-base font-black text-white">{t("forgotPasswordTitle")}</h2>
              <p className="text-xs text-slate-400 font-medium">
                {step === 1 && `1. ${t("stepCodeRequest")}`}
                {step === 2 && `2. ${t("stepEnterCode")}`}
                {step === 3 && `3. ${t("stepNewPassword")}`}
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={handleClose}
            aria-label={t("close")}
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
                {t("adminForgotInstructions")}
              </p>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  {t("emailOrUsername")}
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <Mail size={18} />
                  </div>
                  <input
                    type="text"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder={t("adminForgotPlaceholder")}
                    className={inputClass}
                  />
                </div>
              </div>

              <div className="flex items-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={handleClose}
                  className="w-1/2 py-3 px-4 rounded-xl border border-slate-700 bg-slate-800/60 font-black uppercase tracking-widest text-[10px] text-slate-300 hover:bg-slate-800 transition-colors cursor-pointer"
                >
                  {t("cancel")}
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="w-1/2 flex items-center justify-center gap-2 rounded-xl bg-linear-to-r from-indigo-600 to-violet-600 py-3 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 disabled:opacity-50 cursor-pointer"
                >
                  {loading ? <Loader2 className="animate-spin" size={18} /> : (
                    <>
                      {t("sendCode")}
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
                <span className="text-slate-400 font-medium">{t("targetAccount")}: <strong className="text-slate-200">{email}</strong></span>
                <button
                  type="button"
                  onClick={() => {
                    setStep(1);
                    setError(null);
                    setSuccessMessage(null);
                  }}
                  className="text-indigo-400 hover:underline font-bold text-[11px] cursor-pointer"
                >
                  {t("change")}
                </button>
              </div>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  {t("otpCodeLabel")}
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
                  {t("cancel")}
                </button>
                <button
                  type="submit"
                  disabled={loading || code.trim().length !== 6}
                  className="w-1/2 flex items-center justify-center gap-2 rounded-xl bg-linear-to-r from-indigo-600 to-violet-600 py-3 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 disabled:opacity-50 cursor-pointer"
                >
                  {loading ? <Loader2 className="animate-spin" size={18} /> : (
                    <>
                      {t("continueAction")}
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
                {t("adminVerifiedResetInstruction")}
              </p>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  {t("newPassword")}
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type={showPassword ? "text" : "password"}
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder={t("minPassword")}
                    className={inputClass}
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((prev) => !prev)}
                    tabIndex={-1}
                    className="absolute inset-y-0 right-0 flex items-center pr-4 text-slate-500 hover:text-indigo-400 transition-colors cursor-pointer"
                    aria-label={showPassword ? t("hidePassword") : t("showPassword")}
                  >
                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="ml-1 text-[10px] font-black uppercase tracking-widest text-slate-400">
                  {t("confirmNewPassword")}
                </label>
                <div className="relative group/input">
                  <div className="absolute inset-y-0 left-0 flex items-center pl-4 text-slate-500 pointer-events-none group-focus-within/input:text-indigo-400 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type={showPassword ? "text" : "password"}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    placeholder={t("confirmNewPasswordPlaceholder")}
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
                  {t("cancel")}
                </button>
                <button
                  type="submit"
                  disabled={loading || !!successMessage}
                  className="w-1/2 flex items-center justify-center gap-2 rounded-xl bg-linear-to-r from-indigo-600 to-violet-600 py-3 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 disabled:opacity-50 cursor-pointer"
                >
                  {loading ? <Loader2 className="animate-spin" size={18} /> : t("resetPassword")}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};
