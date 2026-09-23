"use client";

import React, { useState, useRef, useEffect } from "react";
import { ChevronDown, Check } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { cn } from "@/lib/utils";

export interface PortalSelectOption<T extends string | number = string> {
  value: T;
  label: string;
  count?: number;
}

export interface PortalSelectProps<T extends string | number = string> {
  label?: string;
  icon?: React.ReactNode;
  value: T;
  onChange: (value: T) => void;
  options: PortalSelectOption<T>[];
  placeholder?: string;
  className?: string;
  dropdownWidth?: string;
  disabled?: boolean;
}

export function PortalSelect<T extends string | number = string>({
  label,
  icon,
  value,
  onChange,
  options,
  placeholder = "Seçiniz",
  className,
  dropdownWidth = "w-56 sm:w-64",
  disabled = false,
}: PortalSelectProps<T>) {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((opt) => opt.value === value);

  // Close on outside click or escape
  useEffect(() => {
    if (!isOpen) return;

    const handleMouseDown = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleMouseDown);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handleMouseDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen]);

  const handleSelect = (optionValue: T) => {
    onChange(optionValue);
    setIsOpen(false);
  };

  return (
    <div
      ref={containerRef}
      className={cn("relative inline-block text-left", isOpen ? "z-50" : "z-10", className)}
    >
      {/* Trigger Button */}
      <button
        type="button"
        disabled={disabled}
        onClick={() => setIsOpen((prev) => !prev)}
        aria-haspopup="listbox"
        aria-expanded={isOpen}
        className={cn(
          "group flex items-center justify-between gap-2.5 bg-obsidian-850/90 border border-slate-800/90 hover:border-purple-500/40 px-3.5 py-2 rounded-xl transition-all duration-200 shadow-sm cursor-pointer select-none",
          isOpen && "border-purple-500/50 shadow-glow-purple/20 bg-obsidian-850",
          disabled && "opacity-50 cursor-not-allowed"
        )}
      >
        <div className="flex items-center gap-2 min-w-0">
          {icon && <span className="shrink-0">{icon}</span>}
          {label && <span className="text-xs text-slate-400 font-semibold shrink-0">{label}</span>}
          <span className="text-xs text-slate-100 font-bold truncate max-w-[150px] sm:max-w-[180px]">
            {selectedOption ? selectedOption.label : placeholder}
          </span>
        </div>

        <ChevronDown
          size={13}
          className={cn(
            "text-slate-400 transition-transform duration-200 shrink-0 group-hover:text-slate-200",
            isOpen && "rotate-180 text-purple-400"
          )}
        />
      </button>

      {/* Dropdown Menu */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: 4, scale: 0.98 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 4, scale: 0.98 }}
            transition={{ duration: 0.14, ease: "easeOut" }}
            className={cn(
              "absolute left-0 sm:left-auto sm:right-0 mt-2 bg-obsidian-900/95 border border-slate-800 rounded-2xl shadow-2xl p-1.5 z-[100] backdrop-blur-xl max-h-64 overflow-y-auto hide-scrollbar",
              dropdownWidth
            )}
            role="listbox"
          >
            {label && (
              <div className="px-3 py-1.5 border-b border-slate-800/70 mb-1">
                <span className="text-[10px] font-black uppercase tracking-wider text-slate-500">
                  {label.replace(":", "")}
                </span>
              </div>
            )}

            <div className="space-y-0.5">
              {options.map((option) => {
                const isSelected = option.value === value;
                return (
                  <button
                    key={String(option.value)}
                    type="button"
                    onClick={() => handleSelect(option.value)}
                    role="option"
                    aria-selected={isSelected}
                    className={cn(
                      "w-full flex items-center justify-between gap-2 px-3 py-2 text-left text-xs rounded-xl transition cursor-pointer group",
                      isSelected
                        ? "bg-purple-500/20 text-purple-300 font-bold border border-purple-500/30 shadow-sm"
                        : "text-slate-300 hover:bg-obsidian-800 hover:text-white"
                    )}
                  >
                    <span className="truncate">{option.label}</span>

                    <div className="flex items-center gap-1.5 shrink-0">
                      {option.count !== undefined && (
                        <span
                          className={cn(
                            "text-[10px] px-1.5 py-0.5 rounded-md font-bold tabular-nums",
                            isSelected
                              ? "bg-purple-500/30 text-purple-200"
                              : "bg-obsidian-800 text-slate-500 group-hover:text-slate-400 border border-slate-800"
                          )}
                        >
                          {option.count}
                        </span>
                      )}
                      {isSelected && <Check size={13} className="text-purple-400" />}
                    </div>
                  </button>
                );
              })}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
