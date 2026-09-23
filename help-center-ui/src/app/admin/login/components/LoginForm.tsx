import React, { useState } from "react";
import { Mail, Lock, Loader2, ArrowRight, Eye, EyeOff } from "lucide-react";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

interface LoginFormProps {
  email: string;
  setEmail: (email: string) => void;
  password: string;
  setPassword: (password: string) => void;
  loading: boolean;
  onSubmit: (e: React.FormEvent) => void;
  onForgotPassword?: () => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({
  email,
  setEmail,
  password,
  setPassword,
  loading,
  onSubmit,
  onForgotPassword
}) => {
  const [showPassword, setShowPassword] = useState(false);

  const values = { email, password };
  const { register, handleSubmit } = useValidatedForm<{ email: string; password: string }>(values, {
    email: { label: "E-Posta / Kullanıcı Adı", rules: { required: true } },
    password: { label: "Şifre", rules: { required: true } },
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    if (e.target.name === "email") setEmail(e.target.value);
    else if (e.target.name === "password") setPassword(e.target.value);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
      <FormField
        name="email"
        label="E-Posta / Kullanıcı Adı"
        value={email}
        onChange={handleChange}
        onBlur={register("email").onBlur}
        error={register("email").error}
        required
        icon={<Mail size={18} />}
        placeholder="admin@helpcenter.com veya kullanıcı adı"
      />

      <FormField
        name="password"
        label="Şifre"
        type={showPassword ? "text" : "password"}
        value={password}
        onChange={handleChange}
        onBlur={register("password").onBlur}
        error={register("password").error}
        required
        icon={<Lock size={18} />}
        placeholder="••••••••"
        right={
          <button
            type="button"
            onClick={() => setShowPassword((prev) => !prev)}
            tabIndex={-1}
            className="p-2 text-slate-500 hover:text-indigo-400 transition-colors"
            aria-label={showPassword ? "Şifreyi gizle" : "Şifreyi göster"}
          >
            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
          </button>
        }
      />

      <div className="flex items-center justify-between py-2 text-sm">
        <label className="flex cursor-pointer items-center gap-2 select-none">
          <input type="checkbox" className="h-4 w-4 rounded border-slate-700 bg-slate-800 text-indigo-500 focus:ring-indigo-500" />
          <span className="font-medium text-slate-400">Beni hatırla</span>
        </label>
        <button
          type="button"
          onClick={onForgotPassword}
          className="font-bold text-indigo-400 hover:text-indigo-300 hover:underline cursor-pointer"
        >
          Şifremi unuttum
        </button>
      </div>

      <button
        type="submit"
        disabled={loading}
        className="group/btn flex w-full items-center justify-center gap-2 rounded-2xl bg-linear-to-r from-indigo-600 to-violet-600 py-4 font-black uppercase tracking-widest text-[10px] text-white shadow-lg shadow-indigo-600/20 transition-all hover:from-indigo-500 hover:to-violet-500 active:scale-[0.98] disabled:opacity-50"
      >
        {loading ? (
          <Loader2 className="animate-spin" size={20} />
        ) : (
          <>
            Sisteme Giriş Yap
            <ArrowRight size={18} className="transition-transform group-hover/btn:translate-x-1" />
          </>
        )}
      </button>
    </form>
  );
};

