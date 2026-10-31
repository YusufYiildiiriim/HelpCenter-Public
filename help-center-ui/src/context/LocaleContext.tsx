"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import api from "@/lib/axios";
import { getAccessToken } from "@/lib/authSession";
import { DEFAULT_LOCALE, localeFromPreference, localeFromPath, localePath, messages, type Locale, type MessageKey } from "@/lib/i18n";

interface LocaleContextValue {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: MessageKey) => string;
}

const LocaleContext = createContext<LocaleContextValue | null>(null);
const COOKIE_NAME = "hc_locale";

function writeLocaleCookie(locale: Locale) {
  if (typeof document === "undefined") return;
  document.cookie = COOKIE_NAME + "=" + encodeURIComponent(locale) + "; Path=/; Max-Age=31536000; SameSite=Lax";
}

export function LocaleProvider({ initialLocale, children }: { initialLocale: Locale; children: React.ReactNode }) {
  const [locale, updateLocale] = useState<Locale>(initialLocale);
  const pathname = usePathname();
  const router = useRouter();

  useEffect(() => {
    const cookieLocale = document.cookie.split("; ").find((item) => item.startsWith(COOKIE_NAME + "="))?.split("=")[1];
    const preferred = localeFromPreference(cookieLocale ? decodeURIComponent(cookieLocale) : null);
    if (preferred) updateLocale(preferred);

    const restoreAccountLocale = () => {
      if (!getAccessToken()) return;
      void api.get<{ locale?: string | null }>("/api/auth/language-preference")
        .then(({ data }) => {
          const saved = localeFromPreference(data.locale);
          if (saved) {
            writeLocaleCookie(saved);
            updateLocale(saved);
          }
        })
        .catch(() => undefined);
    };

    window.addEventListener("helpcenter:login-success", restoreAccountLocale);
    restoreAccountLocale();
    return () => window.removeEventListener("helpcenter:login-success", restoreAccountLocale);
  }, []);

  useEffect(() => {
    document.documentElement.lang = locale;
    writeLocaleCookie(locale);
    api.defaults.headers.common["Accept-Language"] = locale;
  }, [locale]);

  const setLocale = useCallback((nextLocale: Locale) => {
    updateLocale(nextLocale);
    const pathLocale = localeFromPath(pathname.split("/")[1]);
    if (pathLocale) {
      const segments = pathname.split("/");
      segments[1] = localePath(nextLocale);
      router.push(segments.join("/") || "/" + localePath(nextLocale));
    }

    if (typeof window !== "undefined" && getAccessToken() !== null) {
      void api.put("/api/auth/language-preference", { locale: nextLocale }).catch(() => undefined);
    }
  }, [pathname, router]);

  const value = useMemo<LocaleContextValue>(() => ({
    locale,
    setLocale,
    t: (key) => messages[locale][key] ?? messages[DEFAULT_LOCALE][key],
  }), [locale, setLocale]);

  return <LocaleContext.Provider value={value}>{children}</LocaleContext.Provider>;
}

export function useLocale() {
  const context = useContext(LocaleContext);
  if (!context) throw new Error("useLocale must be used inside LocaleProvider");
  return context;
}
