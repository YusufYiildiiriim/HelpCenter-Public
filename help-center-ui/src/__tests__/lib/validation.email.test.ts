import { describe, expect, it } from "vitest";
import { isValidEmailAddress, validateField } from "@/lib/validation";

describe("email validation", () => {
  it("accepts a normal address and rejects malformed input", () => {
    expect(isValidEmailAddress("user@example.com")).toBe(true);
    expect(isValidEmailAddress("user@localhost")).toBe(false);
    expect(isValidEmailAddress("user@@example.com")).toBe(false);
  });

  it("returns localized validation messages for both supported locales", () => {
    expect(validateField("", { required: true }, "Request title", "en-US")).toBe("Request title is required.");
    expect(validateField("", { required: true }, "Talep Başlığı", "tr-TR")).toBe("Talep Başlığı alanı zorunludur.");
    expect(validateField("bad", { type: "email" }, "Email", "en-US")).toBe("Enter a valid email address.");
    expect(validateField("bad", { type: "email" }, "E-posta", "tr-TR")).toBe("Geçerli bir e-posta adresi giriniz.");
    expect(validateField("ab", { minLength: 4 }, "Name", "en-US")).toBe("Enter at least 4 characters.");
    expect(validateField("12345", { maxLength: 4 }, "Name", "en-US")).toBe("Enter no more than 4 characters.");
  });
});