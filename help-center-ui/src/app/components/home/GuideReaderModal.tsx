"use client";

import React, { useMemo } from "react";
import { BookOpen, ChevronRight, Zap, X, Play, ExternalLink, Maximize2, FileText, ArrowRight, Check } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import type { PublicGuide as Guide } from "@/services/public/PublicGuideService";
import { toApiUrl } from "@/lib/env";

interface GuideReaderModalProps {
  selectedGuide: Guide | null;
  setSelectedGuide: (guide: Guide | null) => void;
  guides: Guide[];
  handleNextStep: () => void;
  handlePrevStep: () => void;
  getSequencedGuides: (moduleName?: string) => Guide[];
}

export const GuideReaderModal: React.FC<GuideReaderModalProps> = ({
  selectedGuide,
  setSelectedGuide,
  guides,
  handleNextStep,
  handlePrevStep,
  getSequencedGuides,
}) => {
  const sequencedGuides = useMemo<Guide[]>(() => {
    if (!selectedGuide) return [];
    return getSequencedGuides(selectedGuide.module);
  }, [selectedGuide, getSequencedGuides]);

  const currentStepIndex = useMemo(() => {
    if (!selectedGuide) return 0;
    return sequencedGuides.findIndex((g: Guide) => g.publicId === selectedGuide.publicId) + 1;
  }, [selectedGuide, sequencedGuides]);

  return (
    <AnimatePresence>
      {selectedGuide && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-2 sm:p-4 md:p-8">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.15 }}
            className="absolute inset-0 bg-obsidian-950/92 sm:backdrop-blur-xl"
            onClick={() => setSelectedGuide(null)}
          />

          <motion.div
            initial={{ opacity: 0, scale: 0.96, y: 12 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.96, y: 12 }}
            transition={{ duration: 0.16, ease: [0.16, 1, 0.3, 1] }}
            className="relative w-full max-w-6xl bg-obsidian-900 rounded-3xl md:rounded-[2.5rem] border border-slate-800 shadow-2xl shadow-purple-950/40 overflow-hidden flex flex-col max-h-[95vh] sm:max-h-[90vh] will-change-transform transform-gpu"
          >
          {/* Header */}
          <div className="px-4 sm:px-6 py-3.5 border-b border-slate-800/80 flex justify-between items-center bg-obsidian-900">
            <div className="flex items-center gap-2 sm:gap-3 min-w-0">
              <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-purple-600 via-indigo-500 to-fuchsia-500 p-0.5 shadow-glow-purple flex items-center justify-center shrink-0">
                <div className="w-full h-full bg-obsidian-900 rounded-[6px] flex items-center justify-center">
                  <BookOpen size={14} className="text-purple-400" />
                </div>
              </div>
              <div className="flex items-center gap-1.5 sm:gap-2 min-w-0">
                <span className="text-[10px] font-black uppercase tracking-wider text-purple-400 bg-purple-950/70 border border-purple-800/40 px-2 py-0.5 rounded-md truncate">
                  {selectedGuide.module}
                </span>
                <ChevronRight size={10} className="text-slate-600 shrink-0" />
                <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 truncate">
                  Rehber Detayı
                </span>
              </div>
            </div>
            <div className="flex items-center gap-2 shrink-0">
              <button
                onClick={() => setSelectedGuide(null)}
                className="w-8 h-8 rounded-xl bg-obsidian-850 flex items-center justify-center text-slate-400 hover:bg-red-500/10 hover:text-red-400 transition-all active:scale-95 border border-slate-700/60 cursor-pointer"
                aria-label="Kapat"
              >
                <X size={15} />
              </button>
            </div>
          </div>

          <div className="flex-1 overflow-y-auto hide-scrollbar bg-obsidian-950/50">
            <div className="p-4 sm:p-6 md:p-8">
              <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 sm:gap-8 items-start">
                {/* Left Column */}
                <div className="lg:col-span-7 space-y-6">
                  <div className="bg-obsidian-850/80 border border-slate-800 rounded-2xl sm:rounded-[2rem] p-5 sm:p-8 md:p-10">
                    <h1 className="text-xl sm:text-2xl md:text-3xl font-black text-white mb-4 sm:mb-6 leading-tight tracking-tight uppercase">
                      {selectedGuide.title}
                    </h1>
                    <div className="ck-content ck-content-dark max-w-none mb-6 sm:mb-8">
                      <div
                        dangerouslySetInnerHTML={{ __html: selectedGuide.description }}
                        className="text-sm sm:text-base text-slate-300 font-medium leading-relaxed border-l-4 border-purple-500/50 pl-4 sm:pl-5 py-1"
                      />
                    </div>

                    {selectedGuide.youtubeUrl && (
                      <div className="pt-6 border-t border-slate-800/80">
                        <a
                          href={selectedGuide.youtubeUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="flex items-center justify-between p-4 sm:p-5 bg-red-500/5 border border-red-500/20 rounded-xl group hover:bg-red-500/10 transition-all"
                        >
                          <div className="flex items-center gap-3 sm:gap-4">
                            <div className="w-9 h-9 sm:w-10 sm:h-10 bg-obsidian-900 rounded-xl flex items-center justify-center text-red-400 shadow-sm group-hover:scale-110 transition-transform shrink-0 border border-red-500/20">
                              <Play size={18} fill="currentColor" />
                            </div>
                            <div>
                              <p className="text-[9px] font-black text-red-400 uppercase tracking-widest mb-0.5">
                                Eğitim Videosu
                              </p>
                              <p className="text-xs font-bold text-slate-300">Bu adımı YouTube üzerinde izleyin</p>
                            </div>
                          </div>
                          <ExternalLink size={16} className="text-red-400 shrink-0" />
                        </a>
                      </div>
                    )}
                  </div>
                </div>

                {/* Right Column */}
                <div className="lg:col-span-5 lg:sticky lg:top-0 space-y-6 self-start max-h-[calc(90vh-200px)] overflow-y-auto hide-scrollbar pr-1">
                  {selectedGuide.documents && selectedGuide.documents.length > 0 ? (
                    <div className="space-y-4">
                      <div className="flex items-center gap-3 px-2">
                        <div className="w-1.5 h-1.5 rounded-full bg-purple-500" />
                        <h4 className="text-[10px] font-black text-slate-400 uppercase tracking-[0.25em]">
                          EK DOSYALAR VE GÖRSELLER
                        </h4>
                      </div>

                      <div className="grid grid-cols-1 gap-4 sm:gap-5">
                        {selectedGuide.documents.map((doc, idx) => {
                          const isImg = /\.(jpg|jpeg|png|gif|webp)$/i.test(doc.path);
                          const isVid = /\.(mp4|webm|ogg)$/i.test(doc.path);
                          const fullUrl = toApiUrl(doc.path);

                          return (
                            <div
                              key={idx}
                              className="bg-obsidian-850/80 rounded-2xl border border-slate-800 overflow-hidden hover:border-purple-500/40 transition-all group"
                            >
                              {isImg ? (
                                <div className="relative aspect-[16/9] overflow-hidden bg-obsidian-900">
                                  {/* eslint-disable-next-line @next/next/no-img-element */}
                                  <img
                                    src={fullUrl}
                                    alt={doc.fileName}
                                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
                                  />
                                  <div className="absolute inset-0 bg-obsidian-950/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                    <a
                                      href={fullUrl}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                      className="w-10 h-10 bg-white rounded-full flex items-center justify-center text-slate-900 shadow-xl hover:scale-110 transition-all"
                                    >
                                      <Maximize2 size={16} />
                                    </a>
                                  </div>
                                </div>
                              ) : isVid ? (
                                <div className="aspect-video bg-black">
                                  <video controls className="w-full h-full">
                                    <source src={fullUrl} type={`video/${doc.path.split(".").pop()}`} />
                                  </video>
                                </div>
                              ) : (
                                <div className="p-4 sm:p-5 flex items-center gap-3 sm:gap-4">
                                  <div className="w-9 h-9 sm:w-10 sm:h-10 bg-purple-500/10 text-purple-400 rounded-xl flex items-center justify-center shrink-0 border border-purple-500/20">
                                    <FileText size={18} />
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <p className="text-xs font-bold text-slate-300 truncate">{doc.fileName}</p>
                                    <a
                                      href={fullUrl}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                      className="text-[10px] font-bold text-purple-400 uppercase tracking-widest hover:underline"
                                    >
                                      Dosyayı İndir
                                    </a>
                                  </div>
                                </div>
                              )}
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  ) : (
                    <div className="bg-obsidian-850/40 border border-dashed border-slate-800 rounded-2xl p-6 sm:p-10 flex flex-col items-center justify-center text-center">
                      <div className="w-10 h-10 sm:w-12 sm:h-12 bg-obsidian-800 rounded-xl flex items-center justify-center text-slate-600 mb-2.5 shadow-sm">
                        <Zap size={22} />
                      </div>
                      <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">
                        Bu adım için ek medya bulunmuyor
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Navigation Footer */}
          <div className="p-4 sm:p-6 bg-obsidian-900 border-t border-slate-800/80">
            <div className="max-w-4xl mx-auto flex flex-col sm:flex-row items-stretch sm:items-center gap-3 sm:gap-4">
              {guides.find((g) => g.publicId === selectedGuide.previousGuidePublicId) ? (
                <button
                  type="button"
                  onClick={handlePrevStep}
                  className="w-full sm:flex-1 bg-obsidian-850 hover:bg-obsidian-800 border border-slate-800 hover:border-purple-500/30 px-4 sm:px-6 py-3 rounded-xl sm:rounded-2xl flex items-center gap-3 sm:gap-4 group transition-all cursor-pointer"
                >
                  <div className="w-8 h-8 rounded-lg bg-obsidian-900 flex items-center justify-center text-slate-400 group-hover:text-purple-400 transition-colors shrink-0">
                    <ArrowRight size={16} className="rotate-180" />
                  </div>
                  <div className="text-left min-w-0">
                    <p className="text-[9px] font-bold text-slate-500 uppercase tracking-widest">Önceki</p>
                    <p className="text-xs font-bold text-slate-300 truncate">
                      {guides.find((g) => g.publicId === selectedGuide.previousGuidePublicId)?.title}
                    </p>
                  </div>
                </button>
              ) : (
                <div className="hidden sm:flex flex-1" />
              )}

              <div className="flex items-center justify-center gap-2 px-4 py-2 bg-obsidian-850 border border-slate-800 rounded-full text-[10px] font-bold text-purple-400 uppercase tracking-widest shrink-0 self-center">
                <Check size={14} className="text-purple-400" />
                Adım {currentStepIndex} / {sequencedGuides.length}
              </div>

              {guides.find((g) => g.previousGuidePublicId === selectedGuide.publicId) ? (
                <button
                  type="button"
                  onClick={handleNextStep}
                  className="w-full sm:flex-1 bg-gradient-to-r from-purple-600 via-indigo-600 to-fuchsia-600 hover:opacity-95 text-white px-4 sm:px-6 py-3 rounded-xl sm:rounded-2xl flex items-center justify-between group transition-all shadow-glow-purple cursor-pointer"
                >
                  <div className="text-left min-w-0">
                    <p className="text-[9px] font-bold text-purple-200 uppercase tracking-widest">Sonraki</p>
                    <p className="text-xs font-bold text-white truncate">
                      {guides.find((g) => g.previousGuidePublicId === selectedGuide.publicId)?.title}
                    </p>
                  </div>
                  <div className="w-8 h-8 rounded-lg bg-white/10 flex items-center justify-center text-white group-hover:bg-white group-hover:text-purple-600 transition-all shrink-0">
                    <ArrowRight size={16} />
                  </div>
                </button>
              ) : (
                <div className="hidden sm:flex flex-1" />
              )}
            </div>
          </div>
        </motion.div>
      </div>
    )}
  </AnimatePresence>
  );
};
