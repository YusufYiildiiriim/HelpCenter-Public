"use client";

import React, { useEffect, useRef, useMemo } from "react";
import { Layers } from "lucide-react";
import { cn } from "@/lib/utils";
import type { PublicProject } from "@/services/public/PublicProjectService";
import type { PublicModule as Module } from "@/services/public/PublicModuleService";
import { PortalSelect, type PortalSelectOption } from "./PortalSelect";

export interface HeroSectionProps {
  searchTerm: string;
  setSearchTerm: (term: string) => void;
  activeTab?: "guide" | "sss" | "flow";
  setActiveTab?: (tab: "guide" | "sss" | "flow") => void;
  guidesCount?: number;
  faqsCount?: number;
  flowsCount?: number;
  // Modules, Category Header & Separated Sorting
  modules?: Module[];
  selectedModule?: string;
  setSelectedModule?: (mod: string) => void;
  sortBy?: "newest" | "az" | "za";
  setSortBy?: (sort: "newest" | "az" | "za") => void;
  // Compatibility props for existing usage
  projects?: PublicProject[];
  selectedProjectId?: number | null;
  onProjectChange?: (projectId: number) => void;
  modulesCount?: number;
  placeholder?: string;
}

export const HeroSection: React.FC<HeroSectionProps> = ({
  searchTerm,
  setSearchTerm,
  activeTab = "guide",
  setActiveTab,
  guidesCount = 0,
  faqsCount = 0,
  flowsCount = 0,
  modules = [],
  selectedModule = "",
  setSelectedModule,
  sortBy = "newest",
  setSortBy,
  placeholder,
}) => {
  const inputRef = useRef<HTMLInputElement>(null);

  // ⌘K / Ctrl+K keyboard shortcut → focus search
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        inputRef.current?.focus();
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const activeCategoryTitle = useMemo(() => {
    if (activeTab === "guide") return "Kullanım Kılavuzları & Başlangıç Rehberleri";
    if (activeTab === "sss") return "Sıkça Sorulan Sorular & Çözümler";
    return "Zihin Haritası & Süreç Akışları";
  }, [activeTab]);

  const activeCategoryDesc = useMemo(() => {
    if (activeTab === "guide")
      return "İlgili dokümantasyonları adım adım inceleyebilir veya PDF olarak indirebilirsiniz.";
    if (activeTab === "sss")
      return "Sistem ve entegrasyonlar hakkında merak edilen soruların detaylı yanıtları.";
    return "Tüm iş akış adımlarını baştan sona sıralı olarak takip edin.";
  }, [activeTab]);

  const moduleOptions: PortalSelectOption<string>[] = useMemo(() => {
    return [
      { value: "", label: `Tüm Modüller (${modules.length})` },
      ...modules.map((m) => ({ value: m.name, label: m.name })),
    ];
  }, [modules]);

  const sortOptions: PortalSelectOption<"newest" | "az" | "za">[] = [
    { value: "newest", label: "En Yeni Güncellenen" },
    { value: "az", label: "Başlığa Göre (A-Z)" },
    { value: "za", label: "Başlığa Göre (Z-A)" },
  ];

  return (
    <section className="relative z-20 w-full space-y-4" data-purpose="navigation-and-search-layer">
      {/* Segmented Navigation Category Bar */}
      {setActiveTab && (
        <nav aria-label="Portal Kategorileri" className="grid grid-cols-1 md:grid-cols-3 gap-3">
          {/* Tab 1: Kullanım Kılavuzları */}
          <button
            type="button"
            onClick={() => setActiveTab("guide")}
            className={cn(
              "flex items-center justify-between p-3.5 rounded-2xl transition-all text-left relative overflow-hidden group cursor-pointer",
              activeTab === "guide"
                ? "bg-obsidian-850 glass-border-active shadow-glow-purple/20"
                : "bg-obsidian-900/90 glass-border hover:bg-obsidian-850 hover:border-slate-600/50"
            )}
          >
            {activeTab === "guide" && (
              <div className="absolute inset-0 bg-gradient-to-r from-purple-600/10 to-indigo-600/5 opacity-100" />
            )}
            <div className="relative flex items-center space-x-3">
              <div
                className={cn(
                  "p-2 rounded-xl transition",
                  activeTab === "guide"
                    ? "bg-purple-500/20 text-purple-300"
                    : "bg-slate-800 text-slate-400 group-hover:text-slate-200"
                )}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="2"
                  />
                </svg>
              </div>
              <span
                className={cn(
                  "text-sm font-semibold",
                  activeTab === "guide" ? "text-white" : "text-slate-300 group-hover:text-white"
                )}
              >
                Kullanım Kılavuzları
              </span>
            </div>
            <span
              className={cn(
                "relative text-xs px-2.5 py-0.5 rounded-full font-bold tabular-nums",
                activeTab === "guide"
                  ? "bg-purple-500/20 text-purple-300 border border-purple-500/30"
                  : "bg-slate-800 text-slate-400 border border-slate-700"
              )}
            >
              {guidesCount}
            </span>
          </button>

          {/* Tab 2: Sıkça Sorulan Sorular */}
          <button
            type="button"
            onClick={() => setActiveTab("sss")}
            className={cn(
              "flex items-center justify-between p-3.5 rounded-2xl transition-all text-left relative overflow-hidden group cursor-pointer",
              activeTab === "sss"
                ? "bg-obsidian-850 glass-border-active shadow-glow-purple/20"
                : "bg-obsidian-900/90 glass-border hover:bg-obsidian-850 hover:border-slate-600/50"
            )}
          >
            {activeTab === "sss" && (
              <div className="absolute inset-0 bg-gradient-to-r from-purple-600/10 to-indigo-600/5 opacity-100" />
            )}
            <div className="relative flex items-center space-x-3">
              <div
                className={cn(
                  "p-2 rounded-xl transition",
                  activeTab === "sss"
                    ? "bg-purple-500/20 text-purple-300"
                    : "bg-slate-800 text-slate-400 group-hover:text-slate-200"
                )}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="2"
                  />
                </svg>
              </div>
              <span
                className={cn(
                  "text-sm font-semibold",
                  activeTab === "sss" ? "text-white" : "text-slate-300 group-hover:text-white"
                )}
              >
                Sıkça Sorulan Sorular
              </span>
            </div>
            <span
              className={cn(
                "relative text-xs px-2.5 py-0.5 rounded-full font-bold tabular-nums",
                activeTab === "sss"
                  ? "bg-purple-500/20 text-purple-300 border border-purple-500/30"
                  : "bg-slate-800 text-slate-400 border border-slate-700"
              )}
            >
              {faqsCount}
            </span>
          </button>

          {/* Tab 3: Zihin Haritası / Süreçler */}
          <button
            type="button"
            onClick={() => setActiveTab("flow")}
            className={cn(
              "flex items-center justify-between p-3.5 rounded-2xl transition-all text-left relative overflow-hidden group cursor-pointer",
              activeTab === "flow"
                ? "bg-obsidian-850 glass-border-active shadow-glow-purple/20"
                : "bg-obsidian-900/90 glass-border hover:bg-obsidian-850 hover:border-slate-600/50"
            )}
          >
            {activeTab === "flow" && (
              <div className="absolute inset-0 bg-gradient-to-r from-purple-600/10 to-indigo-600/5 opacity-100" />
            )}
            <div className="relative flex items-center space-x-3">
              <div
                className={cn(
                  "p-2 rounded-xl transition",
                  activeTab === "flow"
                    ? "bg-purple-500/20 text-purple-300"
                    : "bg-slate-800 text-slate-400 group-hover:text-slate-200"
                )}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path d="M13 10V3L4 14h7v7l9-11h-7z" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
                </svg>
              </div>
              <span
                className={cn(
                  "text-sm font-semibold",
                  activeTab === "flow" ? "text-white" : "text-slate-300 group-hover:text-white"
                )}
              >
                Zihin Haritası / Süreçler
              </span>
            </div>
            <span
              className={cn(
                "relative text-xs px-2.5 py-0.5 rounded-full font-bold tabular-nums",
                activeTab === "flow"
                  ? "bg-purple-500/20 text-purple-300 border border-purple-500/30"
                  : "bg-slate-800 text-slate-400 border border-slate-700"
              )}
            >
              {flowsCount}
            </span>
          </button>
        </nav>
      )}

      {/* Category Header, Descriptions, Sorting & Modules Control Card */}
      <div className="relative z-30 w-full bg-obsidian-900/80 backdrop-blur-md glass-border rounded-2xl p-4 sm:p-5 shadow-xl">
        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
          {/* Left: Category Title, Badge, Description */}
          <div className="min-w-0 lg:max-w-md">
            <div className="flex items-center space-x-2.5">
              <div className="w-2 h-2 rounded-full bg-purple-400 shadow-glow-purple shrink-0" />
              <h2 className="text-base sm:text-lg font-bold text-white tracking-wide truncate">
                {activeCategoryTitle}
              </h2>
              <span className="text-[10px] uppercase font-bold tracking-wider px-2.5 py-0.5 bg-purple-500/10 text-purple-400 border border-purple-500/20 rounded-md shrink-0">
                Aktif Kategori
              </span>
            </div>
            <p className="text-xs text-slate-400 mt-1 pl-4.5">{activeCategoryDesc}</p>
          </div>

          {/* Center: Module Select Dropdown ("modülleri ortaya ve select itemi ile") */}
          {modules && modules.length > 0 && setSelectedModule && (
            <div className="flex items-center justify-start lg:justify-center">
              <PortalSelect
                label="Modül:"
                icon={<Layers size={14} className="text-purple-400" />}
                value={selectedModule}
                onChange={setSelectedModule}
                options={moduleOptions}
                dropdownWidth="w-64"
              />
            </div>
          )}

          {/* Right: Sorting Select Dropdown */}
          {setSortBy && (
            <div className="flex items-center justify-start lg:justify-end shrink-0">
              <PortalSelect<"newest" | "az" | "za">
                label="Sıralama:"
                value={sortBy}
                onChange={setSortBy}
                options={sortOptions}
                dropdownWidth="w-56"
              />
            </div>
          )}
        </div>
      </div>

      {/* Search Bar - Positioned under Category & Module Control Card */}
      <div className="w-full bg-obsidian-900/80 backdrop-blur-md glass-border rounded-2xl p-2.5 sm:p-3 flex items-center justify-between shadow-xl gap-2">
        <div className="flex items-center flex-1 px-2 space-x-3">
          <svg className="w-5 h-5 text-purple-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
          </svg>
          <input
            ref={inputRef}
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-transparent border-none text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-0"
            placeholder={placeholder || "Help Center içinde arama yapın, kılavuz veya çözüm sorgulayın..."}
            aria-label="Kılavuzlarda ara"
          />
        </div>
        <div className="flex items-center space-x-2 shrink-0">
          <button
            type="button"
            onClick={() => inputRef.current?.focus()}
            className="px-4 py-2 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white text-xs font-semibold rounded-xl shadow transition cursor-pointer"
          >
            ARA
          </button>
        </div>
      </div>
    </section>
  );
};

export const MiddleNavigationLayer = HeroSection;
