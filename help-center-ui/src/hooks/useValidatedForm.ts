import { useCallback, useRef, useState } from "react";
import { validateField, type FieldRules } from "@/lib/validation";

export interface FieldConfig {
  rules?: FieldRules;
  label: string;
}

export function useValidatedForm<T extends object>(
  values: T,
  fieldConfig: Partial<Record<keyof T, FieldConfig>>
) {
  const [touched, setTouched] = useState<Set<keyof T>>(new Set());
  const submitCbRef = useRef<(e: React.FormEvent) => void>(() => {});

  const computeErrors = useCallback((): Partial<Record<keyof T, string>> => {
    const next: Partial<Record<keyof T, string>> = {};
    (Object.keys(fieldConfig) as (keyof T)[]).forEach((name) => {
      const cfg = fieldConfig[name];
      if (!cfg) return;
      const err = validateField(values[name], cfg.rules, cfg.label);
      if (err) next[name] = err;
    });
    return next;
  }, [values, fieldConfig]);

  const markAllTouched = useCallback(() => {
    setTouched(new Set(Object.keys(fieldConfig) as (keyof T)[]));
  }, [fieldConfig]);

  const register = useCallback(
    (name: keyof T) => {
      const cfg = fieldConfig[name];
      const err = cfg ? validateField(values[name], cfg.rules, cfg.label) : undefined;
      return {
        error: touched.has(name) ? err : undefined,
        onBlur: () => setTouched((prev) => {
          if (prev.has(name)) return prev;
          const s = new Set(prev);
          s.add(name);
          return s;
        }),
      };
    },
    [values, fieldConfig, touched]
  );

  const validateAll = useCallback((): boolean => {
    const errors = computeErrors();
    markAllTouched();
    return Object.keys(errors).length === 0;
  }, [computeErrors, markAllTouched]);

  const validateFields = useCallback((fieldNames: (keyof T)[]): boolean => {
    let valid = true;
    fieldNames.forEach((name) => {
      const cfg = fieldConfig[name];
      if (!cfg) return;
      if (validateField(values[name], cfg.rules, cfg.label)) valid = false;
    });
    setTouched((prev) => {
      const s = new Set(prev);
      fieldNames.forEach((name) => s.add(name));
      return s;
    });
    return valid;
  }, [values, fieldConfig]);

  const handleSubmit = useCallback((cb: (e: React.FormEvent) => void) => {
    submitCbRef.current = cb;
    return (e: React.FormEvent) => {
      e.preventDefault();
      const errors = computeErrors();
      markAllTouched();
      if (Object.keys(errors).length > 0) {
        const first = Object.keys(errors)[0];
        const el = document.querySelector<HTMLElement>(`[name="${first}"]`);
        el?.focus();
        return;
      }
      submitCbRef.current(e);
    };
  }, [computeErrors, markAllTouched]);

  const reset = useCallback(() => setTouched(new Set()), []);

  return { register, validateAll, validateFields, handleSubmit, reset };
}
