import React, { useMemo } from "react";
import { User, Loader2, Save } from "lucide-react";
import type { CustomerProfile } from "@/services/customer/CustomerProfileService";
import { FormField } from "@/components/common/FormField";
import { useValidatedForm } from "@/hooks/useValidatedForm";

interface ProfileViewProps {
  profile: CustomerProfile | null;
  setProfile: (profile: CustomerProfile) => void;
  onSubmit: (e: React.FormEvent) => void;
  loading: boolean;
}

export const ProfileView: React.FC<ProfileViewProps> = ({
  profile,
  setProfile,
  onSubmit,
  loading
}) => {
  const values = useMemo(
    () => ({ firstName: profile?.firstName || "", lastName: profile?.lastName || "" }),
    [profile]
  );

  const { register, handleSubmit } = useValidatedForm<typeof values>(values, {
    firstName: { label: "Ad", rules: { required: true } },
    lastName: { label: "Soyad", rules: { required: true } },
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    if (!profile) return;
    setProfile({ ...profile, [e.target.name]: e.target.value });
  };

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
        <div className="bg-slate-900/90 backdrop-blur-xl p-10 rounded-[3rem] border border-slate-800 shadow-xl shadow-slate-950/30">
            <div className="flex items-center gap-6 mb-10">
                <div className="w-20 h-20 bg-slate-800 rounded-[2rem] flex items-center justify-center text-slate-400 border border-slate-700">
                    <User size={40} />
                </div>
                <div>
                    <h2 className="text-3xl font-black text-white tracking-tight">Profil Ayarları</h2>
                    <p className="text-slate-400 font-medium">Hesap bilgilerinizi buradan görüntüleyebilir ve güncelleyebilirsiniz.</p>
                </div>
            </div>

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-8">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    <FormField
                        name="firstName"
                        label="Ad"
                        value={profile?.firstName || ""}
                        onChange={handleChange}
                        onBlur={register("firstName").onBlur}
                        error={register("firstName").error}
                        required
                    />
                    <FormField
                        name="lastName"
                        label="Soyad"
                        value={profile?.lastName || ""}
                        onChange={handleChange}
                        onBlur={register("lastName").onBlur}
                        error={register("lastName").error}
                        required
                    />
                    <div className="space-y-2">
                        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">E-Posta (Değiştirilemez)</label>
                        <input 
                            disabled
                            value={profile?.email || ""}
                            className="w-full px-6 py-4 bg-slate-800/50 border border-slate-700 rounded-2xl text-slate-500 cursor-not-allowed font-medium"
                        />
                    </div>
                </div>

                <div className="pt-4">
                    <button 
                        disabled={loading}
                        className="bg-indigo-600 hover:bg-indigo-500 text-white px-10 py-5 rounded-2xl font-black shadow-xl shadow-indigo-600/20 flex items-center gap-3 transition-all active:scale-95 disabled:opacity-50"
                    >
                        {loading ? <Loader2 className="animate-spin" size={24} /> : (
                            <><Save size={20} /> Değişiklikleri Kaydet</>
                        )}
                    </button>
                </div>
            </form>
        </div>
    </div>
  );
};
