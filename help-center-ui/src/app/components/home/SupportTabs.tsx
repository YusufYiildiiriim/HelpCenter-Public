"use client";

import React, { useMemo } from "react";
import { AlertCircle, FileText } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { cn } from "@/lib/utils";
import type { PublicFaq as Faq } from "@/services/public/PublicFaqService";
import type { PublicGuide as Guide } from "@/services/public/PublicGuideService";
import type { PublicModule as Module } from "@/services/public/PublicModuleService";
import { MindMapFlow } from "./MindMapFlow";
import { Pagination } from "@/components/common/Pagination";

interface PageInfo {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
}

interface SupportTabsProps {
  activeTab: "sss" | "guide" | "flow";
  setActiveTab?: (tab: "sss" | "guide" | "flow") => void;
  loading: boolean;
  contentLoading?: boolean;
  filteredFaqs: Faq[];
  modules?: Module[];
  selectedModule?: string;
  setSelectedModule?: (mod: string) => void;
  filteredGuides: Guide[];
  setSelectedGuide: (guide: Guide) => void;
  onSelectFaq: (faq: Faq) => void;
  allGuides?: Guide[];
  sortBy?: "newest" | "az" | "za";
  projectName?: string;
  guidesPagination?: PageInfo;
  onGuidesPageChange?: (page: number) => void;
  onGuidesPageSizeChange?: (size: number) => void;
  faqsPagination?: PageInfo;
  onFaqsPageChange?: (page: number) => void;
  onFaqsPageSizeChange?: (size: number) => void;
}

const PAGINATION_CONTAINER_CLASS =
  "w-full flex flex-col sm:flex-row items-center justify-between gap-4 px-4 sm:px-5 py-3.5 mt-5 bg-obsidian-900/80 backdrop-blur-md border border-slate-800/80 rounded-2xl shadow-sm text-xs font-medium text-slate-300";

// Module badge color mapping for visual variety
const MODULE_COLORS = [
  { text: "text-purple-400", bg: "bg-purple-950/70", border: "border-purple-800/40" },
  { text: "text-indigo-400", bg: "bg-indigo-950/70", border: "border-indigo-800/40" },
  { text: "text-pink-400", bg: "bg-pink-950/70", border: "border-pink-800/40" },
  { text: "text-emerald-400", bg: "bg-emerald-950/70", border: "border-emerald-800/40" },
  { text: "text-cyan-400", bg: "bg-cyan-950/70", border: "border-cyan-800/40" },
  { text: "text-amber-400", bg: "bg-amber-950/70", border: "border-amber-800/40" },
];

function getModuleColor(index: number) {
  return MODULE_COLORS[index % MODULE_COLORS.length];
}

const SkeletonCard: React.FC = () => (
  <div className="p-4 rounded-2xl bg-obsidian-850/80 border border-slate-800 animate-pulse flex flex-col justify-between min-h-[140px]">
    <div>
      <div className="mb-3">
        <div className="h-4 w-24 bg-slate-800 rounded-md" />
      </div>
      <div className="h-4 w-3/4 bg-slate-800 rounded-md mb-2.5" />
    </div>
    <div className="pt-4 mt-3 border-t border-slate-800/60 flex items-center justify-between">
      <div className="h-3 w-16 bg-slate-800/70 rounded-md" />
      <div className="h-4 w-16 bg-slate-800 rounded-md" />
    </div>
  </div>
);

const EmptyBlock: React.FC<{ title: string; hint?: string }> = ({ title, hint }) => (
  <div className="col-span-full flex flex-col items-center justify-center py-16 bg-obsidian-850/40 rounded-2xl border border-dashed border-slate-800 gap-3 px-6 text-center">
    <div className="w-12 h-12 rounded-2xl bg-obsidian-800 border border-slate-700 flex items-center justify-center">
      <AlertCircle className="text-slate-500" size={24} />
    </div>
    <p className="text-slate-300 font-bold text-sm">{title}</p>
    {hint && <p className="text-slate-500 text-xs max-w-sm">{hint}</p>}
  </div>
);

export const SupportTabs: React.FC<SupportTabsProps> = ({
  activeTab,
  loading,
  contentLoading = false,
  filteredFaqs,
  modules = [],
  selectedModule = "",
  filteredGuides,
  setSelectedGuide,
  onSelectFaq,
  allGuides = [],
  sortBy = "newest",
  projectName = "Yardım Merkezi",
  guidesPagination,
  onGuidesPageChange,
  onGuidesPageSizeChange,
  faqsPagination,
  onFaqsPageChange,
  onFaqsPageSizeChange,
}) => {
  const guidesForMap = useMemo(() => {
    return allGuides && allGuides.length > 0 ? allGuides : filteredGuides;
  }, [allGuides, filteredGuides]);

  // Sort guides
  const sortedGuides = useMemo(() => {
    const list = [...filteredGuides];
    if (sortBy === "az") {
      list.sort((a, b) => a.title.localeCompare(b.title, "tr"));
    } else if (sortBy === "za") {
      list.sort((a, b) => b.title.localeCompare(a.title, "tr"));
    }
    return list;
  }, [filteredGuides, sortBy]);

  // Sort FAQs
  const sortedFaqs = useMemo(() => {
    const list = [...filteredFaqs];
    if (sortBy === "az") {
      list.sort((a, b) => a.title.localeCompare(b.title, "tr"));
    } else if (sortBy === "za") {
      list.sort((a, b) => b.title.localeCompare(a.title, "tr"));
    }
    return list;
  }, [filteredFaqs, sortBy]);

  return (
    <main
      className="relative z-10 w-full min-h-[746px] bg-obsidian-900/90 backdrop-blur-xl glass-border rounded-3xl p-5 sm:p-7 shadow-2xl flex-1 flex flex-col justify-start overflow-hidden"
      data-purpose="dynamic-content-frame"
    >
      {/* Content Rendering - Clean, uncluttered and spacious */}
      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 auto-rows-fr">
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
        </div>
      ) : (
        <AnimatePresence mode="wait">
          <motion.div
            key={activeTab}
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.18 }}
            className="flex-1"
          >
            {/* 1. KILAVUZLAR GRID */}
            {activeTab === "guide" && (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 auto-rows-fr">
                {sortedGuides.length > 0 ? (
                  sortedGuides.map((guide, idx) => {
                    const color = getModuleColor(idx);

                    return (
                      <article
                        key={guide.publicId}
                        className="p-4 rounded-2xl bg-obsidian-850/80 border border-slate-800 hover:border-purple-500/40 transition-all duration-200 group flex flex-col justify-between min-h-[140px]"
                      >
                        <div>
                          <div className="mb-3">
                            <span
                              className={cn(
                                "text-[11px] font-semibold px-2 py-0.5 rounded-md border truncate max-w-[140px] inline-block",
                                color.text,
                                color.bg,
                                color.border
                              )}
                            >
                              {guide.module}
                            </span>
                          </div>

                          <h3 className="text-sm font-semibold text-slate-100 group-hover:text-purple-300 transition-colors leading-snug">
                            {guide.title}
                          </h3>
                        </div>

                        <div className="pt-4 mt-4 border-t border-slate-800/60 flex items-center justify-between text-xs">
                          {guide.documents && guide.documents.length > 0 ? (
                            <span className="inline-flex items-center gap-1.5 text-[11px] text-slate-400 font-medium">
                              <FileText size={12} className="text-purple-400" />
                              {guide.documents.length} Ek Dosya
                            </span>
                          ) : (
                            <span className="text-[11px] text-slate-500">Standart Rehber</span>
                          )}

                          <button
                            type="button"
                            onClick={() => setSelectedGuide(guide)}
                            className="text-purple-400 hover:text-purple-300 font-medium inline-flex items-center space-x-1 cursor-pointer transition ml-auto group-hover:translate-x-0.5"
                          >
                            <span>İncele</span>
                            <svg
                              className="w-3.5 h-3.5 transition-transform"
                              fill="none"
                              stroke="currentColor"
                              viewBox="0 0 24 24"
                            >
                              <path d="M9 5l7 7-7 7" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
                            </svg>
                          </button>
                        </div>
                      </article>
                    );
                  })
                ) : (
                  <EmptyBlock
                    title="Kullanım Kılavuzu Bulunamadı"
                    hint={
                      selectedModule
                        ? `"${selectedModule}" modülü için arama kriterine uygun kılavuz bulunamadı.`
                        : "Bu proje için henüz kullanım kılavuzu yayınlanmamış."
                    }
                  />
                )}
              </div>
            )}
            {activeTab === "guide" && guidesPagination && guidesPagination.totalCount > 0 && onGuidesPageChange && onGuidesPageSizeChange && (
              <Pagination
                currentPage={guidesPagination.currentPage}
                totalPages={guidesPagination.totalPages}
                totalItems={guidesPagination.totalCount}
                pageSize={guidesPagination.pageSize}
                onPageChange={onGuidesPageChange}
                onPageSizeChange={onGuidesPageSizeChange}
                itemsLabel="kılavuz"
                containerClassName={cn(PAGINATION_CONTAINER_CLASS, contentLoading && "opacity-60 pointer-events-none")}
              />
            )}

            {/* 2. SIKÇA SORULAN SORULAR GRID */}
            {activeTab === "sss" && (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 auto-rows-fr">
                {sortedFaqs.length > 0 ? (
                  sortedFaqs.map((faq, idx) => {
                    const color = getModuleColor(idx);

                    return (
                      <article
                        key={idx}
                        onClick={() => onSelectFaq(faq)}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === "Enter" || e.key === " ") {
                            e.preventDefault();
                            onSelectFaq(faq);
                          }
                        }}
                        className="p-4 rounded-2xl bg-obsidian-850/80 border border-slate-800 hover:border-purple-500/40 transition-all duration-200 group flex flex-col justify-between min-h-[140px] cursor-pointer"
                      >
                        <div>
                          <div className="flex items-center justify-between mb-3">
                            <span
                              className={cn(
                                "text-[11px] font-semibold px-2 py-0.5 rounded-md border truncate max-w-[140px]",
                                color.text,
                                color.bg,
                                color.border
                              )}
                            >
                              {faq.moduleName || "Genel"}
                            </span>
                            <span className="text-[11px] text-slate-400">SSS</span>
                          </div>

                          <h3 className="text-sm font-semibold text-slate-100 group-hover:text-purple-300 transition-colors leading-snug">
                            {faq.title}
                          </h3>
                        </div>

                        <div className="pt-4 mt-4 border-t border-slate-800/60 flex items-center justify-between text-xs">
                          <span className="text-purple-400 group-hover:text-purple-300 font-medium inline-flex items-center space-x-1 transition ml-auto group-hover:translate-x-0.5">
                            <span>Detayı Gör</span>
                            <svg
                              className="w-3.5 h-3.5 transition-transform"
                              fill="none"
                              stroke="currentColor"
                              viewBox="0 0 24 24"
                            >
                              <path d="M9 5l7 7-7 7" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
                            </svg>
                          </span>
                        </div>
                      </article>
                    );
                  })
                ) : (
                  <EmptyBlock
                    title="SSS İçeriği Bulunamadı"
                    hint="Arama kriterlerinize uygun sıkça sorulan soru bulunamadı."
                  />
                )}
              </div>
            )}
            {activeTab === "sss" && faqsPagination && faqsPagination.totalCount > 0 && onFaqsPageChange && onFaqsPageSizeChange && (
              <Pagination
                currentPage={faqsPagination.currentPage}
                totalPages={faqsPagination.totalPages}
                totalItems={faqsPagination.totalCount}
                pageSize={faqsPagination.pageSize}
                onPageChange={onFaqsPageChange}
                onPageSizeChange={onFaqsPageSizeChange}
                itemsLabel="SSS"
                containerClassName={cn(PAGINATION_CONTAINER_CLASS, contentLoading && "opacity-60 pointer-events-none")}
              />
            )}

            {/* 3. ZİHİN HARİTASI / SÜREÇLER */}
            {activeTab === "flow" && (
              <div className="w-full">
                {guidesForMap.length > 0 || modules.length > 0 ? (
                  <MindMapFlow
                    projectName={projectName}
                    modules={modules}
                    guides={guidesForMap}
                    selectedModule={selectedModule}
                    onSelectGuide={setSelectedGuide}
                  />
                ) : (
                  <EmptyBlock
                    title="Zihin Haritası Bulunamadı"
                    hint="Bu modül veya proje için zihin haritası süreç adımları henüz tanımlanmamış."
                  />
                )}
              </div>
            )}
          </motion.div>
        </AnimatePresence>
      )}
    </main>
  );
};
