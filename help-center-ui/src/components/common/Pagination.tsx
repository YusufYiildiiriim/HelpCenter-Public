"use client";

import React, { useState, useRef, useEffect } from "react";
import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  ChevronDown,
  Check,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { motion, AnimatePresence } from "framer-motion";

const PAGE_SIZE_OPTIONS = [5, 10, 20, 50, 100];

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  itemsLabel?: string;
  containerClassName?: string;
}

const DEFAULT_CONTAINER =
  "w-full flex flex-col sm:flex-row items-center justify-between gap-4 px-5 py-4 bg-white dark:bg-slate-900/70 border border-slate-200/80 dark:border-slate-800/80 rounded-2xl shadow-sm text-xs font-medium text-slate-600 dark:text-slate-300";

const PAGE_BUTTON_CLASS =
  "p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700/80 text-slate-700 dark:text-slate-200 hover:bg-indigo-500 hover:text-white dark:hover:bg-indigo-600 hover:border-indigo-500 disabled:opacity-30 disabled:hover:bg-white dark:disabled:hover:bg-slate-900 disabled:hover:text-slate-700 dark:disabled:hover:text-slate-200 disabled:cursor-not-allowed transition-all shadow-sm cursor-pointer";

export function Pagination({
  currentPage,
  totalPages,
  totalItems,
  pageSize,
  onPageChange,
  onPageSizeChange,
  itemsLabel = "kayıt",
  containerClassName = DEFAULT_CONTAINER,
}: PaginationProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const start = totalItems === 0 ? 0 : (currentPage - 1) * pageSize + 1;
  const end = Math.min(currentPage * pageSize, totalItems);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  const handlePageChange = (page: number) => {
    if (page < 1) page = 1;
    if (page > totalPages) page = totalPages;
    onPageChange(page);
  };

  return (
    <div className={containerClassName}>
      <div className="flex flex-wrap items-center justify-center sm:justify-start gap-2 sm:gap-3 text-center sm:text-left w-full sm:w-auto">
        <div className="flex items-center gap-2">
          <span className="text-slate-400 font-medium text-[11px] sm:text-xs">Sayfa başına:</span>
          
          <div className="relative" ref={dropdownRef}>
            <button
              type="button"
              onClick={() => setIsOpen(!isOpen)}
              className="flex items-center gap-1.5 bg-slate-900 hover:bg-slate-800 border border-slate-700/80 hover:border-slate-600 rounded-xl px-2.5 py-1 text-xs font-bold text-slate-100 shadow-sm transition-all cursor-pointer focus:outline-none focus:ring-2 focus:ring-indigo-500/40"
            >
              <span>{pageSize}</span>
              <ChevronDown size={13} className={cn("text-slate-400 transition-transform duration-200", isOpen && "rotate-180")} />
            </button>

            <AnimatePresence>
              {isOpen && (
                <motion.div
                  initial={{ opacity: 0, y: 6, scale: 0.95 }}
                  animate={{ opacity: 1, y: 0, scale: 1 }}
                  exit={{ opacity: 0, y: 6, scale: 0.95 }}
                  transition={{ duration: 0.15 }}
                  className="absolute bottom-full mb-1.5 left-0 z-50 min-w-[70px] bg-slate-900 border border-slate-700 rounded-xl shadow-2xl p-1 overflow-hidden"
                >
                  {PAGE_SIZE_OPTIONS.map((size) => (
                    <button
                      key={size}
                      type="button"
                      onClick={() => {
                        onPageSizeChange(size);
                        setIsOpen(false);
                      }}
                      className={cn(
                        "w-full flex items-center justify-between px-2.5 py-1.5 text-xs font-bold rounded-lg transition-colors cursor-pointer text-left",
                        pageSize === size
                          ? "bg-indigo-600 text-white shadow-sm"
                          : "text-slate-300 hover:bg-slate-800 hover:text-white"
                      )}
                    >
                      <span>{size}</span>
                      {pageSize === size && <Check size={12} strokeWidth={3} className="text-white shrink-0 ml-1.5" />}
                    </button>
                  ))}
                </motion.div>
              )}
            </AnimatePresence>
          </div>
        </div>

        <span className="text-slate-400 text-[11px] sm:text-xs">
          <strong className="text-slate-100 font-extrabold">{totalItems}</strong> {itemsLabel} (
          <strong className="text-slate-100 font-extrabold">{start}-{end}</strong>)
        </span>
      </div>

      <div className="flex items-center justify-center gap-1.5 sm:gap-2 w-full sm:w-auto">
        <button
          type="button"
          onClick={() => handlePageChange(1)}
          disabled={currentPage <= 1 || totalPages === 0}
          className={PAGE_BUTTON_CLASS}
          title="İlk Sayfa"
        >
          <ChevronsLeft className="w-4 h-4" />
        </button>
        <button
          type="button"
          onClick={() => handlePageChange(currentPage - 1)}
          disabled={currentPage <= 1 || totalPages === 0}
          className={PAGE_BUTTON_CLASS}
          title="Önceki Sayfa"
        >
          <ChevronLeft className="w-4 h-4" />
        </button>

        <span className="px-3 py-1.5 font-bold text-xs text-slate-100 bg-slate-900 border border-slate-700/80 rounded-xl shadow-sm">
          {currentPage} / {totalPages || 1}
        </span>

        <button
          type="button"
          onClick={() => handlePageChange(currentPage + 1)}
          disabled={currentPage >= totalPages || totalPages === 0}
          className={PAGE_BUTTON_CLASS}
          title="Sonraki Sayfa"
        >
          <ChevronRight className="w-4 h-4" />
        </button>
        <button
          type="button"
          onClick={() => handlePageChange(totalPages)}
          disabled={currentPage >= totalPages || totalPages === 0}
          className={PAGE_BUTTON_CLASS}
          title="Son Sayfa"
        >
          <ChevronsRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}
