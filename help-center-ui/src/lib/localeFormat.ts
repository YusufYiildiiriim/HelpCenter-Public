import { DEFAULT_LOCALE, localeFromPreference, type Locale } from "./i18n";

export function getActiveLocale(): Locale {
  if (typeof document === "undefined") return DEFAULT_LOCALE;
  const saved = document.cookie.split("; ").find((entry) => entry.startsWith("hc_locale="))?.slice("hc_locale=".length);
  return localeFromPreference(saved ? decodeURIComponent(saved) : null) ?? DEFAULT_LOCALE;
}

function asDate(value: Date | string | number): Date {
  return value instanceof Date ? value : new Date(value);
}

export function formatLocaleDate(value: Date | string | number, options: Intl.DateTimeFormatOptions = { dateStyle: "short" }, locale = getActiveLocale()): string {
  const date = asDate(value);
  return Number.isNaN(date.getTime()) ? "—" : new Intl.DateTimeFormat(locale, options).format(date);
}

export function formatLocaleDateTime(value: Date | string | number, options: Intl.DateTimeFormatOptions = { dateStyle: "short", timeStyle: "short" }, locale = getActiveLocale()): string {
  const date = asDate(value);
  return Number.isNaN(date.getTime()) ? "—" : new Intl.DateTimeFormat(locale, options).format(date);
}

export function formatLocaleTime(value: Date | string | number, options: Intl.DateTimeFormatOptions = { hour: "2-digit", minute: "2-digit" }, locale = getActiveLocale()): string {
  const date = asDate(value);
  return Number.isNaN(date.getTime()) ? "—" : new Intl.DateTimeFormat(locale, options).format(date);
}

export function formatLocaleNumber(value: number, options?: Intl.NumberFormatOptions, locale = getActiveLocale()): string {
  return new Intl.NumberFormat(locale, options).format(value);
}
