"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { AuthService } from "@/services/common/AuthService";
import { PublicOrganizationService } from "@/services/public/PublicOrganizationService";
import { LoginForm } from "./components/LoginForm";
import { AdminForgotPasswordModal } from "./components/AdminForgotPasswordModal";
import { usePageTitle } from "@/lib/usePageTitle";
import { getApiErrorMessage } from "@/lib/api/errors";
import { toApiUrl } from "@/lib/env";

export default function AdminLoginPage() {
  usePageTitle("Admin Girişi");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [isForgotPasswordOpen, setIsForgotPasswordOpen] = useState(false);
  const [orgName, setOrgName] = useState("HelpCenter");
  const [logoUrl, setLogoUrl] = useState("/logo.png");
  const router = useRouter();

  React.useEffect(() => {
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

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      await AuthService.adminLogin({
        email: email,
        password,
      });

      router.push("/admin");
    } catch (err: unknown) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPasswordSuccess = (resetEmail: string) => {
    setEmail(resetEmail);
    setPassword("");
    setError("");
  };

  return (
    <div className="relative min-h-screen bg-slate-950 flex items-center justify-center p-6 overflow-hidden">
      {/* Background Decorative Accents */}
      <div className="pointer-events-none absolute top-0 right-0 w-125 h-125 bg-indigo-600/10 rounded-full blur-[120px] -translate-y-1/3 translate-x-1/3" />
      <div className="pointer-events-none absolute bottom-0 left-0 w-125 h-125 bg-violet-600/10 rounded-full blur-[120px] translate-y-1/3 -translate-x-1/3" />
      <div className="pointer-events-none absolute inset-x-0 top-1/2 left-0 h-72 bg-slate-900/40 blur-[100px]" />

      <div className="relative w-full max-w-md">
        <div className="relative overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/80 shadow-2xl shadow-indigo-600/10 backdrop-blur-xl">
          <div className="pointer-events-none absolute -top-24 -right-24 h-48 w-48 rounded-full bg-indigo-600/5 blur-3xl" />
          <div className="pointer-events-none absolute -bottom-24 -left-24 h-48 w-48 rounded-full bg-violet-600/5 blur-3xl" />

          <div className="relative p-8 md:p-10">
            {/* Logo & Branding */}
            <div className="mb-8 flex flex-col items-center gap-4">
              <div className="flex h-16 w-16 items-center justify-center rounded-2xl border border-slate-800 bg-slate-950 p-2 shadow-lg shadow-indigo-600/20">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={logoUrl} alt={`${orgName} Logo`} className="h-full w-full object-contain" />
              </div>
              <div className="text-center">
                <h1 className="text-2xl font-black tracking-tight text-white">{orgName}</h1>
                <span className="mt-1.5 inline-flex items-center gap-1.5 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-2.5 py-1 text-[10px] font-black uppercase tracking-widest text-indigo-300">
                  Admin Paneli v2.0
                </span>
              </div>
            </div>

            <div className="mb-8 text-center">
              <p className="text-sm font-medium text-slate-400">Lütfen yetkili bilgileri ile oturum açın.</p>
            </div>

            {error && (
              <div className="mb-6 flex items-center gap-2 rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm font-semibold text-red-400 animate-shake">
                <div className="h-1.5 w-1.5 shrink-0 rounded-full bg-red-500 animate-pulse" />
                {error}
              </div>
            )}

            <LoginForm
              email={email}
              setEmail={setEmail}
              password={password}
              setPassword={setPassword}
              loading={loading}
              onSubmit={handleLogin}
              onForgotPassword={() => setIsForgotPasswordOpen(true)}
            />
          </div>
        </div>

        <p className="mt-8 text-center text-sm font-medium text-slate-500">
          {orgName} Admin Paneli v2.0.4 &copy; {new Date().getFullYear()}
        </p>
      </div>

      <AdminForgotPasswordModal
        isOpen={isForgotPasswordOpen}
        onClose={() => setIsForgotPasswordOpen(false)}
        onSuccess={handleForgotPasswordSuccess}
      />
    </div>
  );
}
