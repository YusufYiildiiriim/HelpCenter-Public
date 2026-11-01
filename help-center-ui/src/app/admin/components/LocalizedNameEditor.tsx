"use client";

import { useState } from "react";
import type { RequestSubjectTranslation } from "@/services/admin/AdminRequestSubjectService";

interface Props {
  sourceLocale: string;
  onSourceLocaleChange: (locale: string) => void;
  translations: RequestSubjectTranslation[];
  onTranslationsChange: (translations: RequestSubjectTranslation[]) => void;
  legacyName: string;
  legacyDescription: string;
  onLegacyChange: (name: string, description: string) => void;
  nameLabel: string;
}

const LOCALES = [{ locale: "tr-TR", label: "Türkçe", short: "TR" }, { locale: "en-US", label: "English", short: "EN" }];

export function LocalizedNameEditor(props: Props) {
  const [activeLocale, setActiveLocale] = useState("tr-TR");
  const active = props.translations.find(item => item.locale === activeLocale);

  const selectSource = (locale: string) => {
    props.onSourceLocaleChange(locale);
    setActiveLocale(locale || "tr-TR");
    if (locale && !props.translations.some(item => item.locale === locale)) {
      props.onTranslationsChange([...props.translations, { locale, name: props.legacyName, description: props.legacyDescription }]);
    }
  };

  const change = (field: "name" | "description", value: string) => {
    const next = props.translations.filter(item => item.locale !== activeLocale);
    const entry = { locale: activeLocale, name: active?.name ?? "", description: active?.description ?? "", [field]: value };
    next.push(entry);
    props.onTranslationsChange(next);
    if (activeLocale === props.sourceLocale) props.onLegacyChange(entry.name, entry.description);
  };

  return <section className="space-y-3">
    <label className="block text-xs font-bold text-slate-300">Kaynak dil
      <select aria-label="Kaynak dil" value={props.sourceLocale} onChange={event => selectSource(event.target.value)} required className="mt-1 w-full rounded-xl bg-slate-800 border border-slate-700 p-3 text-white">
        <option value="">Kaynak dili seçin</option><option value="tr-TR">Türkçe</option><option value="en-US">English</option>
      </select>
    </label>
    <div className="flex gap-2" role="tablist" aria-label="Çeviri dilleri">
      {LOCALES.map(item => {
        const value = props.translations.find(t => t.locale === item.locale);
        const missing = !value?.name.trim() || !value.description.trim();
        return <button key={item.locale} type="button" role="tab" aria-selected={activeLocale === item.locale} onClick={() => setActiveLocale(item.locale)} className={`rounded-lg px-3 py-2 text-xs font-bold ${activeLocale === item.locale ? "bg-indigo-600 text-white" : "bg-slate-800 text-slate-300"}`}>
          {item.label}{missing ? " · Eksik" : ""}
        </button>;
      })}
    </div>
    <label className="block text-xs font-bold text-slate-300">{props.nameLabel}
      <input aria-label={`${props.nameLabel} (${LOCALES.find(x => x.locale === activeLocale)?.short})`} value={active?.name ?? ""} onChange={event => change("name", event.target.value)} required={activeLocale === props.sourceLocale} className="mt-1 w-full rounded-xl bg-slate-800 border border-slate-700 p-3 text-white" />
    </label>
    <label className="block text-xs font-bold text-slate-300">Açıklama
      <textarea aria-label={`Açıklama (${LOCALES.find(x => x.locale === activeLocale)?.short})`} value={active?.description ?? ""} onChange={event => change("description", event.target.value)} required={activeLocale === props.sourceLocale} rows={4} className="mt-1 w-full rounded-xl bg-slate-800 border border-slate-700 p-3 text-white" />
    </label>
    <p className="text-[11px] text-slate-400">Kaynak dil zorunludur. Diğer dilin çevirisi isteğe bağlıdır ve eksikse içerik kaynak dilinde gösterilir.</p>
  </section>;
}
