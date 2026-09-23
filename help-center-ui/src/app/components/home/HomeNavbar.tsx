"use client";

import React, { useState, useRef, useEffect } from "react";
import Link from "next/link";
import { ChevronDown, Check, FolderKanban } from "lucide-react";
import { useOrganization } from "@/context/OrganizationContext";
import type { PublicProject as Project } from "@/services/public/PublicProjectService";

interface HomeNavbarProps {
  projects?: Project[];
  selectedProjectId?: number | null;
  onProjectChange?: (projectId: number) => void;
}

export const HomeNavbar: React.FC<HomeNavbarProps> = ({
  projects = [],
  selectedProjectId = null,
  onProjectChange,
}) => {
  const { orgName, logoUrl } = useOrganization();
  const [projectMenuOpen, setProjectMenuOpen] = useState(false);
  const projectMenuRef = useRef<HTMLDivElement>(null);

  const selectedProject = projects.find((p) => p.id === selectedProjectId);

  useEffect(() => {
    if (!projectMenuOpen) return;
    const handler = (e: MouseEvent) => {
      if (projectMenuRef.current && !projectMenuRef.current.contains(e.target as Node)) {
        setProjectMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [projectMenuOpen]);

  return (
    <header
      className="relative z-40 w-full bg-obsidian-900/80 backdrop-blur-xl glass-border rounded-3xl p-4 sm:px-6 sm:py-4 shadow-2xl flex flex-wrap items-center justify-between gap-4"
      data-purpose="header-container"
    >
      {/* Left spacer / Project Selector */}
      <div className="hidden md:flex flex-1 items-center justify-start">
        {projects.length > 0 && onProjectChange && (
          <div className="relative" ref={projectMenuRef}>
            <button
              type="button"
              onClick={() => setProjectMenuOpen((o) => !o)}
              className="px-3.5 py-2 rounded-xl bg-obsidian-850 hover:bg-obsidian-800 border border-slate-700/60 text-xs font-semibold text-slate-300 hover:text-white transition flex items-center gap-2 shadow-sm cursor-pointer"
            >
              <FolderKanban size={14} className="text-purple-400" />
              <span className="text-[11px] text-slate-400 font-medium">Proje:</span>
              <span className="font-bold text-white max-w-[150px] truncate">
                {selectedProject?.name || "Seçin"}
              </span>
              <ChevronDown
                size={12}
                className={`text-slate-400 transition-transform ${projectMenuOpen ? "rotate-180" : ""}`}
              />
            </button>

            {projectMenuOpen && (
              <div className="absolute left-0 top-full mt-2 w-64 bg-obsidian-900 border border-slate-800 rounded-2xl shadow-2xl p-1.5 z-50 backdrop-blur-xl">
                <div className="px-3 py-1.5 border-b border-slate-800/80">
                  <p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">Projeler</p>
                </div>
                <ul className="max-h-60 overflow-y-auto py-1">
                  {projects.map((p) => {
                    const isActive = p.id === selectedProjectId;
                    return (
                      <li key={p.id}>
                        <button
                          type="button"
                          onClick={() => {
                            onProjectChange(p.id);
                            setProjectMenuOpen(false);
                          }}
                          className={`w-full flex items-center justify-between px-3 py-2 text-left text-xs rounded-xl transition ${
                            isActive
                              ? "bg-purple-500/20 text-purple-300 font-bold"
                              : "text-slate-300 hover:bg-obsidian-800 hover:text-white"
                          }`}
                        >
                          <span className="truncate">{p.name}</span>
                          {isActive && <Check size={14} className="text-purple-400 shrink-0" />}
                        </button>
                      </li>
                    );
                  })}
                </ul>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Centered Brand & Version Segment */}
      <div className="flex items-center justify-center space-x-3.5 mx-auto md:mx-0" data-purpose="brand-info">
        <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-purple-600 via-indigo-500 to-fuchsia-500 p-0.5 shadow-glow-purple flex items-center justify-center shrink-0">
          <div className="w-full h-full bg-obsidian-900 rounded-[10px] flex items-center justify-center overflow-hidden">
            {logoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={logoUrl} alt={`${orgName} Logo`} className="w-5 h-5 object-contain" />
            ) : (
              <svg className="w-5 h-5 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path d="M13 10V3L4 14h7v7l9-11h-7z" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
              </svg>
            )}
          </div>
        </div>
        <div className="flex flex-col text-left">
          <div className="flex items-center space-x-2">
            <span className="font-bold text-lg tracking-tight text-white leading-tight">
              {orgName || "Help Center"}
            </span>
            <span className="text-[11px] font-semibold tracking-wide px-2 py-0.5 rounded-full bg-purple-500/10 text-purple-400 border border-purple-500/20 uppercase whitespace-nowrap">
              v2.4 Portal
            </span>
          </div>
          <p className="text-xs text-slate-400 leading-normal mt-0.5 hidden sm:block">
            Kurumsal Bilgi Bankası &amp; Destek Merkezi
          </p>
        </div>
      </div>

      {/* Right Action Buttons */}
      <div className="flex flex-1 items-center justify-end space-x-3 ml-auto" data-purpose="header-actions">
        {/* Ticket Button */}
        <Link
          href="/dashboard"
          className="px-4 py-2 text-xs sm:text-sm font-medium text-slate-300 hover:text-white bg-obsidian-800 hover:bg-obsidian-750 border border-slate-700/60 rounded-xl transition-all duration-200 flex items-center space-x-2 shadow-sm"
        >
          <svg className="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
            />
          </svg>
          <span>Talep Oluştur</span>
        </Link>

        {/* Admin Login Button */}
        <Link
          href="/admin/login"
          className="px-4 py-2 text-xs sm:text-sm font-medium text-white bg-gradient-to-r from-purple-600 via-indigo-600 to-fuchsia-600 hover:opacity-95 rounded-xl shadow-glow-purple transition-all duration-200 flex items-center space-x-2"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
            />
          </svg>
          <span>Admin Giriş</span>
        </Link>
      </div>
    </header>
  );
};
