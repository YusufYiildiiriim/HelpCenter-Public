"use client";

import { useId } from "react";
import { ChevronDown } from "lucide-react";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuRadioGroup,
  DropdownMenuRadioItem, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface ModalSelectProps {
  name?: string;
  label: string;
  value: string;
  options: { value: string; label: string }[];
  onValueChange: (value: string) => void;
  onBlur?: () => void;
  disabled?: boolean;
  error?: string;
}

/** Portal positioning avoids the native mobile select popup inside fixed modals. */
export function ModalSelect({ name, label, value, options, onValueChange, onBlur, disabled, error }: ModalSelectProps) {
  const id = useId();
  return (
    <div className="space-y-1.5 min-w-0">
      <label htmlFor={id} className="block text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">{label}</label>
      <DropdownMenu modal={false}>
        <DropdownMenuTrigger asChild>
          <button id={id} name={name} type="button" disabled={disabled} onBlur={onBlur}
            aria-describedby={error ? `${id}-error` : undefined}
            className="flex w-full items-center justify-between gap-2 rounded-xl border border-slate-700 bg-slate-800 px-4 py-2.5 text-left text-sm font-bold text-slate-100 outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 disabled:opacity-50">
            <span className="truncate">{options.find(option => option.value === value)?.label ?? "Seçin"}</span>
            <ChevronDown size={16} className="shrink-0" aria-hidden="true" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start" collisionPadding={8}
          style={{ width: "var(--radix-dropdown-menu-trigger-width)", maxHeight: "min(280px, var(--radix-dropdown-menu-content-available-height))" }}
          className="z-[200] min-w-0 overflow-y-auto overscroll-contain border-slate-700 bg-slate-900 text-slate-100">
          <DropdownMenuRadioGroup value={value} onValueChange={onValueChange} aria-label={label}>
            {options.map(option => (
              <DropdownMenuRadioItem key={option.value} value={option.value}
                className="min-h-11 whitespace-normal break-words focus:bg-slate-700 focus:text-white">
                {option.label}
              </DropdownMenuRadioItem>
            ))}
          </DropdownMenuRadioGroup>
        </DropdownMenuContent>
      </DropdownMenu>
      {error && <p id={`${id}-error`} className="text-xs text-red-400">{error}</p>}
    </div>
  );
}
