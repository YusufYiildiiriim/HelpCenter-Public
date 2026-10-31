import { DEFAULT_LOCALE, localeFromPreference, type Locale } from "@/lib/i18n";

export type FieldType = "text" | "email" | "number" | "phone" | "password";

export interface FieldRules {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  type?: FieldType;
  pattern?: RegExp;
  patternMessage?: string;
  validate?: (value: string) => string | undefined;
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function isValidEmailAddress(value: string): boolean {
  const normalized = value.trim();
  return normalized.length <= 254 && EMAIL_REGEX.test(normalized);
}

const validationMessages: Record<Locale, {
  required: (label: string) => string;
  email: string;
  number: string;
  phoneLength: string;
  phoneDigits: string;
  minLength: (min: number) => string;
  maxLength: (max: number) => string;
  invalid: string;
}> = {
  "tr-TR": {
    required: (label) => `${label} alanı zorunludur.`,
    email: "Geçerli bir e-posta adresi giriniz.",
    number: "Geçerli bir sayı giriniz.",
    phoneLength: "Telefon numarası 10-15 hane arasında olmalıdır.",
    phoneDigits: "Telefon numarası yalnızca rakam içermelidir.",
    minLength: (min) => `En az ${min} karakter olmalıdır.`,
    maxLength: (max) => `En fazla ${max} karakter olmalıdır.`,
    invalid: "Bu alan geçerli değildir.",
  },
  "en-US": {
    required: (label) => `${label} is required.`,
    email: "Enter a valid email address.",
    number: "Enter a valid number.",
    phoneLength: "The phone number must contain 10 to 15 digits.",
    phoneDigits: "The phone number can contain digits only.",
    minLength: (min) => `Enter at least ${min} characters.`,
    maxLength: (max) => `Enter no more than ${max} characters.`,
    invalid: "Enter a valid value.",
  },
};

function getActiveLocale(): Locale {
  if (typeof document === "undefined") return DEFAULT_LOCALE;
  const pageLocale = localeFromPreference(document.documentElement.lang);
  if (pageLocale) return pageLocale;
  const cookieLocale = document.cookie.split("; ").find((item) => item.startsWith("hc_locale="))?.slice("hc_locale=".length);
  return localeFromPreference(cookieLocale ? decodeURIComponent(cookieLocale) : null) ?? DEFAULT_LOCALE;
}

export function validateField(value: unknown, rules: FieldRules | undefined, label: string, locale: Locale = getActiveLocale()): string | undefined {
  if (!rules) return undefined;

  const str = value === null || value === undefined ? "" : String(value).trim();
  const required = rules.required ?? false;
  const message = validationMessages[locale];

  if (required && str.length === 0) {
    return message.required(label);
  }

  if (str.length === 0) {
    return undefined;
  }

  if (rules.type === "email" && !isValidEmailAddress(str)) {
    return message.email;
  }

  if (rules.type === "number" && !/^\d+$/.test(str)) {
    return message.number;
  }

  if (rules.type === "phone") {
    const digits = str.replace(/\D/g, "");
    if (digits.length < 10 || digits.length > 15) {
      return message.phoneLength;
    }
    if (/[a-zA-Z]/.test(str)) {
      return message.phoneDigits;
    }
  }

  if (rules.minLength !== null && rules.minLength !== undefined && str.length < rules.minLength) {
    return message.minLength(rules.minLength);
  }

  if (rules.maxLength !== null && rules.maxLength !== undefined && str.length > rules.maxLength) {
    return message.maxLength(rules.maxLength);
  }

  if (rules.pattern && !rules.pattern.test(str)) {
    return rules.patternMessage ?? message.invalid;
  }

  if (rules.validate) {
    return rules.validate(str);
  }

  return undefined;
}
