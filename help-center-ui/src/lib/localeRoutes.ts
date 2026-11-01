import { localeFromPath, localePath, type Locale } from "@/lib/i18n";

export function localizedRoute(pathname: string, locale: Locale, search = "", hash = ""): string {
  const segments = pathname.split("/");
  if (localeFromPath(segments[1])) segments[1] = localePath(locale);
  return (segments.join("/") || "/") + search + hash;
}
