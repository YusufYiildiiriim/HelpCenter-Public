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

export function validateField(value: unknown, rules: FieldRules | undefined, label: string): string | undefined {
  if (!rules) return undefined;

  const str = value === null || value === undefined ? "" : String(value).trim();
  const required = rules.required ?? false;

  if (required && str.length === 0) {
    return `${label} alanı zorunludur.`;
  }

  if (str.length === 0) {
    return undefined;
  }

  if (rules.type === "email" && !EMAIL_REGEX.test(str)) {
    return "Geçerli bir e-posta adresi giriniz.";
  }

  if (rules.type === "number" && !/^\d+$/.test(str)) {
    return "Geçerli bir sayı giriniz.";
  }

  if (rules.type === "phone") {
    const digits = str.replace(/\D/g, "");
    if (digits.length < 10 || digits.length > 15) {
      return "Telefon numarası 10-15 hane arasında olmalıdır.";
    }
    if (/[a-zA-Z]/.test(str)) {
      return "Telefon numarası yalnızca rakam içermelidir.";
    }
  }

  if (rules.minLength !== null && rules.minLength !== undefined && str.length < rules.minLength) {
    return `En az ${rules.minLength} karakter olmalıdır.`;
  }

  if (rules.maxLength !== null && rules.maxLength !== undefined && str.length > rules.maxLength) {
    return `En fazla ${rules.maxLength} karakter olmalıdır.`;
  }

  if (rules.pattern && !rules.pattern.test(str)) {
    return rules.patternMessage ?? "Bu alan geçerli değildir.";
  }

  if (rules.validate) {
    return rules.validate(str);
  }

  return undefined;
}
