import { afterEach, describe, expect, it } from "vitest";
import { formatLocaleDate, formatLocaleNumber } from "./localeFormat";

describe("locale formatting", () => {
  afterEach(() => { document.cookie = "hc_locale=; Path=/; Max-Age=0"; });

  it("formats dates using the selected locale", () => {
    const date = "2026-01-15T12:00:00.000Z";
    expect(formatLocaleDate(date, { dateStyle: "short", timeZone: "UTC" }, "tr-TR")).toBe("15.01.2026");
    expect(formatLocaleDate(date, { dateStyle: "short", timeZone: "UTC" }, "en-US")).toBe("1/15/26");
  });

  it("uses the persisted locale for numbers and handles invalid dates", () => {
    document.cookie = "hc_locale=en-US";
    expect(formatLocaleNumber(1234.5)).toBe("1,234.5");
    expect(formatLocaleDate("invalid")).toBe("—");
  });
});
