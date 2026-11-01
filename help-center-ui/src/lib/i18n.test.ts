import { describe, expect, it } from "vitest";
import { DEFAULT_LOCALE, LOCALE_CONFIG, LOCALES, localeFromPath, localeFromPreference, messages } from "./i18n";
import { localizedRoute } from "./localeRoutes";

describe("locale configuration", () => {
  it("supports only Turkish and English canonical locale codes", () => {
    expect(LOCALES).toEqual(["tr-TR", "en-US"]);
    expect(DEFAULT_LOCALE).toBe("tr-TR");
    expect(localeFromPreference("en-GB,en;q=0.9")).toBe("en-US");
    expect(localeFromPreference("tr")).toBe("tr-TR");
    expect(localeFromPreference("fr-FR")).toBeNull();
  });

  it("maps the public URL segments to canonical locales", () => {
    expect(localeFromPath("tr")).toBe("tr-TR");
    expect(localeFromPath("en")).toBe("en-US");
    expect(localeFromPath("fr")).toBeNull();
    expect(LOCALE_CONFIG["en-US"].path).toBe("en");
  });

  it("preserves the current route, query parameters, and fragment when switching languages", () => {
    expect(localizedRoute("/en/faq", "tr-TR", "?search=invoice&page=2", "#top"))
      .toBe("/tr/faq?search=invoice&page=2#top");
  });

  it("keeps the translation catalogs structurally complete", () => {
    expect(Object.keys(messages["en-US"]).sort()).toEqual(Object.keys(messages["tr-TR"]).sort());
  });
});
