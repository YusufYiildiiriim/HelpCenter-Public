export const LOCALES = ["tr-TR", "en-US"] as const;
export type Locale = (typeof LOCALES)[number];
export type LocalePath = "tr" | "en";

export const DEFAULT_LOCALE: Locale = "tr-TR";

export const LOCALE_CONFIG: Record<Locale, { path: LocalePath; label: string; nativeName: string }> = {
  "tr-TR": { path: "tr", label: "Turkish", nativeName: "Türkçe" },
  "en-US": { path: "en", label: "English", nativeName: "English" },
};

export const messages = {
  "tr-TR": {
    language: "Dil", project: "Proje:", choose: "Seçin", projects: "Projeler",
    supportCenter: "Kurumsal Bilgi Bankası ve Destek Merkezi", createRequest: "Talep Oluştur",
    adminLogin: "Yönetici Girişi", noProjects: "Henüz proje bulunmuyor.",
    search: "Rehber veya soru ara...", guides: "Kullanım Kılavuzları",
    faq: "Sıkça Sorulan Sorular", flow: "İş Akışı", newest: "En yeni",
    alphabetical: "A'dan Z'ye", reverseAlphabetical: "Z'den A'ya",
    previous: "Önceki", next: "Sonraki", page: "Sayfa", of: "/", items: "kayıt",
    shownIn: "İçerik şu dilde gösteriliyor:", translationMissing: "Bu dilde çeviri bulunamadı.",
  },
  "en-US": {
    language: "Language", project: "Project:", choose: "Choose", projects: "Projects",
    supportCenter: "Knowledge Base and Support Center", createRequest: "Create a request",
    adminLogin: "Admin sign in", noProjects: "No projects are available yet.",
    search: "Search guides or questions...", guides: "Guides",
    faq: "Frequently asked questions", flow: "Workflow", newest: "Newest",
    alphabetical: "A to Z", reverseAlphabetical: "Z to A",
    previous: "Previous", next: "Next", page: "Page", of: "of", items: "items",
    shownIn: "This content is shown in:", translationMissing: "No translation is available in this language.",
  },
} as const;

export type MessageKey = keyof (typeof messages)["tr-TR"];

export function isLocale(value: string | null | undefined): value is Locale {
  return !!value && (LOCALES as readonly string[]).includes(value);
}

export function localeFromPath(value: string | null | undefined): Locale | null {
  if (value === "tr") return "tr-TR";
  if (value === "en") return "en-US";
  return isLocale(value) ? value : null;
}

export function localePath(locale: Locale): LocalePath {
  return LOCALE_CONFIG[locale].path;
}

export function localeFromPreference(value: string | null | undefined): Locale | null {
  if (!value) return null;
  const normalized = value.toLowerCase();
  if (normalized.startsWith("tr")) return "tr-TR";
  if (normalized.startsWith("en")) return "en-US";
  return null;
}
