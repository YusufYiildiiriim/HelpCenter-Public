"use client";

import React, { useEffect, useState, useCallback, useMemo } from "react";
import { motion } from "framer-motion";
import { toast } from "sonner";
import {
  Building2,
  Phone,
  Mail,
  MapPin,
  Globe,
  FileText,
  Hash,
  Upload,
  Save,
  Loader2,
  ShieldCheck,
} from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import {
  AdminOrganizationService,
  type OrganizationInfoDto,
} from "@/services/admin/AdminOrganizationService";
import { useOrganization } from "@/context/OrganizationContext";
import { FormField } from "@/components/common/FormField";
import { ApiStateView } from "@/components/common/ApiStateView";
import { isForbiddenError } from "@/lib/api/errors";
import { useValidatedForm } from "@/hooks/useValidatedForm";
import { toApiUrl } from "@/lib/env";

const URL_REGEX = /^(https?:\/\/)?([\w-]+\.)+[\w-]{2,}(\/.*)?$/i;
const resolveLogoSrc = (src: string | null): string | null => {
  if (!src) return null;
  if (src.startsWith("data:") || src.startsWith("http")) return src;
  return toApiUrl(src);
};

export default function OrganizationSettingsPage() {
  usePageTitle("Kurum Bilgileri");

  const { userInfo } = useAuth();
  const perm = usePermission(userInfo);
  const { setOrgInfo } = useOrganization();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<OrganizationInfoDto | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [fetchError, setFetchError] = useState<unknown>(null);
  const [isForbidden, setIsForbidden] = useState(false);

  const fieldConfig = useMemo(() => ({
    organizationName: { label: "Kurum Adı", rules: { required: true, maxLength: 200 } },
    email:            { label: "E-Posta",   rules: { type: "email" as const } },
    phone:            { label: "Telefon",   rules: { type: "phone" as const } },
    website:          { label: "Web Sitesi", rules: { pattern: URL_REGEX, patternMessage: "Geçerli bir URL giriniz (örn: https://kurum.com)." } },
    taxNumber:        { label: "Vergi Numarası", rules: { pattern: /^\d{10,11}$/, patternMessage: "Vergi numarası 10 veya 11 haneli olmalıdır." } },
    taxOffice:        { label: "Vergi Dairesi", rules: { maxLength: 100 } },
    footerText:       { label: "Footer Metni", rules: { maxLength: 300 } },
  }), []);

  const { register, handleSubmit, reset } = useValidatedForm(form ?? {} as OrganizationInfoDto, fieldConfig);

  const fetchOrgInfo = useCallback(async () => {
    try {
      setLoading(true);
      const data = await AdminOrganizationService.getOrganizationInfo();
      setForm(data);
      setLogoPreview(data.logoUrl ?? null);
      setFetchError(null);
      setIsForbidden(false);
      reset();
    } catch (err) {
      if (isForbiddenError(err)) setIsForbidden(true);
      else setFetchError(err);
    } finally {
      setLoading(false);
    }
  }, [reset]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional fetch on mount
    fetchOrgInfo();
  }, [fetchOrgInfo]);

  const handleFieldChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    if (!form) return;
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleLogoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      toast.error("Logo dosyası 2MB'den küçük olmalıdır.");
      return;
    }
    setLogoFile(file);
    const reader = new FileReader();
    reader.onload = (ev) => setLogoPreview(ev.target?.result as string);
    reader.readAsDataURL(file);
  };

  const submit = async () => {
    if (!form) return;
    try {
      setSaving(true);
      const formData = new FormData();
      formData.append("Id", String(form.id));
      formData.append("OrganizationName", form.organizationName);
      if (form.phone) formData.append("Phone", form.phone);
      if (form.email) formData.append("Email", form.email);
      if (form.address) formData.append("Address", form.address);
      if (form.website) formData.append("Website", form.website);
      if (form.taxNumber) formData.append("TaxNumber", form.taxNumber);
      if (form.taxOffice) formData.append("TaxOffice", form.taxOffice);
      if (form.footerText) formData.append("FooterText", form.footerText);
      if (form.rowVersion) formData.append("RowVersion", form.rowVersion);
      if (logoFile) formData.append("Logo", logoFile);

      await AdminOrganizationService.updateOrganizationInfo(formData);

      setOrgInfo({
        organizationName: form.organizationName,
        logoUrl: logoPreview ?? form.logoUrl,
        footerText: form.footerText,
      });

      toast.success("Kurum bilgileri başarıyla güncellendi.");
      await fetchOrgInfo();
      setLogoFile(null);
    } catch {
      toast.error("Güncelleme sırasında bir hata oluştu.");
    } finally {
      setSaving(false);
    }
  };

  if (!perm.canRead("OrganizationSettings")) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 p-8 text-center bg-slate-900/90 rounded-3xl border border-slate-800 shadow-xl">
        <div className="w-16 h-16 rounded-2xl bg-rose-500/15 border border-rose-500/30 flex items-center justify-center text-rose-400 shadow-inner">
          <ShieldCheck size={32} />
        </div>
        <h2 className="text-xl font-black tracking-tight text-white">Erişim Yetkisi Sınırlı</h2>
        <p className="text-sm text-slate-400 max-w-md">Bu sayfayı görüntüleme yetkiniz bulunmamaktadır.</p>
      </div>
    );
  }

  const canUpdate = perm.canUpdate("OrganizationSettings");

  return (
    <ApiStateView
      isLoading={loading}
      isForbidden={isForbidden}
      error={fetchError}
      onRetry={fetchOrgInfo}
      variant="page"
    >
      {!form ? (
        <div className="flex items-center justify-center min-h-[60vh] bg-slate-900/60 rounded-3xl border border-slate-800">
          <p className="text-slate-400 font-bold">Kurum bilgisi bulunamadı.</p>
        </div>
      ) : (
      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        className="space-y-8 pb-12"
      >
        {/* Header */}
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between bg-slate-900/90 backdrop-blur-xl border border-slate-800 p-5 md:px-6 md:py-5 rounded-3xl shadow-2xl shadow-slate-950/40">
          <div className="flex items-center gap-4">
            <div className="relative shrink-0">
              <div className="absolute -inset-1 rounded-2xl blur-md opacity-60 bg-gradient-to-br from-indigo-500 to-violet-600" />
              <div className="relative flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-600 text-white shadow-lg">
                <Building2 size={22} />
              </div>
            </div>
            <div>
              <h1 className="text-xl md:text-2xl font-black tracking-tight text-white">Kurum Bilgileri</h1>
              <p className="mt-1 text-xs font-medium text-slate-400">
                Sisteme ait kurum bilgilerini bu sayfadan yapılandırabilirsiniz.
              </p>
            </div>
          </div>
        </div>

        <form onSubmit={handleSubmit(submit)} noValidate className="space-y-6">
        <div className="grid gap-6 lg:grid-cols-3">
          {/* Logo Section */}
          <div className="lg:col-span-1">
            <div className="relative overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
              <div className="pointer-events-none absolute -top-20 -left-20 h-48 w-48 rounded-full bg-indigo-600/10 blur-3xl" />
              <div className="relative z-10">
                <h2 className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-5">
                  Kurum Logosu
                </h2>
                <div className="flex flex-col items-center gap-4">
                  <div className="relative group">
                    <div className="h-36 w-36 rounded-2xl border-2 border-dashed border-slate-700 bg-slate-800/60 flex items-center justify-center overflow-hidden">
                      {(() => {
                        const src = resolveLogoSrc(logoPreview);
                        return src ? (
                          // eslint-disable-next-line @next/next/no-img-element
                          <img src={src} alt="Logo" className="h-full w-full object-contain p-2" />
                        ) : (
                          <Building2 size={44} className="text-slate-600" />
                        );
                      })()}
                    </div>
                    {canUpdate && (
                      <label className="absolute inset-0 flex cursor-pointer items-center justify-center rounded-2xl bg-slate-950/0 transition-all group-hover:bg-slate-950/60">
                        <Upload size={26} className="text-white opacity-0 transition-opacity group-hover:opacity-100" />
                        <input type="file" accept="image/*" onChange={handleLogoChange} className="hidden" />
                      </label>
                    )}
                  </div>
                  <p className="text-[10px] font-bold uppercase tracking-widest text-slate-500 text-center">
                    PNG / JPG / SVG · Maks. 2 MB
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Form Section */}
          <div className="lg:col-span-2 space-y-6">
            {/* Organization Name */}
            <div className="relative overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
              <div className="flex items-center gap-3 mb-5">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-500/15 border border-indigo-500/30 text-indigo-400">
                  <Building2 size={18} />
                </div>
                <div>
                  <h3 className="text-sm font-black text-white">Kurum Adı</h3>
                  <p className="text-[10px] font-bold uppercase tracking-widest text-slate-500">Zorunlu alan</p>
                </div>
              </div>
              <FormField
                name="organizationName"
                label="Kurum Adı"
                hideLabel
                value={form.organizationName}
                onChange={handleFieldChange}
                onBlur={register("organizationName").onBlur}
                error={register("organizationName").error}
                disabled={!canUpdate}
                required
                placeholder="Kurum adını giriniz"
              />
            </div>

            {/* Contact Information */}
            <div className="relative overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
              <h3 className="text-sm font-black text-white mb-5">İletişim Bilgileri</h3>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField
                  name="phone"
                  label="Telefon"
                  value={form.phone ?? ""}
                  onChange={handleFieldChange}
                  onBlur={register("phone").onBlur}
                  error={register("phone").error}
                  disabled={!canUpdate}
                  icon={<Phone size={16} />}
                  placeholder="+90 555 555 55 55"
                />
                <FormField
                  name="email"
                  label="E-Posta"
                  type="email"
                  value={form.email ?? ""}
                  onChange={handleFieldChange}
                  onBlur={register("email").onBlur}
                  error={register("email").error}
                  disabled={!canUpdate}
                  icon={<Mail size={16} />}
                  placeholder="kurum@ornek.com"
                />
                <div className="sm:col-span-2">
                  <FormField
                    as="textarea"
                    rows={2}
                    name="address"
                    label="Adres"
                    value={form.address ?? ""}
                    onChange={handleFieldChange}
                    disabled={!canUpdate}
                    icon={<MapPin size={16} />}
                    placeholder="Kurum adresini giriniz"
                  />
                </div>
                <div className="sm:col-span-2">
                  <FormField
                    name="website"
                    label="Web Sitesi"
                    value={form.website ?? ""}
                    onChange={handleFieldChange}
                    onBlur={register("website").onBlur}
                    error={register("website").error}
                    disabled={!canUpdate}
                    icon={<Globe size={16} />}
                    placeholder="https://www.kurum.com"
                  />
                </div>
              </div>
            </div>

            {/* Tax Information */}
            <div className="relative overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
              <h3 className="text-sm font-black text-white mb-5">Vergi Bilgileri</h3>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField
                  name="taxNumber"
                  label="Vergi Numarası"
                  value={form.taxNumber ?? ""}
                  onChange={handleFieldChange}
                  onBlur={register("taxNumber").onBlur}
                  error={register("taxNumber").error}
                  disabled={!canUpdate}
                  icon={<Hash size={16} />}
                  placeholder="1234567890"
                />
                <FormField
                  name="taxOffice"
                  label="Vergi Dairesi"
                  value={form.taxOffice ?? ""}
                  onChange={handleFieldChange}
                  onBlur={register("taxOffice").onBlur}
                  error={register("taxOffice").error}
                  disabled={!canUpdate}
                  icon={<FileText size={16} />}
                  placeholder="Vergi dairesi adı"
                />
              </div>
            </div>

            {/* Footer Text */}
            <div className="relative overflow-hidden rounded-2xl border border-slate-800 bg-slate-900/90 p-6 shadow-xl shadow-slate-950/30">
              <div className="flex items-center gap-3 mb-5">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-amber-500/15 border border-amber-500/30 text-amber-400">
                  <FileText size={18} />
                </div>
                <div>
                  <h3 className="text-sm font-black text-white">Footer Telif Metni</h3>
                  <p className="text-[10px] font-bold uppercase tracking-widest text-slate-500">Sitenin alt kısmında görüntülenir</p>
                </div>
              </div>
              <FormField
                name="footerText"
                label="Footer Metni"
                hideLabel
                value={form.footerText ?? ""}
                onChange={handleFieldChange}
                onBlur={register("footerText").onBlur}
                error={register("footerText").error}
                disabled={!canUpdate}
                placeholder="© 2026 Kurum Adı — Tüm hakları saklıdır."
              />
            </div>

            {/* Save Button */}
            {canUpdate && (
              <div className="flex justify-end">
                <button
                  type="submit"
                  disabled={saving}
                  className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 px-6 py-3 text-sm font-black text-white shadow-lg shadow-indigo-600/25 transition-all hover:from-indigo-500 hover:to-violet-500 active:scale-95 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {saving ? <Loader2 size={18} className="animate-spin" /> : <Save size={18} />}
                  {saving ? "Kaydediliyor..." : "Değişiklikleri Kaydet"}
                </button>
              </div>
            )}
          </div>
        </div>
        </form>
      </motion.div>
      )}
    </ApiStateView>
  );
}
