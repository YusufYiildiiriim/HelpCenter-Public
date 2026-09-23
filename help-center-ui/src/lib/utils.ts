import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function normalizePhone(value: string): string {
  return value.replace(/[^\d+]/g, "");
}

export function isValidPhone(value: string): boolean {
  const digits = value.replace(/\D/g, "");
  const MIN_LENGTH = 10;
  const MAX_LENGTH = 15;
  if (digits.length < MIN_LENGTH || digits.length > MAX_LENGTH) return false;
  if (value.startsWith("+") && !/^\+/.test(value.trim())) return false;
  if (/[a-zA-Z]/.test(value)) return false;
  return true;
}

export function formatPhoneError(value: string): string | null {
  const digits = value.replace(/\D/g, "");
  if (!value.trim()) return "Telefon alanı zorunludur.";
  if (digits.length < 10 || digits.length > 15) return "Telefon numarası 10-15 hane arasında olmalıdır.";
  if (/[a-zA-Z]/.test(value)) return "Telefon numarası yalnızca rakam içermelidir.";
  return null;
}
