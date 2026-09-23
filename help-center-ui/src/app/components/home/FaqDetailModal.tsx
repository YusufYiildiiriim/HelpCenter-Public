"use client";

import React from "react";
import { HelpCircle, ChevronRight, X } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import type { PublicFaq as Faq } from "@/services/public/PublicFaqService";

interface FaqDetailModalProps {
  selectedFaq: Faq | null;
  setSelectedFaq: (faq: Faq | null) => void;
}

export const FaqDetailModal: React.FC<FaqDetailModalProps> = ({ selectedFaq, setSelectedFaq }) => {
  return (
    <AnimatePresence>
      {selectedFaq && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-2 sm:p-4 md:p-8">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.15 }}
            className="absolute inset-0 bg-obsidian-950/92 sm:backdrop-blur-xl"
            onClick={() => setSelectedFaq(null)}
          />

          <motion.div
            initial={{ opacity: 0, scale: 0.96, y: 12 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.96, y: 12 }}
            transition={{ duration: 0.16, ease: [0.16, 1, 0.3, 1] }}
            className="relative w-full max-w-2xl bg-obsidian-900 rounded-3xl md:rounded-[2.5rem] border border-slate-800 shadow-2xl shadow-purple-950/40 overflow-hidden flex flex-col max-h-[90vh] will-change-transform transform-gpu"
          >
            {/* Header */}
            <div className="px-4 sm:px-6 py-3.5 border-b border-slate-800/80 flex justify-between items-center bg-obsidian-900">
              <div className="flex items-center gap-2 sm:gap-3 min-w-0">
                <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-purple-600 via-indigo-500 to-fuchsia-500 p-0.5 shadow-glow-purple flex items-center justify-center shrink-0">
                  <div className="w-full h-full bg-obsidian-900 rounded-[6px] flex items-center justify-center">
                    <HelpCircle size={14} className="text-purple-400" />
                  </div>
                </div>
                <div className="flex items-center gap-1.5 sm:gap-2 min-w-0">
                  <span className="text-[10px] font-black uppercase tracking-wider text-purple-400 bg-purple-950/70 border border-purple-800/40 px-2 py-0.5 rounded-md truncate">
                    {selectedFaq.moduleName || "Genel"}
                  </span>
                  <ChevronRight size={10} className="text-slate-600 shrink-0" />
                  <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 truncate">
                    SSS Detayı
                  </span>
                </div>
              </div>
              <button
                onClick={() => setSelectedFaq(null)}
                className="w-8 h-8 rounded-xl bg-obsidian-850 flex items-center justify-center text-slate-400 hover:bg-red-500/10 hover:text-red-400 transition-all active:scale-95 border border-slate-700/60 cursor-pointer shrink-0"
                aria-label="Kapat"
              >
                <X size={15} />
              </button>
            </div>

            {/* Content */}
            <div className="flex-1 overflow-y-auto hide-scrollbar bg-obsidian-950/50">
              <div className="p-5 sm:p-8 md:p-10">
                <h1 className="text-lg sm:text-xl md:text-2xl font-black text-white mb-5 sm:mb-6 leading-tight tracking-tight">
                  {selectedFaq.title}
                </h1>
                <div
                  dangerouslySetInnerHTML={{ __html: selectedFaq.description }}
                  className="ck-content ck-content-dark text-sm sm:text-base text-slate-300 leading-relaxed border-l-4 border-purple-500/50 pl-4 sm:pl-5 py-1 prose prose-invert prose-sm max-w-none"
                />
              </div>
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
};
