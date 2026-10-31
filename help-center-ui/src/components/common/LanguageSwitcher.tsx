"use client";

import { Globe2 } from "lucide-react";
import { LOCALE_CONFIG, LOCALES } from "@/lib/i18n";
import { useLocale } from "@/context/LocaleContext";

export function LanguageSwitcher({ className = "" }: { className?: string }) {
  const { locale, setLocale, t } = useLocale();
  return (
    <label className={"inline-flex items-center gap-2 rounded-xl border border-slate-300/70 bg-white/90 px-3 py-2 text-sm text-slate-700 shadow-sm " + className}>
      <Globe2 size={16} aria-hidden="true" />
      <span className="sr-only">{t("language")}</span>
      <select aria-label={t("language")} value={locale}
        onChange={(event) => setLocale(event.target.value as (typeof LOCALES)[number])}
        className="cursor-pointer bg-transparent font-semibold outline-none">
        {LOCALES.map((supportedLocale) => (
          <option key={supportedLocale} value={supportedLocale}>{LOCALE_CONFIG[supportedLocale].nativeName}</option>
        ))}
      </select>
    </label>
  );
}
