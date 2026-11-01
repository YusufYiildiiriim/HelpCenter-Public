"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import api from "@/lib/axios";
import { getAccessToken } from "@/lib/authSession";
import { DEFAULT_LOCALE, localeFromPreference, localeFromPath, messages, type Locale, type MessageKey } from "@/lib/i18n";
import { localizedRoute } from "@/lib/localeRoutes";

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
  const restoredSessionToken = useRef<string | null>(null);
  const pathname = usePathname();
  const router = useRouter();
  const routeLocale = localeFromPath(pathname.split("/")[1]);

  useEffect(() => {
    // A localized public URL is the source of truth, including on direct links.
    // Cookies and account preferences select the locale only on unprefixed routes.
    if (routeLocale) {
      updateLocale(routeLocale);
      return;
    }

    const cookieLocale = document.cookie.split("; ").find((item) => item.startsWith(COOKIE_NAME + "="))?.split("=")[1];
    const preferred = localeFromPreference(cookieLocale ? decodeURIComponent(cookieLocale) : null);
    if (preferred) updateLocale(preferred);

    const restoreAccountLocale = () => {
      if (routeLocale) return;
      const token = getAccessToken();
      if (!token || restoredSessionToken.current === token) return;
      restoredSessionToken.current = token;
      void api.get<{ locale?: string | null }>("/api/auth/language-preference")
        .then(({ data }) => {
          const saved = localeFromPreference(data.locale);
          if (saved) {
            writeLocaleCookie(saved);
            updateLocale(saved);
          }
        })
        .catch(() => {
          if (restoredSessionToken.current === token) restoredSessionToken.current = null;
        });
    };

    window.addEventListener("helpcenter:session-ready", restoreAccountLocale);
    restoreAccountLocale();
    return () => window.removeEventListener("helpcenter:session-ready", restoreAccountLocale);
  }, [routeLocale]);

  useEffect(() => {
    document.documentElement.lang = locale;
    writeLocaleCookie(locale);
    api.defaults.headers.common["Accept-Language"] = locale;
  }, [locale]);

  const setLocale = useCallback((nextLocale: Locale) => {
    updateLocale(nextLocale);
    const pathLocale = localeFromPath(pathname.split("/")[1]);
    if (pathLocale) {
      const query = typeof window === "undefined" ? "" : window.location.search;
      const hash = typeof window === "undefined" ? "" : window.location.hash;
      router.push(localizedRoute(pathname, nextLocale, query, hash));
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
