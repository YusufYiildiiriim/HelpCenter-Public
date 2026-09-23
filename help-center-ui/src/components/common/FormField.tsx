import React, { useId } from "react";
import { cn } from "@/lib/utils";

interface FormFieldProps {
  name: string;
  label: string;
  value: string | number | readonly string[] | undefined;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => void;
  onBlur?: () => void;
  type?: string;
  placeholder?: string;
  icon?: React.ReactNode;
  right?: React.ReactNode;
  error?: string;
  required?: boolean;
  help?: string;
  as?: "input" | "textarea" | "select";
  rows?: number;
  className?: string;
  disabled?: boolean;
  variant?: "dark" | "light";
  hideLabel?: boolean;
  children?: React.ReactNode;
}

const baseInput =
  "w-full px-4 py-2.5 border rounded-xl focus:ring-4 focus:ring-focus-ring/10 focus:border-focus-ring outline-none transition-all text-sm font-bold";

const darkInput =
  "bg-slate-800/70 border-slate-700 text-slate-100 focus:bg-slate-800 placeholder:text-slate-500";

const lightInput =
  "bg-surface-sunken border-border-default text-content-primary focus:bg-surface-raised placeholder:text-content-muted";

const errorInput =
  "border-status-danger/70 focus:border-status-danger focus:ring-status-danger/10";

export function FormField({
  name,
  label,
  value,
  onChange,
  onBlur,
  type = "text",
  placeholder,
  icon,
  right,
  error,
  required,
  help,
  as = "input",
  rows,
  className,
  disabled,
  variant = "dark",
  hideLabel = false,
  children,
}: FormFieldProps) {
  const id = useId();
  const showError = Boolean(error);

  const inputClass = cn(
    baseInput,
    variant === "dark" ? darkInput : lightInput,
    icon ? "pl-10" : "pl-4",
    right ? "pr-12" : "pr-4",
    showError && errorInput,
    className
  );

  const commonProps = {
    id,
    name,
    value,
    onChange,
    onBlur,
    placeholder,
    disabled,
    "aria-invalid": showError,
    "aria-describedby": showError ? `${id}-error` : undefined,
  };

  return (
    <div className="space-y-1.5">
      {!hideLabel && (
        <label
          htmlFor={id}
          className="text-[10px] font-black text-content-muted uppercase tracking-widest ml-1"
        >
          {label}
          {required && <span className="text-status-danger ml-0.5">*</span>}
        </label>
      )}
      <div className="relative">
        {icon && (
          <span className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-content-muted">
            {icon}
          </span>
        )}
        {as === "textarea" ? (
          <textarea {...commonProps} rows={rows ?? 3} className={cn(inputClass, "resize-none")} />
        ) : as === "select" ? (
          <select {...commonProps} className={cn(inputClass, "appearance-none")}>
            {children}
          </select>
        ) : (
          <input {...commonProps} type={type} className={inputClass} />
        )}
        {right && (
          <div className="absolute right-1 top-1/2 -translate-y-1/2 flex items-center">
            {right}
          </div>
        )}
      </div>
      {showError && (
        <p id={`${id}-error`} className="text-[11px] font-bold text-status-danger ml-1">
          {error}
        </p>
      )}
      {!showError && help && (
        <p className="text-[10px] text-content-muted ml-1">{help}</p>
      )}
    </div>
  );
}
