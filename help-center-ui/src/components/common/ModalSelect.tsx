"use client";

import { useId, useMemo, useState } from "react";
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
  searchable?: boolean;
  searchPlaceholder?: string;
}

/** Portal positioning avoids the native mobile select popup inside fixed modals. */
export function ModalSelect({ name, label, value, options, onValueChange, onBlur, disabled, error, searchable = false, searchPlaceholder }: ModalSelectProps) {
  const id = useId();
  const [search, setSearch] = useState("");
  const visibleOptions = useMemo(() => {
    const normalizedSearch = search.trim().toLocaleLowerCase("tr-TR");
    return normalizedSearch
      ? options.filter(option => option.label.toLocaleLowerCase("tr-TR").includes(normalizedSearch))
      : options;
  }, [options, search]);
  return (
    <div className="space-y-1.5 min-w-0">
      <label htmlFor={id} className="block text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">{label}</label>
      <DropdownMenu modal={false} onOpenChange={open => { if (!open) setSearch(""); }}>
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
          {searchable && (
            <div className="sticky top-0 z-10 border-b border-slate-700 bg-slate-900 p-2">
              <input
                type="search"
                value={search}
                onChange={event => setSearch(event.target.value)}
                onKeyDown={event => event.stopPropagation()}
                placeholder={searchPlaceholder ?? `${label} ara...`}
                aria-label={`${label} ara`}
                className="w-full rounded-lg border border-slate-700 bg-slate-800 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-500 focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/30"
              />
            </div>
          )}
          <DropdownMenuRadioGroup value={value} onValueChange={onValueChange} aria-label={label}>
            {visibleOptions.map(option => (
              <DropdownMenuRadioItem key={option.value} value={option.value}
                className="min-h-11 whitespace-normal break-words focus:bg-slate-700 focus:text-white">
                {option.label}
              </DropdownMenuRadioItem>
            ))}
            {visibleOptions.length === 0 && (
              <p className="px-3 py-4 text-sm text-slate-400">Eşleşen seçenek bulunamadı.</p>
            )}
          </DropdownMenuRadioGroup>
        </DropdownMenuContent>
      </DropdownMenu>
      {error && <p id={`${id}-error`} className="text-xs text-red-400">{error}</p>}
    </div>
  );
}
